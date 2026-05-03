using UnityEngine;

public class InvisibleTrigger : MonoBehaviour
{
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private TriggerID triggerId;

    [Header("Shots")]
    [SerializeField] private bool oneShot = true;

    private bool fired;

    private void Awake()
    {
        foreach (var col in GetComponentsInChildren<Collider>())
            col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(requiredTag)) return;
        if (oneShot && fired) return;

        fired = true;
        LevelManager.Instance?.FireTrigger(triggerId);
    }
}