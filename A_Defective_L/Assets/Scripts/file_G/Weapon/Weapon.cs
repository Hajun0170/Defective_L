using UnityEngine;
using System; // Action 사용 필수

public enum WeaponType 
{ 
    Melee,  // 근거리
    Ranged  // 원거리
}

public abstract class Weapon : ScriptableObject
{
    [Header("Weapon Stats")]
    public string weaponName;
    public int damage;          // 공격력 (기존 baseDamage -> damage로 통일)
    public float attackRate;    // 공격 쿨타임
    public Sprite icon;         // UI 아이콘
    public WeaponType type;     // 무기 타입

    [Header("Settings")]
    public GameObject projectilePrefab; // (원거리용)

    // 데미지 계산 (PlayerStats의 배율 적용)
    protected int GetFinalDamage(PlayerStats stats)
    {
        // PlayerStats에 public float DamageMultiplier { get; private set; } = 1.0f; 가 있다고 가정
        return Mathf.RoundToInt(damage * stats.DamageMultiplier);
    }

    // ★ 핵심: 모든 무기는 공격을 수행하고, 끝났을 때 onComplete를 실행해야 한다.
    public abstract void PerformAttack(Transform attackPoint, PlayerStats playerStats, Action onComplete);
}