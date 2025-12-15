using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Current Weapons")]
    // WeaponManager가 이 변수들을 관리함
    public Weapon meleeWeapon; 
    public Weapon rangedWeapon; 
    
    [Header("Setup")]
    public Transform attackPoint; // 공격 위치

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Time.timeScale == 0) return; // 일시정지(무기 교체) 중에는 공격 불가

        bool isRangedMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isRangedMode)
            {
                // 원거리 공격 (Shift + X)
                if (rangedWeapon != null)
                    rangedWeapon.PerformAttack(attackPoint, playerStats);
            }
            else
            {
                // 기본 공격 (X)
                if (meleeWeapon != null)
                    meleeWeapon.PerformAttack(attackPoint, playerStats);
            }
        }
    }
}