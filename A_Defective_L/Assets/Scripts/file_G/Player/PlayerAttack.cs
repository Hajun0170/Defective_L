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

    private Rigidbody2D rb;
    private Animator anim;  // 애니메이터 변수
    //public int skillCost = 5;

     void Start()
    {
        anim = GetComponent<Animator>(); // 내 몸에 붙은 애니메이터 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        HandleAttackInput();

        // [추가] 컷신 중이거나 시간이 멈췄으면 조작 불가
    if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
    if (Time.timeScale == 0) return;
    }

    private void HandleAttackInput()
    {
        if (Time.timeScale == 0) return; // 일시정지(무기 교체) 중에는 공격 불가

       // bool isRangedMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Z))
        {           
                // 기본 공격 (Z)
                if (meleeWeapon != null)
                    meleeWeapon.PerformAttack(attackPoint, playerStats);
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
             // 원거리 공격 (X)

               /*      
            if (playerStats.UseGauge(skillCost)) 
               {
             anim.SetTrigger("R_Skill_1");
                }
                */

                if (rangedWeapon != null)
                    rangedWeapon.PerformAttack(attackPoint, playerStats);
                    
        }

    }
}