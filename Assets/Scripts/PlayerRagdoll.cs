using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    private Rigidbody mainRigidbody;
    private Collider mainCollider;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private bool isRagdoll = false;

    void Awake()
    {
        // Main Rigidbody and Collider on the player
        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();

        // All child rigidbodies and colliders (excluding main ones)
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        DisableRagdoll(); // Start in normal animated state
    }

    void Update()
    {
        // Example toggle
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isRagdoll) EnableRagdoll();
            else DisableRagdoll();
        }
    }

    public void EnableRagdoll()
    {
        if (isRagdoll) return;
        isRagdoll = true;

        // Disable main Rigidbody and Collider
        if (mainRigidbody != null) mainRigidbody.isKinematic = true;
        if (mainCollider != null) mainCollider.enabled = false;

        if (animator != null) animator.enabled = false;

        // Enable all child rigidbodies and colliders, *excluding main*
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != mainRigidbody) rb.isKinematic = false;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider) col.enabled = true;
        }
    }

    public void DisableRagdoll()
    {
        if (!isRagdoll) return;
        isRagdoll = false;

        // Re-enable main Rigidbody and Collider
        if (mainRigidbody != null) mainRigidbody.isKinematic = false;
        if (mainCollider != null) mainCollider.enabled = true;

        if (animator != null) animator.enabled = true;

        // Disable all child rigidbodies and colliders, *excluding main*
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != mainRigidbody) rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider) col.enabled = false;
        }
    }
}