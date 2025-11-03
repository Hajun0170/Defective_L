using UnityEngine;

public class ArmamentController : MonoBehaviour
{
    [Header("Sockets")]
    public Transform tRightArm_socket;
    public Transform tLeftArm_socket;

    [Header("Refs")]
    public PlayerAttackInput iAttackInput;

    [Header("Current")]
    public Weapon wRight;
    public Weapon wLeft;

    /// <summary>
    /// 프리팹(또는 씬 오브젝트)을 장착한다. 픽업 시 이 함수만 호출해주면
    /// - Init/OnEquipped
    /// - PlayerAttackInput.SetEquippedWeapon
    /// - 무기 루트 Kinematic Rigidbody2D 보장
    /// 를 한 번에 처리한다.
    /// </summary>
    public Weapon Equip(Weapon wPrefabOrInstance)
    {
        if (wPrefabOrInstance == null) return null;

        // 프리팹이면 인스턴스 생성
        Weapon wNew = wPrefabOrInstance;
        if (!wNew.gameObject.scene.IsValid())
        {
            wNew = Instantiate(wPrefabOrInstance);
            wNew.name = wPrefabOrInstance.name;
        }

        // 소유자 주입
        wNew.Init(transform);

        bool bRight = (wNew.ePreferredSlot == EWeaponSlot.Right);
        Transform tSocket = bRight ? tRightArm_socket : tLeftArm_socket;

        // 기존 무기 정리(교체)
        if (bRight && wRight != null && wRight != wNew)
        {
            wRight.OnUnequipped();
            Destroy(wRight.gameObject);
            wRight = null;
        }
        else if (!bRight && wLeft != null && wLeft != wNew)
        {
            wLeft.OnUnequipped();
            Destroy(wLeft.gameObject);
            wLeft = null;
        }

        // 장착(부모/포즈/스케일 적용)
        wNew.OnEquipped(tSocket);

        // ⛏ 부모(무기 루트)에 Kinematic Rigidbody2D 보장 → 자식 트리거가 부모 스크립트로 이벤트를 올림
        EnsureKinematicRigid(wNew.gameObject);

        // 🔗 입력 시스템에 현재 무기 연결 (Z키가 여기서부터 먹음)
        if (iAttackInput != null)
            iAttackInput.SetEquippedWeapon(wNew, tSocket);

        // 현재 무기 저장
        if (bRight) wRight = wNew; else wLeft = wNew;

        Debug.Log($"[Armament] Equipped {wNew.name} on {(bRight ? "Right" : "Left")}");
        return wNew;
    }

    void EnsureKinematicRigid(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.simulated = true;
    }

    // ArmamentController.cs 내에 추가
public Weapon EquipRight(Weapon wPrefabOrInstance)
{
    if (wPrefabOrInstance == null) return null;
    wPrefabOrInstance.ePreferredSlot = EWeaponSlot.Right;
    return Equip(wPrefabOrInstance); // ← 아래 Equip(Weapon) 호출
}

public Weapon EquipLeft(Weapon wPrefabOrInstance)
{
    if (wPrefabOrInstance == null) return null;
    wPrefabOrInstance.ePreferredSlot = EWeaponSlot.Left;
    return Equip(wPrefabOrInstance); // ← 아래 Equip(Weapon) 호출
}
}
