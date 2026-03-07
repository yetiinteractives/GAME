using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{


    private Rigidbody mainRigidbody;
    private Collider mainCollider;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Animator animator;
   [SerializeField] private Avatar playerAvatar;
    private bool isRagdoll = false;

    void Awake()
    {

        PlayerHealth.OnPlayerDie += EnableRagdoll;


        // Main Rigidbody and Collider on the player
        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
       // playerAvatar = animator.GetComponent<Avatar>();

        
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        DisableRagdoll();
    }

   

    void Update()
    {
        // Example toggle
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!isRagdoll) EnableRagdoll();
            else DisableRagdoll();
        }
    }

    public void EnableRagdoll()
    {
        if (isRagdoll) return;
        isRagdoll = true;



        if (animator != null)
        {
            //animator.avatar = null; 
            animator.enabled = false;
            animator.Update(0f);
        }
        // Enable all child rigidbodies and colliders, *excluding main*
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != mainRigidbody)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider) col.enabled = true;
        }

        // Disable main Rigidbody and Collider
        if (mainRigidbody != null) mainRigidbody.isKinematic = true;
        if (mainCollider != null) mainCollider.enabled = false;
    }

    public void DisableRagdoll()
    {
        if (!isRagdoll) return;
        isRagdoll = false;



        if (animator != null)
        {
            //animator.avatar = playerAvatar;
            animator.enabled = true;
        }
        // Disable all child rigidbodies and colliders, *excluding main*
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != mainRigidbody) rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider) col.enabled = false;
        }

        // Re-enable main Rigidbody and Collider
        if (mainRigidbody != null) mainRigidbody.isKinematic = false;
        if (mainCollider != null) mainCollider.enabled = true;
    }
}