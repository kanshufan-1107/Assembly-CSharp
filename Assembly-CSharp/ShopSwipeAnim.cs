using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using UnityEngine;

public class ShopSwipeAnim : MonoBehaviour
{
	[SerializeField]
	private PlayMakerFSM m_screenWipePlaymaker;

	[SerializeField]
	private List<GameObject> m_screenWipeGameobjects;

	private float m_animDelay;

	private const string ScreenWipe = "ScreenWipe";

	public bool IsReady
	{
		get
		{
			if (m_animDelay >= 0f)
			{
				return m_screenWipePlaymaker != null;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		SetVisibilityOnAnimationObjects(isVisible: false);
	}

	public void InitAnimation(ProductDataModel product)
	{
		if (product != null)
		{
			string animDelay = product.ShopSwipeAnimDelay;
			if (!string.IsNullOrEmpty(animDelay) && float.TryParse(animDelay, out m_animDelay))
			{
				m_animDelay /= 10f;
			}
		}
	}

	public void SetDefaultDelay(float delay)
	{
		m_animDelay = delay;
	}

	public void ResetAnim()
	{
		SetVisibilityOnAnimationObjects(isVisible: false);
	}

	private void SetVisibilityOnAnimationObjects(bool isVisible)
	{
		foreach (GameObject go in m_screenWipeGameobjects)
		{
			if (go != null)
			{
				go.SetActive(isVisible);
			}
		}
	}

	public IEnumerator PlayShopSwipeWithDelay()
	{
		yield return new WaitForSeconds(m_animDelay);
		SetVisibilityOnAnimationObjects(isVisible: true);
		m_screenWipePlaymaker.SendEvent("ScreenWipe");
	}
}
