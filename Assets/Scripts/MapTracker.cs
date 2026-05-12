using UnityEngine;

public class MapTracker : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera captureCamera;

    [Header("UI Root")]
    public GameObject mapFrame;

    [Header("UI Elements")]
    public RectTransform mapRect;
    public RectTransform markerRect;

    [Header("Rotation Fix")]
    public float rotationOffset = -90f;

    private bool isMapOpen = false;

    void Update()
    {
        HandleToggle();

        if (!isMapOpen) return;

        UpdateMinimap();
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMapOpen = !isMapOpen;
            mapFrame.SetActive(isMapOpen);

            if (isMapOpen)
                OnMapOpened();
            else
                OnMapClosed();
        }
    }

    void OnMapOpened()
    {
        UpdateMinimap(); 
    }

    void OnMapClosed()
    {
    }

    
    void UpdateMinimap()
    {
        Vector3 vp = captureCamera.WorldToViewportPoint(player.position);

        float x = (vp.x - 0.5f) * mapRect.rect.width;
        float y = (vp.y - 0.5f) * mapRect.rect.height;

        markerRect.anchoredPosition = new Vector2(x, y);

        float yaw = player.eulerAngles.y;

        markerRect.localEulerAngles = new Vector3(
            0f,
            0f,
            -(yaw - rotationOffset)
        );
    }
}