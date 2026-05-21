using UnityEngine;

[CreateAssetMenu(menuName = "Loading/Tip", fileName = "NewTip")]
public class TipSO : ScriptableObject
{
    [TextArea(2, 4)]
    public string tipText;
    //public Sprite tipIcon;
}