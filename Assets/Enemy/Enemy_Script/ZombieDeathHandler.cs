using UnityEngine;

public class ZombieDeathHandler : MonoBehaviour
{
    [SerializeField] int numberOfDeathAnimations = 9;

    Animator anim;
    bool deathTriggered = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayRandomDeath()
    {
        if (deathTriggered) return;

        deathTriggered = true;

        int randomIndex = Random.Range(0, numberOfDeathAnimations);

        anim.SetInteger("DeathIndex", randomIndex);
        anim.SetBool("IsDead", true);
    }
}
