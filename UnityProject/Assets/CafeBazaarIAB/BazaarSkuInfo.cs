
using System.Collections.Generic;
using SimpleJSON;

#if UNITY_ANDROID

namespace BazaarPlugin
{
    public class BazaarSkuInfo
    {
        public string Title { get; private set; }
        public string Price { get; private set; }
        public string Type { get; private set; }
        public string Description { get; private set; }
        public string ProductId { get; private set; }

        public static List<BazaarSkuInfo> fromJsonArray(JSONArray items)
        {
            var skuInfos = new List<BazaarSkuInfo>();

            foreach (JSONNode item in items.AsArray)
            {
                BazaarSkuInfo bSkuInfo = new BazaarSkuInfo();
                bSkuInfo.fromJson(item.AsObject);
                skuInfos.Add(bSkuInfo);
            }

            return skuInfos;
        }

        public BazaarSkuInfo() { }

        public void fromJson(JSONClass json)
        {
            Title = json["title"].Value;
            Price = json["price"].Value;
            Type = json["type"].Value;
            Description = json["description"].Value;
            ProductId = json["productId"].Value;
        }

        public override string ToString()
        {
            return string.Format("<BazaarSkuInfo> title: {0}, price: {1}, type: {2}, description: {3}, productId: {4}",
                Title, Price, Type, Description, ProductId);
        }

    }
}
#endif