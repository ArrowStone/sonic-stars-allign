using UnityEngine;

public class TimerDespawn : MonoBehaviour
{
    [SerializeField] private float timer;
    private void Awake()
    {
        Invoke(nameof(Despawn), 1);
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}