using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{

    private CharacterController2D _controller2D;


    private void Awake()
    {
        _controller2D = GetComponent<CharacterController2D>();
    }

    
    public void OnMove(InputValue value)
    {
        var move = value.Get<Vector2>();
        _controller2D.MoveDirection = move.x;
    }

    public void OnJump()
    {
        _controller2D.Jump();
    }
    
    public void OnJumpRelease()
    {
    }
}
