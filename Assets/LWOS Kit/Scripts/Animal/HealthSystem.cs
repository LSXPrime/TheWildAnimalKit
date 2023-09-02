using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;


namespace LWOS
{
    public class HealthSystem : MonoBehaviour
    {
		[Header("Referances")]
		[AutoProperty] public AudioSource audioSource;
        [AutoProperty] public AnimalCore Core;
        public ParticleSystem HitImpact;
		
        [Header("Life Behaviour")]
		public AudioClip HurtSFX;
		public bool Immortal;
		public bool ImmortalVitals;
		public bool Respawnable;
		public bool Alive = true;

        [Foldout("Health", true)]
        [SerializeField] private float health = 100f;
        public float Health { get => health; set => health = value; }
        [Range(0, 1000)] public float MaxHealth = 100f;
        public bool AutoRecoverHealth;
        [ConditionalField(nameof(AutoRecoverHealth))]
        public float RecoveryRate = 0.3f;
        [ConditionalField(nameof(AutoRecoverHealth))]
        public float RecoveryTime = 10f;
        [ConditionalField(nameof(AutoRecoverHealth))]
        public float RecoveryTargetHealth = 80f;

        [Foldout("Hunger", true)]
        [SerializeField] private float hunger = 100f;
        public float Hunger { get => hunger; set => hunger = value; }
        [Range(0, 100)] public float MaxHunger = 100f;
		[Range(0, 1)] public float HungerPerSec = 0.1f;

        [Foldout("Thirst", true)]
        [SerializeField] private float thirst = 100f;
        public float Thirst { get => thirst; set => thirst = value; }
        [Range(0, 100)] public float MaxThirst = 100f;
		[Range(0, 1)] public float ThirstPerSec = 0.1f;

        [Foldout("Stamina", true)]
        [SerializeField] private float stamina = 100f;
        public float Stamina { get => stamina; set => stamina = value; }
        [Range(0, 100)] public float MaxStamina = 100f;
		[Range(0, 10)] public float StaminaPerSec = 5f;
		
		[Foldout("Fall Damage", true)]
		public bool UseFallDamage = true;
		[ConditionalField(nameof(UseFallDamage))]
		public float FallDamagePerUnit = 10f;
		[ConditionalField(nameof(UseFallDamage))]
		public float SafeFallHeight = 7f;

        internal AnimalCore Killer;
        internal GameObject GatherFood;
        internal bool isDead;
        float killerTimeTmp;
		float recoverTimeTmp;
		

        void Update()
		{
			Alive = Health > 0f ? true : false;
            if (isDead)
                return;
            

            if (!Alive)
            {
				if (!Respawnable)
				{
                    if (Killer != null)
					{
						if (Killer.Type == AnimalType.Player)
							Core.GivePlayerExpOnDeath();
						else
							Killer.IncreasePackExp(Core);
					}
                    GatherFood.SetActive(true);
				//X	Destroy(gameObject);
				}
				else
				{
                    GlobalGameManager.Instance.CollectData();
                }

                Core.Die();
                isDead = true;
            }
            
			if (Killer != null)
			{
                killerTimeTmp += Time.deltaTime;
				if (killerTimeTmp >= 10f)
                    Killer = null;
            }
			
			LifeBehaviour();
		}
		
		public void LifeBehaviour()
		{
			if (Immortal || ImmortalVitals || !Alive)
				return;
			
			if (Thirst > 0f) thirst -= Time.deltaTime * ThirstPerSec;
			if (Hunger > 0f) hunger -= Time.deltaTime * HungerPerSec;
			if (Thirst <= 0f || Hunger <= 0f) health -= Time.deltaTime;
            if (Stamina < MaxStamina && Core.State != 2) stamina += Time.deltaTime * StaminaPerSec;
			if (AutoRecoverHealth && Health < RecoveryTargetHealth && Thirst > 0f && Hunger > 0f)
			{
				if (recoverTimeTmp > RecoveryTime)
					health += Time.deltaTime * RecoveryRate;
				else
					recoverTimeTmp += Time.deltaTime;
			}
			else
				recoverTimeTmp = 0f;
        }
		
		
		public void Respawn()
		{
			int pos = Random.Range(0, GlobalGameManager.Instance.RespawnLocations.Length);
			Transform RespawnPosition = GlobalGameManager.Instance.RespawnLocations[pos];
			transform.position = RespawnPosition.position;
			Health = MaxHealth;
			Hunger = MaxHunger;
			Thirst = MaxThirst;
			Stamina = MaxStamina;
            isDead = false;
        }
		
		public void TakeDamage(float damage, AnimalCore killer)
		{
			if (Immortal)
				return;

			HitImpact.Play();
            Health -= damage;
			if (HurtSFX != null) audioSource.PlayOneShot(HurtSFX, damage / MaxHealth);
            Killer = killer;
			if (Core.Type != AnimalType.Player)
                return;
			
            var dir = killer.transform.position - transform.position;
			var angle = Vector3.Angle(dir, transform.position);
			GlobalGameManager.Instance.DamageIndicator.transform.eulerAngles = new Vector3(0, 0, angle);
			GlobalGameManager.Instance.DamageIndicator.alpha = 1f;
		}
		
		public void FallDamage(float Point)
		{
			if (!UseFallDamage)
				return;
			
			float fallMultiply = (Point - transform.position.y) - SafeFallHeight;
			if (fallMultiply > 0)
			{
				float damage = FallDamagePerUnit * fallMultiply;
				TakeDamage(damage, Core);
			}
		}
		
		public void PickUp(ItemData.ConsumableEffects effects)
		{
			StartCoroutine(PickUpCO(effects.Health, effects.Hunger, effects.Thirst, effects.Stamina, effects.PickupTime));
		}
		
		IEnumerator PickUpCO(float amountHealth, float amountHunger, float amountThirst, float amountStamina, float timer)
		{
			float newHealth = amountHealth / timer;
			float newHunger = amountHunger / timer;
			float newThirst = amountThirst / timer;
			float newStamina = amountStamina / timer;
			
			while (timer > 0)
			{		
				Health += Health >= MaxHealth ? 0 : newHealth;
				Hunger += Hunger >= MaxHunger ? 0 : newHunger;
				Thirst += Thirst >= MaxThirst ? 0 : newThirst;
				Stamina += Stamina >= MaxStamina ? 0 : newStamina;
				timer -= 1f;
				yield return new WaitForSeconds(1f);;
			}
			
			if (Health > MaxHealth) Health = MaxHealth;
			if (Hunger > MaxHunger) Hunger = MaxHunger;
			if (Thirst > MaxThirst) Thirst = MaxThirst;
			if (Stamina > MaxStamina) Stamina = MaxStamina;
		}
    }
}
