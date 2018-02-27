package com.bazaar;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.bazaar.util.IabHelper;

public class BazaarIABProxyActivity extends Activity
{
	private static final String TAG = "[BazaarIAB][Proxy]";
	private static final int RC_REQUEST = 10001;
	
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		if (savedInstanceState != null)
			return;
		
		try
		{
			String sku = getIntent().getExtras().getString("sku");
			String itemType = getIntent().getExtras().getString("itemType");
			String developerPayload = getIntent().getExtras().getString("developerPayload");
			
			Log.i(TAG, "proxy received action. sku: " + sku);
			
			BazaarIABPlugin.instance().getIabHelper().launchPurchaseFlow(this, sku, itemType, RC_REQUEST, BazaarIABPlugin.instance(), developerPayload);
		}
		catch (Exception e)
		{
			Log.i(TAG, "unhandled exception while attempting to purchase item: " + e.getMessage());
			Log.i(TAG, "going to end the async operation with null data to clear out the queue");
			if (BazaarIABPlugin.instance().getIabHelper() == null) 
			{
				Log.e(TAG, "FATAL ERROR: Plugin singleton helper is null. Aborting operation.");
			} 
			else 
			{
				BazaarIABPlugin.instance().getIabHelper().handleActivityResult(RC_REQUEST, 0, null);
			}
			finish();
		}
	}
	
	@Override
	public void onSaveInstanceState(Bundle savedInstanceState)
	{
		super.onSaveInstanceState(savedInstanceState);
	}
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		IABLogger.logEntering(getClass().getSimpleName(), "onActivityResult", new Object[] {Integer.valueOf(requestCode), Integer.valueOf(resultCode), data} );
		if (BazaarIABPlugin.instance().getIabHelper() == null)
		{
			Log.e(TAG, "FATAL ERROR: Plugin singleton helper is null in onActivityResult. Attempting to abort operation to avoid a crash.");
			super.onActivityResult(requestCode, resultCode, data);
			finish();
			return;
		}
		
		// Pass on the activity result to the helper for handling
		if (!BazaarIABPlugin.instance().getIabHelper().handleActivityResult(requestCode, resultCode, data))
		{
			// not handled, so handle it ourselves (here's where you'd
            // perform any handling of activity results not related to in-app
            // billing...
			super.onActivityResult(requestCode, resultCode, data);
		}
		else 
		{
			Log.d(TAG, "onActivityResult handled by IABUtil.");
		}
		
		finish();
	}
}
