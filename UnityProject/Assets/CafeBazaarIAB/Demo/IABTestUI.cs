using UnityEngine;

using BazaarPlugin;

public class IABTestUI : MonoBehaviour
{
#if UNITY_ANDROID

    // Enter all the available skus from the CafeBazaar Developer Portal in this array so that item information can be fetched for them
    string[] skus = { "com.fanafzar.bazaarplugin.test1"
                , "com.fanafzar.bazaarplugin.test2"
                , "com.fanafzar.bazaarplugin.test3"
                , "com.fanafzar.bazaarplugin.monthly_subscribtion_test"
                , "com.fanafzar.bazaarplugin.annually_subscribtion_test"};

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 15f, Screen.height - 15f));
        GUI.skin.button.fixedHeight = 50;
        GUI.skin.button.fontSize = 20;

        if (Button("Initialize IAB"))
        {
            var key = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDN72wlPXo4pFK78rElKD+nwc9OnHHL+YYAt0o2Fm6H+7pNoOKLk/fbXrmV3jaL2cz99IClllFKEAvo6VbyRyIOD5cWpBCV+IFVobCPs9dtCV0M4DDqpVY2NUR9WownlNMwr/AwmwW750xS8BvQ9zt5+u7VEhLkAJPVxWJfr+kLHI7519s9T5eb58cdAM+bvJ1vT0pGx6te5DrV8IHUUCKpDYPy7kBfc9wdcT6EBUMCAwEAAQ==";
            BazaarIAB.init(key);
        }

        if (Button("Query Inventory"))
        {
            BazaarIAB.queryInventory(skus);
        }

        if (Button("Query SkuDetails"))
        {
            BazaarIAB.querySkuDetails(skus);
        }

        if (Button("Query Purchases"))
        {
            BazaarIAB.queryPurchases();
        }

        if (Button("Are subscriptions supported?"))
        {
            Debug.Log("subscriptions supported: " + BazaarIAB.areSubscriptionsSupported());
        }

        if (Button("Purchase Product Test1"))
        {
            BazaarIAB.purchaseProduct("com.fanafzar.bazaarplugin.test1");
        }

        if (Button("Purchase Product Test2"))
        {
            BazaarIAB.purchaseProduct("com.fanafzar.bazaarplugin.test2");
        }

        if (Button("Consume Purchase Test1"))
        {
            BazaarIAB.consumeProduct("com.fanafzar.bazaarplugin.test1");
        }

        if (Button("Consume Purchase Test2"))
        {
            BazaarIAB.consumeProduct("com.fanafzar.bazaarplugin.test2");
        }

        if (Button("Consume Multiple Purchases"))
        {
            var skus = new string[] { "com.fanafzar.bazaarplugin.test1", "com.fanafzar.bazaarplugin.test2" };
            BazaarIAB.consumeProducts(skus);
        }

        if (Button("Test Unavailable Item"))
        {
            BazaarIAB.purchaseProduct("com.fanafzar.bazaarplugin.unavailable");
        }

        if (Button("Purchase Monthly Subscription"))
        {
            BazaarIAB.purchaseProduct("com.fanafzar.bazaarplugin.monthly_subscribtion_test", "subscription payload");
        }

        if (Button("Purchase Annually Subscription"))
        {
            BazaarIAB.purchaseProduct("com.fanafzar.bazaarplugin.annually_subscribtion_test", "subscription payload");
        }

        if (Button("Enable High Details Logs"))
        {
            BazaarIAB.enableLogging(true);
        }

        GUILayout.EndArea();
    }

    bool Button(string label)
    {
        GUILayout.Space(5);
        return GUILayout.Button(label);
    }

#endif

}

