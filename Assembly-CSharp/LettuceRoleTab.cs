using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class LettuceRoleTab : BookTab
{
	public Vector3 m_MobileDeselectedLocalScale = new Vector3(10f, 10f, 10f);

	public Vector3 m_MobileSelectedLocalScale = new Vector3(12f, 12f, 12f);

	public float m_MobileSelectedLocalYPos = 0.1259841f;

	public float m_MobileDeselectedLocalYPos;

	public AsyncReference m_roleIconsReference;

	public AsyncReference m_clickFXReference;

	private TAG_ROLE m_roleTag;

	private VisualController m_roleIconsController;

	private VisualController m_clickFXController;

	public void Init(TAG_ROLE roleTag)
	{
		m_roleTag = roleTag;
		m_roleIconsReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_roleIconsController = vc;
		});
		m_clickFXReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_clickFXController = vc;
		});
		StartCoroutine(InitializeWhenReady());
		Init();
	}

	public TAG_ROLE GetRole()
	{
		return m_roleTag;
	}

	public override void SetLargeTab(bool large)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			base.SetLargeTab(large);
		}
		else if (large != m_showLargeTab)
		{
			if (large)
			{
				Vector3 pos = base.transform.localPosition;
				pos.y = m_MobileSelectedLocalYPos;
				base.transform.localPosition = pos;
				Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
				scaleArgs.Add("scale", m_MobileSelectedLocalScale);
				scaleArgs.Add("time", BookTab.SELECT_TAB_ANIM_TIME);
				scaleArgs.Add("name", "scale");
				iTween.ScaleTo(base.gameObject, scaleArgs);
				SoundManager.Get().LoadAndPlay("class_tab_click.prefab:d9cb832f0de5c1947a97685e134ba0da", base.gameObject);
			}
			else
			{
				Vector3 pos2 = base.transform.localPosition;
				pos2.y = m_MobileDeselectedLocalYPos;
				base.transform.localPosition = pos2;
				iTween.StopByName(base.gameObject, "scale");
				base.transform.localScale = m_MobileDeselectedLocalScale;
			}
			m_showLargeTab = large;
		}
	}

	public void PlayClickFX()
	{
		m_clickFXController.SetState("PLAY_CLICK_FX_code");
	}

	public bool IsFinishedLoading()
	{
		if ((bool)m_roleIconsController)
		{
			return m_clickFXController;
		}
		return false;
	}

	private IEnumerator InitializeWhenReady()
	{
		while (!IsFinishedLoading())
		{
			yield return null;
		}
		string eventMsg = m_roleTag.ToString();
		m_roleIconsController.OwningWidget.TriggerEvent(eventMsg);
	}
}
