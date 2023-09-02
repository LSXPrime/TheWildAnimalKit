using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LWOS
{
    public class AnimalStatus : MonoBehaviour
    {
		[Header("Player Details")]
		[SerializeField] private int level = 1;
		[SerializeField] private int wildness = 0;
		[SerializeField] private int attack = 5;
		[SerializeField] private int defence = 3;
		[SerializeField] private int extraAttack = 0;
		[SerializeField] private int extraDefence = 0;
		[SerializeField] private int exp = 0;
		[SerializeField] private int expToNextLevel = 0;
		[SerializeField] private int statusPoints = 0;
		[SerializeField] private int spawnPointID;
		public int Level { get { return level; } }
		public int Wildness { get { return wildness; } }
		public int Attack { get { return attack + extraAttack; } }
		public int Defence { get { return defence + extraDefence; } }
		public int Exp { get { return exp; } }
		public int ExpToNextLevel { get { return expToNextLevel; } }
		public int StatusPoints { get { return statusPoints; } }
        public int SpawnPointID { get => spawnPointID; set => spawnPointID = value; }

        [Header("Player Wolf Eye")]
        public float WolfEyeRaduis = 50f;
        public float WolfEyeUseTime = 5f;
		public float WolfEyeTimer = 60f;
		public float WolfEyeTimeScale = 0.5f;
		public LayerMask WolfEyeTargetsLayer;
		
		[Header("Pack Details")]
		public PackLevelData PackLevel;
		public int PackExp { get { return packExp; } }
		[SerializeField] private int packExp = 0;
		[SerializeField] private int packMaxCount = 0;
		public int PackMaxCount { get { return packMaxCount; } }
		public int PackCurrentCount
		{
			get
			{
				int Count = 0;
                for (int i = Pack.Count - 1; i > 0; i--)
                {
                    if (!Pack[i].Alive || !Pack[i].Faction.Equals("PlayerPack"))
                        Pack.RemoveAt(i);
                    else
                        Count++;
                }

                return Count;
			}
		}
		public int PackCount { get { return Pack.Count; } }
		[SerializeField] private int costPerSummon = 1;
		public int CostPerSummon { get { return costPerSummon; } }
		public enum PackFreedomEnum {Free = 0, FollowPlayer = 1}
		public enum SummonCostEnum {StatusPoints = 0, Health = 1}
		public PackFreedomEnum CurrentPackFreedom = PackFreedomEnum.FollowPlayer;
		public SummonCostEnum SummonCost;
		public List<AnimalCore> Pack = new List<AnimalCore>();

		[Header("Taming AI")]
		public LayerMask AITamingLayer;
		public float AITamingDistance = 10f;
        public float TamingTimer { get { return tamingTimer; } }
		[SerializeField] float tamingTimer = 240f;
		
		[Header("Referances")]
		[AutoProperty] public AnimalController Controller;
		[AutoProperty] public HealthSystem healthSystem;
		[AutoProperty] public AudioSource audioSource;
        [AutoProperty] public AnimalCore Core;
		public AudioClip PackCallSound;

        float tamingTimeTmp;
		internal float TamingTimeTmp { get { return tamingTimeTmp; } }
		float wolfEyeTimeTmp;
        internal float WolfEyeTimeTmp { get { return wolfEyeTimeTmp; } }
        float UtilityTimeTmp;

        void Start()
		{
			foreach (PackLevelData rank in GameData.Instance.PackLevels)
			{
				if (rank.RequiredExp <= packExp)
					PackLevel = rank;
			}
		}

        void OnEnable()
        {
			Extensions.onSaveData += SaveData;
			Extensions.onLoadData += LoadData;
        }
		
		void OnDisable()
        {
			Extensions.onSaveData -= SaveData;
			Extensions.onLoadData -= LoadData;
        }

		void Update()
		{
			UtilityTimeTmp += Time.deltaTime;
			if (wolfEyeTimeTmp < WolfEyeTimer)
                wolfEyeTimeTmp += Time.deltaTime;
			if (tamingTimeTmp < TamingTimer)
				tamingTimeTmp += Time.deltaTime;

			if (GameData.Instance.Platform == Platform.MOBILE)
			{
				GlobalGameManager.Instance.WolfEyeButtonIndictor.fillAmount = wolfEyeTimeTmp / WolfEyeTimer;
				GlobalGameManager.Instance.TameButtonIndictor.fillAmount = tamingTimeTmp / TamingTimer;
			}
			else
			{
				GlobalGameManager.Instance.TameIndictor.fillAmount = tamingTimeTmp / TamingTimer;
                GlobalGameManager.Instance.WolfEyeIndictor.fillAmount = wolfEyeTimeTmp / WolfEyeTimer;
            }

            if (InputManager.Instance.GetButtonDown("PackFreedom"))
				SetPackFreedom(CurrentPackFreedom == PackFreedomEnum.Free ? PackFreedomEnum.FollowPlayer : PackFreedomEnum.Free);

			if (InputManager.Instance.GetButtonDown("WolfEye") && wolfEyeTimeTmp >= WolfEyeTimer)
				StartCoroutine(UnleashWolfEye());

			AITaming();


            if (UtilityTimeTmp >= 5f)
			{
				int cleanNulls = PackCurrentCount;
				UtilityTimeTmp = 0;
			}
				
		}

		public IEnumerator UnleashWolfEye()
		{
            wolfEyeTimeTmp = 0;
            Time.timeScale = WolfEyeTimeScale;
			float unleashTimeTmp = 0;
            Controller.SpeedMultiplier = 2f;

            Collider[] colliders = Physics.OverlapSphere(transform.position, WolfEyeRaduis, WolfEyeTargetsLayer);
			List<GameObject> HostileTargets = new List<GameObject>();
            List<GameObject> NormalTargets = new List<GameObject>();
			UnityEngine.UI.Image[] WolfEyeHostileIndicators = GlobalGameManager.Instance.WolfEyeHostileIndicators;
			UnityEngine.UI.Image[] WolfEyeNormalIndicators = GlobalGameManager.Instance.WolfEyeNormalIndicators;
            foreach (var collider in colliders)
            {
				if (collider.gameObject.CompareTag("AI"))
				{
					AnimalCore core = collider.GetComponent<AnimalCore>();
					if (!core.Alive || core.Faction.Equals(Core.Faction) || AIManager.Instance.GetFactionRelation(Core, core) != FactionRelation.Enemy)
						continue;

					if (core.packData.isWild || (core.AI.Target != null && core.AI.Target == gameObject && !core.AI.IsTargetingLeader()))
                        HostileTargets.Add(core.gameObject);
					else
						NormalTargets.Add(core.gameObject);
				}
				else
					NormalTargets.Add(collider.gameObject);
            }

            GlobalGameManager.Instance.WolfEyePP.SetActive(true);
            GlobalGameManager.Instance.WolfEyeIndicators.SetActive(true);
            foreach (UnityEngine.UI.Image indicator in WolfEyeHostileIndicators) { indicator.enabled = false; }
            foreach (UnityEngine.UI.Image indicator in WolfEyeNormalIndicators) { indicator.enabled = false; }

            while (unleashTimeTmp < WolfEyeUseTime)
			{
				HostileTargets.RemoveAll(x => !x);
                NormalTargets.RemoveAll(x => !x);
				
                for (int i = 0; i < HostileTargets.Count; i++)
				{
					if (WolfEyeHostileIndicators.Length - 1 < i)
						break;

                    WolfEyeHostileIndicators[i].enabled = true;
                    Vector2 WSP = RectTransformUtility.WorldToScreenPoint(GlobalGameManager.Instance.Mcamera, HostileTargets[i].transform.position);
                    WolfEyeHostileIndicators[i].transform.position = new Vector3(WSP.x, WSP.y, 0);
                    WolfEyeHostileIndicators[i].color = Color.red;
                }
				
				for (int i = 0; i < NormalTargets.Count; i++)
				{
                    if (WolfEyeNormalIndicators.Length - 1 < i)
                        break;

					WolfEyeNormalIndicators[i].enabled = true;
					Vector2 WSP = RectTransformUtility.WorldToScreenPoint(GlobalGameManager.Instance.Mcamera, NormalTargets[i].transform.position);
					WolfEyeNormalIndicators[i].transform.position = new Vector3(WSP.x, WSP.y, 0);
                    WolfEyeNormalIndicators[i].color = Color.green;
                }

                unleashTimeTmp += Time.deltaTime;
				yield return new WaitForSeconds(Time.deltaTime);
            }

            GlobalGameManager.Instance.WolfEyePP.SetActive(false);
            GlobalGameManager.Instance.WolfEyeIndicators.SetActive(false);
            foreach (UnityEngine.UI.Image indicator in WolfEyeHostileIndicators) { indicator.enabled = false; }
			foreach (UnityEngine.UI.Image indicator in WolfEyeNormalIndicators) { indicator.enabled = false; }

            Controller.SpeedMultiplier = 1f;
            Time.timeScale = 1;
        }

		public void AITaming()
		{
			if (Physics.Raycast(transform.position, GlobalGameManager.Instance.Mcamera.transform.forward, out RaycastHit hit, AITamingDistance, AITamingLayer))
			{
                if (GameData.Instance.Platform == Platform.MOBILE)
				{
                    GlobalGameManager.Instance.TameButton.alpha = 1;
                    GlobalGameManager.Instance.TameButton.interactable = true;
                }
				else
					GlobalGameManager.Instance.TameIndictor.color = Color.white;

				if (tamingTimeTmp >= TamingTimer && InputManager.Instance.GetButtonDown("Tame"))
				{
					hit.collider.GetComponent<AnimalCore>().TameAI();
                    tamingTimeTmp = 0;
                }
			}
			else
			{
                if (GameData.Instance.Platform == Platform.MOBILE)
                {
                    GlobalGameManager.Instance.TameButton.alpha = 0;
                    GlobalGameManager.Instance.TameButton.interactable = false;
                }
                else
                    GlobalGameManager.Instance.TameIndictor.color = Color.red;
            }
		}

        public int SummonPackMemeber(int PackID)
		{
			if (PackCurrentCount >= packMaxCount)
				return 1;
			
			switch (SummonCost)
			{
				case SummonCostEnum.StatusPoints:
					if (statusPoints < CostPerSummon)
						return 2;
					statusPoints -= CostPerSummon;
					break;
				case SummonCostEnum.Health:
					if (healthSystem.Health < CostPerSummon + 1f)
						return 3;
					healthSystem.TakeDamage(CostPerSummon, Core);
					break;
			}

            AnimalCore animal = AIManager.Instance.SpawnPlayerPack(transform.position, PackID);
			Pack.Add(animal);

			return 0;
		}
		
		public void SetPackFreedom(PackFreedomEnum freedom)
		{
            audioSource.PlayOneShot(PackCallSound);
            Controller.animator.CrossFadeInFixedTime("Call", 0.1f);
            CurrentPackFreedom = freedom;
            foreach (AnimalCore animal in Pack)
            {
				animal.AI.Type = freedom == PackFreedomEnum.FollowPlayer ? AIType.Follower : animal.packData.LeaderBehavior;
                animal.AI.FollowTarget = freedom == PackFreedomEnum.FollowPlayer ? Core : null;
            }
		}
		
		public void AddStatu(string status, int amount)
		{
			switch(status)
			{
				case "Attack":
					extraAttack += amount;
					statusPoints -= amount;
					break;
				case "Defence":
					extraDefence += amount;
					statusPoints -= amount;
					break;
			}
		}
		
		public void SubstractWildness(int wpoints)
		{
			wildness -= wpoints;
		}
		
		public void AddReward(int expoints, int wpoints)
		{
			exp += expoints;
			wildness += wpoints;
			if (exp >= expToNextLevel)
				LevelUp();
		}
		
		public void AddPackReward(int expoints)
		{
			packExp += expoints;
			if (packExp >= PackLevel.RequiredExpToNextLevel)
			{
				PackLevelData realRank = null;
				foreach (PackLevelData rank in GameData.Instance.PackLevels)
				{
					if (rank.RequiredExp <= packExp)
						realRank = rank;
				}
				PackLevel = realRank;
				
				foreach (AnimalCore animal in Pack)
				{
					animal.SetAILevel(PackLevel.Level, AnimalType.Player);
				}

				SaveData();
			}
		}
		
		public void LevelUp()
		{
			int realRank = 0;
			foreach (LevelData rank in GameData.Instance.Levels)
			{
				if (rank.RequiredExp <= Exp)
					realRank = rank.Level;
			}
			level = realRank;
			
			foreach (LevelData rank in GameData.Instance.Levels)
			{
				if (rank.Level == level)
				{
					attack = rank.RewaredAttack;
					defence = rank.RewaredDefence;
					statusPoints += rank.RewaredStatusPoints;
					healthSystem.MaxHealth = rank.PlayerMaxHealth;
					healthSystem.MaxStamina = rank.PlayerMaxStamina;
                    packMaxCount = rank.PackMaxCount;
                    expToNextLevel = rank.RequiredExpToNextLevel;
					break;
				}
			}
			
			healthSystem.Health = healthSystem.MaxHealth;
			healthSystem.Stamina = healthSystem.MaxStamina;
			SaveData();
		}
		
		public void SaveData()
		{
			SaveData Data = new SaveData();
			Data.Level = level;
			Data.Wildness = wildness;
			Data.Attack = attack;
			Data.Defence = defence;
			Data.ExtraAttack = extraAttack;
			Data.ExtraDefence = extraDefence;
			Data.Exp = exp;
			Data.ExpToNextLevel = expToNextLevel;
			Data.StatusPoints = statusPoints;
			Data.SpawnPoint = spawnPointID;

			Data.Health = healthSystem.Health;
			Data.Hunger = healthSystem.Hunger;
			Data.Thirst = healthSystem.Thirst;
			Data.Stamina = healthSystem.Stamina;
			Data.MaxHealth = healthSystem.MaxHealth;
			Data.MaxStamina = healthSystem.MaxStamina;

			Data.PackLevel = PackLevel.Level;
			Data.PackExp = PackExp;
			Data.PackMaxCount = packMaxCount;
			Data.PackData = new List<AISaveData>();

            foreach (AnimalCore member in Pack)
            {
                AISaveData AIData = new AISaveData();

				AIData.PackID = member.PackID;
                AIData.Health = member.healthSystem.Health;
                AIData.Hunger = member.healthSystem.Hunger;
                AIData.Thirst = member.healthSystem.Thirst;
                AIData.Stamina = member.healthSystem.Stamina;

                Data.PackData.Add(AIData);
            }

            string path = Application.persistentDataPath + "/" + "lwos.ri";
			byte[] saveData = Extensions.Encrypt(Data);
            File.WriteAllBytes(path, saveData);
        }

        public void LoadData()
		{
            string path = Application.persistentDataPath + "/" + "lwos.ri";
			if (!File.Exists(path))
			{
#if RI_DEV
				//X Testing Purpose Start
				exp = 4800;
				statusPoints = 117;
				wildness = 72500;
				//X Testing Purpose End
#endif
				LevelUp();
                return;
            }
				

            byte[] encryptedData = File.ReadAllBytes(path);
            SaveData Data = (SaveData)Extensions.Decrypt<SaveData>(encryptedData);

			level = Data.Level;
			wildness = Data.Wildness;
			attack = Data.Attack;
			defence = Data.Defence;
			extraAttack = Data.ExtraAttack;
			extraDefence = Data.ExtraDefence;
			exp = Data.Exp;
			expToNextLevel = Data.ExpToNextLevel;
			statusPoints = Data.StatusPoints;
			spawnPointID = Data.SpawnPoint;

            healthSystem.MaxHealth = Data.MaxHealth;
            healthSystem.MaxStamina = Data.MaxStamina;
            healthSystem.Health = Data.Health;
			healthSystem.Hunger = Data.Hunger;
			healthSystem.Thirst = Data.Thirst;
			healthSystem.Stamina = Data.Stamina;

            PackLevel = GameData.Instance.GetPlayerPackLevel(Data.PackLevel);
            packExp = Data.PackExp;
			packMaxCount = Data.PackMaxCount;

            foreach (AISaveData AIData in Data.PackData)
            {
                AnimalCore animal = AIManager.Instance.SpawnPlayerPack(transform.position, AIData.PackID);

                animal.healthSystem.Health = AIData.Health;
                animal.healthSystem.Hunger = AIData.Hunger;
                animal.healthSystem.Thirst = AIData.Thirst;
                animal.healthSystem.Stamina = AIData.Stamina;

                Pack.Add(animal);
            }

        }


		
		private static AnimalStatus instance;
		public static AnimalStatus Instance
		{
			get
			{
				if (instance == null) { instance = FindObjectOfType<AnimalStatus>(); }
				return instance;
			}
		}
    }
}
