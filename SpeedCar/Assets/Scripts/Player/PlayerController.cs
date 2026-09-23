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

    [Header("좌우 이동 시 살짝 회전(스티어링 틸트)")]
    [Tooltip("좌우로 이동하는 동안 y축(좌우 방향)으로 최대 몇 도까지 살짝 돌아갈지. " +
             "0이면 회전 효과 없이 예전처럼 정면만 바라봅니다.")]
    public float maxTurnAngle = 20f;

    [Tooltip("좌우 이동 속도를 회전 각도로 바꿀 때 곱하는 민감도. 클수록 조금만 움직여도 많이 기울어집니다.")]
    public float turnSensitivity = 6f;

    [Tooltip("현재 회전 각도가 목표 각도(또는 다시 0도)로 부드럽게 따라가는 데 걸리는 대략적인 시간(초)")]
    public float turnRotationSmoothTime = 0.12f;

    Vector3 targetPos;
    bool hasTarget;

    // SmoothDamp 전용 내부 속도 값 (private으로 감춰서 Inspector에서 실수로 값이 남지 않게 함)
    Vector3 velocity;

    // 좌우로 움직이는 동안 살짝 돌아갔다가, 움직임이 멈추면 다시 원래 각도로 돌아오기 위한 값들.
    // baseEuler는 시작할 때의 원래 회전(정면 방향)을 기억해두고, 여기에 currentYawOffset만큼만
    // 임시로 더해서 회전시킵니다.
    Vector3 baseEuler;
    float currentYawOffset;
    float yawOffsetVelocity;
    float previousX;

    /// <summary>가장 최근 클릭에서 x목표가 바뀐 거리(월드 단위). 차선 간격이 1이면
    /// 이 값 자체가 "몇 칸을 건너뛰었는지"와 같습니다. LaneBumper가 충돌 시
    /// 밀려나는 거리를 계산할 때 참고합니다.</summary>
    public float LastJumpDistance { get; private set; }

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (player == null) player = transform;
        targetPos = player.position;
        baseEuler = player.eulerAngles;
        previousX = player.position.x;
    }

    void Update()
    {
        UpdatePlayerPos();
        HandleClick();
        MoveTowardTargetX();
        RotateTowardMovement();
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
                    float previousTargetX = targetPos.x;
                    targetPos.x = controller.movePosition.x;
                    LastJumpDistance = Mathf.Abs(targetPos.x - previousTargetX);
                    hasTarget = true;

                    if (SEManager.Instance != null)
                        SEManager.Instance.PlaySound("PlayerMove");
                }
            }
        }
    }

    void MoveTowardTargetX()
    {
        if (!hasTarget || player == null) return;

        player.position = Vector3.SmoothDamp(player.position, targetPos, ref velocity, smoothTime);
    }

    // 좌우로 실제 이동한 속도(초당 x이동량)에 비례해서 y축으로 살짝 돌아갔다가,
    // 목표 차선에 도착해서 더 이상 좌우로 움직이지 않으면 자연스럽게 다시 0(정면)으로
    // 돌아옵니다. 클릭 순간이 아니라 "실제로 움직이고 있는 정도"를 기준으로 하기 때문에
    // 이동이 끝나면 저절로 원래 각도로 복귀합니다.
    void RotateTowardMovement()
    {
        if (player == null) return;

        float deltaX = player.position.x - previousX;
        previousX = player.position.x;

        float lateralSpeed = Time.deltaTime > 0f ? deltaX / Time.deltaTime : 0f;
        float targetYawOffset = Mathf.Clamp(lateralSpeed * turnSensitivity, -maxTurnAngle, maxTurnAngle);

        currentYawOffset = Mathf.SmoothDampAngle(currentYawOffset, targetYawOffset, ref yawOffsetVelocity, turnRotationSmoothTime);

        player.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + currentYawOffset, baseEuler.z);
    }
}
