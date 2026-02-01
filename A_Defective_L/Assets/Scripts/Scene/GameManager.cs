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

/*
    IEnumerator ProcessGameOverSequence()
    {
        isGameOverProcessing = true;
        Debug.Log("💀 플레이어 사망! 연출 시작");

        // 1. 슬로우 모션 발동! (시간이 5배 느려짐)
        Time.timeScale = 0.2f;

        // 2. 하얀색 빛무리 효과 (페이드 색을 흰색으로 변경)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetFadeColor(Color.white); // 흰색 설정
            
            // 페이드 아웃 (화면이 점점 하얗게 변함)
            // 시간 스케일 무시하고 UI는 제 속도로 움직이게 하려면 별도 처리가 필요하지만,
            // 여기선 분위기를 위해 페이드도 천천히 되도록 둡니다.
            yield return StartCoroutine(UIManager.Instance.FadeOut());
            UIManager.Instance.SetBossHUDActive(false);
        }
        else
        {
            // UI 없으면 그냥 시간만 끔 (비상용)
            yield return new WaitForSecondsRealtime(1f);
        }

        // --- 화면이 완전히 하얘진 상태 (플레이어는 안 보임) ---

        // 3. 시간 정상화 (로딩이나 이동은 제 속도로 해야 하니까)
        Time.timeScale = 1f;

        // 4. 데이터 로드 (마지막 세이브 지점 정보 가져오기)
        if (DataManager.Instance.LoadGame())
        {
            // 저장된 씬 이름 가져오기
            string savedScene = DataManager.Instance.currentData.sceneName;
            
            // 좌표 예약
            float x = DataManager.Instance.currentData.playerX;
            float y = DataManager.Instance.currentData.playerY;
            NextSpawnPoint = new Vector2(x, y);

            // 5. 저장된 체력으로 복구 (또는 풀피로 부활)
            // 여기서는 저장된 체력 대신 꽉 채워주는 게 일반적입니다.
            // (필요하다면 DataManager.Instance.currentData.currentHealth = 5; 등으로 수정)
        }
        else
        {
            // 세이브 파일이 없으면? 태초의 마을(Title)이나 처음으로
            Debug.Log("세이브 데이터 없음. 타이틀로...");
            ChangeStage("Title"); 
            isGameOverProcessing = false;
            yield break;
        }

        // 6. 같은 씬이면 위치만 이동, 다른 씬이면 씬 로드
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == DataManager.Instance.currentData.sceneName)
        {
            // 같은 맵에서 죽었으면 씬 로드 없이 위치만 텔레포트 (최적화)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = NextSpawnPoint;
                NextSpawnPoint = Vector2.zero;
                
                // 플레이어 체력/애니메이션 초기화 (PlayerStats 스크립트가 있다면 함수 호출)
                // player.GetComponent<PlayerStats>().Revive(); 
            }
        }
        else
        {
            // 다른 맵이면 씬 이동 (이미 만든 ChangeStage 함수 활용)
            // ChangeStage 안에 페이드 아웃/인이 또 있으므로, 여기선 페이드 인을 생략하거나 로직 조절 필요
            // 하지만 간단하게 씬만 다시 로드하는 게 속편합니다.
            SceneManager.LoadScene(DataManager.Instance.currentData.sceneName);
        }

        // 잠시 대기 (안정화)
        yield return new WaitForSeconds(0.5f);

        // 7. 다시 화면 밝아짐 (페이드 인)
        if (UIManager.Instance != null)
        {
            yield return StartCoroutine(UIManager.Instance.FadeIn());
            
            // ★ 중요: 페이드가 끝났으니 다시 검은색으로 돌려놔야 맵 이동 때 자연스러움
            UIManager.Instance.SetFadeColor(Color.black);
        }

        // 8. 체력바 UI 갱신 (풀피로 보이기)
        if(UIManager.Instance != null) UIManager.Instance.UpdateHealth(5); // 임시 5

        isGameOverProcessing = false;
        Debug.Log("✨ 부활 완료!");
    }
    */

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