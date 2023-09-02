using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
	public class AIManager : MonoBehaviour
	{
		[Foldout("Packs", true)]
		public SerializedDictionary<int, PackDataEXT> Packs = new SerializedDictionary<int, PackDataEXT>();
		public List<FactionData> AIFactions = new List<FactionData>();

		[Foldout("Optimization", true)]
		public bool EnableOptimization = true;
        public List<AnimalCore> Animals = new List<AnimalCore>();
        public float AICullDistance = 75;
        public float AICullFov = 75;
		public int MaxActiveAnimals = 12;

        int activatedAnimals = 0;
        float utilityTimeTmp;

        private void LateUpdate()
        {
			utilityTimeTmp += Time.deltaTime;
            if (utilityTimeTmp < 1f)
				return;
            utilityTimeTmp = 0f;
			activatedAnimals = 0;

            for (int i = Animals.Count - 1; i > 0; i--)
            {
                if (Animals[i] == null)
                {
                    Animals.RemoveAt(i);
                    continue;
                }
				
				if (!EnableOptimization)
					continue;
				
                float distance = Vector3.Distance(Animals[i].transform.position, GlobalGameManager.Instance.LocalPlayer.transform.position);
                float angle = Vector3.Angle(Animals[i].transform.position - GlobalGameManager.Instance.Mcamera.transform.position, GlobalGameManager.Instance.Mcamera.transform.forward);

                switch (Animals[i].gameObject.activeSelf)
                {
                    case true:
						if (distance > AICullDistance || angle > AICullFov)
							Animals[i].gameObject.SetActive(false);
						else
							activatedAnimals++;
                        break;
                    case false:
                        if (distance < AICullDistance && angle < AICullFov && activatedAnimals < MaxActiveAnimals)
						{
                            Animals[i].gameObject.SetActive(true);
							activatedAnimals++;
						}
                        break;
                }
            }

            foreach (int pack in Packs.Keys)
            {
                if (Packs[pack].Leader == null && Packs[pack].Members.Count == 0)
                {
                    Packs.Remove(pack);
                    return;
                }

                if ((Packs[pack].Leader == null || !Packs[pack].Leader.Alive) && Packs.Values.Count != 0)
                {
                    AnimalCore Leader = Packs[pack].Members[Random.Range(0, Packs[pack].Members.Count)];
                    SetPackLeadership(Packs[pack], Leader);
                }
            }

            foreach (FactionData faction in AIFactions)
            {
                if (faction.Members.Values.Count == 0)
                {
                    AIFactions.Remove(faction);
                    return;
                }

                foreach (string pack in faction.Members.Keys)
                {
                    if (faction.Members[pack].Leader == null && faction.Members[pack].Members.Count == 0)
                    {
                        faction.Members.Remove(pack);
                        return;
                    }

                    if ((faction.Members[pack].Leader == null || !faction.Members[pack].Leader.Alive) && faction.Members.Values.Count != 0)
                    {
                        AnimalCore Leader = faction.Members[pack].Members[Random.Range(0, faction.Members[pack].Members.Count)];
                        SetPackLeadership(faction.Members[pack], Leader);
                    }
                }
            }
        }

        public void SpawnPack(Transform[] Pos, int ID, Transform[] waypoints)
		{
			PackData packData = GameData.Instance.GetPack(ID);
			
			PackDataEXT pack = new PackDataEXT();
			pack.Data = packData;
			pack.ID = Packs.Count + 1;
			Packs.Add(pack.ID, pack);

			FactionData faction = GetFaction(ID);
			if (faction == null)
			{
				faction = new FactionData();
				faction.ID = ID;
				AIFactions.Add(faction);
			}
			string factionName = string.Format("{0}{1}", faction.Data.Name, (faction.Members.Count + 1));
			faction.Members.Add(factionName, pack);
			Extensions.SetPackFaction(factionName, "Neutral", packData.isWild);

			int AnimalCount = Random.Range(packData.PackMaxCount.Min, packData.PackMaxCount.Max);
			for (int i = 0; i < AnimalCount; i++)
			{
				AnimalCore animal = ObjectPool.Instance.Spawn(packData.Animals[Random.Range(0, packData.Animals.Length)], Pos[Random.Range(0, Pos.Length)].position + (Vector3)Random.insideUnitCircle, Quaternion.identity);
                animal.AI.Waypoints = waypoints;
                animal.SetAILevel(AnimalStatus.Instance.PackLevel.Level, AnimalType.AI);
				if (pack.Leader == null)
				{
					pack.Leader = animal;
					animal.AI.Type = packData.LeaderBehavior;
				}
				else
				{
					animal.AI.Type = packData.PackBehavior;
					if (animal.AI.Type == AIType.Follower)
						animal.AI.FollowTarget = pack.Leader;
				}
				
				animal.AI.Faction = factionName;
				pack.Members.Add(animal);
			}
			
		}
		
		public AnimalCore SpawnPlayerPack(Vector3 Pos, int ID)
		{
			AnimalCore player = GlobalGameManager.Instance.LocalPlayerController.Core;
			PackDataEXT pack = null;
			foreach (PackDataEXT packEXT in Packs.Values)
			{
				if (packEXT.Contains(player))
					pack = packEXT;
			}
			
			if (pack == null)
			{
				pack = new PackDataEXT();
				pack.Data = GameData.Instance.GetPack(ID);
				pack.ID = Packs.Count + 1;
				pack.Leader = player;
				Packs.Add(pack.ID, pack);
			}
			
			FactionData faction = GetFaction(ID);
			string factionName = "PlayerPack";
			if (faction == null)
			{
				faction = new FactionData();
				faction.ID = pack.Data.ID;
				AIFactions.Add(faction);
				faction.Members.Add(factionName, pack);
				Extensions.SetPackFaction(factionName, "Neutral", pack.Data.isWild);
			}
			
			AnimalCore animal = ObjectPool.Instance.Spawn(pack.Data.Animals[Random.Range(0, pack.Data.Animals.Length)], Pos + (Vector3)Random.insideUnitCircle, Quaternion.identity);
			animal.AI.Type = pack.Data.PackBehavior;
			animal.AI.FollowTarget = pack.Leader;
			animal.AI.Faction = factionName;
			animal.SetAILevel(player.Player.Status.PackLevel.Level, AnimalType.Player);
			pack.Members.Add(animal);
            player.Player.Status.Pack.Add(animal);

            return animal;
		}

		/*
		public void ChangeFaction(AnimalCore animal, string Name)
		{
			if (animal.Type != AnimalType.AI)
				return;
			
			FactionData OldFaction = GetFaction(animal);
			PackDataEXT OldPack = OldFaction.GetFactionData(animal.AI);
			
			OldPack.Members.Remove(animal);

			PackDataEXT NewPack = GetFactionPack(Name);
			if (NewPack == null)
				return;
			
			NewPack.Members.Add(animal);

			animal.AI.FollowTarget = NewPack.Leader;
			GlobalGameManager.Instance.LocalPlayerController.Status.Pack.Add(animal);
		}
		*/

		public FactionRelation GetFactionRelation(AnimalCore core, AnimalCore target)
		{
			FactionRelation relation = FactionRelation.Natural;
			if (core.packData.isWild || target.packData.isWild)
				relation = FactionRelation.Enemy;
			if (core.PackID == target.PackID)
                relation = FactionRelation.Natural;
			if (core.Faction.Equals(target.Faction))
                relation = FactionRelation.Friendly;
			
			return relation;
		}

		/*
		public FactionRelation GetFactionRelation(AnimalCore core, AnimalCore target)
		{
			foreach (FactionData faction in AIFactions)
			{
				foreach (PackDataEXT pack in faction.Members.Values)
				{
					foreach (AnimalCore animal in pack.Members.Values)
					{
						if (animal == core)
							return faction.GetFactionRelation(target.Faction);
					}
				}
			}
			
			return FactionRelation.Natural;
		}
		*/

		public void SetPackLeadership(PackDataEXT pack, AnimalCore Leader)
		{
            pack.Leader = Leader;
			Leader.AI.Type = pack.Data.LeaderBehavior;
			foreach (AnimalCore animal in pack.Members)
			{
				if (animal == null || animal.Type != AnimalType.AI)
                    continue;
				
                animal.AI.FollowTarget = Leader;
            }
        }

		public void SetFactionRelation(int targetFaction, FactionRelation relation)
		{
			foreach (FactionData faction in AIFactions)
			{
                faction.SetFactionRelation(targetFaction, relation);
            }
		}
		
		public FactionData GetFaction(int ID)
		{
			foreach (FactionData faction in AIFactions)
			{
				if (faction.Data.ID == ID)
					return faction;
			}
			
			return null;
		}
		
		public FactionData GetFaction(string Name)
		{
			foreach (FactionData faction in AIFactions)
			{
				foreach (string pack in faction.Members.Keys)
				{
					if (pack == Name)
						return faction;
				}
			}
			
			return null;
		}

		public PackDataEXT GetFactionPack(string Name)
		{
			foreach (FactionData faction in AIFactions)
			{
				foreach (string pack in faction.Members.Keys)
				{
					if (pack == Name)
						return faction.Members[pack];
				}
			}
			
			return null;
		}
		
		public FactionData GetFaction(AnimalCore core)
		{
			foreach (FactionData faction in AIFactions)
			{
				foreach (PackDataEXT pack in faction.Members.Values)
				{
					foreach (AnimalCore animal in pack.Members)
					{
						if (animal == core)
							return faction;
					}
				}
			}
			
			return null;
		}
		
		public string GetFactionName(AIAnimal AI)
		{
			foreach (FactionData faction in AIFactions)
			{
				foreach (string pack in faction.Members.Keys)
				{
					foreach (AnimalCore animal in faction.Members[pack].Members)
					{
						if (animal.AI == AI)
							return pack;
					}
				}
			}
						
			return string.Empty;
		}
		
		public int GetFactionIndex(string Name)
		{
			foreach (FactionData factionData in AIFactions)
			{
				foreach (string faction in factionData.Members.Keys)
				{
					if (faction == Name)
						return factionData.Members.Values.ToList().IndexOf(factionData.Members[faction]);
				}
			}
			
			return -1;
		}
		
		private static AIManager instance;
		public static AIManager Instance
		{
			get
			{
				if (instance == null) { instance = FindObjectOfType<AIManager>(); }
				return instance;
			}
		}
	}
	
	[System.Serializable]
	public class PackDataEXT
	{
		public PackData Data;
		public int ID;
		public AnimalCore Leader;
		public List<AnimalCore> Members = new List<AnimalCore>();
		
		public bool Contains(AnimalCore Animal)
		{
			foreach (AnimalCore animal in Members)
			{
				if (animal == Animal)
					return true;
			}
			
			return false;
		}
	}
	
	[System.Serializable]
	public class FactionData
	{
		public string Name { get { return Data != null ? Data.Name : string.Empty; } }
		public int ID;
		public PackData Data { get { return GameData.Instance.GetPack(ID); } }
		public SerializedDictionary<string, PackDataEXT> Members = new SerializedDictionary<string, PackDataEXT>();
		public SerializedDictionary<int, FactionRelation> Relations = new SerializedDictionary<int, FactionRelation>();
		
		public FactionRelation GetFactionRelation(int AnimalFaction)
		{
			foreach (int faction in Relations.Keys)
			{
				if (faction.Equals(AnimalFaction))
                    return Relations[faction];
			}
			
			return FactionRelation.Natural;
		}

		public void SetFactionRelation(int AnimalFaction, FactionRelation relation)
		{
			if (Relations.ContainsKey(AnimalFaction))
				Relations[AnimalFaction] = relation;
			else
                Relations.Add(AnimalFaction, relation);
        }

		public string GetFaction(AnimalCore Animal)
		{
			foreach (string faction in Members.Keys)
			{
				foreach (AnimalCore animal in Members[faction].Members)
				{
					if (animal == Animal)
						return faction;
				}
			}
			
			return string.Empty;
		}
		
		public string GetFaction(AnimalController Animal)
		{
			foreach (string faction in Members.Keys)
			{
				foreach (AnimalCore animal in Members[faction].Members)
				{
					if (animal.Player == Animal)
						return faction;
				}
			}
			
			return string.Empty;
		}
		
		public string GetFaction(AIAnimal Animal)
		{
			foreach (string faction in Members.Keys)
			{
				foreach (AnimalCore animal in Members[faction].Members)
				{
					if (animal.AI == Animal)
						return faction;
				}
			}
			
			return string.Empty;
		}

		public PackDataEXT GetFactionData(AIAnimal Animal)
		{
			foreach (string faction in Members.Keys)
			{
				foreach (AnimalCore animal in Members[faction].Members)
				{
					if (animal.AI == Animal)
						return Members[faction];
				}
			}
			
			return null;
		}
		
		public int GetFactionIndex(AIAnimal Animal)
		{
			foreach (PackDataEXT faction in Members.Values)
			{
				foreach (AnimalCore animal in faction.Members)
				{
					if (animal.AI == Animal)
						return Members.Values.ToList().IndexOf(faction);
				}
			}
			
			return -1;
		}
		
	}

    public enum FactionRelation : short
    {
        Natural = 0,
        Friendly = 1,
        Enemy = 2
    }
}
