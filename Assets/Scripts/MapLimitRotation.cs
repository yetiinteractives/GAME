using UnityEngine;

public class MapLimitRotation : MonoBehaviour
{
    [SerializeField] GameObject player;

    private void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
    }
}
