using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollPhysicsHandler : MonoBehaviour
{
    private Rigidbody rb;
    private RigidbodyConstraints originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

    // Partial ragdoll: temporarily allow physics but keep constraints cleared
    public void EnablePartial()
    {
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.WakeUp();
    }

    public void DisablePartial()
    {
        // back to animated control
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.constraints = originalConstraints;
        rb.Sleep();
    }
}