using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int iAmount, Vector2 vHitDir);
    bool IsDead { get; }
}
