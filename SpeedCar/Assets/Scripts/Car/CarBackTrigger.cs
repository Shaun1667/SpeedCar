using UnityEngine;

/// <summary>
/// 트래픽 자동차 뒷면에 만든 "CarBack" 콜라이더 오브젝트에 붙이는 스크립트입니다.
/// 플레이어가 여기에 부딪히면(즉, 앞차 뒤를 들이받으면) 플레이어만 멈추고(트래픽 등
/// 나머지는 계속 움직임) GameManager를 통해 게임오버 UI를 띄웁니다.
///
/// 사용법: CarBack 콜라이더 오브젝트(태그를 "CarBack"으로 설정하신 그 오브젝트)에
/// 이 스크립트를 붙이세요. 콜라이더가 Is Trigger든 아니든 상관없이 동작하도록
/// OnTriggerEnter/OnCollisionEnter를 모두 확인합니다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CarBackTrigger : MonoBehaviour
{
    [Tooltip("이 태그를 가진 오브젝트가 닿았을 때도 플레이어로 인식합니다. " +
             "(PlayerController/PlayerMovement 컴포넌트로도 확인하니, 태그가 다르더라도 " +
             "대부분은 자동으로 인식됩니다)")]
    public string playerTag = "Player";

    [Tooltip("게임오버가 될 때 재생할 효과음 이름. SEManager의 Sound Effects 배열에 등록된 " +
             "Name과 정확히 일치해야 합니다.")]
    public string crashSoundName = "GameOver";

    void OnTriggerEnter(Collider other) => TryCrash(other);
    void OnCollisionEnter(Collision collision) => TryCrash(collision.collider);

    void TryCrash(Collider other)
    {
        // 주의: "PlayerController"라는 이름이 두 군데서 쓰이고 있어서 헷갈리기 쉽습니다.
        // - PlayerController.cs 스크립트(클릭으로 차선을 바꾸는 실제 플레이어 로직)
        // - ControllerCollider가 붙어있고 태그가 "PlayerController"인, 클릭 대상용
        //   차선 표시 콜라이더(실제 플레이어 차량이 아니라 그냥 클릭 인식용 판)
        // 트래픽 차가 도로를 달리다 보면 이 차선 표시 콜라이더와 계속 겹치게 되는데,
        // 이건 실제 충돌이 아니므로 먼저 확실히 걸러냅니다.
        if (other.GetComponentInParent<ControllerCollider>() != null) return;

        // 플레이어의 물리 콜라이더가 PlayerController/PlayerMovement가 붙어있는 루트
        // 오브젝트가 아니라 자식 오브젝트에 있을 수도 있으므로, GetComponentInParent로
        // 자기 자신과 부모 쪽까지 함께 찾습니다. (LaneBumper에서 겪었던 것과 같은 이유)
        var playerController = other.GetComponentInParent<PlayerController>();
        var playerMovement = other.GetComponentInParent<PlayerMovement>();

        bool isPlayer = playerController != null || playerMovement != null || other.CompareTag(playerTag);
        if (!isPlayer) return;

        // 플레이어를 멈춥니다: 더 이상 앞으로 나가지 않고(PlayerMovement), 클릭으로
        // 차선을 바꿀 수도 없게(PlayerController) 스크립트 자체를 꺼버립니다.
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerController != null) playerController.enabled = false;

        if (SEManager.Instance != null)
            SEManager.Instance.PlaySound(crashSoundName);

        if (GameManager.Instance != null)
            GameManager.Instance.EndGame();
        else
            Debug.LogWarning("[CarBackTrigger] 씬에 GameManager가 없습니다.");
    }
}
