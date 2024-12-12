using UnityEngine;
using UnityEngine.InputSystem;

public class InputComponent : MonoBehaviour
{
    public GameInput I;
    public InputAction MovementInput { get; private set; }
    public Vector2 MoveInputValues => MovementInput.ReadValue<Vector2>();
    public Vector3 VectorMoveInput => new(MoveInputValues.x, 0, MoveInputValues.y);
    public InputAction JumpInput { get; private set; }
    public InputAction RollInput { get; private set; }

    private void Awake()
    {
        I = new GameInput();
        MovementInput = I.Player.Move;
        JumpInput = I.Player.Jump;
        RollInput = I.Player.Crouch;
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