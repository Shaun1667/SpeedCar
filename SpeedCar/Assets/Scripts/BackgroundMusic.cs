using UnityEngine;

/// <summary>
/// 배경음악 오브젝트에 붙이는 스크립트입니다. 씬이 바뀌어도(타이틀 → 인게임,
/// 인게임 → 타이틀) 파괴되지 않고 계속 재생됩니다. 오브젝트가 항상 하나만
/// 존재하도록, 씬이 다시 로드되면서 새로 생긴 배경음악 오브젝트는 즉시 없앱니다.
///
/// 사용법: 타이틀 씬에 빈 GameObject(또는 배경음악용 오브젝트)를 만들고 AudioSource를
/// 붙여서 재생할 음악 클립을 등록하세요(Play On Awake, Loop 체크). 그 오브젝트에
/// 이 스크립트도 같이 붙이면 됩니다. 인게임 씬에는 아무것도 추가할 필요 없습니다 —
/// 타이틀에서 만들어진 이 오브젝트 하나가 씬을 넘나들며 계속 재생됩니다.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 재생 중인 배경음악 오브젝트가 있다는 뜻입니다(타이틀 씬을 다시
            // 불러오면서 이 스크립트가 붙은 오브젝트가 새로 또 생겼을 때). 중복
            // 재생을 막기 위해 새로 생긴 쪽을 바로 파괴합니다.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
