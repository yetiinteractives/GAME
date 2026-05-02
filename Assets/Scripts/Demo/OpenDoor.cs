using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField]private Animator anim;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            anim.SetTrigger("OpenDoor");
        }
    }
}
