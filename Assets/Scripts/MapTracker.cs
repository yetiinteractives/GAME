using UnityEngine;

public class MinimapTracker : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera captureCamera;

    [Header("UI")]
    public RectTransform mapRect;
    public RectTransform markerRect;

    [Header("Rotation Fix ")]
    public float rotationOffset = 90f;

    void Update()
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