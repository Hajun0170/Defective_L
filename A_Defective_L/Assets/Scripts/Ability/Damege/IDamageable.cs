using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int iAmount, Vector2 v2KnockDir);
}
