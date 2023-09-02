using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
    public class AnimalController : MonoBehaviour
    {
		[Header("REFERANCES")]
		[AutoProperty] public Animator animator;
        [AutoProperty] public AnimalCore Core;
        [AutoProperty] public HealthSystem healthSystem;
		[AutoProperty] public AnimalStatus Status;
        [AutoProperty] public AnimalCombat Combat;
        [AutoProperty] public AudioSource audioSource;
		[AutoProperty] public CharacterController CC;
        public AudioClip SatisfySound;
        public AudioClip FootStepSound;
        public float FootstepLength = 1f;
		public bool GetInput;
		
		[Header("MOVEMENT STATES")]
		public PlayerState State;
		public LayerMask MovementLayers;
        public bool isRunning;
		public float SpeedMultiplier = 1f;
		public float Speed;
		public float TurnSpeed = 5f;
		public float WalkSpeed = 4f;
        public float RunSpeed = 7f;
		
		[Header("SLIDE CONTROL")]
		public bool EnableSliding = true;
		[ConditionalField(nameof(EnableSliding))]
		public float SlideLimit = 32f;
		[ConditionalField(nameof(EnableSliding))]
		public float SlidingSpeed = 15f;

		[Header("AIR CONTROL")]
		public float JumpForce = 5f;
		public float airSpeed = 0.5f;
		public float Gravity = 1f;
		
		private bool isGrounded;
		private Vector2 Input;
		private Vector3 desiredMove = Vector3.zero;
		private Vector3 moveDirection = Vector3.zero;
		private float highestPoint;
		private Vector3 currentSurfaceNormal;
		private float SurfaceAngle;
		private float VelocityMagnitude;
		private RaycastHit footStepHit;
        private RaycastHit[] moveRay = new RaycastHit[1];
		
		void Update()
		{
			Animate();
			if (!healthSystem.Alive || !GetInput)
				return;
			
			InputControls();
			switch (State)
			{
				case PlayerState.Idle:
					Speed = 0f;
					break;
				case PlayerState.Walk:
					Speed = isRunning ? RunSpeed : WalkSpeed;
					if (isRunning)
					{
						if (healthSystem.Stamina <= 0f)
							isRunning = false;
						
						healthSystem.Stamina -= Time.deltaTime * healthSystem.StaminaPerSec;
					}
					break;
				case PlayerState.Satisfy:
                    Speed = 0;
                    break;
            }
		}
		
		void FixedUpdate()
		{
			CheckGrounded();
		}
		
		void Animate()
		{
			animator.SetFloat("Speed", Speed);
			animator.SetFloat("Turn", Input.x);
            animator.SetBool("Satisfy", State == PlayerState.Satisfy);
            animator.SetBool("Dead", !healthSystem.Alive);
		}
		
		public void InputControls()
		{
			CharacterRotation();

			Input = new Vector2(InputManager.Instance.GetAxis("Horizontal"), InputManager.Instance.GetAxis("Vertical"));
			
			if (Input == Vector2.zero && isGrounded && State != PlayerState.Satisfy) State = PlayerState.Idle;
			if (Input != Vector2.zero && isGrounded) State = PlayerState.Walk;
			if (Input != Vector2.zero && isGrounded && InputManager.Instance.GetButtonDown("Sprint")) isRunning = !isRunning; //X State = (State == PlayerState.Sprint) ? PlayerState.Walk : PlayerState.Sprint;
			if (InputManager.Instance.GetButtonDown("Jump") && isGrounded)
			{
				State = PlayerState.Jump;
				moveDirection.y = JumpForce;
				animator.CrossFadeInFixedTime("Jump", 0.1f);
			}

            VelocityMagnitude += VelocityMagnitude < FootstepLength ? CC.velocity.magnitude * Time.deltaTime : 0f;
            if (Physics.Raycast(transform.position, -transform.up, out footStepHit, 1f, MovementLayers))
            {
                currentSurfaceNormal = footStepHit.normal;
                if (VelocityMagnitude >= FootstepLength)
                {
                    audioSource.PlayOneShot(FootStepSound);
                    VelocityMagnitude = 0f;
                }
            }
		}
		
		private void CheckGrounded()
		{
			if (!healthSystem.Alive)
			{
				highestPoint = transform.position.y;
				return;
			}
			
			isGrounded = CC.isGrounded;
			if (!isGrounded && transform.position.y > highestPoint)
				highestPoint = transform.position.y;
			else if (isGrounded && transform.position.y < highestPoint)
			{
				healthSystem.FallDamage(highestPoint);
				highestPoint = transform.position.y;
			}
			
			Move();
		}
		
		public void Move()
		{
			desiredMove = (transform.forward * Input.y) + (transform.right * Input.x);
			if (isGrounded)
			{
				
				Physics.SphereCastNonAlloc(transform.position, 0.3f, Vector3.down, moveRay, CC.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
				desiredMove = Vector3.ProjectOnPlane(desiredMove, moveRay[0].normal);
				moveDirection.x = desiredMove.x * Speed * SpeedMultiplier;
				moveDirection.z = desiredMove.z * Speed * SpeedMultiplier;
			}
			else
			{
				moveDirection += Physics.gravity * Gravity * Time.fixedDeltaTime;
				moveDirection.x = (desiredMove.x * Speed * SpeedMultiplier) * airSpeed;
				moveDirection.z = (desiredMove.z * Speed * SpeedMultiplier) * airSpeed;
			}
			
			SurfaceAngle = Vector3.Angle(Vector3.up, currentSurfaceNormal);
            if(EnableSliding && SurfaceAngle > SlideLimit)
			{
				Vector3 slideDirection = currentSurfaceNormal + Vector3.down;
				moveDirection += slideDirection * SlidingSpeed;
			}
			
			if (CC.enabled)
				CC.Move(moveDirection * Time.fixedDeltaTime);
		}
		
		/*
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			currentSurfaceNormal = hit.normal;
		} */
		
		public void CharacterRotation()
		{
			if (!GetInput || Input == Vector2.zero)
				return;

            // Align the player to the surface
            Quaternion quaternion = Quaternion.FromToRotation(transform.up, currentSurfaceNormal) * transform.rotation;
			transform.rotation =  Quaternion.Lerp(transform.rotation, quaternion, Time.deltaTime * 1f);

            // Rotate the player to face the same direction as the camera
            Vector3 cameraForward = GlobalGameManager.Instance.CamController.transform.right * Input.x + GlobalGameManager.Instance.CamController.transform.forward * (Input.y > 0 ? Input.y : -Input.y);
            cameraForward.y = 0f;
            if (cameraForward != Vector3.zero)
			{
				Quaternion quaternion2 = Quaternion.LookRotation(cameraForward, currentSurfaceNormal);
				transform.rotation = Quaternion.Lerp(transform.rotation, quaternion2, Time.deltaTime * TurnSpeed);
			}
        }
		
    }
}
