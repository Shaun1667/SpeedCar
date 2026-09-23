using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태(게임 오버, 점수 등)를 관리하는 간단한 매니저입니다.
///
/// 게임 오버가 되어도 Time.timeScale은 그대로 두기 때문에(=1), 트래픽 차량이나 다른
/// 연출은 계속 움직입니다. 실제로 멈추는 건 플레이어뿐입니다(CarBackTrigger가
/// PlayerMovement/PlayerController를 꺼서 멈춥니다). 대신 게임오버가 되면 이 스크립트가
/// 미리 연결해둔 Game Over Canvas를 SetActive(true)로 화면에 띄웁니다.
///
/// 점수는 LaneBumper가 트래픽 차를 레일 밖으로 날려버릴 때마다(FlyOffMap) 1점씩
/// 올라가고, InGame 캔버스의 Score Text에 바로 반영됩니다.
///
/// 사용법: 빈 GameObject를 하나 만들어 이 스크립트를 붙이고, Game Over Canvas 필드에
/// 게임오버 UI 캔버스(평소엔 비활성화 상태로 씬에 미리 배치)를, Score Text 필드에
/// InGame 캔버스의 점수 표시용 Text를 연결하세요.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("게임 오버가 됐을 때 활성화할 UI 캔버스(또는 패널) 오브젝트. " +
             "평소에는 비활성화된 상태로 씬에 미리 배치해두세요.")]
    public GameObject gameOverCanvas;

    [Tooltip("게임 플레이 중 보여주는 InGame 패널(점수 등). 게임 오버가 되면 자동으로 " +
             "비활성화됩니다.")]
    public GameObject inGamePanel;

    [Tooltip("현재 점수를 표시할 InGame 캔버스의 TMP_Text(TextMeshPro). 자동차를 레일 밖으로 " +
             "날릴 때마다 1점씩 올라갑니다.")]
    public TMP_Text scoreText;

    [Tooltip("게임오버(결과창) 캔버스에서 최종 점수를 표시할 TMP_Text. InGame의 Score Text와 " +
             "똑같은 형식(\"물리친 자동차 : 0\")으로 같이 갱신됩니다.")]
    public TMP_Text gameOverScoreText;

    [Tooltip("점수 앞에 붙일 표시 문구. 예: \"물리친 자동차\" + \" : \" + 점수")]
    public string scoreLabel = "물리친 자동차";

    [Tooltip("게임오버 화면의 \"타이틀로\" 버튼을 눌렀을 때 이동할 타이틀 씬 이름. " +
             "Build Settings(File > Build Profiles > Scene List)에 등록되어 있어야 합니다.")]
    public string titleSceneName = "Title";

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }

    void Awake()
    {
        Instance = this;

        // 시작할 때 혹시 켜져있더라도 확실히 꺼둡니다.
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        UpdateScoreText();
    }

    /// <summary>점수를 더합니다. LaneBumper가 트래픽 차를 레일 밖으로 날려버릴 때 호출합니다.</summary>
    public void AddScore(int amount)
    {
        // 게임오버 이후에는 점수가 더 오르지 않도록 합니다. (트래픽은 게임오버 후에도
        // 계속 움직이기 때문에, 막지 않으면 플레이어와 무관하게 점수가 계속 올라갈 수 있음)
        if (IsGameOver) return;

        Score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        string text = $"{scoreLabel} : {Score}";

        if (scoreText != null)
            scoreText.text = text;

        // 게임오버 캔버스가 꺼져있는 동안에도(Score Text가 비활성 상태여도) 텍스트 값은
        // 미리 갱신해둘 수 있으므로, 게임오버가 뜨는 순간 바로 최종 점수가 보입니다.
        if (gameOverScoreText != null)
            gameOverScoreText.text = text;
    }

    /// <summary>게임을 종료 상태로 만듭니다. 다른 오브젝트(트래픽 등)는 계속 움직이고,
    /// 게임오버 캔버스만 화면에 띄웁니다. 실제로 플레이어를 멈추는 건 호출한 쪽
    /// (CarBackTrigger)에서 PlayerMovement/PlayerController를 꺼서 처리합니다.</summary>
    public void EndGame()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
        else
            Debug.LogWarning("[GameManager] Game Over Canvas가 연결되어 있지 않습니다.");

        if (inGamePanel != null)
            inGamePanel.SetActive(false);

        Debug.Log("[GameManager] Game Over");
    }

    /// <summary>다시 시작할 때(재시작 버튼 등을 만들면 그때) 호출하세요.</summary>
    public void ResetGameState()
    {
        IsGameOver = false;
        Score = 0;
        UpdateScoreText();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        if (inGamePanel != null)
            inGamePanel.SetActive(true);
    }

    /// <summary>게임오버 캔버스의 "타이틀로" 버튼 OnClick에 연결하세요. 타이틀 씬으로
    /// 돌아갑니다. (Time.timeScale을 건드리지 않았으므로 별도로 되돌릴 필요는 없지만,
    /// 혹시 다른 곳에서 0으로 바꿔둔 적이 있다면 여기서 1로 복구합니다)</summary>
    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }
}
