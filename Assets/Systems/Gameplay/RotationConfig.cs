using UnityEngine;

[CreateAssetMenu(fileName = "RotationConfig", menuName = "Gameplay/RotationConfig")]
public class RotationConfig : ScriptableObject
{
    [System.Serializable]
    public class Rotation
    {
        public GameObject ObjectToRotate;
        public float RotationAmount = 0.0f;
    }
    
    [SerializeField] public Rotation[] Rotations;


}
