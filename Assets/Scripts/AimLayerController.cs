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

    // NEW: reload gate
    private bool isReloading = false;

    void Start()
    {
        pistolAimLayerIndex = animator.GetLayerIndex("Pistol Aim Layer");
        shotgunAimLayerIndex = animator.GetLayerIndex("Shotgun Aim Layer");
        shotgunIdleLayerIndex = animator.GetLayerIndex("Shotgun Idle Layer");

        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;

        pistolAimRig.weight = 0f;
        shotgunAimRig.weight = 0f;
        shotgunIdleRig.weight = 0f;
    }

    
    public void SetReloading(bool reloading)
    {
        isReloading = reloading;

        // Only matters for shotgun/sniper since they share rigs
        if (currentWeaponIndex == 2 || currentWeaponIndex == 3)
        {
            if (isReloading)
            {
                // Force disable idle rig so reload animation can play cleanly
                shotgunIdleRigWeight = 0f;
                shotgunIdleRig.weight = 0f;
            }
        }
    }

    private void HandleWeaponSwitch(int weaponIndex)
    {
        if (currentWeaponIndex != weaponIndex)
        {
            animator.SetLayerWeight(pistolAimLayerIndex, 0f);
            pistolAimRig.weight = 0f;
            aimRigWeight = 0f;

            animator.SetLayerWeight(shotgunAimLayerIndex, 0f);
            animator.SetLayerWeight(shotgunIdleLayerIndex, 0f);
            shotgunAimRig.weight = 0f;
            shotgunIdleRig.weight = 0f;
            shotgunAimRigWeight = 0f;
            shotgunIdleRigWeight = 0f;

           
            isReloading = false;
        }

        currentWeaponIndex = weaponIndex;
    }

    void Update()
    {
        if (currentWeaponIndex == 1) // Pistol
        {
            if (GameInput.Aim)
            {
                targetWeight = 0.85f;
                StartCoroutine(AimRigWeightDelay());
            }
            else
            {
                targetWeight = 0f;
                aimRigWeight = 0f;
            }

            float currentWeight = animator.GetLayerWeight(pistolAimLayerIndex);
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * blendSpeed);
            animator.SetLayerWeight(pistolAimLayerIndex, newWeight);

            pistolAimRig.weight = Mathf.Lerp(pistolAimRig.weight, aimRigWeight, Time.deltaTime * 25f);
        }
        else if (currentWeaponIndex == 2 || currentWeaponIndex == 3) // Shotgun or Sniper
        {
           
            if (isReloading)
            {
                shotgunIdleRigWeight = 0f;
                shotgunIdleRig.weight = 0f;

                
                shotgunAimRig.weight = Mathf.Lerp(
                    shotgunAimRig.weight,
                    shotgunAimRigWeight,
                    Time.deltaTime * 25f
                );

                return;
            }

            if (GameInput.Aim)
            {
                animator.SetLayerWeight(shotgunAimLayerIndex,
                    Mathf.Lerp(animator.GetLayerWeight(shotgunAimLayerIndex), 1f, Time.deltaTime * blendSpeed));

                animator.SetLayerWeight(shotgunIdleLayerIndex,
                    Mathf.Lerp(animator.GetLayerWeight(shotgunIdleLayerIndex), 0f, Time.deltaTime * blendSpeed));

                shotgunAimRigWeight = 0.75f;
                shotgunIdleRigWeight = 0f;
            }
            else
            {
                animator.SetLayerWeight(shotgunAimLayerIndex,
                    Mathf.Lerp(animator.GetLayerWeight(shotgunAimLayerIndex), 0f, Time.deltaTime * blendSpeed));

                animator.SetLayerWeight(shotgunIdleLayerIndex,
                    Mathf.Lerp(animator.GetLayerWeight(shotgunIdleLayerIndex), 1f, Time.deltaTime * blendSpeed));

                shotgunAimRigWeight = 0f;
                shotgunIdleRigWeight = 1f;
            }

            shotgunAimRig.weight = Mathf.Lerp(shotgunAimRig.weight, shotgunAimRigWeight, Time.deltaTime * 25f);
            shotgunIdleRig.weight = Mathf.Lerp(shotgunIdleRig.weight, shotgunIdleRigWeight, Time.deltaTime * 25f);
        }
    }

    private IEnumerator AimRigWeightDelay()
    {
        yield return new WaitForSeconds(0.25f);
        aimRigWeight = 0.75f;
    }

    private void OnDestroy()
    {
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
    }
}