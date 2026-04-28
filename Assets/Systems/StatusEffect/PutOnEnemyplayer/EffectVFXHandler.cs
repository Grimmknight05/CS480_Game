using UnityEngine;
using System.Collections.Generic;

public class EffectVFXHandler : MonoBehaviour
{
    private Dictionary<string, GameObject> activeVFX = new Dictionary<string, GameObject>();

    public GameObject AttachVFX(string key, GameObject prefab)
    {
        if (activeVFX.ContainsKey(key))
            return activeVFX[key];

        GameObject vfx = Instantiate(prefab, transform);
        vfx.transform.localPosition = Vector3.zero;

        activeVFX[key] = vfx;
        return vfx;
    }

    public void RemoveVFX(string key)
    {
        if (activeVFX.TryGetValue(key, out GameObject vfx))
        {
            Destroy(vfx);
            activeVFX.Remove(key);
        }
    }
}