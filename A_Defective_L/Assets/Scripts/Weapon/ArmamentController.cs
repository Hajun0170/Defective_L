using UnityEngine;

public class ArmamentController : MonoBehaviour
{
    [Header("Sockets")]
    public Transform tRightArmSocket; // 근접
    public Transform tLeftArmSocket;  // 원거리
    public Transform tSpriteRoot;

    [Header("Current")]
    public Weapon mRight;
    public Weapon mLeft;

    public void EquipRight(Weapon pPrefab){
        if (mRight){ mRight.OnUnequipped(); Destroy(mRight.gameObject); }
        mRight = Instantiate(pPrefab);
        mRight.Init(transform);
        mRight.OnEquipped(tRightArmSocket);
    }

    public void EquipLeft(Weapon pPrefab){
        if (mLeft){ mLeft.OnUnequipped(); Destroy(mLeft.gameObject); }
        mLeft = Instantiate(pPrefab);
        mLeft.Init(transform);
        mLeft.OnEquipped(tLeftArmSocket);
    }

    public void TryUseRight(){ if (mRight!=null && mRight.CanUse()) mRight.Use(); }
    public void TryUseLeft(){  if (mLeft!=null  && mLeft.CanUse())  mLeft.Use(); }

    public void SetFacing(int iDirX){
        if (iDirX==0) return;
        var v = tSpriteRoot.localScale;
        v.x = Mathf.Abs(v.x) * (iDirX>0?1f:-1f);
        tSpriteRoot.localScale = v;
    }
}
