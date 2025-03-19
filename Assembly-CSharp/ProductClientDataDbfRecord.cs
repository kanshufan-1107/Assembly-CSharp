using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ProductClientDataDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_productId;

	[SerializeField]
	private long m_pmtProductId;

	[SerializeField]
	private DbfLocValue m_popupTitle;

	[SerializeField]
	private DbfLocValue m_popupBody;

	[DbfField("PMT_PRODUCT_ID")]
	public long PmtProductId => m_pmtProductId;

	[DbfField("POPUP_TITLE")]
	public DbfLocValue PopupTitle => m_popupTitle;

	[DbfField("POPUP_BODY")]
	public DbfLocValue PopupBody => m_popupBody;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PRODUCT_ID" => m_productId, 
			"PMT_PRODUCT_ID" => m_pmtProductId, 
			"POPUP_TITLE" => m_popupTitle, 
			"POPUP_BODY" => m_popupBody, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "PRODUCT_ID":
			m_productId = (int)val;
			break;
		case "PMT_PRODUCT_ID":
			m_pmtProductId = (long)val;
			break;
		case "POPUP_TITLE":
			m_popupTitle = (DbfLocValue)val;
			break;
		case "POPUP_BODY":
			m_popupBody = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PRODUCT_ID" => typeof(int), 
			"PMT_PRODUCT_ID" => typeof(long), 
			"POPUP_TITLE" => typeof(DbfLocValue), 
			"POPUP_BODY" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadProductClientDataDbfRecords loadRecords = new LoadProductClientDataDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ProductClientDataDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ProductClientDataDbfAsset)) as ProductClientDataDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ProductClientDataDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
		m_popupTitle.StripUnusedLocales();
		m_popupBody.StripUnusedLocales();
	}
}
