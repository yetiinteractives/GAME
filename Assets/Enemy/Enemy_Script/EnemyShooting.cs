using UnityEngine;
using System.Collections;

public class EnemyShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletsPrefab;
    public Transform firing_position;
    public float bulletSpeed = 30f;
    public float fireRate = 2f;
    public float bulletLifeTime = 5f;
    public bool canFire = true;

    public void Shoot(Transform target)
    {
        if (!canFire) return;
        if (bulletsPrefab == null || firing_position == null) return;
        GameObject bullet = Instantiate(bulletsPrefab, firing_position.position, firing_position.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firing_position.forward * bulletSpeed;
        }
        Destroy(bullet, bulletLifeTime);
        if (fireRate > 0f)
        {
            StartCoroutine(FireCooldown());

        }
    }
    IEnumerator FireCooldown(){
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;

    }
}
