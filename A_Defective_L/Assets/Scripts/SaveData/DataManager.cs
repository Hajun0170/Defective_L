using UnityEngine;
using System.IO; // 파일 관리를 위해 필수

public class DataManager : MonoBehaviour
{
    public static DataManager Instance; // 어디서든 접근 가능하게(싱글톤)

    public SaveData currentData; // 현재 게임 데이터
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
        Debug.Log("로드 성공!");
        return true;
    }
}