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
    // ★ [추가] 강화 데이터를 저장/로드할 때 어떤 무기인지 구별하기 위한 번호
    // 0: 기본검, 1: 총, 2: 창... 이런 식으로 순서대로 매겨주세요.
    public int weaponID;

    [Header("Weapon Stats")]
    public string weaponName;
    //public int damage;          // 공격력 (기존 baseDamage -> damage로 통일)
    public float attackRate;    // 공격 쿨타임
    public Sprite icon;         // UI 아이콘
    public WeaponType type;     // 무기 타입

    [Header("Settings")]
    public GameObject projectilePrefab; // (원거리용)

    [Header("Damage Settings")]
    // ★ [변경] 고정 damage 대신 '기본 데미지'와 '성장치'로 변경
    public int baseDamage = 10;      // 0강일 때 데미지
    public int damagePerLevel = 2;   // 1강당 올라가는 데미지

    // ★ [추가] 이 무기를 들었을 때 사용할 애니메이션 오버라이드
    [Header("Animation")]
    public AnimatorOverrideController overrideController;

    [Header("Effects & Sound")]
    public GameObject hitEffectPrefab; // 타격 이펙트
    public AudioClip hitSound;         // ★ [추가] 타격 사운드 변수

    // 데미지 계산 (PlayerStats의 배율 적용)
    /*
    protected int GetFinalDamage(PlayerStats stats)
    {
        // PlayerStats에 public float DamageMultiplier { get; private set; } = 1.0f; 가 있다고 가정
        return Mathf.RoundToInt(damage * stats.DamageMultiplier);
    }
    */

    // ★ [수정됨] 레벨과 플레이어 버프를 모두 적용한 최종 데미지 계산
    protected int GetFinalDamage(PlayerStats stats)
    {
        int level = 0;
        if (DataManager.Instance != null && weaponID >= 0 && weaponID < DataManager.Instance.currentData.weaponLevels.Length)
        {
            level = DataManager.Instance.currentData.weaponLevels[weaponID];
        }

        // 1. 깡뎀 (기본 + 레벨)
        int rawDamage = baseDamage + (level * damagePerLevel);

        // 2. ★ [수정됨] (기본 배율) * (교체 버프 배율) 모두 적용
        // stats.DamageMultiplier: 아이템 등에 의한 영구적 배율
        // stats.CurrentBuffMultiplier: 교체 버프 배율 (1.5 or 1.0)
        float totalMultiplier = stats.DamageMultiplier * stats.CurrentBuffMultiplier;

        return Mathf.RoundToInt(rawDamage * totalMultiplier);
    }

    // ★ 핵심: 모든 무기는 공격을 수행하고, 끝났을 때 onComplete를 실행해야 한다.
    public abstract void PerformAttack(Transform attackPoint, PlayerStats playerStats, Action onComplete);

    // ★ [추가] 스킬 사용 함수 (기본적으로는 아무것도 안 함)
    public virtual void UseSkill(Transform firePoint, PlayerStats playerStats, System.Action onComplete) 
    {
        // 자식들이 오버라이드해서 구현
        onComplete?.Invoke();
    }
}