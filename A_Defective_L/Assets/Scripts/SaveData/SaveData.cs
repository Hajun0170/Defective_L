using System;
using System.Collections.Generic; // ★ 리스트를 쓰려면 이게 꼭 필요합니다!

[Serializable] // ★ 이 줄이 없으면 저장이 안 됩니다!
public class SaveData
{
    // 1. 씬 및 위치 정보
    public string sceneName;      // 저장된 씬 이름
    public int shelterID;         // 마지막 들린 쉼터 ID (부활용)
    public float playerX;
    public float playerY;

    // 2. 플레이어 스탯
    public int currentHealth;
    public int maxHealth = 5;       // 최대 체력
    public int currentGauge;      // 무기 게이지
    
    // ★ [추가] 에러 해결을 위해 꼭 필요한 변수들!
    public int currentTickets; // 티켓 개수
    public bool isDead;        // 플레이어 사망 상태 (true면 죽음)
    
    // 3. 게임 진행도 (보스 클리어 여부 등)
    public bool isTutorialCleared;

    // ★ 스킬 해금 여부
    public bool hasWallCling; // 벽 매달리기 능력 보유 여부
    public bool hasSprint;    // 질주 능력 (2스테이지 보스 보상)
    

    // ★ 2. 처치한 보스들의 이름 목록 (ID 리스트)
    public List<string> defeatedBosses = new List<string>();

    // ▼ 아래 두 줄을 꼭 추가해주세요! ▼
    public int gold = 0;              // 재화
    public int[] weaponLevels = new int[6]; // 무기별 레벨 (0~5번)

    // ★ [추가] 캐릭터 성장 요소
    public int potionCapacity = 1; // 회복 키트 최대 소지량 (기본 1개)
    public int currentPotions = 1; // 현재 가지고 있는 키트 개수
    
    // ★ [추가] 먹은 아이템 ID 목록 (중복 획득 방지)
    public List<string> collectedItems = new List<string>();
    
}