using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Tooltip("현재 이동 속도 (m/s). 게임이 진행될수록 자동으로 증가합니다.")]
    public float speed = 1f;

    [Tooltip("속도가 계속 증가해도 되는 최대값 (m/s)")]
    public float maxSpeed = 20f;

    [Tooltip("초당 속도 증가량 (m/s per second). 값이 클수록 더 빨리 빨라집니다.")]
    public float acceleration = 0.5f;

    void Update()
    {
        // 시간이 지날수록 speed를 maxSpeed까지 점차 증가시킴
        if (speed < maxSpeed)
        {
            speed += acceleration * Time.deltaTime;
            speed = Mathf.Min(speed, maxSpeed);
        }

        // 자기 자신의 로컬 forward 방향(파란 축, Z+)으로 매 프레임 이동
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
