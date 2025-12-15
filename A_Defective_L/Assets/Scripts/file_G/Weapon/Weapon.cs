using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName;
    public int baseDamage;   // 기본 공격력
    public float attackRate; // 공격 딜레이
    
    protected float nextAttackTime = 0f;

    // 데미지 계산 (버프 적용)
    protected int GetFinalDamage(PlayerStats stats)
    {
        return Mathf.RoundToInt(baseDamage * stats.DamageMultiplier);
    }

    // 자식 클래스에서 반드시 구현해야 할 함수
    public abstract void PerformAttack(Transform firePoint, PlayerStats playerStats);
}