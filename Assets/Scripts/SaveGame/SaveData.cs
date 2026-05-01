using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public string savedAtUtc;

    public string currentScene;

    public ResourceSaveData resources = new ResourceSaveData();
    public PlayerRuntimeSaveData player = new PlayerRuntimeSaveData();
    public LevelStateData levelState = new LevelStateData();
}

[Serializable]
public class PlayerRuntimeSaveData
{
    public float health = 100f;
    public Vector3 position = Vector3.zero;
    public Vector3 eulerRotation = Vector3.zero;
    public bool hasTransform = false;
}
[Serializable]
public class ResourceSaveData
{
    public int pistolAmmoCount;
    public int shotgunAmmoCount;
    public int sniperAmmoCount;

    // ADD THESE 3:
    public int pistolMagAmmo;
    public int shotgunMagAmmo;
    public int sniperMagAmmo;

    public int grenadeCount;
    public int landmineCount;

    public int medkitCount;
    public int bandageCount;
    public int shotgunShellCount;
    public int silencerCount;

    public int alcoholCount;
    public int ragCount;
    public int bindingCount;
    public int gunpowderCount;
    public int canCount;

    public bool isPistolSilencerEquipped;
    public int pistolSilencerDurability;
}

