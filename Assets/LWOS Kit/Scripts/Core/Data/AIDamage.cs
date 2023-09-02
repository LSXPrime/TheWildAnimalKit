using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace LWOS
{
    [System.Serializable]
	public class AIDamage
	{
        public int AnimationIndex = 0;
		[MinMaxRange(5, 100)] 
        public RangedFloat Damage = new RangedFloat(10f, 20f);
        [Range(0, 100)] public int CriticalChance = 25;
        public float CriticalMultiplier = 2.5f;
        public GameObject VFXImpact;
    }
		
}
