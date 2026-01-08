using UnityEngine;

public class HazardDamagePlayer : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
           
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(20f); // Deal 20 damage on collision
            }
        }
    }
}
