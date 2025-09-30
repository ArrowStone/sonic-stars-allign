using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEditor.UI;

// Game input
public class InputComponent : MonoBehaviour
{

    public GameInput I;
    public InputAction MovementInput { get; private set; }
    public InputAction CameraInput { get; private set; }
    public InputAction MouseInput { get; private set; }

    public Vector2 MoveInputValues => MovementInput.ReadValue<Vector2>();
    public Vector3 VectorMoveInput => new(MoveInputValues.x, 0, MoveInputValues.y);
    public Vector2 CameraInputValues => CameraInput.ReadValue<Vector2>();
    public InputAction JumpInput { get; private set; }
    public InputAction CrouchInput { get; private set; }
    public InputAction BounceInput { get; private set; }
    public InputAction ReactionInput { get; private set; }
    public InputAction AttackInput { get; private set; }
    public InputAction BackCameraInput { get; private set; }
    public InputAction StartInput { get; private set; }
    public InputAction SweepInput { get; private set; }

    private void Awake()
    {
        I = new GameInput();
        MovementInput = I.Player.Move;
        CameraInput = I.Player.Look;
        MouseInput = I.Player.Mouse;
        JumpInput = I.Player.Jump;
        CrouchInput = I.Player.Crouch;
        BounceInput = I.Player.Bounce;
        ReactionInput = I.Player.ReactionCommand;
        AttackInput = I.Player.Attack;
        BackCameraInput = I.Player.BackCamera;
        StartInput = I.Player.Start;
        SweepInput = I.Player.SweepKick;
    }

    private void OnEnable()
    {
        I.Enable();
    }

    private void OnDisable()
    {
        I.Disable();
    }

    public void EnableControls(bool _on)
    {
        if (_on)
        {
            I.Enable();
        }
        else
        {
            I.Disable();
        }
    }

    public void SetAction(InputActionReference _key, bool _on)
    {
        InputAction _cache = I.FindAction(_key.action.name);
        if (_on)
        {
            _cache.Enable();
        }
        else
        {
            _cache.Disable();
        }
    }
}