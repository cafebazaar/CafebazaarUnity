/* Copyright (c) 2012 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.bazaar.util;

import org.json.JSONException;
import org.json.JSONObject;

/**
 * Represents an in-app billing purchase.
 */
public class Purchase {
    String mItemType;  // ITEM_TYPE_INAPP or ITEM_TYPE_SUBS
    String mOrderId;
    String mPackageName;
    String mSku;
    long mPurchaseTime;
    int mPurchaseState;
    String mDeveloperPayload;
    String mToken;
    String mOriginalJson;
    String mOriginalJsonWithSignature;
    String mSignature;

    public Purchase(String itemType, String jsonPurchaseInfo, String signature) throws JSONException {
        mItemType = itemType;
        mOriginalJson = jsonPurchaseInfo;
        JSONObject jsonObj = new JSONObject(mOriginalJson);
        mOrderId = jsonObj.optString("orderId");
        mPackageName = jsonObj.optString("packageName");
        mSku = jsonObj.optString("productId");
        mPurchaseTime = jsonObj.optLong("purchaseTime");
        mPurchaseState = jsonObj.optInt("purchaseState");
        mDeveloperPayload = jsonObj.optString("developerPayload");
        mToken = jsonObj.optString("token", jsonObj.optString("purchaseToken"));
        mSignature = signature;

        jsonObj.put("signature", mSignature);
        jsonObj.put("originalJson", mOriginalJson);
        jsonObj.put("itemType", mItemType);

        mOriginalJsonWithSignature = jsonObj.toString();
    }

    public String getItemType() { return mItemType; }
    public String getOrderId() { return mOrderId; }
    public String getPackageName() { return mPackageName; }
    public String getSku() { return mSku; }
    public long getPurchaseTime() { return mPurchaseTime; }
    public int getPurchaseState() { return mPurchaseState; }
    public String getDeveloperPayload() { return mDeveloperPayload; }
    public String getToken() { return mToken; }
    public String getOriginalJson() { return mOriginalJson; }
    public String getSignature() { return mSignature; }

    @Override
    public String toString() { return "PurchaseInfo(type:" + mItemType + "):" + mOriginalJson; }

    public String toJson() { return mOriginalJsonWithSignature; }
}
