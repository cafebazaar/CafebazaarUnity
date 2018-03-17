
using UnityEngine;

#if UNITY_ANDROID
namespace BazaarPlugin
{
    public class BazaarIAB
    {
        private static AndroidJavaObject mPlugin;

        public static string GetVersion()
        {
            return "1.0.3";
        }

        static BazaarIAB()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            // Get the plugin instance
            using (var pluginClass = new AndroidJavaClass("com.bazaar.BazaarIABPlugin"))
                mPlugin = pluginClass.CallStatic<AndroidJavaObject>("instance");
        }

        // Toggles high detail logging on/off
        public static void enableLogging(bool enable)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            if (enable)
                Debug.LogWarning("YOU HAVE ENABLED HIGH DETAIL LOGS. DO NOT DISTRIBUTE THE GENERATED APK PUBLICLY. IT WILL DUMP SENSITIVE INFORMATION TO THE CONSOLE!");

            mPlugin.Call("enableLogging", enable);
        }


        // Initializes the billing system
        public static void init(string publicKey)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("init", publicKey);
        }


        // Unbinds and shuts down the billing service
        public static void unbindService()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("unbindService");
        }


        // Returns whether subscriptions are supported on the current device
        public static bool areSubscriptionsSupported()
        {
            if (Application.platform != RuntimePlatform.Android)
                return false;

            return mPlugin.Call<bool>("areSubscriptionsSupported");
        }


        // Sends a request to get all completed purchases and product information as setup in the CafeBazaar portal about the provided skus
        public static void queryInventory(string[] skus)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("queryInventory", new object[] { skus });
        }

        // Sends a request to get all product information as setup in the CafeBazaar portal about the provided skus
        public static void querySkuDetails(string[] skus)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("querySkuDetails", new object[] { skus });
        }

        // Sends a request to get all completed purchases
        public static void queryPurchases()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("queryPurchases");
        }


        // Sends out a request to purchase the product
        public static void purchaseProduct(string sku)
        {
            purchaseProduct(sku, string.Empty);
        }

        public static void purchaseProduct(string sku, string developerPayload)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("purchaseProduct", sku, developerPayload);
        }


        // Sends out a request to consume the product
        public static void consumeProduct(string sku)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("consumeProduct", sku);
        }


        // Sends out a request to consume all of the provided products
        public static void consumeProducts(string[] skus)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPlugin.Call("consumeProducts", new object[] { skus });
        }

    }
}

#endif
