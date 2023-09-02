using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyBox;

namespace LWOS
{
	[CreateAssetMenu(menuName = "LSXGaming/LWOS/Global Item Data")]
	public class ItemData : ScriptableObject
	{
		public Sprite Icon;
		public string Name;
		public int ID;
		[SerializeField] private int price;
		public int Price { get { return price; } }
		public bool Owned { get { return LocalPrefs.GetBool(ID.ToString()); } }
		public ItemType Type;
		[ConditionalField(nameof(Type), false, ItemType.VITAL)]
		public GameObject DropPrefab;
		[ConditionalField(nameof(Type), false, ItemType.VITAL)]
		public ConsumableEffects ConsumableEffect = new ConsumableEffects();
		public UnityEvent EventsOnUse = null;
		public AudioClip[] UseSounds;
		
		[Serializable]
		public class ConsumableEffects
		{
			public float PickupTime;
			
			[Space(5f)]
			[Header("Life Behaviour")]
			public float Health;
			public float Hunger;
			public float Thirst;
			public float Stamina;
		}
	}
}
