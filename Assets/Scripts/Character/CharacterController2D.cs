using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float _jumpHeight = 5f;							// Amount of force added when the player jumps.
	[SerializeField] private float _speedForce = 10f;							// sonic speedf
	[Range(0, .3f)] [SerializeField] private float _movementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private LayerMask _whatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform _groundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform _ceilingCheck;							// A position marking where to check for ceilings
	
	[SerializeField] private float _groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool _grounded;            // Whether or not the player is grounded.
	[SerializeField] private float _ceilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D _rigidbody2D;
	private bool _facingRight = true;  // For determining which way the player is currently facing.
	private Vector3 _velocity = Vector3.zero;
	
	private bool _wantToJump;
	
	private Vector2 _groundBox => new Vector2(Mathf.Abs(transform.localScale.x)*0.98f, _groundedRadius);
	
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
		Collider2D[] colliders = Physics2D.OverlapBoxAll(_groundCheck.position, _groundBox, 0,_whatIsGround);
		_grounded = colliders.Length > 0;
		
		Move(MoveDirection);
		if(_grounded && _wantToJump)
			Jump();
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
}