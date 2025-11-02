using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public int iMaxHp = 10;
    public int iHp;

    [Header("I-Frame")]
    public bool bInvincible;
    public float fHurtIFrameSeconds = 0.6f;

    public bool IsDead => iHp <= 0;

    void Awake(){ iHp = iMaxHp; }

    public void TakeDamage(int iAmount, Vector2 vHitDir){
        if (bInvincible || IsDead) return;
        iHp = Mathf.Max(0, iHp - iAmount);
        if (iHp == 0){ /* TODO: Death */ }
        else { StartCoroutine(CoIFrame()); }
        // TODO: HP UI 갱신
    }

    IEnumerator CoIFrame(){
        bInvincible = true;
        yield return new WaitForSeconds(fHurtIFrameSeconds);
        bInvincible = false;
    }
}
