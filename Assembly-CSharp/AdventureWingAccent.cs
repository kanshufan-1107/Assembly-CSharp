using System.Collections.Generic;
using UnityEngine;

public class AdventureWingAccent : MonoBehaviour
{
	[SerializeField]
	public AdventureWing AssociatedWing;

	[SerializeField]
	public List<WingAccentMapping> WingAccentMappingList;

	private void Start()
	{
		if (!(AssociatedWing == null))
		{
			WingDbId wingId = AssociatedWing.GetWingId();
			GameObject accent = GetAccentObjectFromWingId(wingId);
			if (!(accent == null))
			{
				accent.SetActive(value: true);
			}
		}
	}

	private GameObject GetAccentObjectFromWingId(WingDbId wingId)
	{
		foreach (WingAccentMapping mapping in WingAccentMappingList)
		{
			if (mapping.WingId == wingId)
			{
				return mapping.AccentObject;
			}
		}
		return null;
	}
}
