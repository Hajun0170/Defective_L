using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float hitInvincibilityTime = 1.0f;
    private int currentHealth;

    [Header("Cylinder Gauge")]
    [SerializeField] private int maxGauge = 10;
    [SerializeField] private int gaugeForTicket = 5;
    private int currentGauge = 0;
    private int accumulatedGauge = 0;

    [Header("Tickets")]
    [SerializeField] private int maxTickets = 3;
    private int currentTickets = 0;

    public float DamageMultiplier { get; private set; } = 1.0f;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    
    // 외부 확인용 프로퍼티
    public int CurrentHealth => currentHealth;
    public int CurrentGauge => currentGauge;
    public int CurrentTickets => currentTickets;

    // ★ [수정 1] 싱글톤 Instance 변수는 남겨두되(혹시 참조하는 애들이 있을까봐), 
    // 할당은 하지 않거나 신중해야 합니다. 
    // 가장 좋은 건 GetComponent로 통신하는 것이므로 일단 삭제하거나 주석처리 추천합니다.
    // public static PlayerStats Instance; <--- 삭제 추천 (PlayerMovement 등에서 GetComponent로 쓰세요)

    private Rigidbody2D rb;
    private Animator anim;


    private void Awake()
    {
        // ★ [수정 1] 싱글톤 패턴 삭제
        // 플레이어는 씬마다 새로 생기는 "프리팹"이므로 DontDestroyOnLoad를 쓰면 안 됩니다!
        // 그냥 컴포넌트만 가져오면 끝입니다.
        
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>(); // (Start에 있던거 여기로 옮겨도 됨)
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // ★ [핵심 1] 태어나자마자 DataManager(은행)에서 내 스탯 가져오기
        if (DataManager.Instance != null)
        {
            // 데이터 매니저에 저장된 값으로 내 몸 상태를 동기화
            currentHealth = DataManager.Instance.currentData.currentHealth;
            currentGauge = DataManager.Instance.currentData.currentGauge;
            
            // (티켓도 저장한다면 추가 필요)
             currentTickets = DataManager.Instance.currentData.currentTickets; 

            // 만약 새 게임이라 데이터가 0이거나 이상하면 최대치로 설정
            if (currentHealth <= 0 && !DataManager.Instance.currentData.isDead)
            {
                currentHealth = maxHealth;
                currentGauge = 0;
            }
        }
        else
        {
            // 테스트용 (매니저 없이 씬만 켰을 때)
            currentHealth = maxHealth;
        }
        /*
        // 1. 데이터 동기화 (GameManager -> 나)
        if (GameManager.Instance != null)
        {
            currentHealth = GameManager.Instance.storedHealth;
            currentGauge = GameManager.Instance.storedGauge;
            currentTickets = GameManager.Instance.storedTickets;
            
        }
        else
        {
            // GameManager가 없으면 기본값 (테스트용)
            currentHealth = maxHealth;
            currentGauge = 0;
            currentTickets = 0;
        }
        */

        // 2. 초기 UI 갱신
        UpdateAllUI();
    }

    // --- 데미지 처리 ---
    public void TakeDamage(int amount, Transform attacker)
    {
        if (isInvincible) return;
        
        // 2. 체력 감소 (1 대신 들어온 데미지 amount를 쓰는 게 더 유연합니다)
        currentHealth -= amount;
        
        // 3. 피격 애니메이션
        if(anim != null) anim.SetTrigger("Hit");

       // anim.SetTrigger("Hit");
       // currentHealth -= 1; // 데미지 적용
       

        // ★ [핵심 2] 스탯이 변할 때마다 즉시 DataManager에 보고!
        SyncDataToManager();

        UpdateAllUI(); // UI 갱신

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        
        //GetComponent<PlayerMovement>()?.ApplyKnockback(attacker);

        // 5. 넉백 (밀려남) 효과
        // PlayerMovement 스크립트가 있다면 넉백 실행
        if (TryGetComponent(out PlayerMovement movement))
        {
            movement.ApplyKnockback(attacker);
        }

        StartCoroutine(HitInvincibilityRoutine());
    }

    void Die()
{
    // 1. 죽는 애니메이션 재생 (있다면)
    // animator.SetTrigger("Die");

    // 2. 조작 불가능하게 막기 (이동 스크립트 끄기)
   //  GetComponent<PlayerController>().enabled = false;
    // GetComponent<Collider2D>().enabled = false; // 무적 상태

    // 3. ★ [핵심] 게임 매니저에게 사망 처리 요청 (슬로우 모션 + 부활)
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnPlayerDead();
    }
    
    // 주의: 절대 Destroy(gameObject) 하지 마세요! 
    // 매니저가 위치만 옮겨서 재활용할 겁니다.
}

    // --- 자원 관리 ---
    public void AddGauge(int amount)
    {
        // ★ [수정 3] 순서 변경 (계산 먼저 -> UI 갱신 나중)
        
        // 1. 계산
        currentGauge = Mathf.Clamp(currentGauge + amount, 0, maxGauge);
        accumulatedGauge += amount;

        // 2. 티켓 변환
        if (accumulatedGauge >= gaugeForTicket)
        {
            int newTickets = accumulatedGauge / gaugeForTicket;
            AddTicket(newTickets);
            accumulatedGauge %= gaugeForTicket;
        }

        // ★ 변할 때마다 보고
        SyncDataToManager();
        UpdateAllUI();

        /*
        
        Debug.Log($"[자원] 게이지: {currentGauge}, 누적: {accumulatedGauge}");

        // 3. UI 갱신 (주석 해제)
        if (UIManager.Instance != null) 
            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
            */
    }

    private void AddTicket(int amount)
    {
        currentTickets = Mathf.Clamp(currentTickets + amount, 0, maxTickets);
   
   // ★ 변할 때마다 보고
// ★ 변할 때마다 보고
        SyncDataToManager();
        UpdateAllUI();

/*
        // ★ [수정 2] 주석 해제
        if (UIManager.Instance != null) 
            UIManager.Instance.UpdateTickets(currentTickets);
            */
    }

    // ★ [가장 중요] 내 현재 상태를 DataManager에 덮어쓰는 함수
    private void SyncDataToManager()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.currentHealth = currentHealth;
            DataManager.Instance.currentData.currentGauge = currentGauge;
            DataManager.Instance.currentData.currentTickets = currentTickets; // 필요시 추가
        }
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge >= amount)
        {
            currentGauge -= amount;
            anim.SetTrigger("R_Skill_1");

            // ★ [수정 2] 주석 해제
            if (UIManager.Instance != null) 
                UIManager.Instance.UpdateGauge(currentGauge, maxGauge);

            Debug.Log($"[자원] 게이지 소모: -{amount}");
            return true;
        }
        else
        {
           Debug.Log("게이지 부족!");
        }
        return false;
    }

    public bool UseTicket()
    {
        if (currentTickets > 0)
        {
            currentTickets--;
            
            // ★ [수정 2] 주석 해제
            if (UIManager.Instance != null) 
                UIManager.Instance.UpdateTickets(currentTickets);
            
            Debug.Log($"[자원] 🎟️ 교환권 사용! 남은 수: {currentTickets}");
            return true;
        }
        return false;
    }

    // --- 유틸리티 ---
    private void UpdateAllUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth);
            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
            UIManager.Instance.UpdateTickets(currentTickets);
        }
    }
    
    // ... (무적 코루틴, 버프 코루틴 등 나머지 코드는 그대로 두셔도 됩니다) ...
    private IEnumerator HitInvincibilityRoutine()
    {
       // ... 기존 코드 유지 ...
       isInvincible = true;
       // ... 생략 ...
       yield return new WaitForSeconds(hitInvincibilityTime); // 예시
       isInvincible = false;
    }
    
    public void SetInvincible(float duration) { StartCoroutine(InvincibilityCoroutine(duration)); }
    private IEnumerator InvincibilityCoroutine(float duration) {
        isInvincible = true; yield return new WaitForSeconds(duration); isInvincible = false;
    }
    public void ActivateSwapBuff() { StartCoroutine(nameof(BuffCoroutine)); }
    private IEnumerator BuffCoroutine() {
        DamageMultiplier = 1.2f; yield return new WaitForSeconds(2.0f); DamageMultiplier = 1.0f;
    }
}