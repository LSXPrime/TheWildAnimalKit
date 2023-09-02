using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
	// Player Movement While Attack
	public enum InAttack
	{
		Target = 0, // Move Player to Target Position Instantly
		Forward = 1, // Dash Player Little Forward
		InPlace = 2, // Keep Player In his Place
		Free = 3 // Let Player Move Freely
	}

	public enum AttackCondition
	{
		None = 0,
		Jumping = 1,
		Running = 2,
		Idle = 3
	}

	[System.Serializable]
	public class AttackCombo
	{
		public int ID;
		public InAttack Movement;
		public AttackCondition Condition;
		public bool AnimationBased = false; // is Called from script or AnimationEvents
		public string KeyCode;
        public float DamageMultiplier = 1;
		public AudioClip Sound;
    }
	
    public class AnimalCombat : MonoBehaviour
    {
		[Header("Referances")]
		[AutoProperty] public Animator animator;
		[AutoProperty] public AnimalController Controller;
		[AutoProperty] public AnimalStatus playerStatus;
		[AutoProperty] public AudioSource audioSource;
        [AutoProperty] public AnimalCore Core;

        [Header("Combat Details")]
		public AttackCombo[] Attacks;
		public float FieldOfView = 75f;
		public float AttackRange = 1f;
		public LayerMask CombatTargets;

		[Header("Aim Assist")]
        public float AimMaxDistance = 10f;
        public float AimAssistRadius = 2f;
		public float AimSnapSpeed = 3f;
        public AnimationCurve aimAssistCurve;

		private Collider[] assistedTargets = new Collider[10];
        internal AnimalCore Target;
        float nextFire = 0.0f;
		bool isAttacking;
		AttackCombo attackIndex;

		void Update()
		{
			if (!Controller.GetInput)
				return;
			if (Target != null && (!Target.Alive || Vector3.Distance(Target.transform.position, transform.position) > AimMaxDistance)) 
				Target = null;

			if (Time.time > nextFire && !isAttacking)
			{
				foreach (AttackCombo combo in Attacks)
				{
					if (InputManager.Instance.GetButtonDown(combo.KeyCode))
						ExecuteAttack(combo);
				}
			}
		}

        private void FixedUpdate()
        {
			if (Controller.CC.velocity.magnitude == 0 || !GameData.Instance.AimAssist)
				return;

            float nearestDistance = Mathf.Infinity;
            if (Target == null)
			{
                Physics.OverlapSphereNonAlloc(transform.position, AimMaxDistance, assistedTargets, CombatTargets);
                Transform nearestTarget = null;

                foreach (Collider targetCollider in assistedTargets)
                {
					if (targetCollider == null)
						continue;

                    Transform targetTransform = targetCollider.transform;
                    float distance = Vector3.Distance(transform.position, targetTransform.position);
                    if (distance <= AimMaxDistance && distance < nearestDistance)
                    {
                        nearestTarget = targetTransform;
                        nearestDistance = distance;
                    }
                }

				if (nearestTarget != null && nearestTarget.TryGetComponent(out AnimalCore core))
					Target = core;
            }
            
            if (Target == null || aimAssistCurve.Evaluate(nearestDistance / AimMaxDistance) == 0f)
                return;

            Vector3 targetDirection = Target.transform.position - transform.position;
            targetDirection.y = 0f;
            float aimAssistStrength = aimAssistCurve.Evaluate(nearestDistance / AimMaxDistance);
            Vector3 aimAssistDirection = Vector3.Lerp(transform.forward, targetDirection.normalized, aimAssistStrength);

            Vector3 aimAssistPosition = transform.position + aimAssistDirection * AimAssistRadius;
            transform.LookAt(Vector3.Lerp(transform.position, aimAssistPosition, AimSnapSpeed * Time.fixedDeltaTime));
        }

        void ExecuteAttack(AttackCombo combo)
		{			
			isAttacking = true;
			attackIndex = combo;

			switch (combo.Condition)
			{
				case AttackCondition.None:
					break;
				case AttackCondition.Jumping:
					if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
					{
						isAttacking = false;
						return;
					}
					break;
				case AttackCondition.Running:
					if (Controller.State != PlayerState.Walk || !Controller.isRunning)
					{
						isAttacking = false;
						return;
					}
					break;
				case AttackCondition.Idle:
					if (Controller.State != PlayerState.Idle)
					{
						isAttacking = false;
						return;
					}
					break;
			}

			switch (combo.Movement)
			{
				case InAttack.Forward:
					Controller.GetInput = false;
					Vector3 dir = transform.TransformDirection(Vector3.forward);
					dir.y = -0.1f;
					Controller.CC.Move(dir);
					break;
				case InAttack.InPlace:
					Controller.GetInput = false;
					break;
			}

            animator.SetFloat("AttackID", (float)combo.ID);
            animator.SetTrigger("Attack");
			if (!combo.AnimationBased)
				MeleeAttack();
			
			AnimatorStateInfo animatorClip = animator.GetCurrentAnimatorStateInfo(1);
			nextFire = Time.time + animatorClip.length;
			
			isAttacking = false;
			Controller.GetInput = true;
		}
		
		public void MeleeAttack()
		{
            audioSource.PlayOneShot(attackIndex.Sound);
            var colliders = Physics.OverlapSphere(transform.position, AttackRange, CombatTargets);
			foreach (var hit in colliders)
			{
                if (!hit.gameObject.CompareTag("AI"))
					continue;
				
				var dir = hit.transform.position - transform.position;
				var angle = Vector3.Angle(dir, transform.position);
                if (angle > FieldOfView)
					continue;


                Target = hit.gameObject.GetComponent<AnimalCore>();
				if (Target != null)
				{
                    if (attackIndex.Movement == InAttack.Target)
                    {
						Vector3 lookAtPos = new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z);
						transform.LookAt(lookAtPos);
						Controller.CC.Move(Target.transform.position - transform.position);
                    }

                    float Damage = Random.Range(playerStatus.Attack, playerStatus.Attack / 2) * (attackIndex != null ? attackIndex.DamageMultiplier : 1f);
                    Target.healthSystem.TakeDamage(Damage, Core);

					if (!Target.healthSystem.Alive)
						Target = null;
				}
			}
		}
		
		
    }
}
