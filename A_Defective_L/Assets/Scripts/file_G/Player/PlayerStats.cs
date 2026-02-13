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
    // ★ [추가] 외부(투사체)에서 무적 여부를 확인할 수 있게 읽기 전용으로 열어줌
    public bool IsInvincible => isInvincible;
    private SpriteRenderer spriteRenderer;

    [Header("Swap Buff Settings")]
    public float swapBuffMultiplier = 1.5f; // 공격력 1.5배 증가
    public float swapBuffDuration = 3.0f;   // 버프 지속 시간 (근접 무기용)
    
    private bool isSwapBuffActive = false;
    private Coroutine buffCoroutine;

    [Header("Sound Effects")]
    public AudioClip hitSound;   // 맞았을 때
    public AudioClip healSound;  // 회복했을 때
    public AudioClip deathSound; // 죽었을 때
    
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

    [Header("2. Potion Settings")]
    public int potionCapacity = 1; // ★ 키트 주머니 크기 (로컬 변수로 관리 추천)
    public int currentPotions = 0; // 현재 키트 수
    public int healAmountPerKit = 3; // 키트 하나당 회복량

    [Header("3. Economy")]
    public int currentGold = 0;    // ★ [추가] 현재 재화
    
    private bool isDead = false; // 사망 여부 체크용 변수 추가

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
        isDead = false;
        // 1. DataManager가 있으면 저장된 값으로 내 스탯을 덮어씌움 (Load)
        if (DataManager.Instance != null)
        {
            Debug.Log("🔄 [PlayerStats] 데이터 로드 시도...");

            // ★ [순서 중요] 최대치(Max)와 재화(Gold)를 먼저 불러와야 합니다.
            maxHealth = DataManager.Instance.currentData.maxHealth;
            potionCapacity = DataManager.Instance.currentData.potionCapacity;
            currentGold = DataManager.Instance.currentData.gold;

            // 그 다음 현재 상태(Current) 불러오기
            currentHealth = DataManager.Instance.currentData.currentHealth;
            currentGauge = DataManager.Instance.currentData.currentGauge;
            currentTickets = DataManager.Instance.currentData.currentTickets;
            currentPotions = DataManager.Instance.currentData.currentPotions;

            // ★ [데이터 검증] 만약 로드된 데이터가 비정상(0)이면 기본값 사용
            // (새 게임이거나, 저장이 제대로 안 됐을 경우를 대비)
            if (maxHealth < 5) maxHealth = 5;
            if (potionCapacity < 1) potionCapacity = 1;
            
            // 만약 현재 체력이 0인데 죽은 건 아니라면 풀피로 (버그 방지)
            if (currentHealth <= 0 && !DataManager.Instance.currentData.isDead) 
                currentHealth = maxHealth;

            Debug.Log($"✅ 로드 완료: 체력({currentHealth}/{maxHealth}), 돈({currentGold})");
        }
        else
        {
            // 매니저 없을 때 (테스트용)
            currentHealth = maxHealth;
        }

        UpdateAllUI();
        StartCoroutine(LateUIUpdate());
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
        // 이미 무적이거나 '이미 죽었다면' 로직 실행 안 함
        if (isInvincible || isDead) return;
        
        // 2. 체력 감소 (1 대신 들어온 데미지 amount를 쓰는 게 더 유연합니다)
        currentHealth -= amount;
        
 if (currentHealth <= 0)
        {
            isDead = true; // 죽음 확정
        currentHealth = 0;
        AudioManager.Instance.PlaySFX(hitSound); // 사망음 딱 한 번 재생
            Die();
            return;
        }
        
        
    // 살아있을 때만 피격음 재생
    AudioManager.Instance.PlaySFX(hitSound);

        // 3. 피격 애니메이션
        if(anim != null) anim.SetTrigger("Hit");

        // ★ [핵심 2] 스탯이 변할 때마다 즉시 DataManager에 보고!
        SyncDataToManager();

        UpdateAllUI(); // UI 갱신

       
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
            DataManager.Instance.currentData.currentPotions = currentPotions;
            DataManager.Instance.currentData.gold = currentGold; // 돈도 동기화
            // ★ [누락된 핵심 코드] 최대 체력도 변했으면 저장해야 함!
            DataManager.Instance.currentData.maxHealth = maxHealth;
        }
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge >= amount)
        {
            currentGauge -= amount;
            //anim.SetTrigger("R_Skill_1");

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
   // [수정] UI 업데이트 시 로컬 변수 동기화
    private void UpdateAllUI()
    {
        if (UIManager.Instance != null && DataManager.Instance != null)
        {
            // ★ UI 그리기 전에 로컬 변수를 매니저 값으로 덮어쓰기 (안전장치)
            potionCapacity = DataManager.Instance.currentData.potionCapacity;
            currentPotions = DataManager.Instance.currentData.currentPotions;
            currentGold = DataManager.Instance.currentData.gold;

            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
            UIManager.Instance.UpdateTickets(currentTickets);
            
            // 포션 UI
            UIManager.Instance.UpdatePotionUI(currentPotions, potionCapacity);
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
    
    /*
    // UI 갱신 (이미 연결되어 있다면)
    if (UIManager.Instance != null)
    {
      // 최대 용량(potionCapacity)만큼 현재 개수(currentPotions)를 채움
        DataManager.Instance.currentData.currentPotions = DataManager.Instance.currentData.potionCapacity;
    }
    */

    // (3) UI 및 데이터 갱신
    SyncDataToManager(); // 변경된 체력을 데이터 매니저에 즉시 반영
    UpdateAllUI();       // 체력바, 포션UI 등 모든 UI 갱신
}

// ★ [신규] 쉼터에서 호출할 함수 (체력 + 포션 모두 리필)
    public void RestAtShelter()
    {
        // 1. 체력 완충
        currentHealth = maxHealth;

        // 2. 포션 완충 (DataManager 값 이용)
        if (DataManager.Instance != null)
        {
            // 용량만큼 현재 개수 채우기
            DataManager.Instance.currentData.currentPotions = DataManager.Instance.currentData.potionCapacity;
            
            // 로컬 변수도 싱크 맞추기 (중요)
            currentPotions = DataManager.Instance.currentData.currentPotions;
        }

        SyncDataToManager();
        UpdateAllUI();
        
        Debug.Log("💤 쉼터 휴식 완료: 체력/포션 모두 회복!");
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
        if (DataManager.Instance != null){
            DataManager.Instance.currentData.maxHealth = maxHealth;
        }
        // 보통 최대 체력이 늘면 체력을 꽉 채워줍니다 (선택사항)
        HealToFull();
        
        UpdateAllUI();
        Debug.Log($"최대 체력 증가! 현재: {maxHealth}");
    }

    // 3. 키트 소지 한도 증가 아이템 획득 시 호출
    public void UpgradePotionCapacity()
    {
        /*
        DataManager.Instance.currentData.potionCapacity++;

        // ★ 늘어난 용량을 즉시 저장
        if (DataManager.Instance != null)
            DataManager.Instance.currentData.potionCapacity = potionCapacity;

        // 얻자마자 키트 하나 채워주기
        DataManager.Instance.currentData.currentPotions++;
        
        UpdateAllUI();
        Debug.Log($"키트 용량 증가! 최대: {DataManager.Instance.currentData.potionCapacity}");
        */
        // 1. 매니저 데이터 증가
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.potionCapacity++;
            DataManager.Instance.currentData.currentPotions++; // 얻자마자 하나 줌
            
            // ★ 2. 로컬 변수 즉시 동기화 (이게 빠져서 꼬였던 것!)
            potionCapacity = DataManager.Instance.currentData.potionCapacity;
            currentPotions = DataManager.Instance.currentData.currentPotions;
        }

        UpdateAllUI();
        Debug.Log($"키트 용량 증가! 최대: {potionCapacity}");
    }

    // 3. 재화(골드) 획득
    public void AddGold(int amount)
    {
        currentGold += amount;

       // 2. 장부(DataManager)에 즉시 기록하기
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.gold = currentGold;
        } 
        // UI 갱신 (UpdateAllUI에 골드 갱신 로직이 있다면 자동 처리됨)
        UpdateAllUI(); 
    }

    
    // (기존) 힐 함수 수정: 최대 체력 넘지 않게
    public void Heal(int amount)
    {
        currentHealth += amount;
        
        // ★ [추가] 회복 소리 재생
        AudioManager.Instance.PlaySFX(healSound);

        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateAllUI();
    }

    // ★ 0.1초 뒤에 UI를 강제로 다시 맞춤
    IEnumerator LateUIUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateAllUI();
        // Debug.Log("UI 지연 갱신 완료"); 
    }

    // ★ [핵심 추가] 씬 이동 직전에 GameManager가 호출할 "강제 저장" 함수
    public void SaveStatsToManager()
    {
        if (DataManager.Instance != null)
        {
            // 현재 가진 모든 중요한 스탯을 DataManager에 밀어 넣습니다.
            DataManager.Instance.currentData.maxHealth = maxHealth;         // 최대 체력
            DataManager.Instance.currentData.currentHealth = currentHealth; // 현재 체력
            DataManager.Instance.currentData.gold = currentGold;            // 돈
            
            DataManager.Instance.currentData.potionCapacity = potionCapacity;
            DataManager.Instance.currentData.currentPotions = currentPotions;
            
            DataManager.Instance.currentData.currentGauge = currentGauge;
            DataManager.Instance.currentData.currentTickets = currentTickets;

            Debug.Log($"💾 [PlayerStats] 씬 이동 전 데이터 백업 완료! (MaxHP: {maxHealth}, Gold: {currentGold})");
        }
    }

}