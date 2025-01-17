﻿using UnityEngine;

public class DebugCameraStart : MonoBehaviour
{
    [SerializeField]
    private CamBrain _brain;

    [SerializeField]
    private CamPoint_NormalPlayer _player;

    public void Start()
    {
        _brain.Point = _player;
        _player.OnEnter(_brain);
        _brain.MachineTransition(CameraStates.Alive);
    }
}