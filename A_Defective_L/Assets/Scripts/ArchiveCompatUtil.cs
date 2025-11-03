using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Archive API가 프로젝트마다 다른 걸 흡수하는 호환 유틸.
/// - 소유 표시: SetHas(id,bool) / Unlock(id) / AddWeapon(id) 중 있는 걸 자동 호출
/// - 재료 지급: AddMaterial(id,int) 있으면 호출, 없으면 경고 로그
/// </summary>
public static class ArchiveCompatUtil
{
    public static void MarkOwned(object archive, object weaponId)
    {
        if (archive == null || weaponId == null) return;
        var t = archive.GetType();

        // 1) SetHas(WeaponId, bool)
        var m = t.GetMethod("SetHas", new Type[] { weaponId.GetType(), typeof(bool) });
        if (m != null) { m.Invoke(archive, new object[] { weaponId, true }); return; }

        // 2) Unlock(WeaponId)
        m = t.GetMethod("Unlock", new Type[] { weaponId.GetType() });
        if (m != null) { m.Invoke(archive, new object[] { weaponId }); return; }

        // 3) AddWeapon(WeaponId)
        m = t.GetMethod("AddWeapon", new Type[] { weaponId.GetType() });
        if (m != null) { m.Invoke(archive, new object[] { weaponId }); return; }

        Debug.LogWarning("[ArchiveCompat] No suitable method to mark weapon owned (tried SetHas/Unlock/AddWeapon).");
    }

    public static void AddMaterial(object archive, object weaponId, int amount)
    {
        if (archive == null || weaponId == null) return;
        var t = archive.GetType();

        // AddMaterial(WeaponId, int)
        var m = t.GetMethod("AddMaterial", new Type[] { weaponId.GetType(), typeof(int) });
        if (m != null) { m.Invoke(archive, new object[] { weaponId, amount }); return; }

        Debug.Log($"[ArchiveCompat] AddMaterial not implemented on Archive. ({weaponId}, +{amount})");
    }
}
