using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab; 
    public Transform firePoint;        
    public float attackRange = 5f;     
    public float fireRate = 1f;        
    
    [Header("Burst Settings (개틀링용)")]
    public int burstCount = 1;         
    public float burstSpeed = 0.1f;    

    private float nextAttackTime;
    private Transform player;
    
    // ★ [오류 1 해결] 프로젝트에 'Enemy'라는 스크립트가 있어야 이 줄이 에러가 안 납니다.
    // 만약 Enemy 스크립트가 없다면 이 줄과 Start의 GetComponent를 지우세요.
    private EnemyHealth enemyBase; 

    [Header("Reward & Audio")]
    public Weapon dropWeapon; 
    
    // ★ [오류 2 해결] 이 변수 선언이 빠져서 에러가 났던 것입니다. 추가하세요!
    public AudioClip deathSound; 

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Enemy 스크립트가 같은 오브젝트에 붙어있다고 가정
        enemyBase = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + fireRate;
        }
    }

    IEnumerator AttackRoutine()
    {
        // 공격 중 이동 멈춤 (Enemy 스크립트가 있을 때만)
        // if (enemyBase != null) enemyBase.StopMoving();

        Vector3 direction = (player.position - transform.position).normalized;
        // (회전 로직 등...)

        for (int i = 0; i < burstCount; i++)
        {
            if (projectilePrefab != null && firePoint != null)
            {
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                // 발사 소리도 여기서 추가 가능
            }
            
            if (burstCount > 1) yield return new WaitForSeconds(burstSpeed);
        }
        
        // 이동 재개
        // if (enemyBase != null) enemyBase.ResumeMoving();
    }

    // ★ 주의: 체력 관리를 하는 Enemy 스크립트에도 Die()가 있을 텐데, 
    // 여기서 또 Die()를 만들면 헷갈릴 수 있습니다.
    // 보통은 Enemy 스크립트가 체력이 0이 되면 보상을 주고 죽습니다.

}