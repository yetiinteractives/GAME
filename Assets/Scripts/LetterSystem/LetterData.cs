using UnityEngine;

[CreateAssetMenu(menuName = "Game/Letter Data")]
public class LetterData : ScriptableObject
{
    public string letterTitle;
    [TextArea(4, 12)]
    public string[] pages;
    public Sprite letterImage;


}
