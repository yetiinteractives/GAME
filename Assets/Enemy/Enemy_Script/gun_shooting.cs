using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class gun_shooting : MonoBehaviour
{
    [Header("refrences")]
    public InputActionReference fireAction;
    public GameObject bulletsPrefab;
    public Transform firing_position;

    [Header("Bullet Settings")]
    public float bulletSpeed = 300f;
    public float fireRate = 0.2f;
    public float bulletLifeTime = 5f;
    private bool canFire = true;


    void OnEnable()
    {
        fireAction.action.performed += OnFirePerformed;
        fireAction.action.Enable();

    }
    void OnDisable()
    {
        fireAction.action.performed -= OnFirePerformed;
        fireAction.action.Disable();

    }
    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        TryFire();
    }
    void TryFire()
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
    IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;
        
             }
}
