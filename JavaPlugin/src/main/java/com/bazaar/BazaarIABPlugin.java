package com.bazaar;

import com.bazaar.util.IabHelper;
import com.bazaar.util.IabHelper.OnConsumeFinishedListener;
import com.bazaar.util.IabHelper.OnConsumeMultiFinishedListener;
import com.bazaar.util.IabHelper.OnIabPurchaseFinishedListener;
import com.bazaar.util.IabHelper.OnIabSetupFinishedListener;
import com.bazaar.util.IabHelper.QueryInventoryFinishedListener;
import com.bazaar.util.IabHelper.QuerySkuDetailsFinishedListener;
import com.bazaar.util.IabHelper.QueryPurchasesFinishedListener;
import com.bazaar.util.IabResult;
import com.bazaar.util.Inventory;
import com.bazaar.util.Purchase;
import com.bazaar.util.SkuDetails;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import org.json.JSONException;
import org.json.JSONObject;

public class BazaarIABPlugin extends BazaarIABPluginBase
	implements 
	  IabHelper.QueryInventoryFinishedListener
	, IabHelper.QuerySkuDetailsFinishedListener
	, IabHelper.QueryPurchasesFinishedListener
	, IabHelper.OnIabPurchaseFinishedListener
	, IabHelper.OnConsumeFinishedListener
	, IabHelper.OnConsumeMultiFinishedListener
{
	private static final String BILLING_NOT_RUNNING_ERROR = "The billing service is not running or billing is not supported. Aborting.";
	private static final String STORE_CONNECTION_IS_NULL = "The billing service connection is null";

	private static BazaarIABPlugin mInstance;
	
	private IabHelper mHelper;
	private List<Purchase> mPurchases = new ArrayList();
	private List<SkuDetails> mSkus;
	
	public static BazaarIABPlugin instance()
	{
		if (mInstance == null)
			mInstance = new BazaarIABPlugin();
		
		return mInstance;
	}
	
	public IabHelper getIabHelper()
	{
		return mHelper;
	}
	
	public void enableLogging(boolean shouldEnable)
	{
		IABLogger.DEBUG = shouldEnable;
		if (mHelper != null)
			mHelper.enableDebugLogging(true);
	}
	
	public void init(String publicKey)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "init", publicKey);
		
		mPurchases = new ArrayList();
		mHelper = new IabHelper(getActivity(), publicKey);
		mHelper.startSetup(new IabHelper.OnIabSetupFinishedListener()
		{
			public void onIabSetupFinished(IabResult result)
			{
				if (result.isSuccess())
				{
					UnitySendMessage(BazaarUnityCallbacks.BILLING_SUPPORTED, "");
				}
				else
				{
					Log.i(TAG, "billing not supported: " + result.getMessage());
					UnitySendMessage(BazaarUnityCallbacks.BILLING_NOT_SUPPORTED, result.getMessage());
					mHelper = null;
				}
			}
		});
	}
	
	public void unbindService()
	{
		IABLogger.logEntering(getClass().getSimpleName(), "unbindService");
		if (mHelper != null)
		{
			mHelper.dispose();
			mHelper = null;
		}
	}
	
	public boolean areSubscriptionsSupported()
	{
		IABLogger.logEntering(getClass().getSimpleName(), "areSubscriptionsSupported");
		if (mHelper == null) {
			return false;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			return false;
		}
		return mHelper.subscriptionsSupported();
	}
	
	public void queryInventory(final String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "queryInventory", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_INVENTORY_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_INVENTORY_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.queryInventoryAsync(true, Arrays.asList(skus), BazaarIABPlugin.this);
			}
		}, BazaarUnityCallbacks.QUERY_INVENTORY_FAILED);
	}
	
	public void onQueryInventoryFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mPurchases = inventory.getAllPurchases();
			mSkus = inventory.getAllSkuDetails();
			
			UnitySendMessage(BazaarUnityCallbacks.QUERY_INVENTORY_SUCCEEDED, inventory.getAllSkusAndPurchasesAsJson());
		}
		else
		{
			UnitySendMessage(BazaarUnityCallbacks.QUERY_INVENTORY_FAILED, result.getMessage());
		}
	}
	
	public void querySkuDetails(final String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "querySkuDetails", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_SKU_DETAILS_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_SKU_DETAILS_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.querySkuDetailsAsync(Arrays.asList(skus), BazaarIABPlugin.this);
			}
		}, BazaarUnityCallbacks.QUERY_SKU_DETAILS_FAILED);
	}
	
	public void onQuerySkuDetailsFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mSkus = inventory.getAllSkuDetails();
			String skusStr = inventory.getAllSkusAsJson().toString();
			
			UnitySendMessage(BazaarUnityCallbacks.QUERY_SKU_DETAILS_SUCCEEDED, skusStr);
		}
		else
		{
			UnitySendMessage(BazaarUnityCallbacks.QUERY_SKU_DETAILS_FAILED, result.getMessage());
		}
	}
	
	public void queryPurchases()
	{
		IABLogger.logEntering(getClass().getSimpleName(), "queryPurchases");
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_PURCHASES_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.QUERY_PURCHASES_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.queryPurchasesAsync(BazaarIABPlugin.this);
			}
		}, BazaarUnityCallbacks.QUERY_PURCHASES_FAILED);
	}
	
	public void onQueryPurchasesFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mPurchases = inventory.getAllPurchases();
			String purchasesStr = inventory.getAllPurchasesAsJson().toString();
		
			UnitySendMessage(BazaarUnityCallbacks.QUERY_PURCHASES_SUCCEEDED, purchasesStr);
		}
		else
		{
			UnitySendMessage(BazaarUnityCallbacks.QUERY_PURCHASES_FAILED, result.getMessage());
		}
	}
	
	public void purchaseProduct(final String sku, final String developerPayload)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "purchaseProduct", new Object[] { sku, developerPayload });
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.PURCHASE_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.PURCHASE_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
		
		for (Purchase p : mPurchases)
		{
			if (p.getSku().equalsIgnoreCase(sku)) 
			{
				Log.i(TAG, "Attempting to purchase an item that has already been purchased. That is probably not a good idea: " + sku);
			}
		}
		
		final String f_itemType = "inapp";
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				Intent proxyStarter = new Intent(getActivity(), BazaarIABProxyActivity.class);
				proxyStarter.putExtra("sku", sku);
				proxyStarter.putExtra("itemType", f_itemType);
				proxyStarter.putExtra("developerPayload", developerPayload);
				getActivity().startActivity(proxyStarter);
			}
		}, BazaarUnityCallbacks.PURCHASE_FAILED);
	}
	
	public void onIabPurchaseFinished(IabResult result, Purchase info)
	{
		if (result.isSuccess())
		{
			if (!mPurchases.contains(info)) {
				mPurchases.add(info);
			}
			UnitySendMessage(BazaarUnityCallbacks.PURCHASE_SUCCEEDED, info.toJson());
		}
		else
		{
			UnitySendMessage(BazaarUnityCallbacks.PURCHASE_FAILED, result.getMessage());
		}
	}
	
	public void consumeProduct(String sku)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "consumeProduct", sku);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
			
		final Purchase purchase = getPurchasedProductForSku(sku);
		if (purchase == null)
		{
			Log.i(TAG, "Attempting to consume an item that has not been purchased. Aborting to avoid exception. sku: " + sku);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, sku + ": you cannot consume a project that has not been purchased or if you have not first queried your inventory to retreive the purchases.");
			return;
		}
		
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.consumeAsync(purchase, BazaarIABPlugin.this);
			}
		}, BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED);
	}
	
	private Purchase getPurchasedProductForSku(String sku)
	{
		for (Purchase p : mPurchases) 
		{
			if (p.getSku().equalsIgnoreCase(sku)) 
			{
				return p;
			}
		}
		return null;
	}
	
	public void onConsumeFinished(Purchase purchase, IabResult result)
	{
		if (result.isSuccess())
		{
			if (mPurchases.contains(purchase)) 
			{
				mPurchases.remove(purchase);
			}
			
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_SUCCEEDED, purchase.toJson());
		}
		else
		{
			String res = purchase.getSku() + ": " + result.getMessage();
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, res);
		}
	}
	
	public void consumeProducts(String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "consumeProducts", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		if (mHelper.connectionIsNull()){
			Log.i(TAG, STORE_CONNECTION_IS_NULL);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, STORE_CONNECTION_IS_NULL);
			return;
		}
		
		if ((mPurchases == null) || (mPurchases.size() == 0))
		{
			String error = "there are no purchases available to consume";
			Log.e(TAG, error);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, error);
			return;
		}
		
		final List<Purchase> confirmedPurchases = new ArrayList();
		for (String sku : skus)
		{
			Purchase purchase = getPurchasedProductForSku(sku);
			if (purchase != null) 
			{
				confirmedPurchases.add(purchase);
			}
		}
		
		if (confirmedPurchases.size() != skus.length)
		{
			String error = "Attempting to consume " + skus.length + " item(s) but only " + confirmedPurchases.size() + " item(s) were found to be purchased. Aborting.";
			Log.i(TAG, error);
			UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, error);
			return;
		}
		
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.consumeAsync(confirmedPurchases, BazaarIABPlugin.this);
			}
		}, BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED);
	}
	
	public void onConsumeMultiFinished(List<Purchase> purchases, List<IabResult> results)
	{
		for (int i = 0; i < results.size(); ++i)
		{
			IabResult result = (IabResult)results.get(i);
			Purchase purchase = (Purchase)purchases.get(i);
			if (result.isSuccess())
			{
				if (mPurchases.contains(purchase)) 
				{
					mPurchases.remove(purchase);
				}
				
				UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_SUCCEEDED, purchase.toJson());
			}
			else
			{
				String res = purchase.getSku() + ": " + result.getMessage();
				UnitySendMessage(BazaarUnityCallbacks.CONSUME_PURCHASE_FAILED, res);
			}
		}
	}
	
}
