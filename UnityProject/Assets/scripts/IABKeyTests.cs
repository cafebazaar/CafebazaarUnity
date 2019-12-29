using BazaarPlugin;
using UnityEngine;
using UnityEngine.UI;

public class IABKeyTests : MonoBehaviour, IEventListner
{
    public GameObject buttonPrefab;
    public Transform skuButtonContentPanel;
    public string[] skus;
    public TMPro.TextMeshProUGUI m_text;
    
    KeyCollection _keys;
    string _currentSku;

    private void OnDestroy()
    {
        UnhookEvents();
        BazaarIAB.unbindService();
    }

    private void Start()
    {
        try
        {
            var key = " MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwC5nloPoQjrAbAsTYl4ZTluzzRA0My6JyPup/2Aoi23EnPpV16A3bReFCcXRYIkGrEYkV8sQOLF9OM3oqcEnZvRMbq+Ux9SEpo3pKAN9LnQ+JnhaJRzodgSgUNJ0C6GpOjcBX0csELsz8w68s0FokYKpysrjbRn9KMUa+Gcq3wJeOhtJGUfvkfByG0itSERfmwD0xhPm49FCRtorhYE6qkmavV2G+fBc8xF+Os19QkCAwEAAQ==";
            BazaarIAB.init(key);
            HookEvents();
            if (skus.Length <= 0)
            {
                Log("Unrecoverable error: add some sku through the inspector and relaunch the project");
                return;
            }
            _currentSku = skus[0];
            _keys = GetComponent<KeyCollection>();
            for (int i = 0; i < skus.Length; i++)
            {
                var bAddon = Instantiate(buttonPrefab, skuButtonContentPanel).GetComponent<SkuButtonAddon>();
                bAddon.Initialize(this, skus[i]);
            }
        }
        catch (System.Exception e)
        {
            Log(e.Message);
        }
    }

    public void SetSKU(string sku)
    {
        _currentSku = sku;
        Log("Current SKu was changed to " + _currentSku);
    }

    public void BuyKey()
    {
        _keys.BuyKeyAsync(_currentSku);
    }

    public void Consume()
    {
        _keys.ConsumeKey(_currentSku);
    }

    public void Retrieve()
    {
        var res = _keys.RetrieveKey(_currentSku);
        if (res)
        {
            Log("Retrieved the " + _currentSku + " successfully");
        }
        else
        {
            Log("Failed to retrive the " + _currentSku + " sku");
        }
    }

    #region IEventlistner impl
    public void HookEvents()
    {
        IABEventManager.purchaseFailedEvent += IABEventManager_purchaseFailedEvent;
        IABEventManager.purchaseSucceededEvent += IABEventManager_purchaseSucceededEvent;
        IABEventManager.consumePurchaseSucceededEvent += IABEventManager_consumePurchaseSucceededEvent;
        IABEventManager.consumePurchaseFailedEvent += IABEventManager_consumePurchaseFailedEvent;
    }
   
    public void UnhookEvents()
    {
        IABEventManager.purchaseFailedEvent -= IABEventManager_purchaseFailedEvent;
        IABEventManager.purchaseSucceededEvent -= IABEventManager_purchaseSucceededEvent;
        IABEventManager.consumePurchaseSucceededEvent -= IABEventManager_consumePurchaseSucceededEvent;
        IABEventManager.consumePurchaseFailedEvent -= IABEventManager_consumePurchaseFailedEvent;
    }

    private void IABEventManager_purchaseFailedEvent(string obj)
    {
        Log("Failed to buy the item; errCode : " + obj);
    }

    private void IABEventManager_purchaseSucceededEvent(BazaarPurchase obj)
    {
        _keys.cachedObj.AddKeyRecord(obj.PurchaseToken);
        Log("bought the item : " + obj.PackageName + " with purchase Token " + obj.PurchaseToken);
    }

    private void IABEventManager_consumePurchaseSucceededEvent(BazaarPurchase obj)
    {
        var res = _keys.ConsumeKey(_currentSku);
        Log("Consume sucseeded of sku " + _currentSku + " was: " + res);
    }

    private void IABEventManager_consumePurchaseFailedEvent(string obj)
    {
        Log("Consume failed" + _currentSku + " was: ");
    }


    private void Log(string str)
    {
        m_text.text += "\n" + str;
        m_text.GetComponent<LayoutElement>().preferredHeight += 150;
    }
    #endregion
}
