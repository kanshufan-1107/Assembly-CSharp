using System.Collections.Generic;
using UnityEngine;

public class BaconBoardSkinCorner : MonoBehaviour
{
	public GameObject m_TopContainer;

	public GameObject m_BackContainer;

	private List<GameObject> m_CopiedBackside = new List<GameObject>();

	public void CopyToBackside(GameObject source)
	{
		foreach (GameObject item in m_CopiedBackside)
		{
			Object.Destroy(item);
		}
		m_CopiedBackside.Clear();
		if (source == null)
		{
			return;
		}
		foreach (Transform child in source.transform)
		{
			m_CopiedBackside.Add(Object.Instantiate(child.gameObject, m_BackContainer.transform));
		}
	}

	public void OnDestroy()
	{
		foreach (GameObject item in m_CopiedBackside)
		{
			Object.Destroy(item);
		}
		m_CopiedBackside.Clear();
	}
}
