using UnityEngine;
using System; // Action 사용 필수

public enum WeaponType 
{ 
    Melee,  // 근거리
    Ranged  // 원거리
}

public abstract class Weapon : ScriptableObject
{
    [Header("Identity")]
    //  강화 데이터를 저장/로드할 때 어떤 무기인지 구별하기 위한 번호
    public int weaponID;

    [Header("Weapon Stats")]
    public string weaponName;
    public float attackRate;    // 공격 쿨타임
    public Sprite icon;         // UI 아이콘
    public WeaponType type;     // 무기 타입

    [Header("Settings")]
    public GameObject projectilePrefab; // (원거리용)

    [Header("Damage Settings")]
    //기본 데미지와 성장치로 변경
    public int baseDamage = 10;      // 0강일 때 데미지
    public int damagePerLevel = 2;   // 1강당 올라가는 데미지

    // 이 무기를 들었을 때 사용할 애니메이션 오버라이드
    [Header("Animation")]
    public AnimatorOverrideController overrideController;

    [Header("Effects & Sound")]
    public GameObject hitEffectPrefab; // 타격 이펙트
    public AudioClip hitSound;         // 타격 사운드 변수


    // 레벨과 플레이어 버프를 모두 적용한 최종 데미지 계산
    protected int GetFinalDamage(PlayerStats stats)
    {
        int level = 0;
        if (DataManager.Instance != null && weaponID >= 0 && weaponID < DataManager.Instance.currentData.weaponLevels.Length)
        {
            level = DataManager.Instance.currentData.weaponLevels[weaponID];
        }

        // 1. 깡뎀 (기본 + 레벨)
        int rawDamage = baseDamage + (level * damagePerLevel);

        // 교체 버프: 교체 시 일시적으로 데미지 증가
        // stats.CurrentBuffMultiplier: 교체 버프 배율 (1.5 or 1.0)
        float totalMultiplier = stats.DamageMultiplier * stats.CurrentBuffMultiplier;

        return Mathf.RoundToInt(rawDamage * totalMultiplier);
    }

    // 모든 무기는 공격을 수행하고, 끝났을 때 onComplete를 실행
    public abstract void PerformAttack(Transform attackPoint, PlayerStats playerStats, Action onComplete);

    // 스킬 사용 함수 (기본적으로는 아무것도 안 함)
    public virtual void UseSkill(Transform firePoint, PlayerStats playerStats, System.Action onComplete) 
    {
        // 자식들이 오버라이드해서 구현
        onComplete?.Invoke();
    }
}