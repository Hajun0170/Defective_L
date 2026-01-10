using UnityEngine;
public class ItemPickup : MonoBehaviour
{
    public Weapon weaponData; // 이 아이템이 무슨 무기인지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WeaponManager wm = collision.GetComponent<WeaponManager>();
            if (wm != null)
            {
                // 무기 획득 함수 호출
                wm.AddWeapon(weaponData); 
                Destroy(gameObject); // 아이템 사라짐
            }
        }
    }
}