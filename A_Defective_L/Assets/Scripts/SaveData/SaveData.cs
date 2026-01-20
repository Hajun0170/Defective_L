using System;

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
    public int maxHealth;
    public int currentGauge;      // 무기 게이지
    
    // 3. 게임 진행도 (보스 클리어 여부 등)
    public bool isTutorialCleared;

    // ★ 스킬 해금 여부
    public bool hasWallCling; // 벽 매달리기 능력 보유 여부
    
}