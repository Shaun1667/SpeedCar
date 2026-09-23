using System.Collections;
using UnityEngine;

/// <summary>
/// 트래픽 자동차에 붙는 컴포넌트입니다. 플레이어 또는 다른 트래픽 차량과 부딪히면
/// 옆 차선으로 튕겨져 나갑니다. 이미 맨 끝(가장 바깥) 차선이라 더 튕겨날 차선이
/// 없다면, 도로 밖으로 완전히 날아가서 사라집니다.
///
/// 주의: 유니티에서 Kinematic Rigidbody끼리는 트리거 이벤트가 서로 발생하지
/// 않습니다 (트래픽 차량끼리 부딪혀도 반응이 없던 이유가 이것입니다). 그래서 이
/// 스크립트는 Rigidbody를 Kinematic이 아닌 일반(Dynamic) Rigidbody로 두되,
/// 중력을 끄고 모든 축을 Freeze시켜서 물리 힘의 영향은 받지 않으면서도
/// 트래픽 차량-트래픽 차량 사이의 트리거 이벤트가 확실히 발생하도록 합니다.
/// 이 오브젝트의 Collider는 Is Trigger를 켜두는 걸 추천합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LaneBumper : MonoBehaviour
{
    [Tooltip("차선 x좌표들. PlayerController/TrafficOnModule과 동일하게 맞춰주세요.")]
    public float[] lanePositions = { -2f, -1f, 0f, 1f, 2f };

    [Tooltip("플레이어와 부딪혔을 때, 플레이어의 최근 차선 이동 거리 1당 추가로 " +
             "몇 칸을 더 밀어낼지. 1이면 \"2칸 이동해서 부딪히면 2칸 밀려남\"이 됩니다.")]
    public float playerJumpPushMultiplier = 1f;

    [Tooltip("옆 차선으로 튕겨 이동하는 데 걸리는 시간(초)")]
    public float bounceDuration = 0.3f;

    [Tooltip("차선 밖(맵 밖)으로 날아갈 때의 좌우 속도(m/s)")]
    public float flyOffSpeed = 15f;

    [Tooltip("맵 밖으로 날아간 뒤 몇 초 후에 오브젝트를 삭제할지")]
    public float flyOffDestroyDelay = 2f;

    [Tooltip("한 번 반응한 뒤 다시 반응하기까지 최소 대기 시간(초). 같은 충돌에 " +
             "여러 프레임 동안 겹쳐서 여러 번 튕기는 것을 방지합니다.")]
    public float cooldown = 0.5f;

    [Tooltip("반응할 상대방 태그들")]
    public string[] reactTags = { "Player", "Traffic" };

    [Tooltip("옆 차선으로 튕겨나가는 동안 y축(좌우 방향)으로 최대 몇 도까지 살짝 돌아갔다가 " +
             "도착하면 다시 원래 각도로 돌아올지. 0이면 회전 효과 없이 예전처럼 미끄러지듯 이동만 합니다.")]
    public float maxTurnAngle = 25f;

    Rigidbody rb;
    float lastBumpTime = -999f;
    bool flungOff;
    Vector3 flyDirection;

    // 스폰될 때(또는 처음 Awake될 때)의 원래 회전. 튕겨나가는 동안 잠깐 여기서 벗어났다가
    // 다시 정확히 이 각도로 복귀합니다.
    Vector3 baseEuler;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Kinematic vs Kinematic 조합은 트리거 이벤트가 서로 안 나기 때문에,
        // Dynamic으로 두고 중력/회전/이동을 전부 Freeze해서 물리 힘에는
        // 영향받지 않으면서 트리거 감지만 확실히 되도록 합니다.
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        baseEuler = transform.eulerAngles;
    }

    void Update()
    {
        if (flungOff)
            transform.position += flyDirection * flyOffSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) => TryBump(other.transform, other.tag);
    void OnCollisionEnter(Collision collision) => TryBump(collision.transform, collision.transform.tag);

    void TryBump(Transform other, string otherTag)
    {
        if (flungOff) return;
        if (Time.time - lastBumpTime < cooldown) return;

        bool shouldReact = false;
        foreach (var tag in reactTags)
        {
            if (otherTag == tag) { shouldReact = true; break; }
        }
        if (!shouldReact) return;

        lastBumpTime = Time.time;

        int myLane = FindNearestLaneIndex(transform.position.x);

        // 상대보다 내가 오른쪽에 있으면 오른쪽 차선으로, 왼쪽에 있으면 왼쪽 차선으로 튕겨남
        // (x가 거의 같다면 무작위로 방향 결정)
        int direction;
        if (Mathf.Approximately(transform.position.x, other.position.x))
            direction = Random.value < 0.5f ? -1 : 1;
        else
            direction = transform.position.x > other.position.x ? 1 : -1;

        // 기본은 한 칸만 밀려나지만, 부딪힌 상대가 플레이어이고 방금 여러 차선을
        // 한 번에 건너뛰어 온 상태라면 그 칸 수만큼 더 멀리 밀려납니다.
        // other는 "부딪힌 콜라이더가 붙어있는 오브젝트"의 Transform입니다. 플레이어 차량이
        // 루트에 PlayerController를 두고 실제 충돌용 Collider는 자식 오브젝트(차체 메쉬 등)에
        // 붙어있는 구조라면, 여기서 그냥 GetComponent를 쓰면 같은 오브젝트에서만 찾기 때문에
        // 못 찾고 null이 되어 항상 기본값(1칸)으로만 밀려나는 문제가 생깁니다. 그래서
        // GetComponentInParent로 자기 자신과 부모 쪽까지 함께 찾도록 합니다.
        int pushLanes = 1;
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            int extra = Mathf.RoundToInt(player.LastJumpDistance * playerJumpPushMultiplier);
            pushLanes = Mathf.Max(1, extra);
        }

        int targetLane = myLane + direction * pushLanes;
        bool outOfRange = targetLane < 0 || targetLane >= lanePositions.Length;

        if (outOfRange && player != null)
        {
            // 플레이어에게 밀려서 범위를 벗어난 경우에만 맵 밖으로 날아갑니다.
            FlyOffMap(direction);
        }
        else
        {
            // 자동차끼리 부딪힌 경우에는 맵 밖으로 나가지 않고, 범위를 벗어나면 그냥
            // 가장 가장자리 차선에서 멈춥니다.
            int clampedLane = Mathf.Clamp(targetLane, 0, lanePositions.Length - 1);

            // 플레이어에게 밀린 경우에만 소리를 재생합니다. 자동차끼리 부딪혀서
            // 밀려나는 경우에는 소리를 재생하지 않습니다.
            if (player != null && SEManager.Instance != null)
                SEManager.Instance.PlaySound("CarPush");

            StopAllCoroutines();
            StartCoroutine(BounceToLane(lanePositions[clampedLane]));
        }
    }

    IEnumerator BounceToLane(float targetX)
    {
        float startX = transform.position.x;

        // 이동 방향(왼쪽/오른쪽)에 따라 회전을 어느 쪽으로 기울일지 정합니다.
        float turnDirection = Mathf.Approximately(targetX, startX) ? 0f : Mathf.Sign(targetX - startX);

        float t = 0f;
        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / bounceDuration);

            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startX, targetX, ratio);
            transform.position = pos;

            // sin(0)=0, sin(중간)=1, sin(끝)=0 이 되는 곡선이라, 이동을 시작할 때 0도에서
            // 점점 기울어졌다가 목표 차선에 도착할 즈음엔 다시 자연스럽게 0도(원래 각도)로
            // 돌아옵니다.
            float turnAmount = Mathf.Sin(ratio * Mathf.PI) * maxTurnAngle * turnDirection;
            transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + turnAmount, baseEuler.z);

            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;

        // 혹시 도중에 오차가 남아있을 수 있으니, 끝나면 확실하게 원래 각도로 맞춰줍니다.
        transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y, baseEuler.z);
    }

    void FlyOffMap(int direction)
    {
        flungOff = true;
        flyDirection = new Vector3(direction, 0f, 0f);
        Destroy(gameObject, flyOffDestroyDelay);

        if (SEManager.Instance != null)
            SEManager.Instance.PlaySound("CarFlyOff");

        // 차 한 대를 레일 밖으로 날려버릴 때마다 점수 1점을 올립니다.
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(1);
    }

    int FindNearestLaneIndex(float x)
    {
        int best = 0;
        float bestDist = Mathf.Abs(lanePositions[0] - x);
        for (int i = 1; i < lanePositions.Length; i++)
        {
            float dist = Mathf.Abs(lanePositions[i] - x);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }
}
