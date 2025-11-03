//해금 이벤트
using System;
using System.Collections.Generic;
using UnityEngine;

public class Archive : MonoBehaviour
{
    // 해금 무기 집합 & 재료 수량
    public HashSet<WeaponId> setOwned = new();
    public Dictionary<WeaponId,int> dictMat = new();

    // 해금/재료 변경 알림
    public event Action<WeaponId> OnWeaponUnlocked;
    public event Action<WeaponId,int> OnMaterialChanged;

    public bool Has(WeaponId id) => setOwned.Contains(id);

    public void AddWeapon(WeaponId id){
        if (setOwned.Add(id)){
            OnWeaponUnlocked?.Invoke(id);
            // TODO: 세이브에 반영
        }
    }

    public int GetMaterial(WeaponId id){
        return dictMat.TryGetValue(id, out var v) ? v : 0;
    }
    public void AddMaterial(WeaponId id, int iAdd){
        int cur = GetMaterial(id) + Mathf.Max(0, iAdd);
        dictMat[id] = cur;
        OnMaterialChanged?.Invoke(id, cur);
        // TODO: 세이브에 반영
    }
}
