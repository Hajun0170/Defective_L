using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int iMaxHp = 30;

    [Header("Knockback")]
    public float fKnockForce = 6f;         // Rigidbody2D.Dynamic일 때 AddForce로 사용
    public float fKinematicKnockDist = 0.5f; // Rigidbody2D.Kinematic일 때 이동 거리
    public float fKinematicKnockTime = 0.08f;

    [Header("VFX")]
    public HitFlash2D cHitFlash; // 없으면 자동 탐색

    public event Action OnDeath;

    int iHp;
    Rigidbody2D rb;
    bool bKnocking;

    void Awake()
    {
        iHp = iMaxHp;
        rb  = GetComponent<Rigidbody2D>();
        if (cHitFlash == null) cHitFlash = GetComponentInChildren<HitFlash2D>();
    }

    public void TakeDamage(int iAmount, Vector2 v2KnockDir)
    {
        // 데미지 적용
        int iDmg = Mathf.Max(0, iAmount);
        iHp -= iDmg;
        Debug.Log($"[EnemyHealth] {name} took {iDmg} -> {iHp}/{iMaxHp}");

        // 깜빡임
        if (cHitFlash != null) cHitFlash.Flash();

        // 노크백
        ApplyKnockback(v2KnockDir);

        // 사망 체크
        if (iHp <= 0) Die();
    }

    void ApplyKnockback(Vector2 dir)
    {
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.AddForce(dir * fKnockForce, ForceMode2D.Impulse);
        }
        else
        {
            // Kinematic 또는 Rigidbody가 없을 때: 짧게 위치 보간
            if (!bKnocking) StartCoroutine(CoKinematicKnock(dir));
        }
    }

    System.Collections.IEnumerator CoKinematicKnock(Vector2 dir)
    {
        bKnocking = true;
        Vector3 start = transform.position;
        Vector3 end   = start + (Vector3)(dir * fKinematicKnockDist);

        float t = 0f;
        while (t < fKinematicKnockTime)
        {
            t += Time.deltaTime;
            float p = t / fKinematicKnockTime;
            // ease-out
            float eased = 1f - (1f - p) * (1f - p);
            transform.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }
        bKnocking = false;
    }

    void Die()
    {
        try { OnDeath?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
        Debug.Log($"[EnemyHealth] {name} died");
        Destroy(gameObject);
    }
}
