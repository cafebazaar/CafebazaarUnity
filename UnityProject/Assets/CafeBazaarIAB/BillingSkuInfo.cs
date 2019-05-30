using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BillingSkuInfo
{

	public string Title { get; set; }
	public string Price { get; set;	}
	public string Type { get; set; }
	public string Description { get; set; }
	public string ProductId { get; set; }

	public BillingSkuInfo (){}

	public override string ToString()
	{
		return string.Format("<BillingSkuInfo> title: {0}, price: {1}, type: {2}, description: {3}, productId: {4}",
			Title, Price, Type, Description, ProductId);
	}

	public int GetPriceAsInt(){
		if (string.IsNullOrEmpty (Price))
			return 0;

		return int.Parse( ConvertPersionDigitToEnglishDigit (Price, true));
	}

	public string GetPriceAsNormalString( bool removeDelimiter,string default_value){
		if (string.IsNullOrEmpty (Price))
			return default_value;

		return ConvertPersionDigitToEnglishDigit (Price, removeDelimiter);
	}

	private string ConvertPersionDigitToEnglishDigit(string input, bool removeDelimiter)
	{
		
		string[] persian = new string[11] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹", "صفر" };

		input = input.Trim ();
		input = input.Replace (persian[10], persian[0]);
		if(removeDelimiter)
			input = input.Replace ("٬", "");
		else
			input = input.Replace ("٬", ",");
		
		string [] split = input.Split (new string []{" "}, System.StringSplitOptions.None);
		for (int j = 0; j < (persian.Length - 1); j++) {
			split[0] = split[0].Replace (persian [j], j.ToString ());
		}

		return split[0];
	}
}

