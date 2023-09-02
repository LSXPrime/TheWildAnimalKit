using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LWOS
{
    public class ShopItemUI : MonoBehaviour
    {
        public Image Icon;
		public Text Name;
		public Text Price;
		public Text Type;
        public UIGradient Selected;
		internal ItemData Item;
        
		public void Set(ItemData item)
		{
			Item = item;
			Icon.sprite = Item.Icon;
			Name.text = Item.Name;
			Type.text = Item.Type.ToString();
			Price.text = Item.Owned ? "OWNED" : Item.Price.ToString();
            if (Selected != null)
                Selected.enabled = LocalPrefs.GetInt("PlayerSkin") == Item.ID ? true : false;
		}
		
		public void Get()
		{
            ////X Debug.Log("ShopItemUI Get " + Name.text + Item.Owned);
            if (Item.Owned)
			{
				switch (Item.Type)
				{
					case ItemType.SKIN:
						StatusUI.Instance.SetPlayerCharacter(Item.ID);
						 break;
					case ItemType.MEMBER:
						StatusUI.Instance.AddPackMember(Item.ID);
						break;
				}
			}
            else
                StatusUI.Instance.ItemBuy(Item);
		}
    }
}
