using System;
using System.Collections.Generic;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class TooltipZone : MonoBehaviour, IPopupRendering
{
	public delegate void TooltipChangeCallback(bool shown);

	public enum TooltipLayoutDirection
	{
		DOWN,
		UP
	}

	public GameObject tooltipPrefab;

	public Transform tooltipDisplayLocation;

	public Transform touchTooltipLocation;

	public GameObject targetObject;

	public TooltipLayoutDirection m_tooltipDirection;

	private List<GameObject> m_tooltips = new List<GameObject>();

	private TooltipChangeCallback m_changeCallback;

	private List<Action> m_onTooltipHiddenCallbacks = new List<Action>();

	private string m_defaultHeadlineText = string.Empty;

	private string m_defaultBodyText = string.Empty;

	private float m_defaultScale = 1f;

	private GameLayer? m_layerOverride;

	private GameLayer? m_screenConstraintLayerOverride;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	[Overridable]
	public GameLayer LayerOverride
	{
		get
		{
			if (!m_layerOverride.HasValue)
			{
				return GameLayer.Default;
			}
			return m_layerOverride.Value;
		}
		set
		{
			m_layerOverride = value;
			GameObject tooltipObj = GetTooltipObject();
			if (tooltipObj != null)
			{
				LayerUtils.SetLayer(tooltipObj, value);
			}
		}
	}

	[Overridable]
	public GameLayer ScreenConstraintLayerOverride
	{
		get
		{
			if (!m_screenConstraintLayerOverride.HasValue)
			{
				return GameLayer.Default;
			}
			return m_screenConstraintLayerOverride.Value;
		}
		set
		{
			m_screenConstraintLayerOverride = value;
			GameObject tooltipObj = GetTooltipObject();
			if (tooltipObj != null)
			{
				LayerUtils.SetLayer(tooltipObj, value);
			}
		}
	}

	[Overridable]
	public string HeadlineText
	{
		get
		{
			return m_defaultHeadlineText;
		}
		set
		{
			m_defaultHeadlineText = GameStrings.Get(value);
			TooltipPanel tooltipPanel = GetTooltipPanel();
			if (tooltipPanel != null)
			{
				tooltipPanel.SetName(m_defaultHeadlineText);
				tooltipPanel.m_name.UpdateNow();
			}
		}
	}

	[Overridable]
	public string BodyText
	{
		get
		{
			return m_defaultBodyText;
		}
		set
		{
			m_defaultBodyText = GameStrings.Get(value);
			TooltipPanel tooltipPanel = GetTooltipPanel();
			if (tooltipPanel != null)
			{
				tooltipPanel.SetBodyText(m_defaultBodyText);
				tooltipPanel.m_body.UpdateNow();
			}
		}
	}

	[Overridable]
	public float Scale
	{
		get
		{
			return m_defaultScale;
		}
		set
		{
			m_defaultScale = value;
			TooltipPanel tooltipPanel = GetTooltipPanel();
			if (tooltipPanel != null)
			{
				tooltipPanel.SetScale(value);
			}
		}
	}

	[Overridable]
	public bool Shown
	{
		get
		{
			return IsShowingTooltip();
		}
		set
		{
			if (value)
			{
				ShowTooltip();
			}
			else
			{
				HideTooltip();
			}
		}
	}

	private void Awake()
	{
		WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
		if (widgetTransform != null && GetComponent<Clickable>() == null)
		{
			widgetTransform.CreateBoxCollider(base.gameObject);
		}
	}

	public GameObject GetTooltipObject(int index = 0)
	{
		if (index < 0 || index >= m_tooltips.Count)
		{
			return null;
		}
		return m_tooltips[index];
	}

	public TooltipPanel GetTooltipPanel(int index = 0)
	{
		GameObject tooltipObj = GetTooltipObject(index);
		if (tooltipObj == null)
		{
			return null;
		}
		return tooltipObj.GetComponent<TooltipPanel>();
	}

	public bool IsShowingTooltip(int index = 0)
	{
		return GetTooltipObject(index) != null;
	}

	public TooltipPanel ShowTooltip(int index = 0)
	{
		TooltipPanel tooltipPanel = ShowTooltip(m_defaultHeadlineText, m_defaultBodyText, m_defaultScale, Vector3.zero, index);
		if (tooltipPanel != null)
		{
			tooltipPanel.SetScale(m_defaultScale);
			if (m_layerOverride.HasValue)
			{
				LayerUtils.SetLayer(tooltipPanel, m_layerOverride.Value);
			}
		}
		return tooltipPanel;
	}

	public TooltipPanel ShowTooltip(string headline, string bodytext, float scale, int index = 0)
	{
		return ShowTooltip(headline, bodytext, scale, Vector3.zero, index);
	}

	public TooltipPanel ShowTooltip(string headline, string bodytext, float scale, Vector3 localOffset, int index = 0)
	{
		if (IsShowingTooltip(index))
		{
			return m_tooltips[index].GetComponent<TooltipPanel>();
		}
		if (index < 0)
		{
			return null;
		}
		while (m_tooltips.Count <= index)
		{
			m_tooltips.Add(null);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			scale *= 2f;
		}
		m_tooltips[index] = UnityEngine.Object.Instantiate(tooltipPrefab);
		TooltipPanel helpPanel = m_tooltips[index].GetComponent<TooltipPanel>();
		helpPanel.Reset();
		helpPanel.Initialize(headline, bodytext);
		helpPanel.SetScale(scale);
		if (UniversalInputManager.Get().IsTouchMode() && touchTooltipLocation != null)
		{
			helpPanel.transform.position = touchTooltipLocation.position;
			helpPanel.transform.rotation = touchTooltipLocation.rotation;
		}
		else if (tooltipDisplayLocation != null)
		{
			helpPanel.transform.position = tooltipDisplayLocation.position;
			helpPanel.transform.rotation = tooltipDisplayLocation.rotation;
		}
		helpPanel.transform.parent = base.transform;
		helpPanel.transform.localPosition += localOffset;
		Vector3 previousTooltipHeights = GetHeightOfPreviousTooltips(index);
		helpPanel.transform.localPosition += previousTooltipHeights;
		int screenConstraintsLayer = base.gameObject.layer;
		if (m_popupRoot != null && !m_popupRoot.IsPerspectivePopup)
		{
			screenConstraintsLayer = 5;
		}
		else if (m_screenConstraintLayerOverride.HasValue)
		{
			screenConstraintsLayer = (int)m_screenConstraintLayerOverride.Value;
		}
		TransformUtil.ConstrainToScreen(m_tooltips[index], screenConstraintsLayer);
		if (m_changeCallback != null)
		{
			m_changeCallback(shown: true);
		}
		helpPanel.ShiftBodyText();
		if (m_popupRoot != null)
		{
			m_popupRoot.ApplyPopupRendering(m_tooltips[index].transform, m_popupRenderingComponents, overrideLayer: true, m_layerOverride.HasValue ? ((int)m_layerOverride.Value) : base.gameObject.layer);
		}
		return helpPanel;
	}

	public void ShowGameplayTooltip(string headline, string bodytext, int index = 0)
	{
		ShowGameplayTooltip(headline, bodytext, Vector3.zero, index);
	}

	public void ShowGameplayTooltip(string headline, string bodytext, Vector3 localOffset, int index = 0)
	{
		ShowTooltip(headline, bodytext, 0.75f, localOffset, index);
	}

	public void ShowGameplayTooltipLarge(string headline, string bodytext, int index = 0)
	{
		ShowGameplayTooltipLarge(headline, bodytext, Vector3.zero, index);
	}

	public void ShowGameplayTooltipLarge(string headline, string bodytext, Vector3 localOffset, int index = 0)
	{
		ShowTooltip(headline, bodytext, TooltipPanel.GAMEPLAY_SCALE_LARGE, localOffset, index);
	}

	public void ShowBoxTooltip(string headline, string bodytext, int index = 0)
	{
		ShowTooltip(headline, bodytext, TooltipPanel.BOX_SCALE, index);
	}

	public void ShowCollectionManagerTooltip(string headline, string bodytext, int index = 0)
	{
		ShowTooltip(headline, bodytext, TooltipPanel.COLLECTION_MANAGER_SCALE, index);
	}

	public TooltipPanel ShowLayerTooltip(string headline, string bodytext, int index = 0)
	{
		return ShowLayerTooltip(headline, bodytext, 1f, index);
	}

	public TooltipPanel ShowLayerTooltip(string headline, string bodytext, float scale, int index = 0)
	{
		TooltipPanel tooltipPanel = ShowTooltip(headline, bodytext, scale, index);
		if (tooltipDisplayLocation == null || tooltipPanel == null)
		{
			return tooltipPanel;
		}
		tooltipPanel.transform.parent = tooltipDisplayLocation.transform;
		Vector3 tooltipScale = new Vector3(scale, scale, scale);
		tooltipPanel.transform.localScale = tooltipScale;
		LayerUtils.SetLayer(m_tooltips[index], tooltipDisplayLocation.gameObject.layer, null);
		return tooltipPanel;
	}

	public void ShowSocialTooltip(Component target, string headline, string bodytext, float scale, GameLayer layer, int index = 0)
	{
		ShowSocialTooltip(target.gameObject, headline, bodytext, scale, layer, index);
	}

	public void ShowSocialTooltip(GameObject tooltipTargetObject, string headline, string bodytext, float scale, GameLayer layer, int index = 0)
	{
		ShowTooltip(headline, bodytext, scale, index);
		LayerUtils.SetLayer(m_tooltips[index], layer);
		Camera targetCamera = CameraUtils.FindFirstByLayer(tooltipTargetObject.layer);
		Camera tooltipCamera = CameraUtils.FindFirstByLayer(m_tooltips[index].layer);
		if (targetCamera != tooltipCamera)
		{
			Vector3 screenPos = targetCamera.WorldToScreenPoint(m_tooltips[index].transform.position);
			Vector3 tooltipPos = tooltipCamera.ScreenToWorldPoint(screenPos);
			m_tooltips[index].transform.position = tooltipPos;
		}
	}

	public void ShowMultiColumnTooltip(string headline, string bodytext, string[] columnsText, float scale, int index = 0)
	{
		TooltipPanel tooltipPanel = ShowTooltip(headline, bodytext, scale, index);
		if (tooltipPanel is MultiColumnTooltipPanel)
		{
			MultiColumnTooltipPanel panel = (MultiColumnTooltipPanel)tooltipPanel;
			if (columnsText.Length > panel.m_textColumns.Count)
			{
				Log.All.PrintWarning("ShowMultiColumnTooltip - Attempting to display {0} columns of text, when the prefab only supports {1} columns!", columnsText.Length, panel.m_textColumns.Count);
			}
			for (int i = 0; i < columnsText.Length && i < panel.m_textColumns.Count; i++)
			{
				panel.m_textColumns[i].Text = columnsText[i];
			}
		}
	}

	private Vector3 GetHeightOfPreviousTooltips(int currentIndex)
	{
		float offset = 0f;
		if (m_tooltipDirection == TooltipLayoutDirection.DOWN)
		{
			for (int index = 0; index < currentIndex; index++)
			{
				if (IsShowingTooltip(index))
				{
					TooltipPanel tooltipPanel = GetTooltipObject(index).GetComponent<TooltipPanel>();
					offset -= tooltipPanel.GetHeight() / 2f;
				}
			}
		}
		else if (m_tooltipDirection == TooltipLayoutDirection.UP)
		{
			for (int i = 1; i <= currentIndex; i++)
			{
				if (IsShowingTooltip(i))
				{
					TooltipPanel tooltipPanel2 = GetTooltipObject(i).GetComponent<TooltipPanel>();
					offset += tooltipPanel2.GetHeight() * 1.5f;
				}
			}
		}
		return new Vector3(0f, 0f, offset);
	}

	public void AnchorTooltipTo(GameObject target, Anchor targetAnchorPoint, Anchor tooltipAnchorPoint, int index = 0)
	{
		if (IsShowingTooltip(index))
		{
			TransformUtil.SetPoint(m_tooltips[index], tooltipAnchorPoint, target, targetAnchorPoint);
		}
	}

	public void HideTooltip()
	{
		m_changeCallback?.Invoke(shown: false);
		foreach (Action onTooltipHiddenCallback in m_onTooltipHiddenCallbacks)
		{
			onTooltipHiddenCallback();
		}
		m_onTooltipHiddenCallbacks.Clear();
		foreach (GameObject tooltip in m_tooltips)
		{
			if (tooltip != null)
			{
				UnityEngine.Object.Destroy(tooltip);
			}
		}
		m_tooltips.Clear();
	}

	public void SetTooltipChangeCallback(TooltipChangeCallback callback)
	{
		m_changeCallback = callback;
	}

	public void RegisterOnTooltipHiddenCallback(Action callback)
	{
		m_onTooltipHiddenCallbacks.Add(callback);
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return false;
	}
}
