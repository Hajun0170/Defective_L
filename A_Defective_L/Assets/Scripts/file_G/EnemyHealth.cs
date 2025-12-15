using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;

    private void Start() => currentHealth = maxHealth;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"💥 {gameObject.name} 피격! 데미지: {damage}, 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 처치됨!");
            Destroy(gameObject);
        }
    }
}