using UnityEngine;
using System.Collections;

public class BillingPurchase
{
	public enum BillingPurchaseState
	{
		Purchased,
		Canceled,
		Refunded
	}

	public string PackageName { get; set; }
	public string OrderId { get; set; }
	public string ProductId { get; set; }
	public string DeveloperPayload { get; set; }
	public string Type { get; set; }
	public long PurchaseTime { get; set; }
	public BillingPurchaseState PurchaseState { get; set; }
	public string PurchaseToken { get; set; }
	public string Signature { get; set; }
	public string OriginalJson { get; set; }
	public bool IsConsumed{ get; set;}

	public BillingPurchase() { }

	public override string ToString()
	{
		return string.Format("<BillingPurchase> consumed: {9}, packageName: {0}, orderId: {1}, productId: {2}, developerPayload: {3}, purchaseToken: {4}, purchaseState: {5}, signature: {6}, type: {7}, json: {8}",
			PackageName, OrderId, ProductId, DeveloperPayload, PurchaseToken, PurchaseState, Signature, Type, OriginalJson, IsConsumed);
	}
}

