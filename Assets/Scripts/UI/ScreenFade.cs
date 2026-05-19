using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ScreenFade : MonoBehaviour
{
    public Image blackimage;
    public float fadeSpeed = 1f;

    public void StartFade()
    {
        StartCoroutine(FadeToBlack());
    }
    IEnumerator FadeToBlack()
    {
        Color color = blackimage.color;
        while (color.a < 1)
        {
            color.a += Time.deltaTime * fadeSpeed;
            blackimage.color = color;
            yield return null;
        }
    }
    private void Start()
    {
        StartFade();
    }
}