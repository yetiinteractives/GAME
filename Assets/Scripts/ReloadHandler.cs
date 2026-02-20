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
        float lerpTime = 0.2f;
        float t = 0f;

        // Smooth IN
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

        // Smooth OUT
        while (t < lerpTime)
        {
            t += Time.deltaTime;
            animator.SetLayerWeight(reloadLayerIndex, Mathf.Lerp(1f, 0f, t / lerpTime));
            yield return null;
        }

        animator.SetLayerWeight(reloadLayerIndex, 0f);
        reloadCoroutine = null;
    }

    public void InterruptReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        animator.SetLayerWeight(reloadLayerIndex, 0f);
    }



    private void WeaponSwitch(int weaponIndex)
    {
        currentWeaponIndex = weaponIndex;

    }
}