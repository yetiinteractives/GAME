using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimLayerController : MonoBehaviour
{
    public Animator animator;
    public Rig pistolAimRig;
    public Rig shotgunAimRig;
    public Rig shotgunIdleRig;

    public float blendSpeed = 5f;

    private float targetWeight = 0f;
    private int pistolAimLayerIndex;
    private int shotgunAimLayerIndex;
    private int shotgunIdleLayerIndex;
    private float aimRigWeight;
    private float shotgunAimRigWeight;
    private float shotgunIdleRigWeight;

    private int currentWeaponIndex = 1;

    void Start()
    {
        pistolAimLayerIndex = animator.GetLayerIndex("Pistol Aim Layer");
        shotgunAimLayerIndex = animator.GetLayerIndex("Shotgun Aim Layer");
        shotgunIdleLayerIndex = animator.GetLayerIndex("Shotgun Idle Layer");

        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;

        // Initialize rig weights
        pistolAimRig.weight = 0f;
        shotgunAimRig.weight = 0f;
        shotgunIdleRig.weight = 0f;
    }

    private void HandleWeaponSwitch(int weaponIndex)
    {
        // Reset all weights when switching weapons
        if (currentWeaponIndex != weaponIndex)
        {
            // Reset pistol
            animator.SetLayerWeight(pistolAimLayerIndex, 0f);
            pistolAimRig.weight = 0f;
            aimRigWeight = 0f;

            // Reset shotgun
            animator.SetLayerWeight(shotgunAimLayerIndex, 0f);
            animator.SetLayerWeight(shotgunIdleLayerIndex, 0f);
            shotgunAimRig.weight = 0f;
            shotgunIdleRig.weight = 0f;
            shotgunAimRigWeight = 0f;
            shotgunIdleRigWeight = 0f;
        }

        currentWeaponIndex = weaponIndex;
    }

    void Update()
    {
        if (currentWeaponIndex == 1) // Pistol
        {
            // Right Mouse Button
            if (Input.GetMouseButton(1))
            {
                // Pistol Aiming
                targetWeight = 0.85f;
                StartCoroutine(AimRigWeightDelay());
            }
            else
            {
                // Pistol Not Aiming
                targetWeight = 0f;
                aimRigWeight = 0f;
            }

            // For that combined animation 
            float currentWeight = animator.GetLayerWeight(pistolAimLayerIndex);
            float newWeight = Mathf.Lerp(
                currentWeight,
                targetWeight,
                Time.deltaTime * blendSpeed
            );

            animator.SetLayerWeight(pistolAimLayerIndex, newWeight);

            // For that procedural aim rig   
            pistolAimRig.weight = Mathf.Lerp(
                pistolAimRig.weight,
                aimRigWeight,
                Time.deltaTime * 25f
            );
        }
        else if (currentWeaponIndex == 2) // Shotgun
        {
            // Right Mouse Button
            if (Input.GetMouseButton(1))
            {
                // Shotgun Aiming
                // Animation Layers
                animator.SetLayerWeight(shotgunAimLayerIndex,
                    Mathf.Lerp(
                        animator.GetLayerWeight(shotgunAimLayerIndex),
                        1f,
                        Time.deltaTime * blendSpeed
                    ));

                animator.SetLayerWeight(shotgunIdleLayerIndex,
                    Mathf.Lerp(
                        animator.GetLayerWeight(shotgunIdleLayerIndex),
                        0f,
                        Time.deltaTime * blendSpeed
                    ));

                // Rig Weights
                shotgunAimRigWeight = 1f;
                shotgunIdleRigWeight = 0f;
            }
            else
            {
                // Shotgun Not Aiming (Idle)
                // Animation Layers
                animator.SetLayerWeight(shotgunAimLayerIndex,
                    Mathf.Lerp(
                        animator.GetLayerWeight(shotgunAimLayerIndex),
                        0f,
                        Time.deltaTime * blendSpeed
                    ));

                animator.SetLayerWeight(shotgunIdleLayerIndex,
                    Mathf.Lerp(
                        animator.GetLayerWeight(shotgunIdleLayerIndex),
                        1f,
                        Time.deltaTime * blendSpeed
                    ));

                // Rig Weights
                shotgunAimRigWeight = 0f;
                shotgunIdleRigWeight = 1f;
            }

            // Apply rig weights with smoothing
            shotgunAimRig.weight = Mathf.Lerp(
                shotgunAimRig.weight,
                shotgunAimRigWeight,
                Time.deltaTime * 25f
            );

            shotgunIdleRig.weight = Mathf.Lerp(
                shotgunIdleRig.weight,
                shotgunIdleRigWeight,
                Time.deltaTime * 25f
            );
        }
    }

    private IEnumerator AimRigWeightDelay()
    {
        yield return new WaitForSeconds(0.25f);
        aimRigWeight = 1f;
    }

    private void OnDestroy()
    {
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
    }
}