using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollPhysicsHandler : MonoBehaviour
{
    private Rigidbody rb;
    private bool originalKinematic;
    private RigidbodyConstraints originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalKinematic = rb.isKinematic;
        originalConstraints = rb.constraints;
        rb.isKinematic = true;
    }

    public Rigidbody Rigidbody => rb;

    public void EnableRagdoll()
    {
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.WakeUp();
    }

    public void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.constraints = originalConstraints;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }

    public void RestoreOriginalSetup()
    {
        rb.isKinematic = originalKinematic;
        rb.constraints = originalConstraints;
    }
}