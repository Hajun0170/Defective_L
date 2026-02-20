using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour // 보스 몬스터의 사망 여부를 체크, 연출도 포함
{
    [Header("Basic")]
    public string bossName = "Boss";
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Collider2D col; 
    
    private Coroutine flashRoutine;

    // 카메라용
    private Transform playerTransform; 

    public bool isDead = false; // 사망 여부
    public bool isIntroFinished = false; // 연출 종료 여부

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (spriteRenderer != null) 
        {
            Color c = spriteRenderer.color;
            c.a = 1f; // 알파값 1 (보이게 표시함)
            spriteRenderer.color = c;
        }

        // Manager가 부를 때까지 대기
    }

    // 등장 연출 

    public IEnumerator StartBossIntro()
    {
        Debug.Log("보스 등장 연출 시작");

        // 필요한 정보 가져오기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        FollowCamera camScript = null;
        if (Camera.main != null) camScript = Camera.main.GetComponent<FollowCamera>();


        // 카메라 자동 추적 끄기 
        if (camScript != null) camScript.SetCutsceneMode(true);

        // 플레이어 -> 보스에게 시선 이동 (1초 동안)
      
        if (camScript != null && player != null)
        {
            Vector3 startPos = camScript.transform.position;
            // 보스 위치 + 카메라 오프셋(Z축 유지)
            Vector3 targetPos = this.transform.position + camScript.vOffset; 

            float duration = 1.0f; // 이동하는 데 걸리는 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // SmoothStep: 시작과 끝을 부드럽게 연결하는 역할
                t = Mathf.SmoothStep(0f, 1f, t); 
                
                camScript.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }
      
        yield return new WaitForSeconds(0.5f);

        // 체력바 UI 
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHUDActive(true);
            UIManager.Instance.UpdateBossHealth(currentHealth, maxHealth);
        }
        yield return new WaitForSeconds(0.5f); // UI 감상 시간

        //  보스 -> 플레이어에게 시선 복귀 (0.5초)

        if (camScript != null && player != null)
        {
            Vector3 startPos = camScript.transform.position;
            Vector3 targetPos = player.transform.position + camScript.vOffset;

            float duration = 0.8f; // 돌아올 땐 좀 더 빠르게 이동
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                camScript.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }

        //  카메라 자동 추적 다시 킴, 플레이어 조작 해제
        if (camScript != null) camScript.SetCutsceneMode(false);

        Debug.Log("보스 전투 시작");
    }

    // 데미지 처리 (EnemyHealth 로직 + 보스 UI)
    public void TakeDamage(int damage)
    {   
        
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"보스 피격! 남은 체력: {currentHealth}");

        // 피격 시, 깜빡임 (EnemyHealth 기능)
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());

        // 보스 전용 UI 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHealth(currentHealth, maxHealth);
        }

        // 사망 체크
        if (currentHealth <= 0)
        {
            isDead = true; // 사망

            // 옆에 붙어있는 AI를 찾아서 멈추라고 전달
         
            FinalBossAI ai = GetComponent<FinalBossAI>();
            if (ai != null)
            {
                ai.StopAllPatterns(); // 공격 중지
            }
            
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator HitColorRoutine()
    {
        // 맞으면 빨간색
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        // 다시 원래색 (흰색)으로 되돌림
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    // 사망 연출
    IEnumerator DeathSequence()
    {     
        
        // 슬로우 모션
        Time.timeScale = 0.2f;

        // UI 끄기
        if (UIManager.Instance != null) UIManager.Instance.SetBossHUDActive(false);

        // 서서히 투명해지기
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime * 0.5f;
            if (spriteRenderer != null)
            
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }
            yield return null;
        }
        // 충돌 끄기
            if(col != null) col.enabled = false;

        Time.timeScale = 1f;

        // 매니저에게 문 여는 것을 전달
        BossBattleManager manager = FindFirstObjectByType<BossBattleManager>();
        if (manager != null) manager.OnBossDefeated();
    }
    // 현재 체력 반환 함수
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // 죽었는지 확인하는 함수
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    // 현재 체력 비율 반환 (0.0 ~ 1.0). 보스 패턴이 체력 기준이라 비율을 지정해서 패턴과 연계하는 경우 때문에 만든 함수
    public float GetHealthPercentage()
    {
        if (maxHealth == 0) return 0;
        return (float)currentHealth / (float)maxHealth;
    }
}