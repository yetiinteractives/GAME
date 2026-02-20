
using System.Collections;
using UnityEngine;

public class EnemyShotKnockback : MonoBehaviour
{
    Animator animator;
    int knockbackLayerIndex;

    public static EnemyShotKnockback Instance { get; private set; }

    [SerializeField]private float knockbackTime = 2f;



    private void Awake()
    {
        Instance = this;
    }
    private void  Start()
    {
        animator = GetComponent<Animator>();
        knockbackLayerIndex = animator.GetLayerIndex("Knockback Layer");

    }

    public void TriggerKnockback()
    {
        StartCoroutine(Knockback());
    }

    IEnumerator Knockback()
    {
        animator.SetLayerWeight(knockbackLayerIndex, 1f);
        animator.SetTrigger("Knockback");

        yield return new WaitForSeconds(knockbackTime);
        animator.SetLayerWeight(knockbackLayerIndex, 0f);
    }
}
