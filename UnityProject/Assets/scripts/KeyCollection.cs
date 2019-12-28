using UnityEngine;

public class KeyCollection : MonoBehaviour, ICachable<IABRecord>, IEventListner
{
    //public KeyView[] m_keyViews;
    [SerializeField] private SaveSystem m_saveSystem;
    #region class methods
    private void Start()
    {
        InvalidateCache();
    }

    private void OnEnable()
    {
        HookEvents();
    }

    private void OnDisable()
    {
        UnhookEvents();
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
    #region IEventlistner impl
    public void HookEvents()
    {
    }

    public void UnhookEvents()
    {
    }
    
    #endregion

    internal bool RetrieveKey(KeyDefinition m_def)
    {
        return cachedObj.SinglePurchaseExists(m_def.m_skuDetail);
    }

    internal void BuyKeyAsync(string sku)
    {
        BazaarPlugin.BazaarIAB.purchaseProduct(sku);
        BazaarPlugin.IABEventManager.purchaseFailedEvent += (res) =>
        {

        };

        BazaarPlugin.IABEventManager.purchaseSucceededEvent += (res) =>
        {
        };
    }
    internal KeyRecordSituations ConsumeKey(KeyDefinition keyDef)
    {
        var res = cachedObj.ConsumeKeyRecord(keyDef.m_skuDetail);
        if (res == KeyRecordSituations.CONSUMED_SUCCESSFULLY)
        {
            m_saveSystem.SavePurchases(cachedObj);
            InvalidateCache();
        }
        return res;
    }
}
