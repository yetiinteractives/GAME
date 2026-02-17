using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Shotgun Specific")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float spreadAngle = 3f;
    [SerializeField] private int pelletDamage = 5; // Each pellet does less damage

    protected override void OnShoot(RaycastHit hit)
    {
        // Get camera position and forward direction
        Camera mainCamera = Camera.main;
        Vector3 shotOrigin = mainCamera.transform.position;

        // Fire multiple pellets
        for (int i = 0; i < pelletCount; i++)
        {
            // Calculate spread for this pellet
            Vector3 spreadDirection = GetSpreadDirection(mainCamera.transform.forward);

            // Raycast for this pellet
            Ray pelletRay = new Ray(shotOrigin, spreadDirection);
            if (Physics.Raycast(pelletRay, out RaycastHit pelletHit, Mathf.Infinity))
            {
                if (ImpactManager.Instance != null)
                {
                    ImpactManager.Instance.SpawnImpact(gunType, pelletHit);
                }


                // Apply damage if target has health component
                DamageTarget(pelletHit, pelletDamage);
            }
        }
    }

    private Vector3 GetSpreadDirection(Vector3 baseDirection)
    {
        // Random spread within the cone
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);

        // Apply spread to direction
        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
        return spreadRotation * baseDirection;
    }

   

    

    private void DamageTarget(RaycastHit hit, int damageAmount)
    {
        ApplyDamage(hit, damageAmount);
    }
}