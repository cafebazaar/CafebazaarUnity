using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

abstract public class AbstractBillingSystem : MonoBehaviour
{
	public enum BillingProgress {
		ERROR,
		WAIT,
		DONE
	}

	public delegate void MyPurchaseEvent(string sku);

	public string encode64BasePublicKey = "";


	[Range(0.1f, 10f)]
	public float marketResponseTime = 3f;
	public bool autoLoad = false;
	public float autoLoadTime = 30f;
	public float autoLoadTimeAfterLoadedSuccess = 60f;
	public string[] initSku;

	[Tooltip("Enable when request for consume sent and disable when consume applied to invoker listener.")]
	public GameObject consumeLoadingBar;
	public MyPurchaseEvent onPurchase;

	private BillingProgress billingProgress = BillingProgress.WAIT;
	private bool firstLoadFlag = false;
	private float autoLoadTimeElapsed = 0f;

	protected float marketResponseTimeElapsed = 0f;

	protected bool isBillingSupported = false;
	protected string error = "";

	protected List<string> allSKUs = new List<string>();
	protected List<BillingPurchase> purchasedList = new List<BillingPurchase> ();
	protected List<BillingPurchase> consumedList = new List<BillingPurchase> ();
	//key is SKU
	protected Dictionary<string, BillingSkuInfo> billingSkuInfo = new Dictionary<string, BillingSkuInfo> ();
	//key is sku, value is developerPayload
	protected string InProgressDeveloperPayload {get; set;}
	protected bool IsInventoryLoadedSuccess { get; set;}

	protected virtual void Start(){
		//
		if (initSku != null) {
			foreach (string sku in initSku) {
				allSKUs.Add (sku);
			}
		}

		IsInventoryLoadedSuccess = false;
	}

	protected virtual void Update(){
		autoLoadTimeElapsed += Time.deltaTime;



		//In case of first load in auto load mode
		if (autoLoad && IsDone ()) {
			if (!firstLoadFlag) {
				firstLoadFlag = true;

				LoadInventory ();
			}
		}

		var loadTIme = autoLoadTime;
		if (IsInventoryLoadedSuccess)
			loadTIme = autoLoadTimeAfterLoadedSuccess;


		//Auto load in every 'autoLoadTime' second
		if (autoLoad && autoLoadTimeElapsed >= loadTIme) {
			if (IsDone () || IsError ()) {
				firstLoadFlag = true;
								
				LoadInventory ();
			}
		}

		if (IsDone ()) {
			//In case of some purches not consumed yet
			CheckPurchesNotConsumed ();
		}

		//Check if need to consume somthings
		ApplyConsumed ();
	}

	protected void LoadingBarActive(bool active){
		if (consumeLoadingBar != null) {
			consumeLoadingBar.SetActive (active);
		}
	}

	public void AddItemSKU(string sku){
		if (allSKUs.Contains (sku))
			return;

		allSKUs.Add (sku);
	}

	virtual public void LoadInventory (){
		autoLoadTimeElapsed = 0;
	}

	abstract public void LoadItems ();//load all item
	abstract public void LoadPurchase ();//load all purchase
	abstract public bool IsMarketConncetionError();

	//developerPayload, could be null
	abstract public void PurchaseItem(string sku, string developerPayload);

	abstract public void ConsumePurchasedItem (string sku);
	abstract public void ConsumePurchasedItem (List<string> skus);

	/*
	 * return BillingSkuInfo or null
	*/
	public BillingSkuInfo GetItemInfo(string sku){
		if (billingSkuInfo.ContainsKey (sku)) {
			return billingSkuInfo [sku];
		}
			
		return null;
	}

	/*
	 * return list BillingSkuInfo, size of list >= 0
	*/
	public List<BillingPurchase> GetItemPurchased(string sku){
		List<BillingPurchase> list = new List<BillingPurchase> ();
		foreach (BillingPurchase p in purchasedList) {
			if (p.ProductId == sku) {
				list.Add (p);
			}
		}
		return list;
	}

	abstract public void Shutdown();

	public bool IsBillingSupported(){
		return isBillingSupported;
	}

	public BillingProgress GetBillingProgress(){
		return billingProgress;
	}

	public bool IsWating(){
		if (billingProgress == BillingProgress.WAIT) {
			return true;
		}

		return false;
	}

	public bool IsError(){
		if (billingProgress == BillingProgress.ERROR) {
			return true;
		}

		return false;
	}

	public bool IsDone(){
		if (billingProgress == BillingProgress.DONE) {
			return true;
		}

		return false;
	}

	public string GetErrorMessage(){
		return error;
	}

	public void setBillingProgress(BillingProgress progress){
		billingProgress = progress;
	}

	protected void UpdateConsumedList(BillingPurchase consumedItem){
		BillingPurchase beforeConsumed = consumedList.Find (item => {
			if(item.OrderId == consumedItem.OrderId &&
				item.DeveloperPayload == consumedItem.DeveloperPayload &&
				item.ProductId == consumedItem.ProductId)
				return true;
			return false;
		});

		if (beforeConsumed == null) {
			consumedList.Add (consumedItem);
		}

		Debug.Log ("Add consumed item to list:" + consumedItem.ToString ());
	}

	public void ApplyConsumed(){
		if (consumedList.Count <= 0)
			return;

		bool disable = false;

		foreach (BillingPurchase consumeItem in consumedList) {
			if (consumeItem.Type == "subs")
				continue;
			if (consumeItem.IsConsumed)
				continue;

			disable = true;

			consumeItem.IsConsumed = true;
			//Event purchase success full
			Debug.Log(string.Format("#####Success Consumed item:{0}.", consumeItem));
			if (onPurchase != null) {
				onPurchase.Invoke (consumeItem.ProductId);
			} else {
				Debug.LogError ("No onPurchase listener set???");
			}
				
		}//end of foreach

		if(disable)
			LoadingBarActive (false);
	}

	public void CheckPurchesNotConsumed(){
		if (purchasedList.Count <= 0)
			return;

		List<string> skus = new List<string> ();
		foreach (BillingPurchase purched in purchasedList) {
			if (!purched.IsConsumed) {
				skus.Add (purched.ProductId);
			}
		}

		if (skus.Count > 0) {
			LoadingBarActive (true);			
			ConsumePurchasedItem (skus);
		}
	}

}

