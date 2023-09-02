using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LWOS
{
    public class StatusUI : MonoBehaviour
    {
		[Header("Referances")]
		public CanvasGroup[] Panels;
		public GameObject[] Popups;
        public float PanelFadeMultiplier = 3;
        public bool UpdateShop;
		public bool UpdatePack;
		public bool UpdatePackMember;

		[Header("Player Status Details")]
		public Text Level;
		public Text Attack;
		public Text Defence;
		public Text Wildness;
		public Text StatusPoints;
		public Text Exp;
		public Slider LevelProgress;
		public GameObject[] UpgradeButtons;
		
		[Header("Player Vitals Details")]
		public Image Health;
		public Image Stamina;
		public Image Food;
		public Image Water;
		
		[Header("Pack Details")]
		public GameObject PackUIPrefab;
		public Transform PackPanel;
		public List<PackMemberUI> PackMembers = new List<PackMemberUI>();
		public Transform AddPackMembersPanel;
		public List<ShopItemUI> AddPackMembers = new List<ShopItemUI>();
		public Text PackCount;
		public Text PackAttack;
		public Text PackHealth;
		public Text PackExp;
		public Text PackLevel;
		public Slider PackLevelProgress;


		[Header("Shop Details")]
		public GameObject ShopUIPrefab;
		public Transform ShopItemsPanel;
		public List<ShopItemUI> ShopItems = new List<ShopItemUI>();


		
		internal AnimalStatus player;
		
		void OnEnable()
		{
			if (UpdateShop)
			{
				for (int i = 0; i < ShopItems.Count; i++) { Destroy(ShopItems[i].gameObject); }
				ShopItems.Clear();
				foreach (ItemData item in GameData.Instance.Items)
				{
					GameObject GO = Instantiate(ShopUIPrefab) as GameObject;
					GO.transform.SetParent(ShopItemsPanel.transform, false);
					ShopItemUI Item = GO.GetComponent<ShopItemUI>();
					Item.Set(item);
					ShopItems.Add(Item);
				}
			}

			if (UpdatePack)
			{
				for (int i = 0; i < PackMembers.Count; i++) { Destroy(PackMembers[i].gameObject); }
				PackMembers.Clear();
				foreach (AnimalCore animal in player.Pack)
				{
					GameObject GO = Instantiate(PackUIPrefab) as GameObject;
					GO.transform.SetParent(PackPanel, false);
					PackMemberUI Item = GO.GetComponent<PackMemberUI>();
                    Item.Animal = animal;
					Item.Name.text = animal.packData.Name;
                    Item.Preview.sprite = animal.packData.Preview;
					PackMembers.Add(Item);
				}
			}

			if (UpdatePackMember)
			{
				for (int i = 0; i < AddPackMembers.Count; i++) { Destroy(AddPackMembers[i].gameObject); }
				AddPackMembers.Clear();
				foreach (ItemData item in GameData.Instance.Items)
				{
					if (item.Type != ItemType.MEMBER || !item.Owned)
						continue;
					
					GameObject GO = Instantiate(ShopUIPrefab) as GameObject;
					GO.transform.SetParent(AddPackMembersPanel, false);
					ShopItemUI Item = GO.GetComponent<ShopItemUI>();
					Item.Set(item);
					AddPackMembers.Add(Item);
				}
			}
		}
		
		void Update()
        {
			if(!player) 
			{
				if (GlobalGameManager.Instance.LocalPlayerController != null && GlobalGameManager.Instance.LocalPlayerController.Status != null)
					player = GlobalGameManager.Instance.LocalPlayerController.Status;
				
				return;
			}
			
			foreach(GameObject UB in UpgradeButtons)
			{
				UB.SetActive(player.StatusPoints > 0);
			}
			
			Wildness.text = "WILDNESS: " + player.Wildness.ToString();
			StatusPoints.text = "STATUS POINTS: " + player.StatusPoints.ToString();
			Level.text = "LV. " + player.Level.ToString();
			Attack.text = "ATTACK: " + player.Attack.ToString();
			Defence.text = "DEFENCE: " + player.Defence.ToString();
			Exp.text = string.Format("EXPERIANCE: {0} / {1}", player.Exp.ToString(), player.ExpToNextLevel.ToString());
			LevelProgress.value = player.Exp;
			LevelProgress.maxValue = player.ExpToNextLevel;
			
			Health.fillAmount = player.healthSystem.Health / player.healthSystem.MaxHealth;
			Stamina.fillAmount = player.healthSystem.Stamina / player.healthSystem.MaxStamina;
			Food.fillAmount = player.healthSystem.Hunger / player.healthSystem.MaxHunger;
			Water.fillAmount = player.healthSystem.Thirst / player.healthSystem.MaxThirst;


			PackHealth.text = string.Format("HEALTH: {0}" , player.PackLevel.Health);
			PackAttack.text = string.Format("ATTACK: {0} / {1}" , player.PackLevel.Attack.Min, player.PackLevel.Attack.Max);
			PackCount.text = string.Format("PACK: {0} / {1}" , player.PackCurrentCount, player.PackMaxCount);
			PackExp.text = string.Format("{0} / {1}" , player.PackExp, player.PackLevel.RequiredExpToNextLevel);
			PackLevel.text = "LV. " + player.PackLevel.Level;
			PackLevelProgress.value = player.PackExp;
			PackLevelProgress.maxValue = player.PackLevel.RequiredExpToNextLevel;
        }

		public void ChangePanel(bool next)
		{
			StartCoroutine(ChangePanelIE(next));
		}

		public IEnumerator ChangePanelIE(bool next)
		{
            int i = 0;
            for (int index = 0; index < Panels.Length; index++)
            {
				if (Panels[index].interactable)
					i = index;
			}
			foreach (GameObject go in Popups)
			{
				go.SetActive(false);
			}

				if (next)
                {
                    if (Panels[i].gameObject.activeSelf)
                    {
                        while (Panels[i].alpha > 0)
						{
							Panels[i].alpha -= Time.deltaTime * PanelFadeMultiplier;
							yield return new WaitForSeconds(Time.deltaTime);
						}
						Panels[i].interactable = false;
						Panels[i].blocksRaycasts = false;

						if (i == Panels.Length - 1)
						{
							while (Panels[0].alpha < 1)
							{
								Panels[0].alpha += Time.deltaTime * PanelFadeMultiplier;
								yield return new WaitForSeconds(Time.deltaTime);
							}
							Panels[0].interactable = true;
							Panels[0].blocksRaycasts = true;
						}
						else
						{
							while (Panels[i+1].alpha < 1)
							{
								Panels[i+1].alpha += Time.deltaTime * PanelFadeMultiplier;
								yield return new WaitForSeconds(Time.deltaTime);
							}
							Panels[i+1].interactable = true;
							Panels[i+1].blocksRaycasts = true;
						}
					}
				}
				else
				{
					if (Panels[i].gameObject.activeSelf)
					{
						while (Panels[i].alpha > 0)
						{
							Panels[i].alpha -= Time.deltaTime * PanelFadeMultiplier;
							yield return new WaitForSeconds(Time.deltaTime);
						}
						Panels[i].interactable = false;
						Panels[i].blocksRaycasts = false;

						if (i == 0)
						{
							while (Panels[Panels.Length - 1].alpha < 1)
							{
								Panels[Panels.Length - 1].alpha += Time.deltaTime * PanelFadeMultiplier;
								yield return new WaitForSeconds(Time.deltaTime);
							}
							Panels[Panels.Length - 1].interactable = true;
							Panels[Panels.Length - 1].blocksRaycasts = true;
						}
						else
						{
							while (Panels[i-1].alpha < 1)
							{
								Panels[i-1].alpha += Time.deltaTime * PanelFadeMultiplier;
								yield return new WaitForSeconds(Time.deltaTime);
							}
							Panels[i-1].interactable = true;
							Panels[i-1].blocksRaycasts = true;
						}
					}
				}
		}
		
		public void UpgradeStatus(string status)
		{
			if(!player || player.StatusPoints <= 0) return;
			
			player.AddStatu(status, 1);
		}

		public void AddPackMember(int PackID)
		{
			int task = player.SummonPackMemeber(PackID);
			switch (task)
			{
				case 0:
					GlobalGameManager.Instance.SetSideText("MEMBER ADDED");
					OnEnable();
					break;
				case 1:
					GlobalGameManager.Instance.SetSideText("PACK MAXED OUT");
					break;
				case 2:
					GlobalGameManager.Instance.SetSideText("INSUFFICIENT SP");
					break;
				case 3:
					GlobalGameManager.Instance.SetSideText("INSUFFICIENT HP");
					break;
			}
		}
		
		public void ItemBuy(ItemData Item)
		{
			if (Item.Price > player.Wildness)
			{
				GlobalGameManager.Instance.SetSideText("INSUFFICIENT WILDNESS");
				return;
			}
			
			switch (Item.Type)
			{
				case ItemType.SKIN:
					LocalPrefs.SetBool(Item.ID.ToString(), true);
					break;
				case ItemType.MEMBER:
					LocalPrefs.SetBool(Item.ID.ToString(), true);
                    break;
                case ItemType.VITAL:
					player.healthSystem.PickUp(Item.ConsumableEffect);
					break;
			}

			player.SubstractWildness(Item.Price);
			GlobalGameManager.Instance.SetSideText("PURCHASE COMPLETE");
		}

		public void SetPlayerCharacter(int ID)
		{
			LocalPrefs.SetInt("PlayerSkin", ID);
			RuntimeMaterialChanger materialChanger = player.GetComponentInChildren<RuntimeMaterialChanger>();
			materialChanger.isDone = false;
            materialChanger.enabled = true;

            foreach (ShopItemUI item in ShopItems)
			{
				if (item.Selected == null)
					continue;
				
				item.Selected.enabled = item.Item.ID == ID ? true : false;
			}
		}

		public void Back()
		{
			foreach (CanvasGroup cg in Panels)
			{
				cg.alpha = 0;
				cg.interactable = false;
				cg.blocksRaycasts = false;
			}

			foreach (GameObject go in Popups)
			{
				go.SetActive(false);
			}

			Panels[0].alpha = 1;
			Panels[0].interactable = true;
			Panels[0].blocksRaycasts = true;
            GlobalGameManager.Instance.PauseHandler("Resume");
        }

		private static StatusUI instance;
		public static StatusUI Instance
		{
			get
			{
				if (instance == null) { instance = FindObjectOfType<StatusUI>(); }
				return instance;
			}
		}
    }
}
