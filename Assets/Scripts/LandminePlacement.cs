using System;
using System.Collections;
using UnityEngine;

public class LandminePlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject landminePrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private AimLayerController aimLayerController;
    [SerializeField] private FreeLookADS freeLookAds;

    [Header("Placement")]
    [SerializeField] private float forwardDistance = 2f;
    [SerializeField] private float rayHeight = 2f;
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Animation Timing")]
    [SerializeField] private float animationStartDelay = 0.05f;
    [SerializeField] private float landmineSpawnDelay = 0.6f;
    [SerializeField] private float animationResetDelay = 0.3f; // Time to hold before resetting layer

    private bool isWeaponSwitchUIOn;
    private bool isPlacing = false;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();

        if (aimLayerController == null)
        {
            aimLayerController = GetComponentInParent<AimLayerController>();
        }
    }

    private void Start()
    {
        SwitchWeapons.OnToggleWeaponSwitchUI += isWeaponSwitchUIActive;
    }

    private void isWeaponSwitchUIActive(bool isActive)
    {
        isWeaponSwitchUIOn = isActive;
    }

    private void Update()
    {
        // Prevent input during placement animation
        if (isPlacing) return;

        if (Input.GetMouseButton(1))
        {
            if (freeLookAds != null)
            {
                freeLookAds.SetADSState();
            }
        }
        else
        {
            if (freeLookAds != null)
            {
                freeLookAds.SetNormalState();
            }
        }

        if (Input.GetMouseButtonDown(0) && !isWeaponSwitchUIOn)
        {
            StartCoroutine(PlaceLandmineCoroutine());
        }
    }

    private IEnumerator PlaceLandmineCoroutine()
    {
        isPlacing = true;

        
        if (aimLayerController != null)
        {
            aimLayerController.ForceDisableAllRigs();
        }

        // Small delay before starting animation
        yield return new WaitForSeconds(animationStartDelay);

        
        if (aimLayerController != null)
        {
            aimLayerController.SetLandmineLayerWeight(1f);
        }
        freeLookAds.SetADSState();
        
        

        
        yield return new WaitForSeconds(landmineSpawnDelay);

        
        PlaceMine();

        
        yield return new WaitForSeconds(animationResetDelay);
        freeLookAds.SetNormalState();


        
        if (aimLayerController != null)
        {
            aimLayerController.SetLandmineLayerWeight(0f);
        }

        isPlacing = false;
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

    private void OnDestroy()
    {
        SwitchWeapons.OnToggleWeaponSwitchUI -= isWeaponSwitchUIActive;
    }
}