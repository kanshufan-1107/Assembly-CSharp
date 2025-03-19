using System.Collections;
using UnityEngine;

public class CustomFrameDiamondPrefab : MonoBehaviour
{
	public GameObject PortraitRTT;

	public bool m_loading;

	public void Load()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(ShowNextFrame());
		}
	}

	private IEnumerator ShowNextFrame()
	{
		m_loading = true;
		yield return new WaitForEndOfFrame();
		m_loading = false;
	}
}
