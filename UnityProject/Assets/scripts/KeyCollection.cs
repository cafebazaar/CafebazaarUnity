using BazaarPlugin;
using UnityEngine;

public class KeyCollection : MonoBehaviour, ICachable<IABRecord>/*, IEventListner*/
{
    private SaveSystem m_saveSystem;
    #region class methods
    private void Start()
    {
        InvalidateCache();
    }

    private void OnEnable()
    {
        m_saveSystem = new SaveSystem();
        //HookEvents();
    }

    private void OnDisable()
    {
        //UnhookEvents();
    }
    #endregion

    #region ICachable impl
    public bool isCacheValid { get; private set; }

    public IABRecord cachedObj
    {
        get
        { 
            if (!isCacheValid) 
            {
                _cached = m_saveSystem.LoadPurchase();
                isCacheValid = true;
            }
            return _cached;
        }
        set
        {
            _cached = value;
        }
    }


    private IABRecord _cached;

    public void InvalidateCache()
    {
        isCacheValid = false;
    }
    #endregion
    //#region IEventlistner impl
    //public void HookEvents()
    //{
    //    IABEventManager.purchaseFailedEvent += IABEventManager_purchaseFailedEvent;
    //    IABEventManager.purchaseSucceededEvent += IABEventManager_purchaseSucceededEvent;
    //}


    //public void UnhookEvents()
    //{
    //    IABEventManager.purchaseFailedEvent -= IABEventManager_purchaseFailedEvent;
    //    IABEventManager.purchaseSucceededEvent -= IABEventManager_purchaseSucceededEvent;
    //}

    //private void IABEventManager_purchaseFailedEvent(string obj)
    //{

    //}

    //private void IABEventManager_purchaseSucceededEvent(BazaarPurchase obj)
    //{
    //    cachedObj.AddKeyRecord(obj.PurchaseToken);
    //}

    //#endregion

    internal bool RetrieveKey(string m_skuDetail)
    {
        return cachedObj.SinglePurchaseExists(m_skuDetail);
    }

    internal void BuyKeyAsync(string sku)
    {
        BazaarIAB.purchaseProduct(sku);
    }
    internal KeyRecordSituations ConsumeKey(string sku)
    {
        var res = cachedObj.ConsumeKeyRecord(sku);
        if (res == KeyRecordSituations.CONSUMED_SUCCESSFULLY)
        {
            m_saveSystem.SavePurchases(cachedObj);
            InvalidateCache();
        }
        return res;
    }
}
