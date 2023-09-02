using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace LWOS
{
    public class GlobalGameManager : MonoBehaviour
    {
		[Foldout("Settings", true)]
		public UIState State;
		public Transform[] RespawnLocations;
		
		[Foldout("Camera", true)]
		public Cinemachine.CinemachineVirtualCamera CamController;
		public Camera Mcamera;
		public GameObject WolfEyePP;
		
		[Foldout("Life Behaviour UI", true)]	
		public Image HealthBar;
		public Image StaminaBar;
		public Image FoodBar;
		public Image WaterBar;
		public CanvasGroup DamageIndicator;
		public float DamageIndicatorFadeSpeed = 2f;
		
		[Foldout("Elements UI", true)]
		public GameObject SideTextGO;
		public GameObject GameUI;
		public GameObject PausePanel;
		public GameObject SettingsPanel;
		public StatusUI StatusPanel;
	    public MinimapUI MinimapUI;
		public GameObject WolfEyeIndicators;
        public Image[] WolfEyeHostileIndicators;
		public Image[] WolfEyeNormalIndicators;

        [Foldout("Controls UI", true)]
		public GameObject MobileControls;
		public CanvasGroup TameButton;
        public Image TameButtonIndictor;
		public Image TameIndictor;
        public GameObject SatisfyButton;
        public Image SatisfyButtonIndictor;
		public Image WolfEyeButtonIndictor;
        public Image WolfEyeIndictor;
        public Image PackButton;
        public Sprite CallPack;
        public Sprite FreePack;
		
		[Foldout("Results UI", true)]
		public GameObject ResultsUI;
		public Text FinalLevel;
		public Text FinalExp;
		public Slider FinalLevelProgress;

        internal GameObject LocalPlayer;
        internal AnimalController LocalPlayerController;
        private float SaveTimeTmp;
		
		
		void Start()
		{
			StartCoroutine(GameData.LoadData());
			SetLocalPlayer();
			Extensions.LoadGameData();
            MobileControls.SetActive(GameData.Instance.Platform == Platform.MOBILE);
            InputManager.Instance.platform = GameData.Instance.Platform;
        }
		
		public void SetLocalPlayer()
		{
			LocalPlayer = Instantiate(GameData.Instance.Player.gameObject, RespawnLocations[Random.Range(0, RespawnLocations.Length)].position, Quaternion.identity);
			LocalPlayerController = LocalPlayer.GetComponent<AnimalController>();
			CamController.Follow = LocalPlayer.transform;
			CamController.LookAt = LocalPlayer.transform;
			MinimapUI.player = LocalPlayer.transform;
			StatusPanel.player = LocalPlayerController.Status;
		}
		
		void Update()
		{
			if (!LocalPlayer)
				return;
			
			LocalPlayerController.GetInput = State == UIState.Play;
			LocalPlayerController.Speed = State == UIState.Play ? LocalPlayerController.Speed : 0f;
			
			PanelsHandler();
			LifeBehaviourUI();
			EffectsHandler();
			SaveTimeTmp += Time.deltaTime;
			if (SaveTimeTmp >= GameData.Instance.SaveInterval)
			{
				Extensions.SaveGameData();
				SaveTimeTmp = 0f;
				SetSideText("GAME SAVED");
			}
		}

		// Why LateUpdate? SetActive Have a GC Allocation, let's keep it low
		public void LateUpdate()
		{
			MinimapUI.gameObject.SetActive(State == UIState.Play);
            PackButton.gameObject.SetActive(LocalPlayerController.Status.PackCount > 0);
            PackButton.sprite = LocalPlayerController.Status.CurrentPackFreedom == AnimalStatus.PackFreedomEnum.Free ? CallPack : FreePack;
			Cursor.visible = State != UIState.Play;
			Cursor.lockState = State != UIState.Play ? CursorLockMode.None : CursorLockMode.Locked;
        }
		
		public void PanelsHandler()
		{
			if (InputManager.Instance.GetButtonDown("Cancel"))
			{
				State = State == UIState.Pause ? UIState.Play : UIState.Pause;
				PauseHandler(string.Empty);
			}
						
			PausePanel.gameObject.SetActive(State == UIState.Pause);
			StatusPanel.gameObject.SetActive(State == UIState.Status);
            SettingsPanel.gameObject.SetActive(State == UIState.Settings);
        }
		
		public void PauseHandler(string menu)
		{
			switch (menu)
			{
				case "Resume":
					State = UIState.Play;
					break;
				case "Status":
					State = UIState.Status;
					break;
				case "Settings":
					State = UIState.Settings;
					break;
				case "Leave":
					Leave();
					break;
			}
		}
		
		public void EffectsHandler()
		{
			if (LocalPlayer == null || LocalPlayerController == null)
				return;
			
			DamageIndicator.alpha = Mathf.Lerp(DamageIndicator.alpha, 0f, DamageIndicatorFadeSpeed * Time.deltaTime);
		}
		
		public void SetSideText(string side)
		{
			StartCoroutine(SideText(side));
		}
		
		IEnumerator SideText(string side)
		{
			SideTextGO.SetActive(true);
			Text SideText = SideTextGO.GetComponentInChildren<Text>();
			SideText.text = side;
			yield return new WaitForSeconds(3);
			SideTextGO.SetActive(false);
			SideText.text = string.Empty;
		}
		
		void LifeBehaviourUI()
		{
			if (LocalPlayerController == null)
				return;
			
			HealthBar.fillAmount = LocalPlayerController.healthSystem.Health / LocalPlayerController.healthSystem.MaxHealth;
			StaminaBar.fillAmount = LocalPlayerController.healthSystem.Stamina / LocalPlayerController.healthSystem.MaxStamina;
			FoodBar.fillAmount = LocalPlayerController.healthSystem.Hunger / LocalPlayerController.healthSystem.MaxHunger;
			WaterBar.fillAmount = LocalPlayerController.healthSystem.Thirst / LocalPlayerController.healthSystem.MaxThirst;
		}
		
		public void CollectData()
		{
			FinalLevel.text = string.Format("<size=100>{0}</size>\nLV.", LocalPlayerController.Status.Level);
			FinalExp.text = string.Format("EXPERIANCE: {0} / {1}", LocalPlayerController.Status.Exp, LocalPlayerController.Status.ExpToNextLevel);
            FinalLevelProgress.value = LocalPlayerController.Status.Exp;
			FinalLevelProgress.maxValue = LocalPlayerController.Status.ExpToNextLevel;
            ResultsUI.SetActive(true);
			State = UIState.Results;
		}

		public void Respawn()
		{
			if (LocalPlayerController == null)
				return;
			
			LocalPlayerController.healthSystem.Respawn();
			ResultsUI.SetActive(false);
			State = UIState.Play;
		}
		
		public void Leave()
		{
			Extensions.SaveGameData();
			Application.Quit();
		}
		
		
		private static GlobalGameManager instance;
		public static GlobalGameManager Instance
		{
			get
			{
				if (instance == null) { instance = FindObjectOfType<GlobalGameManager>(); }
				return instance;
			}
		}
    }
}
