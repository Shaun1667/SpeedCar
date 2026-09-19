using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("비워두면 Camera.main을 사용합니다.")]
    public Camera cam;

    [Tooltip("좌우로 움직일 플레이어(자동차). 비워두면 이 스크립트가 붙은 오브젝트 자신을 사용합니다.")]
    public Transform player;

    [Header("이동 설정 (SmoothDamp)")]
    [Tooltip("목표 위치까지 도달하는 데 걸리는 대략적인 시간 (초). 작을수록 빠르게 붙음")]
    public float smoothTime = 0.3f;

    [Tooltip("Raycast 최대 거리")]
    public float maxRayDistance = 500f;

    Vector3 targetPos;
    bool hasTarget;

    // SmoothDamp 전용 내부 속도 값 (private으로 감춰서 Inspector에서 실수로 값이 남지 않게 함)
    Vector3 velocity;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (player == null) player = transform;
        targetPos = player.position;
    }

    void Update()
    {
        UpdatePlayerPos();
        HandleClick();
        MoveTowardTargetX();
    }

    void UpdatePlayerPos()
    {
        targetPos.y = player.position.y;
        targetPos.z = player.position.z;
    }

    void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
        if (cam == null) return;

        Vector2 screenPos = mouse.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            if (hit.collider.CompareTag("PlayerController"))
            {
                var controller = hit.transform.GetComponent<ControllerCollider>();
                if (controller != null)
                {
                    targetPos.x = controller.movePosition.x;
                    hasTarget = true;
                }
            }
        }
    }

    void MoveTowardTargetX()
    {
        if (!hasTarget || player == null) return;

        player.position = Vector3.SmoothDamp(player.position, targetPos, ref velocity, smoothTime);
    }
}
