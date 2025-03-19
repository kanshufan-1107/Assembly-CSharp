using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class DataDrivenSignatureTooltipPanel : MonoBehaviour, IPopupRendering
{
	public const string CODE_HIDE_SIGNATURE_TOOLTIP = "CODE_HIDE_SIGNATURE_TOOLTIP";

	public const string CODE_SHOW_SIGNATURE_TOOLTIP = "CODE_SHOW_SIGNATURE_TOOLTIP";

	public const string CODE_HIDE_SIGNATURE_TOOLTIP_MPO = "CODE_HIDE_SIGNATURE_TOOLTIP_MPO";

	public const string CODE_SHOW_SIGNATURE_TOOLTIP_MPO = "CODE_SHOW_SIGNATURE_TOOLTIP_MPO";

	[SerializeField]
	private GameObject m_signatureTooltipBone;

	[SerializeField]
	private GameObject m_signatureTooltipBoneForRewardPreview;

	[SerializeField]
	private GameObject m_signatureTooltipBoneForRewardScroll;

	[SerializeField]
	private GameObject m_signatureTooltipBoneForShop;

	[SerializeField]
	private GameObject m_signatureTooltipBoneMPO;

	private SignatureTooltipPanel m_signatureTooltipPanel;

	private WidgetTemplate m_widgetTemplate;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private bool m_hasAppliedPopupRendering;

	private void Awake()
	{
		m_widgetTemplate = GetComponent<WidgetTemplate>();
		if (m_widgetTemplate != null)
		{
			m_widgetTemplate.RegisterEventListener(HandleEvent);
		}
	}

	private void OnDisable()
	{
		DisablePopupRendering();
		TooltipPanelManager.Get()?.HideTooltipPanels();
		m_signatureTooltipPanel = null;
	}

	private void OnDestroy()
	{
		if (m_widgetTemplate != null)
		{
			m_widgetTemplate.RemoveEventListener(HandleEvent);
		}
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_HIDE_SIGNATURE_TOOLTIP":
			TooltipPanelManager.Get().HideTooltipPanels();
			m_widgetTemplate.Hide();
			break;
		case "CODE_SHOW_SIGNATURE_TOOLTIP":
			m_widgetTemplate.Show();
			StartCoroutine(ShowTooltip());
			break;
		case "CODE_HIDE_SIGNATURE_TOOLTIP_MPO":
			TooltipPanelManager.Get().HideTooltipPanels();
			break;
		case "CODE_SHOW_SIGNATURE_TOOLTIP_MPO":
			StartCoroutine(ShowTooltip());
			break;
		}
	}

	private IEnumerator ShowTooltip()
	{
		bool isRewardScroll = GetComponentInParent<RewardScroll>() != null;
		bool isRewardTrackPopup = GetComponentInParent<RewardTrackItemClaimListener>() != null;
		bool isShop = GetComponentInParent<IndividualProductPreview>() != null;
		bool isMassPackOpening = GetComponentInParent<MassPackOpeningSummary>() != null;
		if (!isRewardScroll && !isRewardTrackPopup && !isShop && !isMassPackOpening)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.4f);
		if (!(m_widgetTemplate != null) || !m_widgetTemplate.GetDataModel(27, out var dataModel))
		{
			yield break;
		}
		CardDataModel cardDataModel = dataModel as CardDataModel;
		if (ActorNames.GetSignatureFrameId(cardDataModel.CardId) != 1)
		{
			GameObject customTransform = m_signatureTooltipBone;
			if (isRewardTrackPopup && m_signatureTooltipBoneForRewardPreview != null)
			{
				customTransform = m_signatureTooltipBoneForRewardPreview;
			}
			else if (isRewardScroll && m_signatureTooltipBoneForRewardScroll != null)
			{
				customTransform = m_signatureTooltipBoneForRewardScroll;
			}
			else if (isShop && m_signatureTooltipBoneForShop != null)
			{
				customTransform = m_signatureTooltipBoneForShop;
			}
			else if (isMassPackOpening && m_signatureTooltipBoneMPO != null)
			{
				customTransform = m_signatureTooltipBoneMPO;
			}
			m_signatureTooltipPanel = TooltipPanelManager.Get().UpdateSignatureTooltipFromDataModelAtCustomTransform(cardDataModel, customTransform.transform, useCustomTransformScale: true);
			if (m_popupRoot != null && !m_hasAppliedPopupRendering)
			{
				PropagatePopupRendering(null);
			}
		}
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
		PropagatePopupRendering(null);
	}

	private void PropagatePopupRendering(object unused)
	{
		if (m_signatureTooltipPanel != null)
		{
			m_popupRoot.ApplyPopupRendering(m_signatureTooltipPanel.transform, m_popupRenderingComponents, overrideLayer: true, 29);
			m_hasAppliedPopupRendering = true;
		}
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
		}
		m_popupRoot = null;
		m_hasAppliedPopupRendering = false;
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}
}
