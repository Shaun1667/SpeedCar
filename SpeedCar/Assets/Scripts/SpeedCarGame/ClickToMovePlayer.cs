using UnityEngine;
using UnityEngine.InputSystem;

namespace SpeedCarGame
{
    /// <summary>
    /// 화면을 클릭하면 카메라에서 Raycast를 쏩니다. 맞은 콜라이더의 태그가
    /// "PlayerController"이면, 그 클릭 지점의 x좌표로 플레이어(자동차)를
    /// 좌우로 부드럽게 이동시킵니다. (앞뒤 이동은 건드리지 않고 x축만 조정)
    ///
    /// 사용법:
    /// 1. Project Settings > Tags and Layers 에서 "PlayerController" 태그를 새로 만듭니다.
    /// 2. 클릭을 받을 대상(예: 도로 바닥 전체를 덮는 넓고 평평한 콜라이더, 또는
    ///    보이지 않는 클릭 인식용 Plane)에 그 "PlayerController" 태그를 붙입니다.
    /// 3. 이 스크립트를 아무 오브젝트(예: 플레이어 자동차 또는 빈 GameObject)에 붙이고
    ///    player 필드에 자동차 Transform을 연결합니다. (비워두면 자기 자신을 사용)
    /// </summary>
    public class ClickToMovePlayer : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비워두면 Camera.main을 사용합니다.")]
        public Camera cam;

        [Tooltip("좌우로 움직일 플레이어(자동차). 비워두면 이 스크립트가 붙은 오브젝트 자신을 사용합니다.")]
        public Transform player;

        [Header("이동 설정")]
        [Tooltip("클릭한 x좌표로 이동하는 속도 (m/s)")]
        public float moveSpeed = 15f;

        [Tooltip("도로 좌우 이동 가능 범위 제한 (도로 폭에 맞게 조절)")]
        public float roadHalfWidth = 6f;

        [Tooltip("Raycast 최대 거리")]
        public float maxRayDistance = 500f;

        float targetX;
        bool hasTarget;

        void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (player == null) player = transform;
            targetX = player.position.x;
        }

        void Update()
        {
            HandleClick();
            MoveTowardTargetX();
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
                    targetX = Mathf.Clamp(hit.point.x, -roadHalfWidth, roadHalfWidth);
                    hasTarget = true;
                }
            }
        }

        void MoveTowardTargetX()
        {
            if (!hasTarget || player == null) return;

            Vector3 pos = player.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
            player.position = pos;
        }
    }
}
