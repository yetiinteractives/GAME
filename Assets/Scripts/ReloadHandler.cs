using System;
using System.Collections;
using UnityEngine;

public class ReloadHandler : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public static ReloadHandler Instance { get; private set; }

    private int currentWeaponIndex = 1;

    private int reloadLayerIndex;
    private Coroutine reloadCoroutine;

    // NEW: reference to AimLayerController (assign in inspector or auto-find)
    [SerializeField] private AimLayerController aimLayerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        animator = GetComponent<Animator>();
        reloadLayerIndex = animator.GetLayerIndex("Reload Layer");

        if (aimLayerController == null)
            aimLayerController = FindFirstObjectByType<AimLayerController>();

        SwitchWeapons.OnWeaponSwitch += WeaponSwitch;
    }

    public void HandleReload()
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);

        reloadCoroutine = StartCoroutine(ReloadAnimation());
    }

    IEnumerator ReloadAnimation()
    {
        // NEW: disable idle shotgun rig during reload when using shotgun/sniper
        if ((currentWeaponIndex == 2 || currentWeaponIndex == 3) && aimLayerController != null)
            aimLayerController.SetReloading(true);

        float lerpTime = 0.2f;
        float t = 0f;

        while (t < lerpTime)
        {
            t += Time.deltaTime;
            animator.SetLayerWeight(reloadLayerIndex, Mathf.Lerp(0f, 1f, t / lerpTime));
            yield return null;
        }

        animator.SetLayerWeight(reloadLayerIndex, 1f);
        animator.SetTrigger("Reload");

        yield return new WaitForSeconds(2f);

        t = 0f;

        while (t < lerpTime)
        {
            t += Time.deltaTime;
            animator.SetLayerWeight(reloadLayerIndex, Mathf.Lerp(1f, 0f, t / lerpTime));
            yield return null;
        }

        animator.SetLayerWeight(reloadLayerIndex, 0f);
        reloadCoroutine = null;

        // NEW: restore rig control after reload
        if ((currentWeaponIndex == 2 || currentWeaponIndex == 3) && aimLayerController != null)
            aimLayerController.SetReloading(false);
    }

    public void InterruptReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        animator.SetLayerWeight(reloadLayerIndex, 0f);

        // NEW: restore rig control if reload was interrupted
        if ((currentWeaponIndex == 2 || currentWeaponIndex == 3) && aimLayerController != null)
            aimLayerController.SetReloading(false);
    }

    private void WeaponSwitch(int weaponIndex)
    {
        currentWeaponIndex = weaponIndex;
    }
}