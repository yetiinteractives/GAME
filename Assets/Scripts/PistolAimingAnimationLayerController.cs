using UnityEngine;

public class AimLayerController : MonoBehaviour
{
    public Animator animator;
    public float blendSpeed = 5f;

    private float targetWeight = 0f;
    private int aimLayerIndex;

    void Start()
    {
        aimLayerIndex = animator.GetLayerIndex("PistolAim Layer");
    }

    void Update()
    {
        // Right Mouse Button
        if (Input.GetMouseButton(1))
            targetWeight = 0.7f;
        else
            targetWeight = 0f;

        float currentWeight = animator.GetLayerWeight(aimLayerIndex);
        float newWeight = Mathf.Lerp(
            currentWeight,
            targetWeight,
            Time.deltaTime * blendSpeed
        );

        animator.SetLayerWeight(aimLayerIndex, newWeight);
    }
}
