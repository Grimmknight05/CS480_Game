using System.Collections;
using UnityEngine;

public class StatusEffectRunner : MonoBehaviour
{
    public void Run(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}