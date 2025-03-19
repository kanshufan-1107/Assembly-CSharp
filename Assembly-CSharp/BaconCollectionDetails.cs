using System;
using Hearthstone;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public abstract class BaconCollectionDetails : MonoBehaviour
{
	[SerializeField]
	private float m_animationTime = 0.15f;

	[SerializeField]
	private UberText m_DebugText;

	[SerializeField]
	protected Widget m_widget;

	[SerializeField]
	protected GameObject m_scaleParent;

	protected bool m_animating;

	protected bool m_isShown;

	protected Vector3 m_originalScale;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected abstract string DebugTextValue { get; }

	protected abstract bool ValidateDataModels(IDataModel dataModel, IDataModel pageDataModel);

	public abstract void AssignDataModels(IDataModel dataModel, IDataModel pageDataModel);

	protected abstract void ClearDataModels();

	protected abstract void DetailsEventListener(string eventName);

	protected virtual void Start()
	{
		if (m_widget == null)
		{
			Debug.LogError(GetType().Name + ": No widget found, will not be able to show.");
			return;
		}
		m_widget.RegisterEventListener(DetailsEventListener);
		m_widget.RegisterReadyListener(delegate
		{
			OnWidgetReady();
		});
		base.gameObject.SetActive(value: false);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public bool CanShow(IDataModel dataModel, IDataModel pageDataModel)
	{
		if (m_animating || m_isShown)
		{
			return false;
		}
		if (m_widget == null)
		{
			Debug.LogError(GetType().Name + ": No widget assigned, cannot show.");
			return false;
		}
		if (!ValidateDataModels(dataModel, pageDataModel))
		{
			Debug.LogError(GetType().Name + ": Invalid data models assigned, cannot show");
			return false;
		}
		return true;
	}

	public virtual void Show()
	{
		m_isShown = true;
		if (CollectionManager.Get() != null && CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
			if (cpm != null)
			{
				cpm.EnablePageTurn(enable: false);
				cpm.EnablePageTurnArrows(enable: false);
			}
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = m_animationTime;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		base.gameObject.SetActive(value: true);
		m_scaleParent.SetActive(value: true);
		m_animating = true;
		m_scaleParent.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		iTween.ScaleTo(m_scaleParent, iTween.Hash("scale", m_originalScale, "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", new Action<object>(OnShowAnimationComplete)));
		ShowDebugText();
	}

	public bool CanHide()
	{
		if (m_animating || !m_isShown)
		{
			return false;
		}
		return true;
	}

	public virtual void Hide()
	{
		m_isShown = false;
		CollectionManager cm = CollectionManager.Get();
		if (cm != null && cm.GetCollectibleDisplay() != null)
		{
			CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
			if (cpm != null)
			{
				cpm.EnablePageTurn(enable: true);
				cpm.EnablePageTurnArrows(enable: true);
			}
			m_screenEffectsHandle.StopEffect();
		}
		m_animating = true;
		iTween.ScaleTo(m_scaleParent, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", new Action<object>(OnHideAnimationComplete)));
		ClearDataModels();
		LuckyDrawManager.Get()?.UpdateAllRewardsOwnedStatus();
	}

	public void Unload()
	{
		if (m_widget != null)
		{
			m_widget.RemoveEventListener(DetailsEventListener);
		}
	}

	protected virtual void OnShowAnimationComplete(object objectData)
	{
		m_animating = false;
	}

	protected virtual void OnHideAnimationComplete(object objectData)
	{
		m_animating = false;
		if ((bool)base.gameObject && !m_isShown)
		{
			m_scaleParent.SetActive(value: false);
			m_scaleParent.transform.localScale = m_originalScale;
		}
	}

	protected virtual void OnWidgetReady()
	{
		InitializeScaleParent();
	}

	private void ShowDebugText()
	{
		if (!(m_DebugText == null))
		{
			if (HearthstoneApplication.IsInternal() && Options.Get().GetBool(Option.DEBUG_SHOW_BATTLEGROUND_SKIN_IDS))
			{
				m_DebugText.Text = DebugTextValue;
				m_DebugText.gameObject.SetActive(value: true);
			}
			else
			{
				m_DebugText.Text = string.Empty;
				m_DebugText.gameObject.SetActive(value: false);
			}
		}
	}

	private void InitializeScaleParent()
	{
		if (m_scaleParent == null)
		{
			m_scaleParent = base.gameObject;
		}
		if (m_originalScale == Vector3.zero)
		{
			m_originalScale = m_scaleParent.transform.localScale;
		}
	}
}
