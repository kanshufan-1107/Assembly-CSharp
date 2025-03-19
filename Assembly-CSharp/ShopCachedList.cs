using System.Collections.Generic;
using System.Linq;

public class ShopCachedList
{
	private List<string> m_orderedProducts = new List<string>();

	public int Version { get; set; }

	public HashSet<string> ProductIds { get; }

	public ShopCachedList()
	{
		Version = 0;
		ProductIds = new HashSet<string>();
	}

	public ShopCachedList(int version, List<string> orderedProducts)
	{
		Version = version;
		m_orderedProducts = orderedProducts;
		ProductIds = new HashSet<string>(orderedProducts);
	}

	public List<string> GetXLatestProducts(int numProducts)
	{
		int count = m_orderedProducts.Count;
		if (numProducts <= 0 || m_orderedProducts.Count == 0)
		{
			return new List<string>();
		}
		int startIndex = count - numProducts;
		if (startIndex < 0)
		{
			startIndex = 0;
		}
		return m_orderedProducts.GetRange(startIndex, count - startIndex);
	}

	public void UpdateProductList(ShopBadging.ProductStatus defaultState)
	{
		List<string> updatedProducts = new List<string>();
		foreach (string product in m_orderedProducts)
		{
			if (!string.IsNullOrEmpty(product))
			{
				if (product.Length >= 50)
				{
					Log.Store.PrintError("[ShopCachedList::UpdateProductList] Product Id is too long: " + product + ".");
				}
				else if (!product.Contains("_"))
				{
					string updatedProduct = $"{product}_{(int)defaultState}";
					updatedProducts.Add(updatedProduct);
					ProductIds.Remove(product);
					ProductIds.Add(updatedProduct);
				}
				else
				{
					updatedProducts.Add(product);
				}
			}
		}
		m_orderedProducts = updatedProducts;
	}

	public void UpdateProductState(string productId, ShopBadging.ProductStatus state)
	{
		if (productId.Length >= 50)
		{
			Log.Store.PrintError("[ShopCachedList::UpdateProductState] Product Id is too long: " + productId + ".");
			return;
		}
		string existingProduct = m_orderedProducts.FirstOrDefault((string p) => p.StartsWith(productId + "_"));
		if (existingProduct != null)
		{
			m_orderedProducts.Remove(existingProduct);
			ProductIds.Remove(existingProduct);
		}
		string newProduct = $"{productId}_{(int)state}";
		if (!string.IsNullOrEmpty(newProduct))
		{
			m_orderedProducts.Add(newProduct);
			ProductIds.Add(newProduct);
		}
	}

	public HashSet<string> GetProductsByState(ShopBadging.ProductStatus state)
	{
		HashSet<string> productsByState = new HashSet<string>();
		string stateSuffix = $"_{(int)state}";
		foreach (string product in m_orderedProducts)
		{
			if (product.EndsWith(stateSuffix))
			{
				string productId = product.Split('_')[0];
				productsByState.Add(productId);
			}
		}
		return productsByState;
	}

	public HashSet<string> GetAllProductIds()
	{
		HashSet<string> allProductIds = new HashSet<string>();
		foreach (string orderedProduct in m_orderedProducts)
		{
			string productId = orderedProduct.Split('_')[0];
			allProductIds.Add(productId);
		}
		return allProductIds;
	}

	public ShopBadging.ProductStatus? GetProductStatus(string productId)
	{
		foreach (string orderedProduct in m_orderedProducts)
		{
			string[] parts = orderedProduct.Split('_');
			if (parts.Length == 2 && parts[0] == productId && int.TryParse(parts[1], out var status))
			{
				return (ShopBadging.ProductStatus)status;
			}
		}
		return null;
	}
}
