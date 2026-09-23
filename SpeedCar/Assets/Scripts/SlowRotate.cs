using UnityEngine;

/// <summary>
/// 오브젝트를 y축(위에서 내려다볼 때 시계/반시계 방향)으로 천천히 계속 회전시키는
/// 간단한 연출용 스크립트입니다. 타이틀 씬에 전시해둔 자동차 등에 붙이면 됩니다.
/// </summary>
public class SlowRotate : MonoBehaviour
{
    [Tooltip("초당 회전 속도(도/초). 값이 클수록 더 빠르게 돕니다. 음수를 넣으면 " +
             "반대 방향으로 돕니다.")]
    public float rotationSpeed = 20f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
