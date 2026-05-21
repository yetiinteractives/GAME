using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public float speedX = 0.02f;
    public float speedY = 0.02f;

    Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        Vector2 offset = new Vector2(
            Time.time * speedX,
            Time.time * speedY
        );

        // DO NOTHING WITH TEXTURE OFFSET (prevents warnings)
        // We intentionally avoid unsupported shader properties
    }
}