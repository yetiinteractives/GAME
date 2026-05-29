using UnityEngine;
using UnityEngine.UI;

public class KeysCollection : MonoBehaviour
{
    public LevelFlag keyName;
    private Image keyImage;

    private void Start()
    {
        keyImage = GetComponentInChildren<Image>(true);
        keyImage.gameObject.SetActive(false);

      
    }

    private void Update()
    {
       
       keyImage.gameObject.SetActive(LevelManager.Instance.GetFlag(keyName));
       
    }


}
