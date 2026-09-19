using UnityEngine;

/// <summary>
/// 각 도로 모듈 프리팹 안에 있는, 태그가 "ModuleStart"인 트리거 콜라이더에 붙입니다.
/// 플레이어가 이 트리거에 닿으면 RoadModuleManager에게 앞쪽에 새 모듈을 만들라고 요청합니다.
///
/// 주의: 이 오브젝트의 Collider는 반드시 "Is Trigger"가 켜져있어야 합니다.
/// (꺼져있으면 플레이어가 물리적으로 부딪혀서 못 지나갑니다)
///
/// 플레이어(자동차) 오브젝트에는 Project Settings > Tags and Layers에서
/// "Player" 태그를 만들어 붙여주세요. (다른 태그를 쓰고 있다면 아래 Player Tag 필드를 바꾸면 됩니다)
/// </summary>
[RequireComponent(typeof(Collider))]
public class ModuleStartTrigger : MonoBehaviour
{
    [Tooltip("이 태그를 가진 오브젝트가 닿았을 때만 반응합니다.")]
    public string playerTag = "Player";

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // 트리거 하나당 새 모듈 생성 요청은 한 번만
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        if (RoadModuleManager.Instance != null)
            RoadModuleManager.Instance.SpawnNextModule();
        else
            Debug.LogWarning("[ModuleStartTrigger] 씬에 RoadModuleManager가 없습니다.");
    }
}
