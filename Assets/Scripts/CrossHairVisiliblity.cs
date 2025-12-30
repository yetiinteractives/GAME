using UnityEngine;
using UnityEngine.UI;

public class CrossHairVisiliblity : MonoBehaviour
{
    public Image _crossHair;

    void Update()
    {
        _crossHair.gameObject.SetActive(Input.GetMouseButton(1));
    }
}
