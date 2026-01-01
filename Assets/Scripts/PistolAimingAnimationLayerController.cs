using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimLayerController : MonoBehaviour
{
    public Animator animator;
    public Rig aimRig; 

    
    public float blendSpeed = 5f;

    private float targetWeight = 0f;
    private int aimLayerIndex;
    private float aimRigWeight;

    void Start()
    {
        aimLayerIndex = animator.GetLayerIndex("Aim Layer");
    }

    void Update()
    {
        // Right Mouse Button
        if (Input.GetMouseButton(1))
        {
            targetWeight = 0.85f;
            StartCoroutine(aimRigWeightDelay());
        }


        else
        {
            targetWeight = 0f;
            aimRigWeight = 0f;
        }
           
        // for that combined animatoin 
        float currentWeight = animator.GetLayerWeight(aimLayerIndex);
        float newWeight = Mathf.Lerp(
            currentWeight,
            targetWeight,
            Time.deltaTime * blendSpeed
        );

        animator.SetLayerWeight(aimLayerIndex, newWeight);

        
        //for that procedural aim rig   
        
        aimRig.weight = Mathf.Lerp(
            aimRig.weight,
            aimRigWeight,
            Time.deltaTime * 25f
        );
        
        
    }

    private IEnumerator aimRigWeightDelay()
    {
        yield return new WaitForSeconds(0.25f);
        aimRig.weight = 1f;
    }
}
