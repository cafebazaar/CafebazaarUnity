using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BazaarPlugin;

public class IABCafebazaar : AbstractBillingSystem
{
	
	private static IABCafebazaar iabCafebazaar;

	private bool waitForResponseFlag = false;
	private bool preventOnDestroyShutDown = false;

	void OnEnable ()
	{
		#if UNITY_ANDROID_CAFE
		// Listen to all events for illustration purposes
		IABEventManager.billingSupportedEvent += billingSupportedEvent;
		IABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
		IABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		IABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		IABEventManager.querySkuDetailsSucceededEvent += querySkuDetailsSucceededEvent;
		IABEventManager.querySkuDetailsFailedEvent += querySkuDetailsFailedEvent;
		IABEventManager.queryPurchasesSucceededEvent += queryPurchasesSucceededEvent;
		IABEventManager.queryPurchasesFailedEvent += queryPurchasesFailedEvent;
		IABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
		IABEventManager.purchaseFailedEvent += purchaseFailedEvent;
		IABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
		IABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
		#endif
	}

	void OnDisable ()
	{
		#if UNITY_ANDROID_CAFE
		// Remove all event handlers
		IABEventManager.billingSupportedEvent -= billingSupportedEvent;
		IABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
		IABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		IABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
		IABEventManager.querySkuDetailsSucceededEvent -= querySkuDetailsSucceededEvent;
		IABEventManager.querySkuDetailsFailedEvent -= querySkuDetailsFailedEvent;
		IABEventManager.queryPurchasesSucceededEvent -= queryPurchasesSucceededEvent;
		IABEventManager.queryPurchasesFailedEvent -= queryPurchasesFailedEvent;
		IABEventManager.purchaseSucceededEvent -= purchaseSucceededEvent;
		IABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
		IABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
		IABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;

		#endif
	}

	// Use this for initialization
	protected override void Start ()
	{
		#if UNITY_ANDROID_CAFE

		if (iabCafebazaar == null) {

			iabCafebazaar = this;
			preventOnDestroyShutDown = false;//must shut down in app exit
			DontDestroyOnLoad (gameObject);
		} else {
			/**
			 * some drag and drop initialize in older object that need new object in current
			 */
			((AbstractBillingSystem)iabCafebazaar).consumeLoadingBar = consumeLoadingBar;

			preventOnDestroyShutDown = true;//must not shut down service in copy object
			Destroy (gameObject);
			return;
		}

		base.Start ();

		BazaarIAB.enableLogging(false);//this is hard coded for debug log in service
		BazaarIAB.init (encode64BasePublicKey);
		Debug.Log ("cafebazaar: Billing Initialize......");
		#endif

		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}


	void billingSupportedEvent ()
	{
		isBillingSupported = true;
		setBillingProgress (BillingProgress.DONE);
		Debug.Log ("cafebazaar: billingSupportedEvent");
	}

	void billingNotSupportedEvent (string error_)
	{
		isBillingSupported = false;
		this.error = error_;
		Debug.Log ("cafebazaar: billingNotSupportedEvent: " + error);
	}

	public override bool IsMarketConncetionError ()
	{
		return !isBillingSupported;
	}
	public override void Shutdown ()
	{
		Debug.Log ("cafebazaar: request shutdown.");
		#if UNITY_ANDROID_CAFE

		BazaarIAB.unbindService ();

		#endif
	}

	void OnDestroy(){
		if(!preventOnDestroyShutDown){
			Shutdown ();//close cafebazaar and resources
		}
	}

	//-------------------------------------------- Query Inventory section
	public override void LoadInventory ()
	{
		base.LoadInventory ();

		if (allSKUs.Count < 1) {
			Debug.Log ("cafebazaar: ignore request load SKU because list is empty.");
			return;
		}

		Debug.Log ("cafebazaar: request load sku and purchased items.");
		#if UNITY_ANDROID_CAFE
		if(CheckBillingSupported()){
			BazaarIAB.queryInventory (allSKUs.ToArray ());
		}

		#endif
		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}

	void queryInventorySucceededEvent (List<BazaarPurchase> purchases, List<BazaarSkuInfo> skus)
	{
		Debug.Log (string.Format ("cafebazaar: queryInventorySucceededEvent. total purchases: {0}, total skus: {1}", purchases.Count, skus.Count));

		UpdatePurchasedList (purchases);
		UpdateSkuList (skus);

		IsInventoryLoadedSuccess = true;
		setBillingProgress (BillingProgress.DONE);
	}

	void UpdatePurchasedList(List<BazaarPurchase> purchases){
		
		purchasedList.Clear ();

		foreach (BazaarPurchase purch in purchases) {

			BillingPurchase itemPurched = ConvertToBillingPurches(purch);
			purchasedList.Add (itemPurched);

			Debug.Log (itemPurched.ToString());
		}
	}

	void UpdatePurchasedList(BazaarPurchase newPurched){
		
		BillingPurchase beforePurched = purchasedList.Find(purched=>{
			if(purched.OrderId == newPurched.OrderId &&
				purched.ProductId == newPurched.ProductId &&//sku
				purched.PurchaseToken == newPurched.PurchaseToken &&
				purched.Signature == newPurched.Signature)
				return true;
			return false;
		});

		if (beforePurched == null) {

			BillingPurchase itemPurched = ConvertToBillingPurches(newPurched);
			purchasedList.Add (itemPurched);
			Debug.Log ("new purched added to list:" + newPurched.ToString ());
		} else {
			Debug.Log ("new purched ignore because already exist in list:" + newPurched.ToString ());
		}
			
	}

	void UpdateSkuList(List<BazaarSkuInfo> skus){

		billingSkuInfo.Clear ();

		foreach (BazaarSkuInfo skuInfo in skus) {
			BillingSkuInfo item = new BillingSkuInfo ();
			item.ProductId = skuInfo.ProductId;
			item.Title = skuInfo.Title;
			item.Price = skuInfo.Price;
			item.Description = skuInfo.Description;
			item.Type = skuInfo.Type;

			if (billingSkuInfo.ContainsKey (item.ProductId))
				billingSkuInfo.Remove (item.ProductId);//repeated item must remove

			billingSkuInfo.Add (item.ProductId, item);
			Debug.Log (item.ToString ());
		}
	}

	void queryInventoryFailedEvent (string error_)
	{
		this.error = error_;
		IsInventoryLoadedSuccess = false;
		setBillingProgress (BillingProgress.ERROR);
		Debug.Log ("cafebazaar: queryInventoryFailedEvent: " + error);
	}

	//-------------------------------------------- Query Item section
	public override void LoadItems ()
	{
		Debug.Log ("cafebazaar: request load sku items.");
		#if UNITY_ANDROID_CAFE
		if(CheckBillingSupported()){
			BazaarIAB.querySkuDetails(allSKUs.ToArray());
		}

		#endif
		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}

	private void querySkuDetailsSucceededEvent (List<BazaarSkuInfo> skus)
	{
		Debug.Log (string.Format ("cafebazaar: querySkuDetailsSucceededEvent. total skus: {0}", skus.Count));

		UpdateSkuList (skus);
		IsInventoryLoadedSuccess = true;//note: i put here because LoadItem() is same as LoadInventory()
		setBillingProgress (BillingProgress.DONE);
	}

	void querySkuDetailsFailedEvent (string error_)
	{
		this.error = error_;

		IsInventoryLoadedSuccess = false;//note: i put here because LoadItem() is same as LoadInventory()
		setBillingProgress (BillingProgress.ERROR);
		Debug.Log ("cafebazaar: querySkuDetailsFailedEvent: " + error);
	}

	//-------------------------------------------- Query Purchased section

	public override void LoadPurchase ()
	{
		Debug.Log ("cafebazaar: request load purchased item.");
		#if UNITY_ANDROID_CAFE
		if(CheckBillingSupported()){
			BazaarIAB.queryPurchases();
		}

		#endif
		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}

	void queryPurchasesSucceededEvent (List<BazaarPurchase> purchases)
	{
		Debug.Log (string.Format ("cafebazaar: queryPurchasesSucceededEvent. total purchases: {0}", purchases.Count));

		UpdatePurchasedList (purchases);
		setBillingProgress (BillingProgress.DONE);
	}

	private void queryPurchasesFailedEvent (string error_)
	{
		this.error = error_;
		setBillingProgress (BillingProgress.ERROR);
		Debug.Log ("cafebazaar: queryPurchasesFailedEvent: " + error);
	}

	//-------------------------------------------- Purchases Flow section
	public override void PurchaseItem (string sku, string developerPayload)
	{
		Debug.Log (string.Format("cafebazaar: request purchase item sku: {0} devPayload: {1}", sku, developerPayload));
		#if UNITY_ANDROID_CAFE

		if(CheckBillingSupported()){
			if(string.IsNullOrEmpty(developerPayload)){
				BazaarIAB.purchaseProduct(sku);
			} else {
				InProgressDeveloperPayload = developerPayload;
				BazaarIAB.purchaseProduct(sku,developerPayload);
			}

		}

		#endif
	}

	void purchaseSucceededEvent (BazaarPurchase purchase)
	{
		Debug.Log ("cafebazaar: purchaseSucceededEvent: " + purchase);
		LoadingBarActive (true);

		if (string.IsNullOrEmpty (InProgressDeveloperPayload)) {
			if (string.IsNullOrEmpty (purchase.DeveloperPayload)) {
				UpdatePurchasedList (purchase);
			} else {
				Debug.LogError ("purchase ignore, developerPayload must be null or empty!");
			}

		} else {
			if (InProgressDeveloperPayload == purchase.DeveloperPayload) {
				UpdatePurchasedList (purchase);
			} else {
				Debug.LogError (string.Format("purchase ignore, developerPayload must be:{0} null or empty!", InProgressDeveloperPayload));
			}
		}

	}

	void purchaseFailedEvent (string error_)
	{
		//error state notify user
		this.error = error_;
		if (error_.Contains ("1005")) {
			/*
			 * errorcode 1005=user cancel and error not occured
			 * do nothings
			 */

		} else {
			setBillingProgress (BillingProgress.ERROR);
		}

		Debug.Log ("cafebazaar: purchaseFailedEvent: " + error);

	}

	//-------------------------------------------- Consume section
	public override void ConsumePurchasedItem (string sku)
	{
		Debug.Log (string.Format("cafebazaar: request consume sku: {0}", sku));
		#if UNITY_ANDROID_CAFE
		if(CheckBillingSupported()){
			BazaarIAB.consumeProduct(sku);
		}

		#endif

		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}

	public override void ConsumePurchasedItem (List<string> skus)
	{
		Debug.Log (string.Format("cafebazaar: request consume sku: {0}", skus.ToArray()));
		#if UNITY_ANDROID_CAFE

		if(CheckBillingSupported()){
			BazaarIAB.consumeProducts(skus.ToArray());
		}

		#endif

		setBillingProgress (BillingProgress.WAIT);
		StartCoroutine (WaitForResponse ());
	}

	void consumePurchaseSucceededEvent (BazaarPurchase purchase)
	{
		Debug.Log ("cafebazaar: consumePurchaseSucceededEvent: " + purchase);

		UpdateConsumedList (ConvertToBillingPurches(purchase));
		setBillingProgress (BillingProgress.DONE);
	}

	void consumePurchaseFailedEvent (string error_)
	{
		Debug.Log ("cafebazaar: consumePurchaseFailedEvent: " + error);

		this.error = error_;
		setBillingProgress (BillingProgress.ERROR);
	}


	bool CheckBillingSupported(){
		if (isBillingSupported)
			return true;
		else
			Debug.LogWarning ("cafebazaar: billing not supported!");
		return false;
	}

	IEnumerator WaitForResponse(){
		
		if (waitForResponseFlag)
			yield break;

		marketResponseTimeElapsed = 0f;
		waitForResponseFlag = true;

		Debug.Log ("Start market Corutines....");

		while (marketResponseTimeElapsed < marketResponseTime) {
			marketResponseTimeElapsed += Time.deltaTime;
			if (!IsWating())
				break;

			//Debug.Log ("In market Corutines....");

			yield return null;
		}

		if (IsWating()) {
			setBillingProgress (BillingProgress.ERROR);
		}

		waitForResponseFlag = false;

		Debug.Log ("End market Corutines....");
	}

	BillingPurchase ConvertToBillingPurches(BazaarPurchase purch){
		BillingPurchase itemPurched = new BillingPurchase ();
		itemPurched.ProductId = purch.ProductId;
		itemPurched.OrderId = purch.OrderId;
		itemPurched.DeveloperPayload = purch.DeveloperPayload;
		itemPurched.PackageName = purch.PackageName;
		itemPurched.Type = purch.Type;
		itemPurched.PurchaseTime = purch.PurchaseTime;
		itemPurched.PurchaseToken = purch.PurchaseToken;
		switch (purch.PurchaseState) {
		case BazaarPurchase.BazaarPurchaseState.Purchased:
			itemPurched.PurchaseState = BillingPurchase.BillingPurchaseState.Purchased;
			break;
		case BazaarPurchase.BazaarPurchaseState.Canceled:
			itemPurched.PurchaseState = BillingPurchase.BillingPurchaseState.Canceled;
			break;
		case BazaarPurchase.BazaarPurchaseState.Refunded:
			itemPurched.PurchaseState = BillingPurchase.BillingPurchaseState.Refunded;
			break;
		}
		itemPurched.Signature = purch.Signature;
		itemPurched.OriginalJson = purch.OriginalJson;

		return itemPurched;
	}
}

