using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Basic Info")]
    public string bossName = "Boss";
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Collider2D col; 
    
    // 깜빡임 코루틴용
    private Coroutine flashRoutine;

    // 카메라 복귀용
    private Transform playerTransform; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // ★ [중요] 시작하자마자 안 보이던 문제 해결!
        // 혹시 투명하게 저장되어 있을까봐 강제로 불투명하게 만듦
        if (spriteRenderer != null) 
        {
            Color c = spriteRenderer.color;
            c.a = 1f; // 알파값 1 (보이게)
            spriteRenderer.color = c;
        }

        // ★ [변경점] Start에서는 아무것도 안 함! 
        // 문지기(Manager)가 부를 때까지 대기
    }

    // ====================================================
    // 🎬 1. 등장 연출 (매니저가 호출함)
    // ====================================================
    public IEnumerator StartBossIntro()
    {
        Debug.Log("👁️ 보스 등장 연출 시작");

        // 1. 필요한 정보 가져오기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        FollowCamera camScript = null;
        if (Camera.main != null) camScript = Camera.main.GetComponent<FollowCamera>();

        // 플레이어 움직임 멈춤 (선택사항)
        // if(player != null) player.GetComponent<PlayerController>().enabled = false;

        // 2. 카메라 자동 추적 끄기 (내가 직접 움직일 거니까)
        if (camScript != null) camScript.SetCutsceneMode(true);

        // -------------------------------------------------------
        // [연출 1] 플레이어 -> 보스에게 시선 이동 (1초 동안)
        // -------------------------------------------------------
        if (camScript != null && player != null)
        {
            Vector3 startPos = camScript.transform.position;
            // 목표: 보스 위치 + 카메라 오프셋(Z축 유지)
            Vector3 targetPos = this.transform.position + camScript.vOffset; 

            float duration = 1.0f; // 이동하는 데 걸리는 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // SmoothStep: 시작과 끝을 부드럽게 (S자 곡선)
                t = Mathf.SmoothStep(0f, 1f, t); 
                
                camScript.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }

        // 3. 보스 비추면서 대기 (1.5초)
        // (이때 보스가 포효하는 애니메이션 재생하면 좋음)
        // if(anim != null) anim.SetTrigger("Intro");
        yield return new WaitForSeconds(0.5f);

        // 4. 체력바 UI 짠!
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHUDActive(true);
            UIManager.Instance.UpdateBossHealth(currentHealth, maxHealth);
        }
        yield return new WaitForSeconds(0.5f); // UI 감상 시간

        // -------------------------------------------------------
        // [연출 2] 보스 -> 플레이어에게 시선 복귀 (0.5초 동안 빠르게)
        // -------------------------------------------------------
        if (camScript != null && player != null)
        {
            Vector3 startPos = camScript.transform.position;
            Vector3 targetPos = player.transform.position + camScript.vOffset;

            float duration = 0.8f; // 돌아올 땐 좀 더 빠르게
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

        // 5. 카메라 자동 추적 다시 켜기 & 플레이어 조작 해제
        if (camScript != null) camScript.SetCutsceneMode(false);
        // if(player != null) player.GetComponent<PlayerController>().enabled = true;

        Debug.Log("⚔️ 보스 전투 개시!");
    }

    // ====================================================
    // 🩸 데미지 처리 (EnemyHealth 로직 + 보스 UI)
    // ====================================================
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"보스 피격! 남은 체력: {currentHealth}");

        // 1. 피격 애니메이션
        //if(anim != null) anim.SetTrigger("Hit");

        // 2. 피격 깜빡임 (EnemyHealth 기능)
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());

        // 3. ★ [핵심] 보스 전용 UI 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHealth(currentHealth, maxHealth);
        }

        // 4. 사망 체크
        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator HitColorRoutine()
    {
        // 맞으면 빨간색
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        // 다시 원래색 (흰색)
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    // ====================================================
    // 💀 2. 사망 연출
    // ====================================================
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

        // ★ 매니저에게 "나 죽었으니 문 열어" 보고
        BossBattleManager manager = FindFirstObjectByType<BossBattleManager>();
        if (manager != null) manager.OnBossDefeated();

        // (선택) 오브젝트 삭제 or 비활성화
        // Destroy(gameObject); // 매니저가 꺼줄 거라 굳이 안 해도 됨
    }
    // 1. 현재 체력 반환 함수
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // 2. 죽었는지 확인하는 함수
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
    // BossController.cs 안의 맨 아래쪽에 추가해주세요

    // ★ [추가] 현재 체력 비율 반환 (0.0 ~ 1.0)
    public float GetHealthPercentage()
    {
        if (maxHealth == 0) return 0;
        return (float)currentHealth / (float)maxHealth;
    }
}