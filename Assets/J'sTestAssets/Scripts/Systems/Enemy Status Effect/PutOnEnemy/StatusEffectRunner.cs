using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectRunner : MonoBehaviour
{
    private Dictionary<string, Coroutine> activeEffects = new Dictionary<string, Coroutine>();

    public void Run(string key, IEnumerator routine)
    {
        if (activeEffects.ContainsKey(key))
        {
            StopCoroutine(activeEffects[key]);
            activeEffects.Remove(key);
        }

        Coroutine c = StartCoroutine(WrapRoutine(key, routine));
        activeEffects[key] = c;
    }

    public void StopEffect(string key)
    {
        if (activeEffects.TryGetValue(key, out Coroutine c))
        {
            StopCoroutine(c);
            activeEffects.Remove(key);
        }
    }

    private IEnumerator WrapRoutine(string key, IEnumerator routine)
    {
        yield return routine;

        if (activeEffects.ContainsKey(key))
        {
            activeEffects.Remove(key);
        }
    }

    public void StopAllEffects()
    {
        foreach (var pair in activeEffects)
        {
            if (pair.Value != null)
                StopCoroutine(pair.Value);
        }

        activeEffects.Clear();
    }
}