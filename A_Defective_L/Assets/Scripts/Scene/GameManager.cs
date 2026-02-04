using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsCutscene { get; private set; } = false;
    public Vector2 NextSpawnPoint { get; set; }

    public int storedHealth = 5;
    public int storedGauge = 0;
    public int storedTickets = 0;

    public string currentStageName = "Prologue";
    
    // ★ [추가] 일시정지 상태 확인용 변수
    public bool isPaused = false;

    // ★ [추가] 사망 연출 진행 중인지 확인 (중복 사망 방지)
    private bool isGameOverProcessing = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ★ [추가] ESC 키 입력을 감지하는 Update 함수
    private void Update()
    {
        // 타이틀이거나 컷씬 중이면 일시정지 금지
        if (currentStageName == "Title" || IsCutscene) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // 1. 만약 강화 패널이 켜져 있다면? -> 일시정지 하지 말고 리턴!
            // (UpgradeManager가 알아서 닫을 테니까)
            if (UIManager.Instance != null && UIManager.Instance.IsUpgradePanelActive())
            {
                return; 
            }

            // 2. 패널이 없을 때만 일시정지 토글
            TogglePause();
        }
    }

    // ★ [추가] 일시정지 토글 함수
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // 시간 정지
            if (UIManager.Instance != null) UIManager.Instance.TogglePauseUI(true);
        }
        else
        {
            Time.timeScale = 1f; // 시간 재개
            if (UIManager.Instance != null) UIManager.Instance.TogglePauseUI(false);
        }
    }

    // ... (기존 컷씬 및 씬 로드 함수들은 그대로 유지) ...

    public void StartCutscene()
    {
        IsCutscene = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void EndCutscene()
    {
        IsCutscene = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (NextSpawnPoint != Vector2.zero)
            {
                player.transform.position = NextSpawnPoint;
                NextSpawnPoint = Vector2.zero;
                Debug.Log("플레이어 위치 이동 완료!");
            }

            if (Camera.main != null)
            {
                 // 카메라 타겟 설정 코드 필요시 여기에...
            }
        }
    }

    public void ChangeStage(string nextSceneName)
    {
        StartCoroutine(ProcessSceneChange(nextSceneName));
    }

    IEnumerator ProcessSceneChange(string nextSceneName)
    {
        if (UIManager.Instance != null)
        {
            yield return StartCoroutine(UIManager.Instance.FadeOut());
        }

        if (!string.IsNullOrEmpty(currentStageName))
        {
            Scene currentScene = SceneManager.GetSceneByName(currentStageName);
            if (currentScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(currentStageName);
            }
        }

        yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

        currentStageName = nextSceneName;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

        yield return new WaitForSeconds(0.1f);

        if (UIManager.Instance != null)
        {
            // 컷씬이나 타이틀이 아니면 HUD를 보여라!
            bool showHUD = (nextSceneName != "Title" && nextSceneName != "Cutscene_Intro");
            UIManager.Instance.SetHUDActive(showHUD);

            yield return StartCoroutine(UIManager.Instance.FadeIn());
        }
    }

    public void SaveCurrentStatus(int hp, int gauge, int tickets)
    {
        storedHealth = hp;
        storedGauge = gauge;
        storedTickets = tickets;
        Debug.Log($"[GameManager] 상태 저장됨: HP {hp}, Gauge {gauge}");
    }

    public void OnPlayerDead()
    {
        if (isGameOverProcessing) return; // 이미 죽는 중이면 무시
        
        StartCoroutine(ProcessGameOverSequence());
    }


    IEnumerator ProcessGameOverSequence()
    {
        isGameOverProcessing = true;
        Debug.Log("💀 사망! 연출 시작");

        // 1. [연출] 슬로우 모션 & UI 끄기
        Time.timeScale = 0.2f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHUDActive(false);
            // 죽었을 땐 하얀색으로 페이드 아웃
            UIManager.Instance.SetFadeColor(Color.white);
            yield return StartCoroutine(UIManager.Instance.FadeOut());
        }

        // --- 화면이 하얘진 상태 ---
        
        // 2. [데이터] 디스크에서 저장된 정보 불러오기
        if (DataManager.Instance.LoadGame()) 
        {
            // 시간 정상화 (로딩해야 하니까)
            Time.timeScale = 1f;

            // ★ [핵심 1] 저장된 위치를 NextSpawnPoint에 입력
            float x = DataManager.Instance.currentData.playerX;
            float y = DataManager.Instance.currentData.playerY;
            NextSpawnPoint = new Vector2(x, y);

            // ★ [핵심 2] 체력 UI도 저장된 값(보통 풀피)으로 미리 복구
            if(UIManager.Instance != null) 
                UIManager.Instance.UpdateHealth(DataManager.Instance.currentData.currentHealth);

            // 3. [이동] 타이틀과 똑같이 ChangeStage 함수에게 모든 걸 맡김!
            // (ChangeStage가 알아서 씬 끄고, 켜고, NextSpawnPoint 위치로 이동시켜 줌)
            string savedSceneName = DataManager.Instance.currentData.sceneName;
            
            // ChangeStage를 부르면 거기서 페이드 아웃/인이 또 발생할 수 있지만,
            // 기능적으로 꼬이는 것보단 안전합니다.
            ChangeStage(savedSceneName);
        }
        else
        {
            // 세이브 파일 없으면 타이틀로
            Time.timeScale = 1f;
            ChangeStage("Title");
        }

        // 4. [마무리] 페이드 색상 복구
        // ChangeStage가 끝나고 화면이 밝아질 때(FadeIn) 검은색으로 돌아와야 함
        // ChangeStage 코루틴이 도는 동안 잠시 대기
        yield return new WaitForSeconds(1.0f); 

        if (UIManager.Instance != null)
        {
            // 혹시 하얀색으로 남아있을까봐 검은색으로 설정
            UIManager.Instance.SetFadeColor(Color.black); 
        }

        isGameOverProcessing = false;
    }
}