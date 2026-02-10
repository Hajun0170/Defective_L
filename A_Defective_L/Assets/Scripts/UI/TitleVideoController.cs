using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TitleVideoController : MonoBehaviour // 타이틀 화면에서 영상 재생 및 게임 시작 처리, 조금 꼬여서 당장 사용 안함
{
    [Header("Settings")]
    public VideoPlayer videoPlayer; // Image에 붙은 VideoPlayer 연결
    public string gameSceneName = "GameScene"; // 넘어갈 게임 씬 이름
    public GameObject startButton;  // 타이틀 시작 버튼

    private bool isVideoPlaying = false;

    private void Start()
    {
        // 영상이 끝나면 실행될 함수 연결
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Stop(); // 일단 정지
        startButton.SetActive(true); // 버튼 보이기
    }

    private void Update()
    {
        // 영상 재생 중 ESC를 누르면 스킵
        if (isVideoPlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            StartGame();
        }
    }

    // 버튼에 연결 함수
    public void OnStartButtonClick()
    {
        startButton.SetActive(false); // 버튼 숨김
        isVideoPlaying = true;
        videoPlayer.Play(); // 영상 재생
    }

    // 영상이 끝까지 재생되었을 때 자동 호출
    private void OnVideoEnd(VideoPlayer vp)
    {
        StartGame();
    }

    private void StartGame()
    {
        // 게임 씬으로 이동
        SceneManager.LoadScene(gameSceneName);
    }
}