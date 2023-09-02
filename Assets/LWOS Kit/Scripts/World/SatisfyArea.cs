using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
    public class SatisfyArea : MonoBehaviour
    {
		[Header("Referances")]
		public VitalType Type;
        [ConditionalField(nameof(Type), false, VitalType.Hunger)]
        public bool ForWild = false;
		[ConditionalField(nameof(Limited), false, false)]
		public float VitalRegainRate = 0.2f;
		[ConditionalField(nameof(Limited), false, false)]
		public float VitalCriticalValue = 25f;
		[ConditionalField(nameof(Limited), false, false)]
		public float VitalCriticalMultiplier = 3f;
		[ConditionalField(nameof(Limited), false, false)]
		public float VitalNormalMultiplier = 1f;

        [Header("Pick Up")]
        public bool Limited;
		[ConditionalField(nameof(Limited))]
        public float Amount = 30f;

        public List<AnimalCore> animals = new List<AnimalCore>();
		private float timeTmp;

        private Coroutine pickUp;

        void Start()
		{
			Collider collider = GetComponent<Collider>();
			if (collider == null)
				collider = gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
		}
		
		void Update()
        {
			if (Limited || animals.Count == 0)
				return;

			/*
			if (animal.Input != Vector2.zero)
			{
				timeTmp += timeTmp >= 3f ? 0f : Time.deltaTime;
				return;
			}
			
			if (timeTmp >= 1f)
			{
				timeTmp -= Time.deltaTime;
				return;
			}
			*/
			animals.RemoveAll(x => !x);
			
			foreach (AnimalCore animal in animals)
			{
				animal.healthSystem.ImmortalVitals = (animal.State == 4);
				if ((animal.Type == AnimalType.Player && animal.Player.State != PlayerState.Satisfy) || (animal.Type == AnimalType.AI && animal.AI.State != AIState.Satisfy))
                    continue;
				
				switch (Type)
				{
					case VitalType.Health:
						animal.healthSystem.Health += animal.healthSystem.Health < animal.healthSystem.MaxHealth ? ((Time.deltaTime * VitalRegainRate) * (animal.healthSystem.Health <= VitalCriticalValue ? VitalCriticalMultiplier : VitalNormalMultiplier)) : 0f;
						break;
					case VitalType.Hunger:
						animal.healthSystem.Hunger += animal.healthSystem.Hunger < animal.healthSystem.MaxHunger ? ((Time.deltaTime * VitalRegainRate) * (animal.healthSystem.Hunger <= VitalCriticalValue ? VitalCriticalMultiplier : VitalNormalMultiplier)) : 0f;
						break;
					case VitalType.Thirst:
						animal.healthSystem.Thirst += animal.healthSystem.Thirst < animal.healthSystem.MaxThirst ? ((Time.deltaTime * VitalRegainRate) * (animal.healthSystem.Thirst <= VitalCriticalValue ? VitalCriticalMultiplier : VitalNormalMultiplier)) : 0f;
						break;
				}
			}
        }

		public void PickUp(AnimalCore animal)
		{
			if (!Limited)
				return;
			
            pickUp = StartCoroutine(PickUP(animal));
        }

		public IEnumerator PickUP(AnimalCore animal)
		{
            AnimatorStateInfo stateInfo = animal.animator.GetCurrentAnimatorStateInfo(1);
			SphereCollider collider = GetComponent<SphereCollider>();
			while (!stateInfo.IsName("Satisfy"))
			{
                yield return new WaitForSeconds(Time.deltaTime);;
				stateInfo = animal.animator.GetCurrentAnimatorStateInfo(1);
				if (animal.Type == AnimalType.Player)
				{
					GlobalGameManager.Instance.SatisfyButton.SetActive(true);
					if (InputManager.Instance.GetButtonDown("Satisfy")) 
					{
						animal.Player.State = PlayerState.Satisfy;
						animal.Player.audioSource.PlayOneShot(animal.Player.SatisfySound); 
					}
				}
				if (Vector3.Distance(transform.position, animal.transform.position) > collider.radius)
                    StopCoroutine(pickUp);
            }
            float stateTimeTmp = 0;
            float AmountPerFrame = Amount / stateInfo.length;
			
            while (stateTimeTmp < stateInfo.length)
			{
				switch (Type)
				{
					case VitalType.Health:
						animal.healthSystem.Health += animal.healthSystem.Health < animal.healthSystem.MaxHealth ? AmountPerFrame * Time.deltaTime : 0;
						break;
					case VitalType.Hunger:
						animal.healthSystem.Hunger += animal.healthSystem.Hunger < animal.healthSystem.MaxHunger ? AmountPerFrame * Time.deltaTime : 0;
						break;
					case VitalType.Thirst:
						animal.healthSystem.Thirst += animal.healthSystem.Thirst < animal.healthSystem.MaxThirst ? AmountPerFrame * Time.deltaTime : 0;
						break;
				}

				if (animal.Type == AnimalType.Player)
					GlobalGameManager.Instance.SatisfyButtonIndictor.fillAmount = stateTimeTmp / stateInfo.length;
				
                stateTimeTmp += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
			if (animal.Type == AnimalType.AI && animal.AI.Target == gameObject)
				animal.AI.Target = null;
			if (animal.Type == AnimalType.Player)
			{
				GlobalGameManager.Instance.SatisfyButton.SetActive(false);
                animal.Player.State = PlayerState.Idle;
            }				
			
            Destroy(gameObject);
        }
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") || other.CompareTag("AI"))
			{
				AnimalCore core = other.GetComponent<AnimalCore>();
				if (core.Type == AnimalType.Player)
				{
					GlobalGameManager.Instance.SatisfyButton.SetActive(true);
					if (InputManager.Instance.GetButtonDown("Satisfy")) 
					{ 
						core.Player.State = PlayerState.Satisfy;
						core.Player.audioSource.PlayOneShot(core.Player.SatisfySound); 
					}
				}
				if (Limited)
				{
					if (core.Type == AnimalType.AI)
					{
                        core.AI.Target = gameObject;
                        core.AI.State = AIState.Satisfy;
                    }
                    PickUp(core);
                    return;
                }
				
				animals.Add(core);
			}
		}
		
		private void OnTriggerStay(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				AnimalCore core = other.GetComponent<AnimalCore>();
				if (Limited)
				{
					if (core.Type == AnimalType.AI)
					{
                        core.AI.Target = gameObject;
                        core.AI.State = AIState.Satisfy;
                    }
                    PickUp(core);
                    return;
                }
				
				animals.Add(core);
			}
		}
		
		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player") || other.CompareTag("AI"))
			{
				AnimalCore core = other.GetComponent<AnimalCore>();
				core.healthSystem.ImmortalVitals = false;
				if (core.Type == AnimalType.Player) { GlobalGameManager.Instance.SatisfyButton.SetActive(false); core.Player.State = PlayerState.Walk;}
				
				if (animals.Contains(core))
                    animals.Remove(core);
            }
		}
    }
}
