using UnityEngine;

using BazaarPlugin;

public class IABTestUI : MonoBehaviour
{
#if UNITY_ANDROID

    // Enter all the available skus from the CafeBazaar Developer Portal in this array so that item information can be fetched for them
    string[] skus = { "com.creeptechz.key",
                      "com.creeptechz.5key",
                      "com.creeptechz.10key" };

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 15f, Screen.height - 15f));
        GUI.skin.button.fixedHeight = 50;
        GUI.skin.button.fontSize = 20;

        if (Button("Initialize IAB"))
        {
            var key = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwC5nloPoQjrAbAsTYl4ZTluzzRA0My6JyPup/2Aoi23EnPpV16A3bReFCcXRYIkGrEYkV8sQOLF9OM3oqcEnZvRMbq+Ux9SEpo3pKAN9LnQ+JnhaJRzodgSgUNJ0C6GpOjcBX0csELsz8w68s0FokYKpysrjbRn9KMUa+Gcq3wJeOhtJGUfvkfByG0itSERfmwD0xhPm49FCRtorhYE6qkmavV2G+fBc8xF+Os19QkCAwEAAQ==";
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

        if (Button("Purchase Product 1 key"))
        {
            BazaarIAB.purchaseProduct(skus[0]);
        }

        if (Button("Purchase Product 5keys"))
        {
            BazaarIAB.purchaseProduct(skus[1]);
        }

        if (Button("Purchase product 10 keys"))
        {
            BazaarIAB.purchaseProduct(skus[2]);
        }

        if (Button("Consume Purchase 1key"))
        {
            BazaarIAB.consumeProduct(skus[0]);
        }

        if (Button("Consume Purchase 5keys"))
        {
            BazaarIAB.consumeProduct(skus[1]);
        }
        if (Button("Consume Purchase 10keys"))
        {
            BazaarIAB.consumeProduct(skus[2]);
        }
        if (Button("Consume Multiple Purchases"))
        {
            var items = new string[] { skus[0], skus[1] };
            BazaarIAB.consumeProducts(items);
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

