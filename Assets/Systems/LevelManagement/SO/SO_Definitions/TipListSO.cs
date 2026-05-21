using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loading/Tip List", fileName = "TipList")]
public class TipListSO : ScriptableObject
{
    public List<TipSO> tips;
}