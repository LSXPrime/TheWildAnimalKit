using UnityEngine;
using UnityEngine.UI;
using MyBox;

namespace LWOS
{
    public class AIAnimalUI : MonoBehaviour
    {
        [AutoProperty] public Canvas ParentCanvas;
        public Text Name;
		public Text Level;
		public Image HealthIMG;
		public Image HungerIMG;
        public Image ThirstIMG;
        public GameObject GatherFood;

        public AnimalCore Animal;

        void Awake()
        {
            if (Animal == null)
                Animal = GetComponentInParent<AnimalCore>();

            ParentCanvas.worldCamera = GlobalGameManager.Instance.Mcamera;
            Animal.healthSystem.GatherFood = GatherFood;
            GatherFood.SetActive(false);

            Name.text = Animal.packData.Name;
            Level.text = "LV." + Animal.Level;
        }

        void LateUpdate()
        {
            if (AIManager.Instance.EnableOptimization)
                ParentCanvas.enabled = Vector3.Angle(transform.position, GlobalGameManager.Instance.Mcamera.transform.forward) < AIManager.Instance.AICullFov ? true : false;

            if (!ParentCanvas.enabled)
                return;

            HealthIMG.fillAmount = Animal.healthSystem.Health / Animal.healthSystem.MaxHealth;
			HungerIMG.fillAmount = Animal.healthSystem.Hunger / Animal.healthSystem.MaxHunger;
            ThirstIMG.fillAmount = Animal.healthSystem.Thirst / Animal.healthSystem.MaxThirst;
            transform.LookAt(-new Vector3 (GlobalGameManager.Instance.Mcamera.transform.position.x, transform.position.y, GlobalGameManager.Instance.Mcamera.transform.position.z));
        }
    }
}
