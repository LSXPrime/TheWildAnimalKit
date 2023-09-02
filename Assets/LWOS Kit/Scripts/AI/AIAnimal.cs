using UnityEngine;
using UnityEngine.AI;
using MyBox;

namespace LWOS
{
    public enum AIType : short
	{
		Aggressive = 0, // Attack anyone in thier Eye Sight
		Passive = 1, // Wandering Freely & Attacking if they got attacked
		Follower = 2, // Follow it's leader and attack their/leader attacker
		Pet = 3 // Doesn't Attack, Doesn't Follow, Just Wandering
	}

    public enum AIMovement : short
	{
		Free = 0, // Allowed to wander whereever it want
		Stationary = 1, // have one position & only allowed to move when a enemy enters thier eyesight
		Waypoints = 2 // Follow pre determined waypoints
	}

    public enum WaypointMovement : short
	{
		Random = 0,
		RoundRobin = 1
	}

	public enum PassiveState
	{
		Escape = 0,
		Attack = 1
	}

  
	
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(BoxCollider))]
	[RequireComponent(typeof(AnimalCore))]
	[RequireComponent(typeof(HealthSystem))]
	[RequireComponent(typeof(AudioSource))]
    public class AIAnimal : MonoBehaviour
    {
        [Header("References")]
		[AutoProperty] public Animator animator;
        [AutoProperty] public AnimalCore Core;
        [AutoProperty] public HealthSystem healthSystem;
		[AutoProperty] public NavMeshAgent agent;
		[AutoProperty] public AudioSource audioSource;

        [Header("AI Data")]
		public AIState State = AIState.Wander;
        public AIType Type;
        public int Level = 1;
        public string Faction;

        [Header("Detection")]
		[ConditionalField(nameof(Type), false, AIType.Follower, AIType.Aggressive, AIType.Passive)]
		public LayerMask TargetsLayer;
		[ConditionalField(nameof(Type), false, AIType.Follower, AIType.Aggressive, AIType.Passive)]
		public GameObject Target;
		[ConditionalField(nameof(Type), false, AIType.Follower)]
        public AnimalCore FollowTarget;
		[ConditionalField(nameof(Type), false, AIType.Follower, AIType.Aggressive, AIType.Passive)]
        public float SearchTargetsInterval = 10f;
        public float EyeSight = 25f;

		[Header("Needs")]
		public LayerMask NeedsLayer;
        public float NeedsCheckInterval = 10f;
		public AudioClip SatisfySound;
		
		[Header("Combat")]
		internal AIDamage[] Attacks;
		[ConditionalField(nameof(Type), false, AIType.Follower, AIType.Aggressive, AIType.Passive)]
		public float AttackCooldown = 1.5f;
        public AudioClip[] AttackSounds;

        [Header("Movement")]
        public AIMovement MovementType;
        public LayerMask MovementLayers;
		[ConditionalField(nameof(MovementType), false, AIMovement.Stationary)]
        public Transform StationaryCenter;
		[ConditionalField(nameof(MovementType), false, AIMovement.Waypoints)]
        public WaypointMovement WaypointsType;
        public float FieldOfView = 120.0f;
		[ConditionalField(nameof(Type), false, AIType.Follower, AIType.Aggressive, AIType.Passive)]
		public float DistanceToAttack = 2f;
		public float DistanceToDetectNearEnemy = 5.0f;
		public float DistanceToLoseEnemy = 35f;
		public float PatrolRange = 25f;
		public float WanderSpeed = 3.5f;
		public float ChaseSpeed = 7f;
        public float TurnSpeed = 2f;
        public float IdleTime = 20f;
        public int IdleAnimations = 1;

		[Header("Foot Steps")]
        public bool FootstepsHandle = true;
        [ConditionalField(nameof(FootstepsHandle))]
        public AudioClip FootStepSound;
        [ConditionalField(nameof(FootstepsHandle))]
        public float FootstepLength = 1f;
        [ConditionalField(nameof(FootstepsHandle))]
        public float FootstepRange = 5f;
        [ConditionalField(nameof(FootstepsHandle))]
        public float FootstepVolume = 0.75f;

        internal Transform[] Waypoints;
        private int waypointIndex;
		private PassiveState passiveState;
        private float idleTimeTmp;
		private float attackTimeTmp;
        private float searchTimeTmp;
        private float needsTimeTmp;
        private float VelocityMagnitude;
        private RaycastHit rHit;
        private Vector3 TargetPos;


        void Start()
		{
			animator.applyRootMotion = false;
			GetComponent<SphereCollider>().isTrigger = true;

            AIManager.Instance.Animals.Add(Core);
		}
		
		void Update()
		{
            if (!healthSystem.Alive)
                State = AIState.Dead;

            switch (State)
            {
                case AIState.Idle:
                    idleTimeTmp += Time.deltaTime;
                    animator.SetFloat("IdleID", Random.Range(0, IdleAnimations - 1));
                    agent.speed = 0f;
                    agent.isStopped = true;
                    if (idleTimeTmp >= IdleTime)
                    {
                        State = AIState.Wander;
                        idleTimeTmp = 0f;
                    }
                    break;
                case AIState.Wander:
                    agent.speed = WanderSpeed;
                    agent.isStopped = false;
                    Movement();
                    break;
                case AIState.Chase:
                    healthSystem.Stamina -= Time.deltaTime * healthSystem.StaminaPerSec;
                    agent.speed = healthSystem.Stamina <= 0f ? WanderSpeed : ChaseSpeed;
                    agent.isStopped = false;
                    Movement();
                    break;
                case AIState.Attack:
                    agent.speed = 0f;
                    agent.isStopped = true;
                    Attack();
                    break;
                case AIState.Satisfy:
                    idleTimeTmp += Time.deltaTime;
                    agent.speed = 0f;
                    agent.isStopped = true;
                    if (idleTimeTmp >= IdleTime)
                    {
                        State = AIState.Wander;
                        idleTimeTmp = 0f;
                    }
                    break;
                case AIState.Dead:
                    agent.isStopped = true;
                    agent.speed = 0f;
                    break;
            }

            animator.SetBool("Idle", State == AIState.Idle);
            animator.SetBool("Walk", State == AIState.Wander);
            animator.SetBool("Run", State == AIState.Chase);
            animator.SetBool("Satisfy", State == AIState.Satisfy);
            animator.SetBool("Dead", State == AIState.Dead);

            needsTimeTmp += Time.deltaTime;
            if (needsTimeTmp >= NeedsCheckInterval)
                NeedsCheck();

            if (Target != null && TargetsLayer.Contains(Target.layer) && !IsTargetingLeader())
			{
				attackTimeTmp += Time.deltaTime;
				if (DistanceToTarget() > EyeSight || DistanceToTarget() > DistanceToLoseEnemy)
				{
					Target = null;
                    agent.ResetPath();
                    State = AIState.Wander;
                    return;
                }

				if (DistanceToTarget() > DistanceToAttack)
					State = AIState.Chase;
				
                else if (DistanceToTarget() <= DistanceToAttack && attackTimeTmp >= AttackCooldown && (Type != AIType.Pet || (Type == AIType.Passive && passiveState == PassiveState.Attack)))
                    State = AIState.Attack;
            }
			else if (Target != null && NeedsLayer.Contains(Target.layer))
			{
				State = AIState.Chase;
				if (DistanceToTargetPos() < DistanceToAttack)
				{
                    State = AIState.Satisfy;
					audioSource.PlayOneShot(SatisfySound);
                }
			}
			else if (Target == null || IsTargetingLeader())
			{
				if (healthSystem.Killer != null)
					Target = healthSystem.Killer.gameObject;
				
				switch (Type)
				{
					case AIType.Aggressive:
						searchTimeTmp += Time.deltaTime;
						if (searchTimeTmp >= SearchTargetsInterval)
							SearchTarget();
						break;
					case AIType.Passive:
							passiveState = DistanceToTarget() <= DistanceToAttack ? PassiveState.Attack : PassiveState.Escape;
						break;
					case AIType.Follower:
						if (FollowTarget.Type == AnimalType.AI && FollowTarget.AI.Type == AIType.Aggressive)
						{
							searchTimeTmp += Time.deltaTime;
							if (searchTimeTmp >= SearchTargetsInterval)
								SearchTarget();
						}

						if (FollowTarget != null)
						{
							Target = FollowTarget.gameObject;
							if (FollowTarget.Type == AnimalType.AI && FollowTarget.AI.Target != null && TargetsLayer.Contains(FollowTarget.AI.Target.layer))
                                Target = FollowTarget.AI.Target;
							if (FollowTarget.Type == AnimalType.Player)
							{
								if (FollowTarget.Player.Combat.Target != null && TargetsLayer.Contains(FollowTarget.Player.Combat.Target.gameObject.layer))
									Target = FollowTarget.Player.Combat.Target.gameObject;
                                else if (FollowTarget.healthSystem.Killer != null && TargetsLayer.Contains(FollowTarget.healthSystem.Killer.gameObject.layer))
									Target = FollowTarget.healthSystem.Killer.gameObject;
								else if (searchTimeTmp >= SearchTargetsInterval)
                                    SearchTarget();
                            }
                        }
						break;
					case AIType.Pet:
                        break;
                }
                
				if (!agent.hasPath && State != AIState.Wander && State != AIState.Satisfy)
					State = AIState.Idle;
			}
        }

		void LateUpdate()
		{
            VelocityMagnitude += VelocityMagnitude < FootstepLength ? agent.velocity.magnitude * Time.deltaTime : 0f;
            FootstepsHandler();
		}

		void NeedsCheck()
		{
            needsTimeTmp = 0f;
           /* if (healthSystem.Health < 25f)
				SearchSatisfy(VitalType.Health); */
			if (healthSystem.Hunger < 25f)
				SearchSatisfy(VitalType.Hunger);
			if (healthSystem.Thirst < 25f)
				SearchSatisfy(VitalType.Thirst);
		}
		
		void SearchSatisfy(VitalType vital)
		{			
			Collider[] colliders = Physics.OverlapSphere(transform.position, EyeSight, NeedsLayer);
			Collider area = null;
			foreach (var collider in colliders) 
			{
				if (!collider.isTrigger) continue;
                SatisfyArea sArea = collider.GetComponent<SatisfyArea>();
				if (sArea != null && sArea.Type == vital)
				{
					if (vital == VitalType.Hunger && sArea.ForWild != Core.packData.isWild)
						continue;

					area = collider;
                    break;
                }
			}

			if (area != null)
				Target = area.gameObject;
			else if (Core.packData.isWild)
					SearchTarget();
		}
		
		void Movement()
		{
			switch (MovementType)
			{
				case AIMovement.Free:
					switch (State)
					{
						case AIState.Wander:
							if (!agent.hasPath && !agent.pathPending)
							{
                                TargetPos = GetRandomPath(transform.position, PatrolRange);
                                agent.SetDestination(TargetPos);
							}
							break;
						case AIState.Chase:
							if (Target == null)
							{
								State = AIState.Wander;
								return;
							}

                            if ((!agent.hasPath || agent.destination == Target.transform.position) && (Type == AIType.Pet || (Type == AIType.Passive && passiveState == PassiveState.Escape)) && TargetsLayer.Contains(Target.layer))
                            {
                                TargetPos = GetRandomPath(transform.position, PatrolRange);
                                agent.SetDestination(TargetPos);
                                break;
                            }

                            if (agent.destination != Target.transform.position)
							{
                                if ((Type == AIType.Pet || (Type == AIType.Passive && passiveState == PassiveState.Escape)) && TargetsLayer.Contains(Target.layer))
                                    break;

                                if (NavMesh.SamplePosition(Target.transform.position, out NavMeshHit nvHit, 100f, agent.areaMask))
								{
									TargetPos = nvHit.position;
                                    agent.SetDestination(TargetPos);
								}

                            }
                            break;
                    }
                    break;
				case AIMovement.Stationary:
					switch (State)
					{
						case AIState.Wander:
							if (transform.position != StationaryCenter.position)
							{
								TargetPos = StationaryCenter.position;
								agent.SetDestination(TargetPos);
							}
							break;
						case AIState.Chase:
                            if (Target == null)
                            {
                                State = AIState.Wander;
                                return;
                            }
                            if (agent.destination != Target.transform.position)
							{
								if (NavMesh.SamplePosition(Target.transform.position, out NavMeshHit nvHit, 100f, agent.areaMask))
                                {
                                    TargetPos = nvHit.position;
                                    agent.SetDestination(TargetPos);
                                }
                            }
                            break;
                    }
                    break;
				case AIMovement.Waypoints:
					switch (State)
					{
						case AIState.Wander:
							Vector3 targetWaypoint = transform.position;
							switch (WaypointsType)
							{
                        		case WaypointMovement.Random:
                                    targetWaypoint = Waypoints[Random.Range(0, Waypoints.Length)].position;
                                    break;
                        		case WaypointMovement.RoundRobin:
									waypointIndex = (waypointIndex + 1) % Waypoints.Length;
                                    targetWaypoint = Waypoints[waypointIndex].position;
                                    break;
                            }
							if (transform.position != targetWaypoint)
							{
								TargetPos = targetWaypoint;
								agent.SetDestination(TargetPos);
							}
								
							break;
						case AIState.Chase:
                            if (Target == null)
                            {
                                State = AIState.Wander;
                                return;
                            }
                            if (agent.destination != Target.transform.position)
							{
								if (NavMesh.SamplePosition(Target.transform.position, out NavMeshHit nvHit, 100f, agent.areaMask))
                                {
                                    TargetPos = nvHit.position;
                                    agent.SetDestination(TargetPos);
                                }
                            }
                            break;
                    }
                    break;
            }
			
		}

		void Attack()
		{
			if (State != AIState.Attack || Target == null)
				return;
			
			Vector3 direction = Target.transform.position - transform.position;
			float angle = Vector3.Angle(direction, transform.forward);
			
			if(angle <= FieldOfView / 2f)
			{
				if (AttackSounds != null && AttackSounds.Length > 0)
				{
					int i = Random.Range (0, AttackSounds.Length);
					if (AttackSounds[i] != null)
						audioSource.PlayOneShot(AttackSounds[i]);	
				}

                AIDamage Damage = Attacks[Random.Range(0, Attacks.Length)];
                animator.SetFloat("AttackID", Damage.AnimationIndex);
                animator.SetTrigger("Attack");
				
				HealthSystem health = Target.GetComponent<HealthSystem>();
				if(health)
				{
                    if (!health.Alive)
					{
						Target = null;
						attackTimeTmp = 0f;
						return;
					}

                    float realDamage = Random.Range(Damage.Damage.Min, Damage.Damage.Max);
                    realDamage = Random.Range(0, 100) < Damage.CriticalChance ? realDamage : realDamage * Damage.CriticalMultiplier;
                    health.TakeDamage(realDamage, Core);

					if (!health.Alive)
                        Target = null;
                }
			}
			
			attackTimeTmp = 0f;
			State = AIState.Chase;
		}
		
		void SearchTarget()
		{
            searchTimeTmp = 0;
            Collider[] colliders = Physics.OverlapSphere(transform.position, EyeSight, TargetsLayer);
			Collider target = null;
			foreach (var collider in colliders) 
			{
				if (collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("AI"))
				{
                    AnimalCore core = collider.GetComponent<AnimalCore>();
					if (!core.Alive || core.Faction.Equals(Faction) || AIManager.Instance.GetFactionRelation(Core, core) != FactionRelation.Enemy)
                        continue;
					
                    target = collider;
					break;
				}
			}

			if (target == null) 
				return;

			var targetPos = target.transform.position;
			var distanceSqr = (transform.position - targetPos).sqrMagnitude;
			if (distanceSqr <= DistanceToAttack) 
			{
				Target = target.gameObject;
				return;
			}
			
			Vector3 targetDistance = (targetPos - transform.forward).normalized;
			float angle = Vector3.Angle(transform.forward, targetDistance);
			if (angle > FieldOfView / 2f)
				return;
			
			Target = target.gameObject;
		}

		public void OnTriggerEnter(Collider collider)
		{
            if (collider.gameObject.CompareTag("AI"))
			{
				if (Target != null && TargetsLayer.Contains(Target.layer) || collider.gameObject == Target)
                    return;
                Vector3 direction = collider.transform.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);
                float distToCollider = Vector3.Distance(collider.transform.position, transform.position);

                if (distToCollider <= DistanceToDetectNearEnemy || (distToCollider <= FieldOfView / 2 && angle <= FieldOfView / 2))
                {
					AnimalCore core = collider.GetComponent<AnimalCore>();
					if (!core.Alive || core.Faction.Equals(Faction) || AIManager.Instance.GetFactionRelation(Core, core) != FactionRelation.Enemy)
						return;
					
					Target = collider.gameObject;
				}
			}
		}

		public void OnTriggerStay(Collider collider)
		{
            if (collider.gameObject.CompareTag("Player"))
			{
				if (Target != null && TargetsLayer.Contains(Target.layer) || collider.gameObject == Target)
                    return;

                Vector3 direction = collider.transform.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);
				float distToCollider = Vector3.Distance(collider.transform.position, transform.position);

                if (distToCollider <= DistanceToDetectNearEnemy || (distToCollider <= FieldOfView / 2 && angle <= FieldOfView / 2))
				{
                    AnimalCore core = collider.GetComponent<AnimalCore>();
					if (!core.Alive || core.Faction.Equals(Faction) || AIManager.Instance.GetFactionRelation(Core, core) != FactionRelation.Enemy)
						return;
					
					
					Target = collider.gameObject;
                }
			}
		}

		public void FootstepsHandler()
		{
			if (!FootstepsHandle|| DistanceToPlayer() > FootstepRange)
                return;

			if (GameData.Instance.Platform == Platform.MOBILE) // Raycast on periods to save performance on mobile
			{
				if (VelocityMagnitude >= FootstepLength)
				{
					if (Physics.Raycast(transform.position, -transform.up, out rHit, 1f))
					{
						Quaternion quaternion = Quaternion.FromToRotation(transform.up, rHit.normal) * transform.rotation;
						transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, Time.deltaTime * TurnSpeed);
						audioSource.PlayOneShot(FootStepSound);
					}

					VelocityMagnitude = 0f;
				}
			}
			else
			{
				if (Physics.Raycast(transform.position, -transform.up, out rHit, 1f, MovementLayers))
				{
					Quaternion quaternion = Quaternion.FromToRotation(transform.up, rHit.normal) * transform.rotation;
					transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, Time.deltaTime * TurnSpeed);

					if (VelocityMagnitude >= FootstepLength)
					{
						audioSource.PlayOneShot(FootStepSound);
						VelocityMagnitude = 0f;
					}
				}
			}
		}

		public bool IsTargetingLeader()
		{
			if (FollowTarget != null && FollowTarget.gameObject == Target)
				return true;

            return false;
        }
		
		public float DistanceToTarget()
		{
			if (Target == null)
				return 1000f;
			
			return Vector3.Distance(Target.transform.position, transform.position);
		}

		public float DistanceToTargetPos()
		{
			return Vector3.Distance(TargetPos, transform.position);
		}
		
		public float DistanceToPlayer()
		{
			return Vector3.Distance(GlobalGameManager.Instance.LocalPlayer.transform.position, transform.position);
		}
		
		public Vector3 GetRandomArea(Vector3 current, float radius)
		{
			Vector2 NewArea = Random.insideUnitCircle * radius;
			return current + new Vector3(NewArea.x, 0, NewArea.y);
		}

		public Vector3 GetRandomPath(Vector3 current, float radius)
		{
            for (int i = 0; i < 10; i++)
            {
				Vector3 NewArea = current + (new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * radius);
                if (Physics.Raycast(NewArea, -transform.up, out RaycastHit hit, 512, MovementLayers, QueryTriggerInteraction.Ignore))
                {
                    NewArea = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    float distance = Vector3.Distance(NewArea, transform.position);
                    if (distance > agent.stoppingDistance + (radius * 0.1f) && NavMesh.SamplePosition(NewArea, out NavMeshHit nvHit, 100f, agent.areaMask))
						return nvHit.position;                        
                }
            }

			return current + (new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * radius);
        }
		
    }
}
