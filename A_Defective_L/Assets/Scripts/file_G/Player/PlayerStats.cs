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

    [Header("Swap Buff Settings")]
    public float swapBuffMultiplier = 1.5f; // 공격력 1.5배 증가
    public float swapBuffDuration = 3.0f;   // 버프 지속 시간 (근접 무기용)
    
    private bool isSwapBuffActive = false;
    private Coroutine buffCoroutine;

    
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

    [Header("Potion Settings")]
    public int healAmountPerKit = 3; // 키트 하나당 회복량

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

             maxHealth = DataManager.Instance.currentData.maxHealth;
            // 키트 개수도 불러오기

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

    void Update()
    {
        // ... (기존 무적시간 로직 등) ...

        // ★ [추가] C키를 누르면 회복 키트 사용
        if (Input.GetKeyDown(KeyCode.D))
        {
            UsePotion();
        }
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
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);

            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
            UIManager.Instance.UpdateTickets(currentTickets);
            // 포션 UI 갱신
            UIManager.Instance.UpdatePotionUI(
                DataManager.Instance.currentData.currentPotions, 
                DataManager.Instance.currentData.potionCapacity);
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
    /*
    public void ActivateSwapBuff() { StartCoroutine(nameof(BuffCoroutine)); }
    private IEnumerator BuffCoroutine() {
        DamageMultiplier = 1.2f; yield return new WaitForSeconds(2.0f); DamageMultiplier = 1.0f;
    }
    */

    // 1. 버프가 켜져 있는지 확인하는 프로퍼티 (무기가 가져다 씀)
    public float CurrentBuffMultiplier
    {
        get { return isSwapBuffActive ? swapBuffMultiplier : 1.0f; }
    }

    // 2. 무기 교체 시 호출할 함수 (버프 시작!)
    public void ActivateSwapBuff()
    {
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        buffCoroutine = StartCoroutine(BuffTimer());
    }

    // 3. (원거리용) 버프 강제 종료 함수
    public void ConsumeSwapBuff()
    {
        isSwapBuffActive = false;
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        // UI 갱신 등 필요하면 추가
        Debug.Log("🔥 원거리 공격으로 교체 버프 소모됨!");
    }

    // 타이머 (근접 무기는 이 시간 동안 계속 셈)
    IEnumerator BuffTimer()
    {
        isSwapBuffActive = true;
        // (선택) 플레이어 몸 색깔이 붉게 빛나는 이펙트 추가 가능
        Debug.Log("⚔️ 교체 버프 발동! 공격력 증가");

        yield return new WaitForSeconds(swapBuffDuration);

        isSwapBuffActive = false;
        Debug.Log("⏳ 교체 버프 종료");
    }

    public void HealToFull()
{
    currentHealth = maxHealth; // 체력 최대치로
    
    // UI 갱신 (이미 연결되어 있다면)
    if (UIManager.Instance != null)
    {
      // 최대 용량(potionCapacity)만큼 현재 개수(currentPotions)를 채움
        DataManager.Instance.currentData.currentPotions = DataManager.Instance.currentData.potionCapacity;
    }

    // (3) UI 및 데이터 갱신
    SyncDataToManager(); // 변경된 체력을 데이터 매니저에 즉시 반영
    UpdateAllUI();       // 체력바, 포션UI 등 모든 UI 갱신
}

// 1. 회복 키트 사용
    void UsePotion()
    {
        // 체력이 꽉 찼거나, 키트가 없으면 사용 불가
        if (currentHealth >= maxHealth) return;
        if (DataManager.Instance.currentData.currentPotions <= 0) 
        {
            Debug.Log("회복 키트가 없습니다!");
            return;
        }

        // 사용 로직
        DataManager.Instance.currentData.currentPotions--;
        Heal(healAmountPerKit); // 체력 회복 함수 호출
        
        // 이펙트 생성 (선택)
        // Instantiate(healEffect, transform.position, Quaternion.identity);

        UpdateAllUI();
    }

    // 2. 최대 체력 증가 아이템 획득 시 호출
    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        DataManager.Instance.currentData.maxHealth = maxHealth;
        
        // 최대 체력이 늘어나면 체력도 꽉 채워주는 게 국룰
        currentHealth = maxHealth; 
        
        UpdateAllUI();
        Debug.Log($"최대 체력 증가! 현재: {maxHealth}");
    }

    // 3. 키트 소지 한도 증가 아이템 획득 시 호출
    public void UpgradePotionCapacity()
    {
        DataManager.Instance.currentData.potionCapacity++;
        // 얻자마자 키트 하나 채워주기
        DataManager.Instance.currentData.currentPotions++;
        
        UpdateAllUI();
        Debug.Log($"키트 용량 증가! 최대: {DataManager.Instance.currentData.potionCapacity}");
    }

    // (기존) 힐 함수 수정: 최대 체력 넘지 않게
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateAllUI();
    }

}