using UnityEngine;

/// <summary>
/// 도로 모듈(조각) 프리팹에 붙이는 선택적 컴포넌트입니다.
/// 이 모듈이 끝나는 지점(다음 모듈이 이어 붙어야 할 위치)을 표시합니다.
///
/// 사용법: 모듈 프리팹의 "끝쪽" 지점에 빈 오브젝트(GameObject > Create Empty)를 만들어
/// 정확히 도로가 끝나는 자리로 옮긴 뒤, 이 컴포넌트의 End Point 필드에 연결하세요.
/// 연결하지 않으면 RoadModuleManager의 Fallback Module Length 값을 대신 사용합니다.
/// </summary>
public class RoadModule : MonoBehaviour
{
    [Tooltip("이 모듈이 끝나는 지점 (다음 모듈을 이어 붙일 위치). " +
             "모듈의 끝쪽에 빈 오브젝트를 만들어 연결하세요.")]
    public Transform endPoint;
}
