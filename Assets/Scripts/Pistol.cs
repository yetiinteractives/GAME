using UnityEngine;

public class Pistol : Weapon
{
    [Header("Pistol Specific")]
    [SerializeField] private float headshotMultiplier = 2f;

    protected override void Awake()
    {
        base.Awake();
        SetWeaponIndex(1); // Pistol is weapon 1
    }

    protected override void ProcessShot(RaycastHit hit, float calculatedDamage)
    {
        
        bool isHeadshot = hit.collider.CompareTag("Head");
        float finalDamage = isHeadshot ? calculatedDamage * headshotMultiplier : calculatedDamage;

        // Call base processing with final damage
        base.ProcessShot(hit, finalDamage);

        Debug.Log($"Pistol shot! Distance: {hit.distance:F1}m, " +
                 $"Base Damage: {baseDamage}, " +
                 $"Final Damage: {finalDamage:F1}, " +
                 $"Headshot: {isHeadshot}");
    }

    protected override float CalculateDamageBasedOnDistance(float hitDistance)
    {
        // Pistol: accurate with moderate range falloff
        if (hitDistance <= effectiveRange)
        {
            return baseDamage;
        }
        else if (hitDistance >= maxRange)
        {
            return baseDamage * 0.3f; // 30% damage at max range for pistol
        }
        else
        {
            float falloffPercent = (hitDistance - effectiveRange) / (maxRange - effectiveRange);
            return baseDamage * Mathf.Lerp(1f, 0.3f, falloffPercent);
        }
    }
}