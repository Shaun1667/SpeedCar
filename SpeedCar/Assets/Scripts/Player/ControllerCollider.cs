using UnityEngine;

/// <summary>
/// 차선(콜라이더)의 목표 x좌표를 오브젝트의 실제 Transform 위치가 아니라
/// controllerIndex로부터 "계산"해서 정확한 정수 값(-2, -1, 0, 1, 2 등)을 보장합니다.
/// Scene 뷰에서 손으로 드래그하면 -1.742 같은 어중간한 값이 되기 쉬운데,
/// 이 방식은 오브젝트를 어디에 두든 목표값은 항상 정확합니다.
/// </summary>
public class ControllerCollider : MonoBehaviour
{
    [Tooltip("이 차선의 순번 (0부터 시작). Controller0, Controller1, Controller2... 순서대로 매겨주세요.")]
    [SerializeField]
    private int controllerIndex;

    [Tooltip("차선 사이의 간격(m). 1이면 옆 차선과 x값이 1씩 차이납니다.")]
    public float laneSpacing = 1f;

    [Tooltip("전체 차선 중 x=0이 되는 기준 인덱스. 차선이 0~4번(5개)이고 " +
             "가운데(2번)가 x=0이어야 한다면 2로 둡니다.")]
    public float centerIndex = 2f;

    // 예: controllerIndex=0, centerIndex=2, laneSpacing=1 => x = (0-2)*1 = -2
    //     controllerIndex=4, centerIndex=2, laneSpacing=1 => x = (4-2)*1 = 2
    public Vector3 movePosition => new Vector3(
        (controllerIndex - centerIndex) * laneSpacing,
        transform.position.y,
        transform.position.z);
}
