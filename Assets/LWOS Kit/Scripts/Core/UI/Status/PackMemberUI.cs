using UnityEngine;
using UnityEngine.UI;

namespace LWOS
{
    public class PackMemberUI : MonoBehaviour
	{
		public Text Name;
		public Image Preview;
		public Text Health;
		public Image HealthIMG;
		public Text Needs;
		public Image HungerIMG;
        public Image ThirstIMG;

        internal AnimalCore Animal;
		
		void Update()
		{
			if (Animal == null)
			{
				StatusUI.Instance.PackMembers.Remove(this);
				Destroy(gameObject);
				return;
			}

            Health.text = string.Format("Health: {0} / {1}" , Animal.healthSystem.Health, Animal.healthSystem.MaxHealth);
			HealthIMG.fillAmount = Animal.healthSystem.Health / Animal.healthSystem.MaxHealth;
			Needs.text = string.Format("Hunger: {0} / {1}\nThirst: {2} / {3}" , Animal.healthSystem.Hunger, Animal.healthSystem.MaxHunger, Animal.healthSystem.Thirst, Animal.healthSystem.MaxThirst);
			HungerIMG.fillAmount = Animal.healthSystem.Hunger / Animal.healthSystem.MaxHunger;
            ThirstIMG.fillAmount = Animal.healthSystem.Thirst / Animal.healthSystem.MaxThirst;
        }
	}
}
