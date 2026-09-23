using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬에 붙이는 스크립트입니다.
/// - "게임 시작" 버튼의 OnClick에 StartGame()을 연결하면 인게임(Main) 씬으로 넘어갑니다.
/// - "게임 방법" 버튼의 OnClick에 ShowHowToPlay()를 연결하면 게임방법 패널이 켜집니다.
/// - 게임방법 패널의 X 버튼 OnClick에 HideHowToPlay()를 연결하면 다시 꺼집니다.
///
/// 사용법: 타이틀 씬의 빈 GameObject(또는 버튼 자신)에 이 스크립트를 붙이고,
/// How To Play Panel 필드에 게임방법 패널(평소엔 비활성화 상태로 씬에 미리 배치)을
/// 연결하세요. Main 씬이 File > Build Profiles(또는 Build Settings)의 Scene List에
/// 등록되어 있어야 이름으로 씬을 불러올 수 있습니다.
/// </summary>
public class TitleController : MonoBehaviour
{
    [Tooltip("게임 시작 버튼을 눌렀을 때 불러올 인게임 씬 이름")]
    public string gameSceneName = "Main";

    [Tooltip("게임 방법 버튼을 누르면 켜지는 패널. 평소엔 비활성화 상태로 씬에 미리 배치해두세요.")]
    public GameObject howToPlayPanel;

    void Awake()
    {
        // 시작할 때 혹시 켜져있더라도 확실히 꺼둡니다.
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }
}
