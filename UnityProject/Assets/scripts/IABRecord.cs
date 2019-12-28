using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyRecordSituations
{
    ADDED_SUCCESSFULLY,
    CONSUMED_SUCCESSFULLY,
    EXISTS,
    DOESNT_EXIST
}

public class IABRecord 
{
    private List<BazaarKeyRecord> m_keys;

    internal IABRecord()
    {
        m_keys = new List<BazaarKeyRecord>();
    }

    internal KeyRecordSituations AddKeyRecord(string purchaseId)
    {
        if (m_keys.FindAll(k => k.m_purchaseToken == purchaseId).Count > 0) return KeyRecordSituations.EXISTS;

        m_keys.Add(new BazaarKeyRecord() { m_isUsed = false, m_purchaseToken = purchaseId });
        return KeyRecordSituations.ADDED_SUCCESSFULLY;
    }

    internal KeyRecordSituations ConsumeKeyRecord(string sku)
    {
        if (m_keys.Count <= 0) return KeyRecordSituations.DOESNT_EXIST;

        var key = m_keys.Find(x => x.m_sku == sku);
        if (key.m_sku != sku)
        {
            return KeyRecordSituations.DOESNT_EXIST;
        }
        key.m_isUsed = true;
        return KeyRecordSituations.CONSUMED_SUCCESSFULLY;
    }

    internal bool SinglePurchaseExists(string m_skuDetail)
    {
        if (m_keys.Count <= 0) return false;

        if (m_keys.Exists(key => key.m_sku == m_skuDetail))
        {
            return true;
        }
        return false;
    }
}

internal struct BazaarKeyRecord
{
    /// <summary>
    /// the Id of the purchase which is returned by CafeBazaar API and should be kept in order to keep track of purchased keys
    /// </summary>
    internal string m_purchaseToken;
    /// <summary>
    /// the sku of the item according to what is defined on the bazaar dashboard
    /// </summary>
    internal string m_sku;
    /// <summary>
    /// is the key used to unlocked an IABMailBox or what
    /// </summary>
    internal bool m_isUsed;
}
