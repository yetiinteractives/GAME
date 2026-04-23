using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Shotgun Specific")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float spreadAngle = 3f;
    [SerializeField] private int pelletDamage = 5;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Pull absolute ammo state from manager when enabled
        if (ResourceManager.Instance != null)
            SetAmmoState(ResourceManager.Instance.ShotgunMagAmmo, ResourceManager.Instance.ShotgunAmmoCount);
    }

    protected override void Shoot(RaycastHit hit)
    {
        // base handles bulletOnMag--, events, VFX/SFX, timers
        base.Shoot(hit);

        // Sync manager with current LOCAL state (absolute, not consume/add deltas)
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.SetShotgunMagAmmo(BulletOnMag);
            ResourceManager.Instance.SetShotgunReserveAbsolute(TotalBullet);
        }
    }

    protected override void OnShoot(RaycastHit hit)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 shotOrigin = mainCamera.transform.position;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 spreadDirection = GetSpreadDirection(mainCamera.transform.forward);
            Ray pelletRay = new Ray(shotOrigin, spreadDirection);

            if (Physics.Raycast(pelletRay, out RaycastHit pelletHit, Mathf.Infinity))
            {
                if (ImpactManager.Instance != null)
                    ImpactManager.Instance.SpawnImpact(gunType, pelletHit);

                ApplyDamage(pelletHit, pelletDamage);
            }
        }
    }

    protected override System.Collections.IEnumerator Reload()
    {
        // keep base reload behavior
        yield return base.Reload();

        // after reload complete, mirror exact values to manager
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.SetShotgunMagAmmo(BulletOnMag);
            ResourceManager.Instance.SetShotgunReserveAbsolute(TotalBullet);
        }
    }

    private Vector3 GetSpreadDirection(Vector3 baseDirection)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
        return spreadRotation * baseDirection;
    }
}