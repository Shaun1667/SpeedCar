using UnityEngine;

/// <summary>
/// 트래픽 자동차에 붙이는 스크립트입니다. 생성되는 순간(Start) 플레이어의
/// "현재" 속도를 읽어와서, 거기에 약간의 무작위 편차를 더한 값을 자기 자신의
/// 전진 속도로 등록하고, 그 뒤로는 플레이어가 더 빨라지든 말든 상관없이 그
/// 속도 그대로 계속 앞으로 갑니다. (차마다 속도가 조금씩 달라서 너무 규칙적으로
/// 보이지 않게 됩니다)
/// </summary>
public class TrafficCarAutoSpeed : MonoBehaviour
{
    [Tooltip("생성 시점에 등록된 전진 속도 (읽기 전용 확인용, Inspector에서 직접 바꿔도 됩니다)")]
    public float speed;

    [Tooltip("플레이어 속도 기준 얼마까지 무작위로 속도를 다르게 할지 (m/s)")]
    public float speedVariance = 5f;

    [Tooltip("무작위 편차를 적용해도 이 값보다 느려지지는 않도록 하는 최소 속도 (m/s)")]
    public float minSpeed = 0.1f;

    void Start()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            speed = player.speed + Random.Range(-speedVariance, 2);
            speed = Mathf.Max(speed, minSpeed);
        }
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
