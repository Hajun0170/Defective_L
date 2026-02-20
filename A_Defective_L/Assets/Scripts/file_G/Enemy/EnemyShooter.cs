using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour //탄환을 연사하는 코드인데, 다른 곳으로 합쳐서 현재는 사용은 안함
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
    
    // 프로젝트에 Enemy 스크립트 필요
    private EnemyHealth enemyBase; 

    [Header("Reward & Audio")]
    public Weapon dropWeapon; 
    public AudioClip deathSound; 

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
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

        Vector3 direction = (player.position - transform.position).normalized;

        for (int i = 0; i < burstCount; i++)
        {
            if (projectilePrefab != null && firePoint != null)
            {
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }
            
            if (burstCount > 1) yield return new WaitForSeconds(burstSpeed);
        }
        
    }

}