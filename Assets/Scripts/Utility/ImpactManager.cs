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

        GameObject prefab = GetImpactPrefab(gunType, surfaceType);

        // SAFETY CHECK
        if (prefab == null)
        {
            Debug.LogWarning($"No impact prefab found for GunType: {gunType} and SurfaceType: {surfaceType}");
            return;
        }
           

        Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
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
            default: return pistolConcrete;
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
            default: return shotgunConcrete;
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
            default: return sniperConcrete;
        }
    }
}
