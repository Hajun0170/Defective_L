using UnityEngine;
using System.IO; // 파일 관리를 위해 필수
using System.Collections.Generic; // ★ List 사용을 위해 추가

public class DataManager : MonoBehaviour
{   
    [Header("★ 개발자 디버그 모드")]
    public bool isDebugMode = false; // 이걸 체크하면 무조건 다 뚫림

    public static DataManager Instance; // 어디서든 접근 가능하게(싱글톤)
    
    public SaveData currentData = new SaveData();
    private string path;         // 저장 경로



    // ★ [추가] 다음 씬에서 태어날 위치 번호 (0: 기본, 1: 왼쪽, 2: 오른쪽...)
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

    // ★ 새 게임 세팅
    public void NewGame()
    {
        currentData = new SaveData(); // 새 데이터 생성

        // 초기값 설정
        currentData.sceneName = "Cutscene_Intro"; // 컷씬부터 시작
        currentData.maxHealth = 5;
        currentData.currentHealth = 5;
        currentData.currentGauge = 0;
        currentData.shelterID = -1; // -1은 쉼터 방문 전이라는 뜻

        // ★ [중요] 보스 목록도 깨끗하게 초기화 (Null 에러 방지)
        currentData.defeatedBosses = new List<string>();
        
        Debug.Log("새 게임 데이터 생성 완료!");
    }

    // ★ 저장하기 (쉼터에서 호출)
    public void SaveGame(Transform player, string sceneName, int shelterID)
    {
        // 현재 상태를 데이터에 기록
        currentData.sceneName = sceneName;
        currentData.shelterID = shelterID;
        currentData.playerX = player.position.x;
        currentData.playerY = player.position.y;
        
        // (PlayerStats가 있다면 여기서 스탯도 갱신해서 넣어야 함)
        // currentData.currentHealth = player.GetComponent<PlayerStats>().currentHealth;

        // 파일로 쓰기
        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(path, json);
        Debug.Log($"저장 완료: {path}");
    }

    // ★ 불러오기 (타이틀에서 호출)
    public bool LoadGame()
    {
        if (!File.Exists(path)) 
        {
            Debug.Log("저장된 파일이 없습니다!");
            return false;
        }

        string json = File.ReadAllText(path);
        currentData = JsonUtility.FromJson<SaveData>(json);

        // ★ [안전장치] 만약 옛날 세이브파일이라 보스 리스트가 없으면 새로 만듦
        if (currentData.defeatedBosses == null)
            currentData.defeatedBosses = new List<string>();
        
        Debug.Log("로드 성공!");
        
        // ★ 파일 로드 후 디버그 모드라면 강제로 해금
        if (isDebugMode)
        {
            currentData.hasSprint = true;
            currentData.hasWallCling = true;
            Debug.Log("🚀 개발자 모드: 모든 스킬 강제 해금!");
        }
        
        return true;
    }

    // =========================================================
    // ▼▼▼ [새로 추가된 기능] 보스 관련 함수들 ▼▼▼
    // =========================================================

    // 1. 보스 죽었을 때 명단에 추가하는 함수
    public void RegisterBossKill(string bossID)
    {
        // 명단에 없는 놈이면 추가
        if (!currentData.defeatedBosses.Contains(bossID))
        {
            currentData.defeatedBosses.Add(bossID);
            Debug.Log($"{bossID} 처치 기록됨!");
        }
    }

    // 2. 보스가 이미 죽었는지 확인하는 함수
    public bool IsBossDefeated(string bossID)
    {
        if (currentData.defeatedBosses == null) return false;
        return currentData.defeatedBosses.Contains(bossID);
    }

    // ★ [추가] 현재 데이터 상태 그대로 파일에 덮어쓰는 함수
    public void SaveDataToDisk()
    {
        string json = JsonUtility.ToJson(currentData, true);
        System.IO.File.WriteAllText(path, json);
        Debug.Log("데이터 파일 덮어쓰기 완료 (New Game 초기화 등)");
    }
}