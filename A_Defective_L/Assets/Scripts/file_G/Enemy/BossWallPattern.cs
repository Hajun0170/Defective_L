using UnityEngine;
using System.Collections;

public class BossWallPattern : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float wallAttackRate = 1.5f; 
    public int projectileCount = 5;    

    [Header("References")]
    public Transform wallPoint;       
    public Transform groundPoint;     
    public GameObject projectilePrefab; 
    public Transform firePoint;       

    // ★ [수정] EnemyHealth 대신 BossController 참조
    private BossController bossController; 
    private Rigidbody2D rb;
    private Animator anim;
    
    private bool isPhaseTwo = false;  
    private bool isClinging = false;  

    private void Awake()
    {
        // ★ 같은 오브젝트에 있는 BossController 가져오기
        bossController = GetComponent<BossController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 예외 처리: 보스 컨트롤러가 없거나 이미 죽었거나 벽에 붙어있으면 패스
        if (bossController == null || isClinging || bossController.IsDead()) return;

        // ★ [수정] BossController의 체력을 확인 (Getter 함수 필요)
        // 체력이 50% 이하고, 아직 2페이즈를 안 했다면?
        if (!isPhaseTwo && bossController.GetCurrentHealth() <= (bossController.maxHealth * 0.5f))
        {
            StartCoroutine(StartWallPhase());
        }
        else
        {
            // (1페이즈 로직...)
        }
    }

    IEnumerator StartWallPhase()
    {
        isPhaseTwo = true; 
        isClinging = true; 

        Debug.Log("보스: 2페이즈 시작! 벽으로 이동!");
        anim.SetTrigger("Move"); 

        // ★ [수정] 거리 오차 범위를 0.1f로 줄이고, 타임아웃(안전장치) 추가
        //float timeLimit = 5.0f; // 5초 안에 못 가면 강제 이동
        float timer = 0f;

        // ★ [수정 1] Z축 문제 방지를 위해 Vector2로 거리 계산
        // 혹시 벽 포인트 Z값이 보스랑 다르면 평생 도착 못할 수도 있음
        while (Vector2.Distance(transform.position, wallPoint.position) > 0.1f) // 0.5f -> 0.1f로 정밀하게
        {
            // 이동
            transform.position = Vector2.MoveTowards(transform.position, wallPoint.position, moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // ★ [핵심 해결책] 반복문이 끝났다는 건 거의 다 왔다는 뜻.
        // 하지만 미세하게 떨어져 있을 수 있으니, 강제로 좌표를 덮어씌워서 "딱 붙게" 만듭니다.
        transform.position = wallPoint.position;
        // 2. 벽 부착
        Debug.Log("보스: 벽 부착!");
        // 리지드바디 끄기 (중력 무시)
        rb.linearVelocity = Vector2.zero; // 혹시 움직이던 관성 제거
        rb.bodyType = RigidbodyType2D.Static; 
        
        // ★ [애니메이션] 벽 타는 모션 실행
        // 애니메이터에 "IsWallCling"이라는 Bool 파라미터를 만들고,
        // 벽에 매달려있는 애니메이션(WallCling_Idle)과 연결해야 합니다.
        anim.SetBool("IsWallCling", true);    

        // 3. 공격 루프
        for (int i = 0; i < projectileCount; i++)
        {
            yield return new WaitForSeconds(wallAttackRate);
            
            anim.SetTrigger("Attack"); 
            Debug.Log("보스: 하단 공격!");

            if (projectilePrefab != null)
            {
                Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, -90));
            }
        }

        yield return new WaitForSeconds(1f); 

        // 4. 복귀
        Debug.Log("보스: 패턴 종료, 복귀.");
        rb.bodyType = RigidbodyType2D.Dynamic; 
        
        // ★ 벽 타기 모션 해제
        anim.SetBool("IsWallCling", false);
        
        while (Vector2.Distance(transform.position, groundPoint.position) > 1f)
        {
             transform.position = Vector2.MoveTowards(transform.position, groundPoint.position, moveSpeed * 2 * Time.deltaTime);
             yield return null;
        }

        isClinging = false; 
    }
}