using UnityEngine;

public class RiverFlow : MonoBehaviour
{
    public float speed = 0.1f;
    Renderer rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float offset = Time.time * speed;
       
        
    }
}
