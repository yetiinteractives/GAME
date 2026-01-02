using System.Collections;
using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Shotgun Specific")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private float pelletBaseDamage = 5f;

    protected override void Awake()
    {
        base.Awake();
        SetWeaponIndex(2); // Shotgun is weapon 2
    }

    protected override void ProcessShot(RaycastHit hit, float calculatedDamage)
    {
        // Shotgun fires multiple pellets in a spread pattern
        Vector3 hitDirection = (hit.point - muzzleTransform.position).normalized;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 pelletDirection = GetPelletDirection(hitDirection);
            FirePellet(pelletDirection, hit.point);
        }

        // Call base processing (ammo reduction, sounds, etc.)
        base.ProcessShot(hit, calculatedDamage);

        Debug.Log($"Shotgun blast! {pelletCount} pellets fired");
    }

    protected override float CalculateDamageBasedOnDistance(float hitDistance)
    {
        // Shotgun: Severe damage falloff beyond effective range
        if (hitDistance <= effectiveRange)
        {
            return pelletBaseDamage * pelletCount; // Full damage up close
        }
        else if (hitDistance >= maxRange)
        {
            return 0f; // No damage at max range for shotgun
        }
        else
        {
            // Exponential falloff for shotgun
            float falloffPercent = (hitDistance - effectiveRange) / (maxRange - effectiveRange);
            float damageMultiplier = Mathf.Pow(1f - falloffPercent, 2f); // Quadratic falloff
            return pelletBaseDamage * pelletCount * damageMultiplier;
        }
    }

    private Vector3 GetPelletDirection(Vector3 baseDirection)
    {
        // Calculate random spread within spread angle
        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        Vector3 axis = Vector3.Cross(baseDirection, Vector3.up).normalized;
        if (axis.magnitude < 0.1f) axis = Vector3.Cross(baseDirection, Vector3.forward).normalized;

        return Quaternion.AngleAxis(randomAngle, axis) * baseDirection;
    }

    private void FirePellet(Vector3 direction, Vector3 targetPoint)
    {
        // Optional: Create visual pellet effect
        if (bulletPrefab && muzzleTransform)
        {
            GameObject pellet = Instantiate(bulletPrefab, muzzleTransform.position, Quaternion.identity);
            StartCoroutine(MovePelletTowardsDirection(pellet, direction));
        }
    }

    private IEnumerator MovePelletTowardsDirection(GameObject pellet, Vector3 direction)
    {
        float travelTime = 0.15f;
        float elapsedTime = 0f;
        Vector3 startPosition = pellet.transform.position;
        Vector3 endPosition = startPosition + (direction * maxRange);

        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            pellet.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        Destroy(pellet);
    }

    protected override IEnumerator ReloadCoroutine()
    {
        // Shotgun can have different reload logic (shell by shell)
        // For now, use base reload
        return base.ReloadCoroutine();
    }
}