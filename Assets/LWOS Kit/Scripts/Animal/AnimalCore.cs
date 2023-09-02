using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
	public enum AnimalType : short
	{
		AI = 0,
		Player = 1
	}

    public class AnimalCore : MonoBehaviour
    {
		public AnimalType Type;
		[ConditionalField(nameof(Type), false, AnimalType.Player)]
		public AnimalController Player;
		[ConditionalField(nameof(Type), false, AnimalType.AI)]
		public AIAnimal AI;
        [AutoProperty] public HealthSystem healthSystem;
        [AutoProperty] public Animator animator;
        public bool Alive { get { return healthSystem.Alive; } }
		public short State { get { return Type == AnimalType.AI ? (short)AI.State : (short)Player.State; } }
		
		[Header("Data")]
		public int PackID;
		public PackData packData { get { return GameData.Instance.GetPack(PackID); } }
		public int Level { get { return Type == AnimalType.AI ? AI.Level : Player.Status.Level; } }
		public string Faction { get { return Type == AnimalType.AI ? AI.Faction : "PlayerPack"; } }
		
		[ConditionalField(nameof(Type), false, AnimalType.AI)]
		public GameObject DropItem;

		void Start()
        {
			Player = GetComponent<AnimalController>();
			AI = GetComponent<AIAnimal>();
        }
		
		public void IncreasePackExp(AnimalCore core)
		{
			if (!Faction.Equals("PlayerPack"))
				return;
			
			int Exp = core != null ? AIManager.Instance.GetFaction(core).Data.ExpPerKill : 30;
			GlobalGameManager.Instance.LocalPlayerController.Status.AddPackReward(Exp);
		}

		public void GivePlayerExpOnDeath()
		{
			if (AI == null)
				return;
			
			int Exp = packData.ExpPerKill;
			int wildness = packData.WildnessPerKill;
			GlobalGameManager.Instance.LocalPlayerController.Status.AddReward(Exp, wildness);
		}

		public void SetAILevel(int level, AnimalType type)
		{
			switch (type)
			{
				case AnimalType.AI:
					AILevelData levelData = packData.GetAILevel(Level);
					AI.Level = levelData.Level;
					healthSystem.Health = levelData.Health;
					AI.Attacks = levelData.Attacks;
                    break;
                case AnimalType.Player:
					PackLevelData packlevelData = GameData.Instance.GetPlayerPackLevel(Level);
					AI.Level = packlevelData.Level;
					healthSystem.Health = packlevelData.Health;
					AI.Attacks = packData.GetAILevel(level).Attacks;
					foreach (AIDamage attack in AI.Attacks)
					{
						if (attack.Damage.Min < packlevelData.Attack.Min)
                            attack.Damage.Min = packlevelData.Attack.Min;
						if (attack.Damage.Max > packlevelData.Attack.Max)
                            attack.Damage.Max = packlevelData.Attack.Max;


                        attack.CriticalChance = packlevelData.CriticalChance;
                        attack.CriticalMultiplier = packlevelData.CriticalMultiplier;
                    }
                    break;
            }
		}

		public void TameAI()
		{
			if (Type != AnimalType.AI || Level >= GlobalGameManager.Instance.LocalPlayerController.Status.Level)
				return;

            bool Tameable = Random.Range(0, 10) >= 5 ? true : false;
			if (Tameable)
			{
                AIManager.Instance.SpawnPlayerPack(transform.position, PackID);
                GlobalGameManager.Instance.SetSideText("TAMING DONE");
                healthSystem.Health = 0;
            }
			else
				GlobalGameManager.Instance.SetSideText("TAMING FAILED");
		}

		public void Die()
		{
			if (DropItem != null)
				Instantiate(DropItem, transform.position, Quaternion.identity);
			if (Type == AnimalType.AI)
				ObjectPool.Instance.Despawn(this, 10f);
		}

		public void ResetAI()
		{
            AI.State = AIState.Idle;
            AI.Target = null;
			healthSystem.Health = healthSystem.MaxHealth;
			healthSystem.Hunger = healthSystem.MaxHunger;
			healthSystem.Thirst = healthSystem.MaxThirst;
			healthSystem.Stamina = healthSystem.MaxStamina;
            healthSystem.Killer = null;
			healthSystem.isDead = false;
			healthSystem.GatherFood.SetActive(false);
        }
    }
}
