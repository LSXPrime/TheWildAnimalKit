using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
    public enum Platform
	{
		PC,
		XBOX,
		PS4,
		MOBILE
	}
	
	public class GameData : ScriptableObject
	{
		[Header("Game Settings")]
		[Tooltip("The Platform you will Build Your Game For (Affecting some things like Mobile Inputs, etc)")]
		public string SecretKey;
		public Platform Platform;
        [Tooltip("The Maximium Frame Rate for the Game")]
		public int TargetFPS = 60;
		[Tooltip("The Maximium Frame Rate for the Game")]
		public float SaveInterval = 600f;
		public bool AimAssist = false;
		public float AimAssistRadius = 2f;

		[Header("Items")]
		public List<ItemData> Items = new List<ItemData>();

		[Header("Packs")]
		public List<PackData> Packs = new List<PackData>();
				
		[Header("Players")]
		[Tooltip("The Player Animal Prefab that will spawn in the world")]
		public AnimalController Player;
		[Tooltip("Levels List have All Player Level Data")]
		public List<LevelData> Levels = new List<LevelData>();
		public List<PackLevelData> PackLevels = new List<PackLevelData>();
		
		public ItemData GetItem(int ID)
		{
			foreach (ItemData item in Items)
			{
				if (item.ID == ID)
					return item;
			}
			
			return null;
		}

		public PackData GetPack(int ID)
		{
			foreach (PackData pack in Packs)
			{
				if (pack.ID == ID)
					return pack;
			}
			
			return null;
		}

        public PackLevelData GetPlayerPackLevel(int level)
		{
			foreach (PackLevelData pack in PackLevels)
			{
				if (pack.Level == level)
                    return pack;
            }

            return null;
        }
		
		private static GameData instance;
		public static GameData Instance
		{
			get
			{
				if (instance == null)
					instance = Resources.Load("GameData", typeof(GameData)) as GameData;

				return instance;
			}
		}
		
		public static IEnumerator LoadData()
		{
			if (instance == null)
			{
				ResourceRequest rr = Resources.LoadAsync("GameData", typeof(GameData));
				while (!rr.isDone) { yield return null; }
				instance = rr.asset as GameData;
			}
		}
	}
}
