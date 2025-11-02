using UnityEngine;

public class Weapon_Sword : Weapon
{
    public int iDamage = 10;
    public float fSwingTime = 0.15f;
    public Collider2D cHitbox;

    float fTimer;

    void Update(){
        if (fTimer>0f){
            fTimer -= Time.deltaTime;
            if (fTimer<=0f) cHitbox.enabled = false;
        }
    }

    public override bool CanUse(){ return fTimer<=0f; }

    public override void Use(){
        fTimer = fSwingTime;
        cHitbox.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other){
        if (!cHitbox.enabled) return;
        var d = other.GetComponent<IDamageable>();
        if (d!=null){
            Vector2 dir = tOwner? ((Vector2)other.transform.position - (Vector2)tOwner.position).normalized : Vector2.right;
            d.TakeDamage(iDamage, dir);
        }
    }
}
