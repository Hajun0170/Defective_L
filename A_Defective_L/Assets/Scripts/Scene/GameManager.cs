using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour //게임 진행 사항 저장하는 핵심 코드
{
    public static GameManager Instance;
    public bool IsCutscene { get; set; }
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

    //  ESC 입력 감지 함수
    private void Update()
    {
        // 타이틀이거나 컷씬 중이면 일시정지 금지
        if (currentStageName == "Title" || IsCutscene) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // 강화 패널이 켜져 있다면 일시정지 하지 말고 리턴: UpgradeManager가 알아서 닫는 기능이 있음
            if (UIManager.Instance != null && UIManager.Instance.IsUpgradePanelActive())
            {
                return; 
            }

            // 패널이 없을 때만 일시정지 토글 띄움
            TogglePause();
        }
    }

    // 일시정지 토글 함수
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; //  정지
            if (UIManager.Instance != null) UIManager.Instance.TogglePauseUI(true);
        }
        else
        {
            Time.timeScale = 1f; // 재개
            if (UIManager.Instance != null) UIManager.Instance.TogglePauseUI(false);
        }
    }

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
                //카메라 코드는 따로 설정되어서 추가는 안 함
            }
        }
    }

   public void ChangeStage(string nextSceneName)
    {
      
        StartCoroutine(ProcessSceneChange(nextSceneName));
    }

    IEnumerator ProcessSceneChange(string nextSceneName)
    {
        //씬을 떠나기 전, 현재 플레이어의 스탯(HP, 돈 등)을 데이터 매니저에 백업
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null) stats.SaveStatsToManager();
    }
        
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
            // 컷씬이나 타이틀이 아니면 HUD를 보여줌
            bool showHUD = (nextSceneName != "Title" && nextSceneName != "Cutscene_Intro");
            UIManager.Instance.SetHUDActive(showHUD);

            yield return StartCoroutine(UIManager.Instance.FadeIn());
        }
        //씬 로드 완료 후 사운드 설정을 다시 한번 로드
        if (AudioManager.Instance != null) AudioManager.Instance.LoadVolumeSettings();
    }

    public void SaveCurrentStatus(int hp, int gauge, int tickets)
    {
        storedHealth = hp;
        storedGauge = gauge;
        storedTickets = tickets;

        // DataManager에게 최신 정보를 넘겨줌 PlayerStats도 해당 내용을 읽음
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.currentHealth = hp;
            DataManager.Instance.currentData.currentGauge = gauge;
            DataManager.Instance.currentData.currentTickets = tickets;
        }

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

        // 슬로우 모션, UI 끄기
        Time.timeScale = 0.2f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHUDActive(false);
            // 죽었을 땐 하얀색으로 페이드 아웃
            UIManager.Instance.SetFadeColor(Color.white);
            yield return StartCoroutine(UIManager.Instance.FadeOut());
        }

        // 2. 분기 처리: 프롤로그 구간인가?
    bool isPrologue = (currentStageName == "Prologue");

    if (isPrologue)
    {
        // [프롤로그 사망] 데이터 리셋 후 프롤로그 1부터 시작
        Debug.Log("🌿 프롤로그 사망: 데이터를 초기화하고 재시작합니다.");
        DataManager.Instance.NewGame();
        
        // 데이터는 밀었지만 설정(사운드)은 다시 불러옴
        if (AudioManager.Instance != null) AudioManager.Instance.LoadVolumeSettings();

        Time.timeScale = 1f;
        ChangeStage("Prologue"); 
    }

        // 화면이 하얘진 연출 먹인 상태에서 자연스럽게 재시작
        else
    {
        // 저장된 정보 불러오기
        if (DataManager.Instance.LoadGame()) 
        {
            // 시간 정상화 (슬로우 모션 해제)
            Time.timeScale = 1f;

            //세이브 파일을 로드한 직후, 사운드 설정을 시스템 설정(PlayerPrefs)에서 다시 덮어씌움
        // 세이브 파일에 사운드 값이 없거나 잘못되어도 현재 설정이 유지하기 위함
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.LoadVolumeSettings();
        }
        
            // 저장된 위치를 NextSpawnPoint에 입력
            float x = DataManager.Instance.currentData.playerX;
            float y = DataManager.Instance.currentData.playerY;
            NextSpawnPoint = new Vector2(x, y);

            // 체력 UI 풀로 미리 복구
            if(UIManager.Instance != null) 
                UIManager.Instance.UpdateHealth(DataManager.Instance.currentData.currentHealth);

            // 타이틀처럼 ChangeStage 함수에게 넘김
            //ChangeStage가 알아서 씬 관련 내용(키고 끔, 스폰 위치 이동 등)을 처리
            string savedSceneName = DataManager.Instance.currentData.sceneName;
            
            // ChangeStage를 불러서 추가적인 페이드 아웃/인이 또 발생하긴 하는데, 기능적으로 꼬이는 건 일단 없음...
            ChangeStage(savedSceneName);
        }
        else
        {
            // 세이브 파일 없으면 타이틀로
            Time.timeScale = 1f;
            ChangeStage("Title");
        }
    }

        
        // 페이드 색상 복구
        // ChangeStage가 끝나고 화면이 밝아질 때 검은색으로 돌아와야 함 ChangeStage의 코루틴이 도는 동안 잠시 대기
        yield return new WaitForSeconds(1.0f); 

        if (UIManager.Instance != null)
        {
            //하얀색으로 남아있는걸 방지하기 위해 검은색으로 설정
            UIManager.Instance.SetFadeColor(Color.black); 
        }

        isGameOverProcessing = false;
    }

    // 보상 획득 통합 함수
    public void GetWeaponReward(Weapon weaponData)
    {
        if (weaponData == null) return;

        // 플레이어에게 무기 지급 (WeaponManager)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WeaponManager weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.AddWeapon(weaponData); // ★ 무기 추가!
                
                // 무기 먹은 후 데이터 저장 (다음 스테이지에서 유지됨)
            }
        }

        // UI 띄우기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRewardPanel(weaponData);
        }
        
    }
}