using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float hitInvincibilityTime = 1.0f; // 피격 후 무적 시간 (1초)
    private int currentHealth;

    [Header("Cylinder Gauge")]
    [SerializeField] private int maxGauge = 10;
    [SerializeField] private int gaugeForTicket = 5; // 티켓 1장당 게이지 5
    private int currentGauge = 0;
    private int accumulatedGauge = 0; // 티켓 변환용 누적 게이지

    [Header("Tickets")]
    [SerializeField] private int maxTickets = 3;
    private int currentTickets = 0;

    // 공격력 버프 (기본 1.0 = 100%)
    public float DamageMultiplier { get; private set; } = 1.0f;

    // 무적 상태 확인
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer; // 깜빡임 효과를 위해 필요

    // 외부 확인용 프로퍼티
    public int CurrentGauge => currentGauge;
    public int CurrentTickets => currentTickets;

    private void Awake()
    {
        // 플레이어의 그래픽(스프라이트)을 제어하기 위해 가져옴
        // 만약 스프라이트가 자식 오브젝트에 있다면 GetComponentInChildren<SpriteRenderer>() 사용
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentGauge = 0;
        currentTickets = 0;

        // 시작 시 UI 초기화
        UIManager.Instance.UpdateHealth(currentHealth);
        UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
        UIManager.Instance.UpdateTickets(currentTickets);
    }

    // --- 데미지 처리 ---
    public void TakeDamage(int amount)
    {
        // 1. 무적 상태면 데미지 무시
        if (isInvincible) return;

        // 2. 체력 감소
        currentHealth -= amount;
        Debug.Log($"플레이어 피격! 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("플레이어 사망!");
            // 게임 오버 로직 추가
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망... (게임 오버)");
        // 여기에 게임 오버 UI 호출이나 캐릭터 파괴 로직 추가
        gameObject.SetActive(false); 
    }

    // --- 무적 설정 (이동 스크립트에서 호출) ---
    public void SetInvincible(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        Debug.Log("🛡️ 무적 상태 시작!");
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("무적 상태 종료.");
    }

    // --- 자원 관리 ---
    public void AddGauge(int amount)
    {
        currentGauge = Mathf.Clamp(currentGauge + amount, 0, maxGauge);
        accumulatedGauge += amount;

        // 게이지 5마다 티켓 1장 충전
        if (accumulatedGauge >= gaugeForTicket)
        {
            int newTickets = accumulatedGauge / gaugeForTicket;
            AddTicket(newTickets);
            accumulatedGauge %= gaugeForTicket;
        }
        Debug.Log($"[자원] 게이지: {currentGauge}, 누적: {accumulatedGauge}");

        // [추가] UI 갱신 요청
        UIManager.Instance.UpdateGauge(currentGauge, maxGauge);
    }

    private void AddTicket(int amount)
    {
        currentTickets = Mathf.Clamp(currentTickets + amount, 0, maxTickets);
        Debug.Log($"[자원] 🎟️ 교환권 획득! 현재: {currentTickets}장");

        // [추가] UI 갱신 요청
        UIManager.Instance.UpdateTickets(currentTickets);
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge >= amount)
        {
            currentGauge -= amount;

            // [추가] 소모 후 즉시 갱신
            UIManager.Instance.UpdateGauge(currentGauge, maxGauge);

            Debug.Log($"[자원] 게이지 소모: -{amount}");
            return true;
        }
        return false;
    }

    public bool UseTicket()
    {
        if (currentTickets > 0)
        {
            currentTickets--;

            // [추가] 소모 후 즉시 갱신
            UIManager.Instance.UpdateTickets(currentTickets);
            
            Debug.Log($"[자원] 🎟️ 교환권 사용! 남은 수: {currentTickets}");
            return true;
        }
        return false;
    }

    // --- 버프 시스템 ---
    public void ActivateSwapBuff()
    {
        StopCoroutine(nameof(BuffCoroutine)); // 기존 버프가 있다면 초기화
        StartCoroutine(nameof(BuffCoroutine));
    }

    private IEnumerator BuffCoroutine()
    {
        DamageMultiplier = 1.2f; // 공격력 20% 증가
        Debug.Log("🔥 버프 발동! 공격력 120%");
        
        yield return new WaitForSeconds(2.0f); // 2초 유지

        DamageMultiplier = 1.0f;
        Debug.Log("버프 종료.");
    }
}