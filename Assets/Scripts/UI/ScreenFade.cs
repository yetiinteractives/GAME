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

        yield return new WaitForSeconds(0.2f);
        //fade out 
        while (color.a > 0)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            blackimage.color = color;
            yield return null;

        }
    }
    public void Start()
    {
        StartFade();
    }
}