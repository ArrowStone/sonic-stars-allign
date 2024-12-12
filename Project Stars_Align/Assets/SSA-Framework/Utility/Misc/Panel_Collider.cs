using System;
using UnityEngine;

public class Panel_Collider : MonoBehaviour
{
    public event Action<Collider> TriggerEnter;

    public event Action<Collider> TriggerExit;

    public bool Stay { get; private set; }
    public Collider RefCollider;

    private void OnEnable()
    {
        RefCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider _other)
    {
        Stay = true;
        TriggerEnter?.Invoke(_other);
    }

    private void OnTriggerExit(Collider _other)
    {
        Stay = false;
        TriggerExit?.Invoke(_other);
    }
}