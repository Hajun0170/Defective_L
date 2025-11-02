using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Mount")]
    public Vector3 vLocalPos, vLocalEuler, vLocalScale = Vector3.one;

    protected Transform tOwner, tMuzzle;

    public virtual void Init(Transform tOwner){
        this.tOwner = tOwner;
        tMuzzle = transform.Find("Muzzle");
    }
    public virtual void OnEquipped(Transform tSocket){
        transform.SetParent(tSocket, false);
        transform.localPosition = vLocalPos;
        transform.localEulerAngles = vLocalEuler;
        transform.localScale = vLocalScale;
    }
    public virtual void OnUnequipped(){ transform.SetParent(null); }

    public abstract bool CanUse();
    public abstract void Use();
}
