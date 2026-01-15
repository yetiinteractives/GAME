using UnityEngine;

public class EndermanTeleportation : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            float teleportRange = 20f;
            Vector3 randomDirection = Random.insideUnitCircle * teleportRange;
            Vector3 newPosition = transform.position + new Vector3(randomDirection.x, 0, randomDirection.y);
            transform.position = newPosition;
            Debug.Log("endersoilder teleported to:" + newPosition);

        }
    }

    
}
