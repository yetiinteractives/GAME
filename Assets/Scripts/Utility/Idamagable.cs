using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    bool IsDead();

    void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse);
}