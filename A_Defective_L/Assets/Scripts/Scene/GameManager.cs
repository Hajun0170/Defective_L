using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsCutscene { get; private set; } = false;

    // 다음 씬에서 플레이어가 스폰될 위치
    public Vector2 NextSpawnPoint { get; set; } 

    // 씬이 넘어가도 기억해야 할 데이터들
    public int storedHealth = 5; 
    public int storedGauge = 0;
    public int storedTickets = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬 로드 이벤트 연결
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 컷신 시작
    public void StartCutscene()
    {
        IsCutscene = true;
        
        // 씬에 있는 플레이어를 태그로 찾음
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Unity 6버전 (linearVelocity)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            // 애니메이션 멈춤 등이 필요하면 추가
            // player.GetComponent<Animator>().SetBool("IsRunning", false);
        }
    }

    public void EndCutscene()
    {
        IsCutscene = false;
    }

    // ★ [핵심 수정] 씬 로딩 완료 시 실행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. "Player" 태그를 가진 오브젝트를 찾습니다.
        // (PlayerStats.Instance 대신 이 방법을 써야 합니다!)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. 위치 이동 (저장된 위치가 있다면)
            if (NextSpawnPoint != Vector2.zero)
            {
                player.transform.position = NextSpawnPoint;
                
                // [중요] 이동 후에는 초기화해줘야 다음번엔 0,0에서 시작 안 함
                NextSpawnPoint = Vector2.zero; 
                
                Debug.Log("플레이어 위치 이동 완료!");
            }

            // 3. 카메라 연결 (카메라도 플레이어를 잃어버렸을 테니 다시 연결)
            // 만약 Cinemachine을 쓴다면 코드가 다르지만, 
            // 일반 스크립트(FollowCamera)를 쓴다면 아래처럼 태그로 찾은 player를 넣어줍니다.
            if (Camera.main != null)
            {
                // FollowCamera 스크립트가 있다면 타겟 설정
                // (FollowCamera 스크립트 이름을 사용하시는 이름으로 바꾸세요)
                // var camScript = Camera.main.GetComponent<FollowCamera>();
                // if (camScript != null) camScript.SetTarget(player.transform);
            }
        }
    }

    // 데이터 저장
    public void SaveCurrentStatus(int hp, int gauge, int tickets)
    {
        storedHealth = hp;
        storedGauge = gauge;
        storedTickets = tickets;
        Debug.Log($"[GameManager] 상태 저장됨: HP {hp}, Gauge {gauge}");
    }
}