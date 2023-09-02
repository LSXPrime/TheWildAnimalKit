using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
	[Serializable]
	public class LevelData
	{
		public string Name;
		public int Level;
		public int RequiredExp;
		public int RequiredExpToNextLevel;
		public int RewaredAttack;
		public int RewaredDefence;
		public int RewaredStatusPoints;
		public int PlayerMaxHealth;
		public int PlayerMaxStamina;
		public int PackMaxCount;
	}
}