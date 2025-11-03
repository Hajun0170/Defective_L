using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Weapon_Sword : Weapon
{
    [Header("Attack")]
    public int iDamage = 10;
    public float fSwingTime = 0.15f;     // 히트박스가 켜져 있을 시간
    public Collider2D cHitbox;           // isTrigger = true (자식 오브젝트의 콜라이더여도 OK)

    [Header("Debug Visual")]
    public bool bShowHitboxWhileSwing = true; // 공격 중에만 보이기
    public SpriteRenderer srHitboxVisual;     // 녹색 사각형 SpriteRenderer (없으면 자동 탐색)
    public bool bDrawGizmos = false;
    public bool bLog = true;

    float fTimer;
    HashSet<Collider2D> hsHit = new HashSet<Collider2D>();

    void Awake()
    {
        if (cHitbox) cHitbox.enabled = false;

        // 시각화 렌더러 자동 할당 (없으면 cHitbox와 같은 오브젝트에서 탐색)
        if (srHitboxVisual == null && cHitbox != null)
            srHitboxVisual = cHitbox.GetComponent<SpriteRenderer>();

        // 기본은 안 보이게
        if (srHitboxVisual) srHitboxVisual.enabled = false;
    }

    void OnEnable()
    {
        if (cHitbox) cHitbox.enabled = false;
        if (srHitboxVisual) srHitboxVisual.enabled = false;
    }

    void Update()
    {
        if (fTimer > 0f)
        {
            fTimer -= Time.deltaTime;
            if (fTimer <= 0f)
            {
                if (cHitbox) cHitbox.enabled = false;
                if (srHitboxVisual) srHitboxVisual.enabled = false;
                hsHit.Clear();
                if (bLog) Debug.Log("[Sword] Swing end");
            }
        }
    }

    public override bool CanUse() { return fTimer <= 0f; }

    public override void Use()
    {
        fTimer = fSwingTime;

        if (cHitbox) cHitbox.enabled = true;
        hsHit.Clear();

        // ✅ 공격 중에만 녹색 히트박스 잠깐 보이기
        if (bShowHitboxWhileSwing && srHitboxVisual) srHitboxVisual.enabled = true;

        if (bLog) Debug.Log($"[Weapon_Sword] Use() - hitbox {(cHitbox!=null && cHitbox.enabled)}");
    }

    // 자식 콜라이더에 맞아도 부모의 EnemyHealth(IDamageable)가 맞도록 InParent로 탐색
    void OnTriggerEnter2D(Collider2D other)
    {
        if (cHitbox == null || !cHitbox.enabled) return;

        // 자기 자신/소유자 제외
        if (tOwner && (other.transform == tOwner || other.transform.IsChildOf(tOwner)))
            return;

        if (hsHit.Contains(other)) return;

        // 🔑 핵심: GetComponentInParent 로 부모까지 탐색 (자식 히트 파츠에 맞아도 OK)
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null)
        {
            Vector2 dir = tOwner
                ? ((Vector2)other.transform.position - (Vector2)tOwner.position).normalized
                : Vector2.right;

            d.TakeDamage(iDamage, dir);
            hsHit.Add(other);

            if (bLog) Debug.Log($"[Sword] Hit {other.name}, dmg={iDamage}");
        }
        else
        {
            if (bLog) Debug.Log($"[Sword] Trigger {other.name} (no IDamageable in parent chain)");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!bDrawGizmos || cHitbox == null) return;
        Gizmos.color = cHitbox.enabled ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.35f);
        if (cHitbox is CircleCollider2D circ)
        {
            Vector3 c = circ.transform.TransformPoint((Vector3)circ.offset);
            float r = circ.radius * Mathf.Max(circ.transform.lossyScale.x, circ.transform.lossyScale.y);
            Gizmos.DrawSphere(c, r);
        }
        else
        {
            Bounds b = cHitbox.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
