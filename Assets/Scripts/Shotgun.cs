using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Shotgun Specific")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float spreadAngle = 3f;
    [SerializeField] private float pelletDamage = 5f; // Each pellet does less damage

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
                // Create pellet impact effects (smaller than main bullet)
                CreatePelletImpact(pelletHit);

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

    private void CreatePelletImpact(RaycastHit hit)
    {
        // Smaller impact effect for pellets
        if (bulletImpactPrefab != null)
        {
            GameObject impact = Instantiate(bulletImpactPrefab, hit.point,
                Quaternion.LookRotation(hit.normal));
            impact.transform.localScale *= .75f; // Smaller scale for pellets
            Destroy(impact, 1f);
        }

        // Smaller bullet hole for pellets
        if (bulleteHolePrefab != null && ShouldCreateBulletHole(hit.collider))
        {
            GameObject hole = Instantiate(bulleteHolePrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(-hit.normal));
            hole.transform.localScale *= 0.75f;
            Destroy(hole, 15f);
        }
    }

    private bool ShouldCreateBulletHole(Collider collider)
    {
        // Don't create bullet holes on characters
        return !collider.CompareTag("Player") && !collider.CompareTag("Enemy");
    }

    private void DamageTarget(RaycastHit hit, float damageAmount)
    {
        
    }
}