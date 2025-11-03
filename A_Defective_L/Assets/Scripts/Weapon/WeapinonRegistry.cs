using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponEntry
{
    public WeaponId eId;
    public Weapon pPrefab; // Weapon 파생 프리팹
}

public class WeaponRegistry : MonoBehaviour
{
    public static WeaponRegistry Instance { get; private set; }
    public List<WeaponEntry> aEntries = new();
    Dictionary<WeaponId, Weapon> _dict;

    void Awake(){
        if (Instance!=null){ Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        _dict = new Dictionary<WeaponId, Weapon>();
        foreach(var e in aEntries){ if(!_dict.ContainsKey(e.eId) && e.pPrefab) _dict.Add(e.eId, e.pPrefab); }
    }

    public Weapon GetPrefab(WeaponId id){
        return (_dict!=null && _dict.TryGetValue(id, out var p))? p : null;
    }
}
