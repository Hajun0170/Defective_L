using UnityEngine;

public class WeaponUnlockHandler : MonoBehaviour
{
    public Archive cArchive;
    public ArmamentController cArms;

    void Awake()
    {
        cArchive.OnWeaponUnlocked += HandleUnlocked;
    }

    void HandleUnlocked(WeaponId id)
    {
        var prefab = WeaponRegistry.Instance.GetPrefab(id);
        if (!prefab) return;

        switch (prefab.ePreferredSlot)
        {
            case EWeaponSlot.Right:
                if (cArms != null) cArms.EquipRight(prefab);
                break;
            case EWeaponSlot.Left:
                if (cArms != null) cArms.EquipLeft(prefab);
                break;
        }
    }
}
