using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리 필수

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool IsCutscene { get; private set; } = false;

    // [추가] 다음 씬에서 플레이어가 스폰될 위치
    public Vector2 NextSpawnPoint { get; set; } 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬이 로드될 때마다 호출될 함수 연결 (이벤트 구독)
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 컷신 시작 (입력 차단)
    public void StartCutscene()
    {
        IsCutscene = true;
        // 필요하다면 여기서 플레이어의 이동 속도를 0으로 초기화
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            // player.GetComponent<Animator>().SetBool("IsRunning", false); // 멈춤 애니메이션
        }
    }

    // 컷신 종료 (입력 허용)
    public void EndCutscene()
    {
        IsCutscene = false;
    }

    // 씬 로딩이 완료되면 자동으로 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 플레이어 찾기 (싱글톤으로 살아있는 플레이어)
        if (PlayerStats.Instance != null)
        {
            // 2. 위치 이동 (Portal에서 설정한 위치로)
            // (주의: NextSpawnPoint가 (0,0)이면 초기화 위치일 수 있으므로 조건 체크 가능)
            if (NextSpawnPoint != Vector2.zero)
            {
                PlayerStats.Instance.transform.position = NextSpawnPoint;
            }
        }

        // 3. 카메라 연결 (카메라는 씬마다 새로 생기므로 다시 플레이어를 찾아야 함)
        // 메인 카메라가 플레이어를 따라다니는 스크립트(FollowCamera)가 있다고 가정
        Camera.main.GetComponent<FollowCamera>()?.SetTarget(PlayerStats.Instance.transform);
        
        // 4. 페이드 인 효과 (선택 사항)
        // UIManager.Instance.FadeIn();
    }
    
    // ... (StartCutscene 등 기존 코드 유지) ...
}