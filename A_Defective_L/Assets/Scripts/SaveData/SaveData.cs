using System;
using System.Collections.Generic; //리스트를 사용

[Serializable] //직렬화 가능하도록 설정
public class SaveData
{
    // 1. 씬 및 위치 정보
    public string sceneName;      // 저장된 씬 이름
    public int shelterID;         // 마지막 들린 쉼터 ID
    public float playerX;
    public float playerY;

    // 2. 플레이어 스탯
    public int currentHealth =5;
    public int maxHealth = 5;       // 최대 체력
    public int currentGauge;      // 무기 게이지
    
    public int currentTickets; // 티켓 개수
    public bool isDead;        // 플레이어 사망 상태. true면 죽음
    
    // 보스 클리어 여부
    public bool isTutorialCleared;

    // 스킬 해금 여부
    public bool hasWallCling; // 벽 매달리기 능력 
    public bool hasSprint;    // 질주 능력 

    // 처치한 보스들의 ID 
    public List<string> defeatedBosses = new List<string>();

    public int gold = 0;              // 재화
    public int[] weaponLevels = new int[6]; // 무기별 레벨 (0~5, 실제로는 1레벨로 표기)

    // 캐릭터 성장 요소
    public int potionCapacity = 1; // 회복 키트 최대 소지량 (기본 1개)
    public int currentPotions = 1; // 현재 가지고 있는 키트 개수
    
    // 먹은 아이템 목록: (중복 획득 방지)
    public List<string> collectedItems = new List<string>();
    public int equippedMeleeIndex = 0; //장착 중인 무기의 번호, 근접
    public int equippedRangedIndex = 0; //원거리

    // 무기 획득 여부를 저장하는 배열 (임시: 최대 20개 무기)
    public bool[] hasWeapons = new bool[20];

    //추가 조작키 표시 여부
    public bool showKeyHints = false;

    
}