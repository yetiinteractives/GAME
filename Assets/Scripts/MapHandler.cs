using UnityEngine;
using UnityEngine.UI;

public class MapHandler: MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Image miniMap;
    [SerializeField] Image fullMap;

    private bool isFullMapActive = false;   
    private float cameraVerticalHeight;
    private void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            
            if(isFullMapActive)
            {
                miniMap.gameObject.SetActive(true);
                fullMap.gameObject.SetActive(false);
                isFullMapActive = false;
            }
            else
            {
                miniMap.gameObject.SetActive(false);
                fullMap.gameObject.SetActive(true);
                isFullMapActive = true;
            }
        }
    }
}
