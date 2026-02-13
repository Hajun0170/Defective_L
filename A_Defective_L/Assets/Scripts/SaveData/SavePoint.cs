using UnityEngine;
using UnityEngine.SceneManagement; 

public class SavePoint : MonoBehaviour
{
    [Header("쉼터 설정")]
    public int shelterID = 0; //이 쉼터의 고유 번호 
    public GameObject saveEffectPrefab; // 저장 시 터트릴 이펙트

    private bool isPlayerInRange = false;
    private Transform playerTransform; // 플레이어 위치 정보를 저장할 변수

    private PlayerStats playerStats;    // 체력 회복을 위해 필요
    
    [Header("UI 연결")]
    public GameObject interactionUI; // 머리 위에 띄울 화살표 아이콘
    public GameObject interactionUI2; 
    private bool hasSaved = false; // 1번 방문했을 때에 저장을 했는지 체크하는 변수

void Start()
    {
        // 시작할 때 화살표는 꺼둠
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    // 플레이어가 쉼터 범위에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = collision.transform; // 플레이어 정보 기억해둠

            // 플레이어의 스탯 스크립트를 미리 가져옴
            playerStats = collision.GetComponent<PlayerStats>();

            // 들어오면 화살표 띄우기
            if (interactionUI != null) interactionUI.SetActive(true);
            if (interactionUI2 != null) interactionUI2.SetActive(true); //휴식처 UI가 2개라서 그럼

            // 상태 초기화 (다시 들어오면 저장부터)
            hasSaved = false;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null; // 나갔으니 비움

            playerStats = null;

            // 쉼터 범위를 벗어나면 강화창이 열려있더라도 강제로 닫습니다
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetUpgradePanelActive(false);
            }

             if (interactionUI != null) interactionUI.SetActive(false);
              if (interactionUI2 != null) interactionUI2.SetActive(false);
              
        }
    }

    
    private void Update()
    {
        if (!isPlayerInRange) return;

        // 위쪽 화살표 키 하나로 모든 동작 제어
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 강화창이 켜져 있다면 화살표 키로도 닫을 수 있게 함
            // ESC로 닫는 건 UIManager에서 처리되므로 여기서 막지 않음
            if (UIManager.Instance != null && UIManager.Instance.IsUpgradePanelActive())
            {
                UIManager.Instance.SetUpgradePanelActive(false);
                return;
            }

            // 로직 분기: 저장 안 했으면 저장, 했으면 강화창
            if (!hasSaved)
            {
                SaveAndHeal();
            }
            else
            {
                OpenUpgradePanel();
            }
        }
    }

    void SaveAndHeal()
    {
        if (playerStats != null)
        {
            // 체력과 포션을 가득 채움 + UI 갱신
            playerStats.HealToFull(); 

            // 동기화
            // 모은 돈 최대 체력을 DataManager에 최신 상태로 밀어 넣음
            playerStats.RestAtShelter(); 
            
            // 파일 저장
            // DataManager에 있는 최신 정보를 파일에 기록
            if (DataManager.Instance != null)
            {
                DataManager.Instance.SaveGame(playerTransform, SceneManager.GetActiveScene().name, shelterID);
            }

            // 4. 이펙트 및 피드백
            if (saveEffectPrefab != null)
            {
                Instantiate(saveEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Debug.Log($"🌿 쉼터({shelterID}) 저장 완료! (Gold, MaxHP 포함)");
            
            // 상태 변경 (한 번 누르면 저장 완료)
            hasSaved = true; 
        }
    }

    void OpenUpgradePanel()
    {
        if (UIManager.Instance != null)
        {
            Debug.Log("🛠️ 강화 패널 오픈");
            UIManager.Instance.SetUpgradePanelActive(true);
        }
    }

    
}