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
    public float cooldown = 0.1f;

    [Tooltip("반응할 상대방 태그들")]
    public string[] reactTags = { "Player", "Traffic" };

    Rigidbody rb;
    float lastBumpTime = -999f;
    bool flungOff;
    Vector3 flyDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Kinematic vs Kinematic 조합은 트리거 이벤트가 서로 안 나기 때문에,
        // Dynamic으로 두고 중력/회전/이동을 전부 Freeze해서 물리 힘에는
        // 영향받지 않으면서 트리거 감지만 확실히 되도록 합니다.
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
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

        int targetLane = myLane + direction;

        if (targetLane < 0 || targetLane >= lanePositions.Length)
        {
            FlyOffMap(direction);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(BounceToLane(lanePositions[targetLane]));
        }
    }

    IEnumerator BounceToLane(float targetX)
    {
        float startX = transform.position.x;
        float t = 0f;
        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startX, targetX, t / bounceDuration);
            transform.position = pos;
            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;
    }

    void FlyOffMap(int direction)
    {
        flungOff = true;
        flyDirection = new Vector3(direction, 0f, 0f);
        Destroy(gameObject, flyOffDestroyDelay);
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
