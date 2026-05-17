using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/EnemyDeathChannel")]
public class EnemyDeathChannel : ScriptableObject
{
    public UnityAction<GameObject> OnEnemyDied;

    public void RaiseEvent(GameObject deadEnemy)
    {
        Debug.Log(deadEnemy+ " Dead");
        OnEnemyDied?.Invoke(deadEnemy);
    }
}