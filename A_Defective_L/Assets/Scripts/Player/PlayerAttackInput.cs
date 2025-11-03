using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAttackInput : MonoBehaviour
{
    [Header("Input")]
    public KeyCode kcAttack = KeyCode.Z;

    [Header("References")]
    public Weapon wEquipped;
    public Transform tAttackSocket;

    Animator aPlayerAnimator;

    void Awake()
    {
        aPlayerAnimator = GetComponent<Animator>();

        if (wEquipped == null)
            wEquipped = GetComponentInChildren<Weapon>(true);

        if (wEquipped != null)
        {
            wEquipped.Init(transform);
            if (tAttackSocket != null)
                wEquipped.OnEquipped(tAttackSocket);
        }
    }

    void Update()
    {
        if (wEquipped == null) return;

        if (Input.GetKeyDown(kcAttack))
        {
            Debug.Log("[PlayerAttackInput] Attack key");
            if (wEquipped.CanUse())
            {
                Debug.Log("[PlayerAttackInput] Use()");
                wEquipped.Use();
            }
            else
            {
                Debug.Log("[PlayerAttackInput] Cooldown");
            }
        }
    }

    public void SetEquippedWeapon(Weapon w, Transform tSocket = null)
    {
        wEquipped = w;
        if (wEquipped == null) return;

        wEquipped.Init(transform);
        if (tSocket != null) wEquipped.OnEquipped(tSocket);
    }
}
