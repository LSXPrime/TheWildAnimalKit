using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
    [Serializable]
	public class PackData
	{
		public string Name;
        public Sprite Preview;
        public int ID;
		public bool isWild;
		public int ExpPerKill;
		public int WildnessPerKill;
        [MinMaxRange(1, 30)]
        public RangedInt PackMaxCount = new RangedInt(1, 9);
		public GameObject[] Animals;
        public AILevelData[] Levels;
		public AIType LeaderBehavior;
		public AIType PackBehavior;

        public AILevelData GetAILevel(int ID)
        {
            foreach (AILevelData level in Levels)
            {
                if (level.Level == ID)
                    return level;
            }

            return null;
        }
	}

    [Serializable]
    public class AILevelData
    {
        public int Level = 1;
        public float Health;
        public AIDamage[] Attacks;
    }

    [Serializable]
    public class PackLevelData
    {
        public int Level;
        public float Health;
        [MinMaxRange(5, 100)] 
        public RangedFloat Attack;
        [Range(0, 100)] public int CriticalChance = 25;
        public float CriticalMultiplier = 2.5f;
        public int RequiredExp;
        public int RequiredExpToNextLevel;
    }
}
