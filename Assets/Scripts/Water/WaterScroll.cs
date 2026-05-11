using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public float speedX = 0.02f;
    public float speedY = 0.02f;

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offsetX = Time.time * speedX;
        float offsetY = Time.time * speedY;

        rend.material.mainTextureOffset =
            new Vector2(offsetX, offsetY);
    }
}
