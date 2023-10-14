using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{

    private CharacterController2D _controller2D;
    [SerializeField] private GameObject _playerSprite;


    private void Awake()
    {
        _controller2D = GetComponent<CharacterController2D>();
    }
    
    private void OnEnable()
    {
        GameManager.GameWon += OnGameWon;
    }

    private void OnDisable()
    {
        GameManager.GameWon -= OnGameWon;
    }

    private void OnGameWon()
    {
        if(_playerSprite)
            _playerSprite.SetActive(false);
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
