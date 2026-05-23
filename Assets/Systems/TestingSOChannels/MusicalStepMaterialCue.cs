using UnityEngine;

/// <summary>
/// Bridges parameterless UnityEvents (e.g. <see cref="MusicalPuzzleStep"/>'s highlight / reset)
/// to <see cref="MaterialController"/>'s bool API. Assign dim = start material, bright = target material.
/// </summary>
public class MusicalStepMaterialCue : MonoBehaviour
{
    [SerializeField] private MaterialController materialController;

    public void ApplyHighlightedMaterials()
    {
        if (materialController == null)
        {
            Debug.LogWarning($"{nameof(MusicalStepMaterialCue)} on '{name}': assign Material Controller in the Inspector.", this);
            return;
        }
        materialController.SetAllMaterials(true);
    }

    public void ApplyIdleMaterials()
    {
        if (materialController == null)
        {
            Debug.LogWarning($"{nameof(MusicalStepMaterialCue)} on '{name}': assign Material Controller in the Inspector.", this);
            return;
        }
        materialController.SetAllMaterials(false);
    }
}
