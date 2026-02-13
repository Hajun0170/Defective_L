using UnityEngine;
using System.IO; // 파일 관리를 위해 필수
using System.Collections.Generic; // List 사용을 위해 추가

public class DataManager : MonoBehaviour
{   
    [Header("테스트용 모드")]
    public bool isDebugMode = false; // 체크하면 무조건 다 뚫림

    public static DataManager Instance; // 어디서든 접근 가능하게 함(싱글톤)
    
    public SaveData currentData = new SaveData();
    private string path;         // 저장 경로



    // 다음 씬에서 이동될 위치 번호 (0, 1, 2)
    public int nextSpawnPointID = 0;

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 파괴 X
            path = Path.Combine(Application.persistentDataPath, "savefile.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 새 게임 세팅
    public void NewGame()
    {
        currentData = new SaveData(); // 새 데이터 생성

        // 초기값 설정
        currentData.sceneName = "Cutscene_Intro"; // 컷씬부터 시작
        currentData.maxHealth = 5;
        currentData.currentHealth = 5;
        currentData.currentGauge = 0;
        currentData.currentPotions = 1;
        currentData.potionCapacity = 1; // 기본 용량
        currentData.gold = 0;
        currentData.shelterID = -1; // -1은 쉼터 방문 전

        currentData.hasWeapons = new bool[6]; 

        // 장착 슬롯을 -1 (없음)으로 설정
        // 0으로 두면 에러 나거나 없는 무기를 사용함
        currentData.equippedMeleeIndex = -1;
        currentData.equippedRangedIndex = -1;

        // 보스 목록 초기화
        currentData.defeatedBosses = new List<string>();
         currentData.collectedItems = new List<string>();
        
            //스킬 초기화

        currentData.hasSprint = false;

        currentData.hasWallCling = false;

        currentData.showKeyHints = false; // 초기 설정값

        // 초기화된 상태를 파일로 즉시 저장해서 덮어씌움

        SaveDataToDisk();

        Debug.Log("새 게임 데이터 생성 완료");
    }

    // 저장하기_쉼터에서 호출
    public void SaveGame(Transform player, string sceneName, int shelterID)
    {
        // 현재 상태를 데이터에 기록
        currentData.sceneName = sceneName;
        currentData.shelterID = shelterID;
        currentData.playerX = player.position.x;
        currentData.playerY = player.position.y;

        // 파일로 쓰기
        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(path, json);
        Debug.Log($"저장 완료: {path}");
    }

    // 불러오기 (타이틀에서 호출)
    public bool LoadGame()
    {
        if (!File.Exists(path)) 
        {
            Debug.Log("저장된 파일이 없습니다");
            return false;
        }

        string json = File.ReadAllText(path);
        currentData = JsonUtility.FromJson<SaveData>(json);

        // 이전 세이브파일이라 보스 리스트가 없으면 새로 만듦
        if (currentData.defeatedBosses == null)
            currentData.defeatedBosses = new List<string>();
        
        Debug.Log("로드 성공");
        
        // 데이터 로드 직후에 사운드 설정을 다시 믹서에 강제 주입
    // 믹서가 초기화될 때, 즉시 사용자의 설정값으로 덮어씌워집니다.
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.LoadVolumeSettings();
    }
    
        // 파일 로드 후 디버그 모드라면 강제로 해금
        if (isDebugMode)
        {
            currentData.hasSprint = true;
            currentData.hasWallCling = true;
            Debug.Log("모든 스킬 강제 해금!");
        }
        
        return true;
    }

    // 보스 죽었을 때 명단에 추가
    public void RegisterBossKill(string bossID)
    {
        // 명단에 없는 놈이면 추가
        if (!currentData.defeatedBosses.Contains(bossID))
        {
            currentData.defeatedBosses.Add(bossID);
            Debug.Log($"{bossID} 처치 기록됨!");
        }
    }

    // 보스가 이미 죽었는지 확인
    public bool IsBossDefeated(string bossID)
    {
        if (currentData.defeatedBosses == null) return false;
        return currentData.defeatedBosses.Contains(bossID);
    }

    // 현재 데이터 상태 그대로 파일에 덮어씀
    public void SaveDataToDisk()
    {
        string json = JsonUtility.ToJson(currentData, true);
        System.IO.File.WriteAllText(path, json);
        Debug.Log("데이터 파일 덮어쓰기 완료 (New Game 초기화 등)");
    }

    // 아이템 먹었을 때 기록하기
    public void RegisterItem(string itemID)
    {
        if (!currentData.collectedItems.Contains(itemID))
        {
            currentData.collectedItems.Add(itemID);
        }
    }

    // 이미 얻은 아이템인지 확인하기
    public bool CheckItemCollected(string itemID)
    {
        return currentData.collectedItems.Contains(itemID);
    }
}