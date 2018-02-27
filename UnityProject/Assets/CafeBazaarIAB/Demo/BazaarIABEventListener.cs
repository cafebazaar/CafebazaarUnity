using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BazaarPlugin;


public class BazaarIABEventListener : MonoBehaviour
{
#if UNITY_ANDROID

	void OnEnable()
	{
		// Listen to all events for illustration purposes
		IABEventManager.billingSupportedEvent += billingSupportedEvent;
		IABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
		IABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		IABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		IABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
		IABEventManager.purchaseFailedEvent += purchaseFailedEvent;
		IABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
		IABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
	}

	void OnDisable()
	{
		// Remove all event handlers
		IABEventManager.billingSupportedEvent -= billingSupportedEvent;
		IABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
		IABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		IABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
		IABEventManager.purchaseSucceededEvent -= purchaseSucceededEvent;
		IABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
		IABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
		IABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
	}


	void billingSupportedEvent()
	{
		Debug.Log("billingSupportedEvent");
	}

	void billingNotSupportedEvent(string error)
	{
		Debug.Log("billingNotSupportedEvent: " + error);
	}

	void queryInventorySucceededEvent(List<BazaarPurchase> purchases, List<BazaarSkuInfo> skus)
	{
		Debug.Log(string.Format("queryInventorySucceededEvent. total purchases: {0}, total skus: {1}", purchases.Count, skus.Count));

        for (int i = 0; i < purchases.Count; ++i)
        {
            Debug.Log(purchases[i].ToString());
        }

        Debug.Log("-----------------------------");

        for (int i = 0; i < skus.Count; ++i)
        {
            Debug.Log(skus[i].ToString());
        }
    }

	void queryInventoryFailedEvent(string error)
	{
		Debug.Log("queryInventoryFailedEvent: " + error);
	}

	void purchaseSucceededEvent(BazaarPurchase purchase)
	{
		Debug.Log("purchaseSucceededEvent: " + purchase);
	}

	void purchaseFailedEvent(string error)
	{
		Debug.Log("purchaseFailedEvent: " + error);
	}

	void consumePurchaseSucceededEvent(BazaarPurchase purchase)
	{
		Debug.Log("consumePurchaseSucceededEvent: " + purchase);
	}

	void consumePurchaseFailedEvent(string error)
	{
		Debug.Log("consumePurchaseFailedEvent: " + error);
	}

#endif

}


