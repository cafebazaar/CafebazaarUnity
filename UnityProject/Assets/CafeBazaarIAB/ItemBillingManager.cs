using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.UI;

public class ItemBillingManager : MonoBehaviour
{
	[Tooltip("Name of game object with 'AbstractBillingSystem' script.")]
	public string billingSystemName = "BillingSystem";
	public string itemSku = "";
	public Text price;

	[Tooltip("Loading progress bar or some waiting tool")]
	public GameObject loading;

	[Tooltip("Text to show price value")]
	public string textDefaultValue;

	[Tooltip("When buying process ready, button enabled else disable, it can be null")]
	public Button buyButton;

	private AbstractBillingSystem billingSystem;

	private float lockBuyAfterRequest = 5f;
	private float lastBuyRequestTimeElapse = 0f;

	// Use this for initialization
	void Start ()
	{
		billingSystem = GameObject.Find (billingSystemName).GetComponent<AbstractBillingSystem>();

		if (!string.IsNullOrEmpty (textDefaultValue)) {
			price.text = textDefaultValue;
		}
		billingSystem.AddItemSKU (itemSku);

		if (loading != null)
			loading.SetActive (true);
		if (buyButton != null)
			buyButton.interactable = false;

		lastBuyRequestTimeElapse = 5f;//enable in first time
	}
	
	// Update is called once per frame
	void Update ()
	{
		lastBuyRequestTimeElapse += Time.deltaTime;
		var activeLoading = true;

		BillingSkuInfo skuInfo = null;
		if (billingSystem.IsDone ()) {
			skuInfo = billingSystem.GetItemInfo (itemSku);
			if (skuInfo != null) {

				//update sku item informations
				price.text = (skuInfo.GetPriceAsNormalString(false, "?"));

			} else {

				if(!billingSystem.IsWating())//must check before load anythings
					billingSystem.LoadInventory ();
			}
		}

		if (skuInfo != null)
			activeLoading = false;

		if (loading != null)
			loading.SetActive (activeLoading);

		if (buyButton != null)
			buyButton.interactable = !activeLoading;
		
	}

	public void BuyItem(){
		if (lastBuyRequestTimeElapse < lockBuyAfterRequest) {
			Debug.Log ("Buy request lock.");
			return;
		}
		
		billingSystem.PurchaseItem(itemSku, GenerateRandomString(12));
		lastBuyRequestTimeElapse = 0f;
	}

	private string GenerateRandomString(int length){
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[Random.Range (0, s.Length)]).ToArray());
	}
}

