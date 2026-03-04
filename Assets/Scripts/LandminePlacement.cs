using UnityEngine;

public class LandminePlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject landminePrefab;

    [Header("Placement")]
    [SerializeField] private float forwardDistance = 2f;
    [SerializeField] private float rayHeight = 2f;
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField]private FreeLookADS freeLookAds;



    private void Update()
    {

        if (Input.GetMouseButton(1))
        {
            freeLookAds.SetADSState();
        }
        else
        {
            freeLookAds.SetNormalState();
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceMine();
        }
    }

    private void PlaceMine()
    {
        // Position in front of player
        Vector3 forwardPos = transform.position + transform.forward * forwardDistance;

        // Start ray slightly above
        Vector3 rayStart = forwardPos + Vector3.up * rayHeight;

        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, groundMask))
        {
            Vector3 spawnPos = hit.point + hit.normal * 0.02f;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Instantiate(landminePrefab, spawnPos, rotation);
        }
    }
}