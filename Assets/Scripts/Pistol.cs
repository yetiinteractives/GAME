using System;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    [SerializeField]ParticleSystem muzzleFlash;
    [SerializeField]AudioSource gunShotAudioSource;
    [SerializeField]ParticleSystem hitEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MousePositoin3D.OnFirePerformed += HandleFirePerformed;

    }

    private void HandleFirePerformed(RaycastHit raycastHitInfo) 
    {
        if(muzzleFlash != null)
            muzzleFlash.Play();

        if(gunShotAudioSource != null)
            gunShotAudioSource.Play();

        if(hitEffect != null)
        {
            hitEffect.transform.position = raycastHitInfo.point;  //move hit effect to hit position
            hitEffect.Play();
        }
           
    }

    
}
