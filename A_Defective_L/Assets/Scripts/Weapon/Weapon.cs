using UnityEngine;

public enum EWeaponSlot { Right, Left }
public abstract class Weapon : MonoBehaviour
{
    [Header("Mount")]
    public Vector3 vLocalPos;
    public Vector3 vLocalEuler;
    public Vector3 vLocalScale = Vector3.one; // (원한다면 사용할 기본 로컬 스케일)

    [Header("Meta")]
    public EWeaponSlot ePreferredSlot = EWeaponSlot.Right;
     
      [Header("Scale Control")]
    public bool bKeepPrefabScale = true; // ✅ 프리팹 크기 고정할지 여부
    Vector3 vAuthorWorldScale;           // 프리팹(부모 없을 때) 기준 월드 스케일 저장
    Vector3 vAuthorLocalScale;           // 프리팹 작성 시 로컬 스케일(참고)
    protected Transform tOwner, tMuzzle;

    
    // ArmamentController에서 Instantiate 직후, 부모 붙이기 전 호출됨
    public virtual void Init(Transform tOwner)
    {
        this.tOwner = tOwner;
        tMuzzle = transform.Find("Muzzle");

        // ✅ 부모 없을 때의 원래 스케일(= 프리팹 보이는 크기) 저장
        vAuthorWorldScale = transform.lossyScale;
        vAuthorLocalScale = transform.localScale;
    }
    public virtual void OnEquipped(Transform tSocket)
    {
        // 부모 스케일 영향을 상쇄하려면 worldPositionStays=false로 붙인 뒤 보정
        transform.SetParent(tSocket, false);

        // 위치/회전 먼저 맞추고
        transform.localPosition    = vLocalPos;
        transform.localEulerAngles = vLocalEuler;

        if (bKeepPrefabScale)
        {
            // ✅ 프리팹 당시 보이는 크기 유지
            SetWorldScaleTo(transform, vAuthorWorldScale);
        }
        else
        {
            // 또는 고정 로컬 스케일 사용(부모 1,1,1 전제일 때)
            transform.localScale = vLocalScale;
        }
    }
    
     // 부모 lossyScale을 나눠서 월드 스케일을 맞춰주는 유틸
      protected void SetWorldScaleTo(Transform t, Vector3 targetWorldScale)
    {
        var p = t.parent;
        if (p == null) { t.localScale = targetWorldScale; return; }

        Vector3 pls = p.lossyScale; // 부모의 실제(부호 포함) 스케일
        // 부모 영향을 나눠서 로컬 스케일 계산 (부호 보존)
        t.localScale = new Vector3(
            pls.x != 0 ? targetWorldScale.x / pls.x : 1f,
            pls.y != 0 ? targetWorldScale.y / pls.y : 1f,
            1f
        );
    }
    public virtual void OnUnequipped() { transform.SetParent(null); }

    public abstract bool CanUse();
    public abstract void Use();
}
