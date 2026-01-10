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

        // 2. 초기 UI 갱신
        UpdateAllUI();
    }

    // --- 데미지 처리 ---
    public void TakeDamage(int amount, Transform attacker)
    {
        if (isInvincible) return;
        
        anim.SetTrigger("Hit");
        currentHealth -= 1; // 데미지 적용
        
        Debug.Log($"플레이어 피격! 남은 체력: {currentHealth}");

        // ★ [수정 2] 주석 해제 (맞을 때마다 UI 갱신해야 함)
        if (UIManager.Instance != null) 
            UIManager.Instance.UpdateHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        
        GetComponent<PlayerMovement>()?.ApplyKnockback(attacker);
        StartCoroutine(HitInvincibilityRoutine());
    }

    private void Die()
    {
        Debug.Log("플레이어 사망...");
        gameObject.SetActive(false); 
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
        
        Debug.Log($"[자원] 게이지: {currentGauge}, 누적: {accumulatedGauge}");

        // 3. UI 갱신 (주석 해제)
        if (UIManager.Instance != null) 
            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
    }

    private void AddTicket(int amount)
    {
        currentTickets = Mathf.Clamp(currentTickets + amount, 0, maxTickets);
        Debug.Log($"[자원] 🎟️ 교환권 획득! 현재: {currentTickets}장");

        // ★ [수정 2] 주석 해제
        if (UIManager.Instance != null) 
            UIManager.Instance.UpdateTickets(currentTickets);
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