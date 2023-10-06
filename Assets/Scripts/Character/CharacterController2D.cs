using System;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(CharacterMovement))]
public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float _jumpHeight = 5f;							// Amount of force added when the player jumps.
	[SerializeField] private float _speedForce = 10f;							// sonic speedf
	[Range(0, .3f)] [SerializeField] private float _movementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private LayerMask _whatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform _groundCheck;							// A position marking where to check if the player is grounded.
	
	[SerializeField] private Animator CharacterAnimator;
	
	[SerializeField] private float _groundedYAxis = .2f; // Radius of the overlap circle to determine if grounded
	[SerializeField] private float _groundedXAxis = .2f; // Radius of the overlap circle to determine if grounded
	private bool _grounded;            // Whether or not the player is grounded.
	private Rigidbody2D _rigidbody2D;
	[SerializeField] private bool _facingRight = true;  // For determining which way the player is currently facing.
	private Vector3 _velocity = Vector3.zero;
	
	private bool _wantToJump;
	
	private static readonly int XAnimator = Animator.StringToHash("X");
	private static readonly int YAnimator = Animator.StringToHash("Y");
	private static readonly int JumpAnimator = Animator.StringToHash("Jumping");
	private static readonly int FallAnimator = Animator.StringToHash("Falling");
	private bool _jumping;

    public static event Action<Vector3> OnMoveEvent; // item, worldPos

    private Vector2 GroundBox => new(_groundedXAxis, _groundedYAxis);
	
	public float MoveDirection { get; set; }


	private void Awake()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
	}


	private void FixedUpdate()
	{
		bool wasGrounded = _grounded;
		_grounded = false;
		
		//ground checker
		Collider2D[] colliders = Physics2D.OverlapBoxAll(_groundCheck.position, GroundBox, 0,_whatIsGround);
		_grounded = colliders.Length > 0;
		
		
		if (_grounded && _rigidbody2D.velocity.y <= 0.1f)
			_jumping = false;
		
		Move(MoveDirection);
		if(_grounded && _wantToJump)
			Jump();

		UpdateAnimation();

		if(_rigidbody2D.velocity.magnitude >  0.01f)
		{
			OnMoveEvent?.Invoke(transform.position);
        }
	}

	private void UpdateAnimation()
	{
		if (!CharacterAnimator)
			return;
		
		CharacterAnimator.SetFloat(XAnimator, MoveDirection);
		
		if(_rigidbody2D.velocity.y <= -3.0f)
			CharacterAnimator.SetBool(FallAnimator, !_grounded);
		else
		{
			CharacterAnimator.SetBool(FallAnimator, !_grounded);
			CharacterAnimator.SetBool(JumpAnimator, _jumping);
		}
	}


	private void Move(float move)
	{
		// Move the character by finding the target velocity
		Vector3 targetVelocity = new Vector2(move * _speedForce, _rigidbody2D.velocity.y);
		// And then smoothing it out and applying it to the character
		_rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref _velocity, _movementSmoothing);

		// If the input is moving the player right and the player is facing left...
		if (move > 0 && !_facingRight)
		{
			Flip();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && _facingRight)
		{
			Flip();
		}
	}

	public void Jump()
	{
		// If the player should jump
		_wantToJump = true;
		if (_grounded)
		{
			_wantToJump = false;
			_jumping = true;
			
			_rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0);
			// Add a vertical force to the player
			_grounded = false;
			float jumpForce = (float)Math.Sqrt(Physics.gravity.y * -2f * _jumpHeight);
			_rigidbody2D.AddForce(new Vector2(0f,jumpForce ), ForceMode2D.Impulse);
		}

		if (_wantToJump)
			StartCoroutine(ResetWantToJump());
	}

	private IEnumerator ResetWantToJump()
	{
		yield return new WaitForSeconds(0.1f);
		_wantToJump = false;
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing
		_facingRight = !_facingRight;

		// Multiply the player's x local scale by -1
		Vector3 newScale = transform.localScale;
		newScale.x *= -1;
		transform.localScale = newScale;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(_groundCheck.position, GroundBox);
	}
}
