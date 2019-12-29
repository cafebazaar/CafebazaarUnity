using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkuButtonAddon : MonoBehaviour
{
    public Text m_text;
    IABKeyTests m_keyTest;

    public void Initialize(IABKeyTests keytest, string sku)
    {
        m_keyTest = keytest;
        GetComponent<Button>().onClick.AddListener(() => m_keyTest.SetSKU(sku));
        m_text.text = sku;
    }
}
