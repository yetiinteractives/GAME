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
    public GameObject pistolFlesh; // particles-only preferred

    [Header("SHOTGUN IMPACTS")]
    public GameObject shotgunConcrete;
    public GameObject shotgunMetal;
    public GameObject shotgunWood;
    public GameObject shotgunFlesh; // particles-only preferred

    [Header("SNIPER IMPACTS")]
    public GameObject sniperConcrete;
    public GameObject sniperMetal;
    public GameObject sniperWood;
    public GameObject sniperFlesh; // particles-only preferred

    [Header("Flesh Decal (World)")]
    [Tooltip("Decal-only prefab. Spawned at hit point and NOT parented to zombie.")]
    [SerializeField] private GameObject bloodDecalPrefab;
    [SerializeField] private float bloodDecalLifetime = 25f;
    [SerializeField] private float decalSurfaceOffset = 0.005f;

    [Header("Lifetimes")]
    [SerializeField] private float fleshImpactLifetime = 0.5f;
    [SerializeField] private float pistolImpactLifetime = 4f;
    [SerializeField] private float shotgunImpactLifetime = 6f;
    [SerializeField] private float sniperImpactLifetime = 8f;

    public void SpawnImpact(GunTypeEnum gunType, RaycastHit hit)
    {
        if (hit.collider == null) return;

        Surface surface = hit.collider.GetComponentInParent<Surface>();
        SurfaceTypeEnum surfaceType = surface != null ? surface.surfaceType : SurfaceTypeEnum.Default;

        if (surfaceType == SurfaceTypeEnum.Default)
            return;

        GameObject prefab = GetImpactPrefab(gunType, surfaceType);
        if (prefab == null)
        {
            Debug.LogWarning($"No impact prefab found for GunType: {gunType} and SurfaceType: {surfaceType}");
            return;
        }

        Vector3 spawnPos = hit.point + hit.normal * 0.01f;
        Quaternion spawnRot = Quaternion.LookRotation(-hit.normal);

        GameObject impact = Instantiate(prefab, spawnPos, spawnRot);

        float sizeMultiplier = GetSizeMultiplier(gunType);
        impact.transform.localScale *= sizeMultiplier;

        if (surfaceType == SurfaceTypeEnum.Flesh)
        {
            // Particles follow zombie movement
            impact.transform.SetParent(hit.collider.transform, true);
            Destroy(impact, fleshImpactLifetime);

            // Decal stays in world (NOT parented)
            SpawnWorldBloodDecal(hit);

            return;
        }

        Destroy(impact, GetLifetime(gunType, surfaceType));
    }

    private void SpawnWorldBloodDecal(RaycastHit hit)
    {
        if (bloodDecalPrefab == null) return;

        Vector3 decalPos = hit.point + hit.normal * decalSurfaceOffset;
        Quaternion decalRot = Quaternion.LookRotation(-hit.normal);

        GameObject decal = Instantiate(bloodDecalPrefab, decalPos, decalRot);
        Destroy(decal, bloodDecalLifetime);
    }

    private GameObject GetImpactPrefab(GunTypeEnum gunType, SurfaceTypeEnum surfaceType)
    {
        switch (gunType)
        {
            case GunTypeEnum.Pistol: return GetPistolImpact(surfaceType);
            case GunTypeEnum.Shotgun: return GetShotgunImpact(surfaceType);
            case GunTypeEnum.Sniper: return GetSniperImpact(surfaceType);
            default: return null;
        }
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
            return fleshImpactLifetime;

        switch (gunType)
        {
            case GunTypeEnum.Pistol: return pistolImpactLifetime;
            case GunTypeEnum.Shotgun: return shotgunImpactLifetime;
            case GunTypeEnum.Sniper: return sniperImpactLifetime;
            default: return pistolImpactLifetime;
        }
    }
}