using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    bool IsDead();

    // Called for lethal shot (ragdoll)
    void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse);

    // Called for non-lethal shot (partial ragdoll flinch)
    void ApplyHitReaction(Collider hitCollider, Vector3 hitPoint, Vector3 impulse, float duration);
}