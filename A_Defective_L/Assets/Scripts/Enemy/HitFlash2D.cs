using System.Collections;
using UnityEngine;

/// <summary>
/// 자식까지 포함한 모든 SpriteRenderer에 깜빡임 연출을 적용.
/// EnemyHealth 등에서 Flash()만 호출해 사용.
/// </summary>
[DisallowMultipleComponent]
public class HitFlash2D : MonoBehaviour
{
    [Header("Flash")]
    public Color cFlashColor = Color.white;
    public float fFlashDuration = 0.15f; // 1번 깜빡임(켜짐+꺼짐) 총 시간
    public int iFlashes = 2;             // 깜빡임 횟수

    SpriteRenderer[] aSr;
    Color[] aOrig;
    Coroutine co;

    void Awake()
    {
        aSr = GetComponentsInChildren<SpriteRenderer>(true);
        aOrig = new Color[aSr.Length];
        for (int i = 0; i < aSr.Length; i++) aOrig[i] = aSr[i].color;
    }

    public void Flash()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoFlash());
    }

    IEnumerator CoFlash()
    {
        // 색을 저장(실시간 변경에 대비)
        for (int i = 0; i < aSr.Length; i++) aOrig[i] = aSr[i].color;

        for (int k = 0; k < iFlashes; k++)
        {
            // 켜짐
            for (int i = 0; i < aSr.Length; i++) aSr[i].color = cFlashColor;
            yield return new WaitForSeconds(fFlashDuration * 0.5f);

            // 꺼짐 (원래 색 복원)
            for (int i = 0; i < aSr.Length; i++) if (aSr[i]) aSr[i].color = aOrig[i];
            yield return new WaitForSeconds(fFlashDuration * 0.5f);
        }
        co = null;
    }
}
