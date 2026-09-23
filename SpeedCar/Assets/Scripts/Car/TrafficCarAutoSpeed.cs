using UnityEngine;

/// <summary>
/// 트래픽 자동차에 붙이는 스크립트입니다. 생성되는 순간(Start) 플레이어의
/// "현재" 속도를 읽어와서, 거기에 약간의 무작위 편차를 더한 값을 자기 자신의
/// 전진 속도로 등록하고, 그 뒤로는 플레이어가 더 빨라지든 말든 상관없이 그
/// 속도 그대로 계속 앞으로 갑니다. (차마다 속도가 조금씩 달라서 너무 규칙적으로
/// 보이지 않게 됩니다)
///
/// 삭제(디스폰)는 더 이상 자신을 스폰한 도로 모듈에 묶여있지 않습니다. 대신 매 프레임
/// 플레이어의 z좌표와 자신의 z좌표를 비교해서, 플레이어보다 일정 거리 이상 뒤로
/// 처지면 스스로 삭제됩니다. (모듈이 삭제될 때 아직 화면에 남아있는 차까지 같이
/// 사라져버리던 문제를 해결하기 위함)
/// </summary>
public class TrafficCarAutoSpeed : MonoBehaviour
{
    [Tooltip("생성 시점에 등록된 전진 속도 (읽기 전용 확인용, Inspector에서 직접 바꿔도 됩니다)")]
    public float speed;

    [Tooltip("플레이어 속도 기준 +- 얼마까지 무작위로 속도를 다르게 할지 (m/s)")]
    public float speedVariance = 3f;

    [Tooltip("무작위 편차를 적용해도 이 값보다 느려지지는 않도록 하는 최소 속도 (m/s)")]
    public float minSpeed = 2f;

    [Tooltip("게임이 시작된 뒤 1초가 지날 때마다, 새로 스폰되는 자동차의 기준 속도(그 순간 " +
             "플레이어 속도)에서 추가로 빼는 속도(m/s). 플레이어 속도를 그대로 따라가기만 하면 " +
             "시간이 지나도 상대 속도차가 똑같아서 점점 빨라지는 느낌이 전혀 안 들기 때문에, " +
             "시간이 지날수록 새로 스폰되는 차들이 플레이어보다 상대적으로 더 느려지게 합니다.")]
    public float speedReductionPerSecond = 0.15f;

    [Tooltip("Speed Reduction Per Second로 인한 감소량의 최댓값(m/s). 게임이 아주 오래 지나도 " +
             "자동차가 뒤로 가버리거나 하지 않도록 감소량 자체를 여기서 제한합니다.")]
    public float maxSpeedReduction = 15f;

    [Tooltip("플레이어보다 z좌표 기준 이 거리(m) 이상 뒤로 처지면 자동으로 삭제됩니다. " +
             "화면 밖으로 완전히 사라진 뒤에 지우도록 카메라가 보는 범위보다 넉넉하게 설정하세요.")]
    public float despawnDistanceBehindPlayer = 40f;

    Transform player;

    void Start()
    {
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;

            // 게임 시작 후 지난 시간만큼 기준 속도에서 조금씩 더 빼서, 나중에 스폰될수록
            // 그 순간 플레이어 속도보다 상대적으로 느린 차가 나오게 합니다. (이미 스폰된
            // 차의 속도는 그대로 유지되고, 새로 스폰되는 차부터 적용됩니다)
            float elapsed = Time.timeSinceLevelLoad;
            float speedReduction = Mathf.Min(elapsed * speedReductionPerSecond, maxSpeedReduction);

            speed = playerMovement.speed - speedReduction + Random.Range(-speedVariance, speedVariance);
            speed = Mathf.Max(speed, minSpeed);
        }
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        CheckDespawn();
    }

    void CheckDespawn()
    {
        if (player == null) return;

        // 플레이어는 항상 앞(+z 방향)으로 나아가므로, 플레이어의 z가 이 차의 z보다
        // despawnDistanceBehindPlayer 이상 커지면 이 차는 화면 밖 뒤쪽으로 충분히
        // 멀어졌다는 뜻입니다. 어느 모듈이 자신을 스폰했는지와는 무관하게 판단합니다.
        if (player.position.z - transform.position.z > despawnDistanceBehindPlayer)
            Destroy(gameObject);
    }
}
