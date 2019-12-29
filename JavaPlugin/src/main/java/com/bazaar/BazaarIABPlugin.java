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
	private static BazaarIABPlugin mInstance;

	private IabHelper mHelper;
	private static String BILLING_NOT_RUNNING_ERROR = "The billing service is not running or billing is not supported. Aborting.";
	private List<Purchase> mPurchases = new ArrayList();
	private List<SkuDetails> mSkus;
	
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

	public static BazaarIABPlugin instance()
	{
		if (mInstance == null)
			mInstance = new BazaarIABPlugin();

		return mInstance;
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
					UnitySendMessage("billingSupported", "");
				}
				else
				{
					Log.i(TAG, "billing not supported: " + result.getMessage());
					UnitySendMessage("billingNotSupported", result.getMessage());
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
		return mHelper.subscriptionsSupported();
	}
	
	public void queryInventory(final String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "queryInventory", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.queryInventoryAsync(true, Arrays.asList(skus), BazaarIABPlugin.this);
			}
		}, "queryInventoryFailed");
	}
	
	public void onQueryInventoryFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mPurchases = inventory.getAllPurchases();
			mSkus = inventory.getAllSkuDetails();
			
			UnitySendMessage("queryInventorySucceeded", inventory.getAllSkusAndPurchasesAsJson());
		}
		else
		{
			UnitySendMessage("queryInventoryFailed", result.getMessage());
		}
	}
	
	public void querySkuDetails(final String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "querySkuDetails", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.querySkuDetailsAsync(Arrays.asList(skus), BazaarIABPlugin.this);
			}
		}, "querySkuDetailsFailed");
	}
	
	public void onQuerySkuDetailsFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mSkus = inventory.getAllSkuDetails();
			String skusStr = inventory.getAllSkusAsJson().toString();
			
			UnitySendMessage("querySkuDetailsSucceeded", skusStr);
		}
		else
		{
			UnitySendMessage("querySkuDetailsFailed", result.getMessage());
		}
	}
	
	public void queryPurchases()
	{
		IABLogger.logEntering(getClass().getSimpleName(), "queryPurchases");
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.queryPurchasesAsync(BazaarIABPlugin.this);
			}
		}, "queryInventoryFailed");
	}
	
	public void onQueryPurchasesFinished(IabResult result, Inventory inventory)
	{
		if (result.isSuccess())
		{
			mPurchases = inventory.getAllPurchases();
			String purchasesStr = inventory.getAllPurchasesAsJson().toString();
		
			UnitySendMessage("queryPurchasesSucceeded", purchasesStr);
		}
		else
		{
			UnitySendMessage("queryPurchasesFailed", result.getMessage());
		}
	}
	
	public void purchaseProduct(final String sku, final String developerPayload)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "purchaseProduct", new Object[] { sku, developerPayload });
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
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
		}, "purchaseFailed");
	}
	
	public void onIabPurchaseFinished(IabResult result, Purchase info)
	{
		if (result.isSuccess())
		{
			if (!mPurchases.contains(info)) {
				mPurchases.add(info);
			}
			UnitySendMessage("purchaseSucceeded", info.toJson());
		}
		else
		{
			UnitySendMessage("purchaseFailed", result.getMessage());
		}
	}
	
	public void consumeProduct(String sku)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "consumeProduct", sku);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			return;
		}
			
		final Purchase purchase = getPurchasedProductForSku(sku);
		if (purchase == null)
		{
			Log.i(TAG, "Attempting to consume an item that has not been purchased. Aborting to avoid exception. sku: " + sku);
			UnitySendMessage("consumePurchaseFailed", sku + ": you cannot consume a project that has not been purchased or if you have not first queried your inventory to retreive the purchases.");
			return;
		}
		
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.consumeAsync(purchase, BazaarIABPlugin.this);
			}
		}, "consumePurchaseFailed");
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
			
			UnitySendMessage("consumePurchaseSucceeded", purchase.toJson());
		}
		else
		{
			String res = purchase.getSku() + ": " + result.getMessage();
			UnitySendMessage("consumePurchaseFailed", res);
		}
	}
	
	public void consumeProducts(String[] skus)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "consumeProducts", skus);
		if (mHelper == null)
		{
			Log.i(TAG, BILLING_NOT_RUNNING_ERROR);
			return;
		}
		
		if ((mPurchases == null) || (mPurchases.size() == 0))
		{
			Log.e(TAG, "there are no purchases available to consume");
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
			Log.i(TAG, "Attempting to consume " + skus.length + " item(s) but only " + confirmedPurchases.size() + " item(s) were found to be purchased. Aborting.");
			return;
		}
		
		runSafelyOnUiThread(new Runnable()
		{
			public void run()
			{
				mHelper.consumeAsync(confirmedPurchases, BazaarIABPlugin.this);
			}
		}, "consumePurchaseFailed");
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
				
				UnitySendMessage("consumePurchaseSucceeded", purchase.toJson());
			}
			else
			{
				String res = purchase.getSku() + ": " + result.getMessage();
				UnitySendMessage("consumePurchaseFailed", res);
			}
		}
	}
	
}
