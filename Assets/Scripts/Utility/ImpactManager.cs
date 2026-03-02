using UnityEngine;

public class ImpactManager : MonoBehaviour
{
    public static ImpactManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [Header("PISTOL IMPACTS")]
    public GameObject pistolConcrete;
    public GameObject pistolMetal;
    public GameObject pistolWood;
    public GameObject pistolFlesh;

    [Header("SHOTGUN IMPACTS")]
    public GameObject shotgunConcrete;
    public GameObject shotgunMetal;
    public GameObject shotgunWood;
    public GameObject shotgunFlesh;

    [Header("SNIPER IMPACTS")]
    public GameObject sniperConcrete;
    public GameObject sniperMetal;
    public GameObject sniperWood;
    public GameObject sniperFlesh;

    public void SpawnImpact(GunTypeEnum gunType, RaycastHit hit)
    {
        if (hit.collider == null) return;

        Surface surface = hit.collider.GetComponentInParent<Surface>();

        SurfaceTypeEnum surfaceType = surface != null
            ? surface.surfaceType
            : SurfaceTypeEnum.Default;

        // If Default, do not instantiate anything
        if (surfaceType == SurfaceTypeEnum.Default)
            return;

        GameObject prefab = GetImpactPrefab(gunType, surfaceType);

        if (prefab == null)
        {
            Debug.LogWarning($"No impact prefab found for GunType: {gunType} and SurfaceType: {surfaceType}");
            return;
        }

        GameObject impact = Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));

        // Apply size variation per gun
        float sizeMultiplier = GetSizeMultiplier(gunType);
        impact.transform.localScale *= sizeMultiplier;

        // Flesh special handling (BloodFXInstance first, fallback to particle timing)
        if (surfaceType == SurfaceTypeEnum.Flesh)
        {
            var bloodFx = impact.GetComponentInChildren<KnowerCoder.BloodFX.BloodFXInstance>();
            if (bloodFx != null)
            {
                bloodFx.Play();
                Destroy(impact, GetLifetime(gunType, surfaceType));
                return;
            }

            // fallback to old behavior
            ParticleSystem ps = impact.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(impact, totalDuration);
                return;
            }
        }

        // Non-flesh lifetime
        float lifetime = GetLifetime(gunType, surfaceType);
        Destroy(impact, lifetime);
    }

    private GameObject GetImpactPrefab(GunTypeEnum gunType, SurfaceTypeEnum surfaceType)
    {
        switch (gunType)
        {
            case GunTypeEnum.Pistol:
                return GetPistolImpact(surfaceType);

            case GunTypeEnum.Shotgun:
                return GetShotgunImpact(surfaceType);

            case GunTypeEnum.Sniper:
                return GetSniperImpact(surfaceType);
        }

        return null;
    }

    private GameObject GetPistolImpact(SurfaceTypeEnum type)
    {
        switch (type)
        {
            case SurfaceTypeEnum.Concrete: return pistolConcrete;
            case SurfaceTypeEnum.Metal: return pistolMetal;
            case SurfaceTypeEnum.Wood: return pistolWood;
            case SurfaceTypeEnum.Flesh: return pistolFlesh;
            default: return null;
        }
    }

    private GameObject GetShotgunImpact(SurfaceTypeEnum type)
    {
        switch (type)
        {
            case SurfaceTypeEnum.Concrete: return shotgunConcrete;
            case SurfaceTypeEnum.Metal: return shotgunMetal;
            case SurfaceTypeEnum.Wood: return shotgunWood;
            case SurfaceTypeEnum.Flesh: return shotgunFlesh;
            default: return null;
        }
    }

    private GameObject GetSniperImpact(SurfaceTypeEnum type)
    {
        switch (type)
        {
            case SurfaceTypeEnum.Concrete: return sniperConcrete;
            case SurfaceTypeEnum.Metal: return sniperMetal;
            case SurfaceTypeEnum.Wood: return sniperWood;
            case SurfaceTypeEnum.Flesh: return sniperFlesh;
            default: return null;
        }
    }

    private float GetSizeMultiplier(GunTypeEnum gunType)
    {
        switch (gunType)
        {
            case GunTypeEnum.Pistol: return 1f;
            case GunTypeEnum.Shotgun: return 0.6f;
            case GunTypeEnum.Sniper: return 1.8f;
            default: return 1f;
        }
    }

    private float GetLifetime(GunTypeEnum gunType, SurfaceTypeEnum surfaceType)
    {
        if (surfaceType == SurfaceTypeEnum.Flesh)
            return 1.2f;

        switch (gunType)
        {
            case GunTypeEnum.Pistol: return 10f;
            case GunTypeEnum.Shotgun: return 20f;
            case GunTypeEnum.Sniper: return 25f;
            default: return 10f;
        }
    }
}