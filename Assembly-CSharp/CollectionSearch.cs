using System.Collections.Generic;
using System.Text;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class CollectionSearch : MonoBehaviour
{
	public delegate void ActivatedListener();

	public delegate void DeactivatedListener(string oldSearchText, string newSearchText);

	public delegate void ClearedListener(bool updateVisuals);

	public UberText m_searchText;

	public PegUIElement m_background;

	public PegUIElement m_clearButton;

	public GameObject m_xMesh;

	public GameObject m_backgroundWhenAtBottom;

	public GameObject m_backgroundWhenAtBottomTavernBrawl;

	public GameObject m_backgroundWhenAtTopNormal;

	public GameObject m_backgroundWhenAtTopWild;

	public Color m_altSearchColor;

	private const float ANIM_TIME = 0.1f;

	private const int MAX_SEARCH_LENGTH = 31;

	private Material m_origSearchMaterial;

	private Vector3 m_origSearchPos;

	private bool m_isActive;

	private string m_prevText;

	private string m_text;

	private bool m_wildModeActive;

	private List<ActivatedListener> m_activatedListeners = new List<ActivatedListener>();

	private List<DeactivatedListener> m_deactivatedListeners = new List<DeactivatedListener>();

	private List<ClearedListener> m_clearedListeners = new List<ClearedListener>();

	private GameLayer m_originalLayer;

	private GameLayer m_activeLayer;

	private bool m_isTouchKeyboardDisplayMode;

	private static readonly Dictionary<char, char> s_SpecialCharReplacements = new Dictionary<char, char>
	{
		{ '‘', '\'' },
		{ '’', '\'' }
	};

	private static string SanitizeSpecialCharacters(string searchStr)
	{
		if (string.IsNullOrEmpty(searchStr))
		{
			return searchStr;
		}
		Locale locale = Localization.GetLocale();
		if (locale == Locale.deDE || locale == Locale.frFR)
		{
			return searchStr;
		}
		bool containedSpecialChars = false;
		StringBuilder strBuilder = new StringBuilder(searchStr.Length);
		for (int i = 0; i < searchStr.Length; i++)
		{
			char nextChar = searchStr[i];
			if (s_SpecialCharReplacements.TryGetValue(nextChar, out var sanitizedChar))
			{
				containedSpecialChars = true;
				nextChar = sanitizedChar;
			}
			strBuilder.Append(nextChar);
		}
		if (!containedSpecialChars)
		{
			return searchStr;
		}
		string newStr = strBuilder.ToString();
		Log.CollectionManager.PrintInfo("Replaced special characters for search string ({0}): (original: {1}) (new: {2})", locale, searchStr, newStr);
		return newStr;
	}

	private void Start()
	{
		m_background.AddEventListener(UIEventType.RELEASE, OnBackgroundReleased);
		m_clearButton.AddEventListener(UIEventType.RELEASE, OnClearReleased);
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		touchScreenService.AddOnVirtualKeyboardShowListener(OnKeyboardShown);
		touchScreenService.AddOnVirtualKeyboardHideListener(OnKeyboardHidden);
		m_origSearchPos = base.transform.localPosition;
		UpdateBackground();
		UpdateSearchText();
	}

	private void OnDestroy()
	{
		if (ServiceManager.TryGet<ITouchScreenService>(out var touchScreenService))
		{
			touchScreenService.RemoveOnVirtualKeyboardShowListener(OnKeyboardShown);
			touchScreenService.RemoveOnVirtualKeyboardHideListener(OnKeyboardHidden);
		}
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().CancelTextInput(base.gameObject);
		}
	}

	public bool IsActive()
	{
		return m_isActive;
	}

	public void SetActiveLayer(GameLayer activeLayer)
	{
		if (activeLayer != m_activeLayer)
		{
			m_activeLayer = activeLayer;
			if (IsActive())
			{
				MoveToActiveLayer(saveOriginalLayer: false);
			}
		}
	}

	public void SetWildModeActive(bool active)
	{
		m_wildModeActive = active;
	}

	public void Activate(bool ignoreTouchMode = false)
	{
		if (!m_isActive)
		{
			m_background.SetEnabled(enabled: false);
			MoveToActiveLayer(saveOriginalLayer: true);
			m_isActive = true;
			m_prevText = m_text;
			ActivatedListener[] array = m_activatedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
			ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
			if (!ignoreTouchMode && UniversalInputManager.Get().UseWindowsTouch() && touchScreenService.IsTouchSupported() && touchScreenService.IsVirtualKeyboardVisible())
			{
				TouchKeyboardSearchDisplay(fromActivate: true);
			}
			else
			{
				ShowInput();
			}
		}
	}

	public void Deactivate()
	{
		if (m_isActive)
		{
			m_background.SetEnabled(enabled: true);
			MoveToOriginalLayer();
			m_isActive = false;
			HideInput();
			ResetSearchDisplay();
			DeactivatedListener[] array = m_deactivatedListeners.ToArray();
			string searchStr = SanitizeSpecialCharacters(m_text);
			string prevSearchStr = SanitizeSpecialCharacters(m_prevText);
			DeactivatedListener[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i](prevSearchStr, searchStr);
			}
		}
	}

	public void Cancel()
	{
		if (m_isActive)
		{
			m_text = m_prevText;
			UpdateSearchText();
			Deactivate();
		}
	}

	public string GetText()
	{
		return m_text;
	}

	public void SetText(string text)
	{
		m_text = text;
		UpdateSearchText();
	}

	public void ClearFilter(bool updateVisuals = true)
	{
		m_text = "";
		UpdateSearchText();
		ClearInput();
		ClearedListener[] array = m_clearedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](updateVisuals);
		}
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		if ((UniversalInputManager.Get().IsTouchMode() && touchScreenService.IsTouchSupported()) || touchScreenService.IsVirtualKeyboardVisible())
		{
			Deactivate();
		}
	}

	public void RegisterActivatedListener(ActivatedListener listener)
	{
		if (!m_activatedListeners.Contains(listener))
		{
			m_activatedListeners.Add(listener);
		}
	}

	public void RemoveActivatedListener(ActivatedListener listener)
	{
		m_activatedListeners.Remove(listener);
	}

	public void RegisterDeactivatedListener(DeactivatedListener listener)
	{
		if (!m_deactivatedListeners.Contains(listener))
		{
			m_deactivatedListeners.Add(listener);
		}
	}

	public void RemoveDeactivatedListener(DeactivatedListener listener)
	{
		m_deactivatedListeners.Remove(listener);
	}

	public void RegisterClearedListener(ClearedListener listener)
	{
		if (!m_clearedListeners.Contains(listener))
		{
			m_clearedListeners.Add(listener);
		}
	}

	public void RemoveClearedListener(ClearedListener listener)
	{
		m_clearedListeners.Remove(listener);
	}

	public void SetEnabled(bool enabled)
	{
		m_background.SetEnabled(enabled);
		m_clearButton.SetEnabled(enabled);
	}

	private void OnBackgroundReleased(UIEvent e)
	{
		Activate();
	}

	private void OnClearReleased(UIEvent e)
	{
		ClearFilter();
	}

	private void OnActivateAnimComplete()
	{
		ShowInput();
	}

	private void OnDeactivateAnimComplete()
	{
		DeactivatedListener[] array = m_deactivatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](m_prevText, m_text);
		}
	}

	private void ShowInput(bool fromActivate = true)
	{
		Bounds textBounds = m_searchText.GetBounds();
		m_searchText.gameObject.SetActive(value: false);
		Rect rect = CameraUtils.CreateGUIViewportRect(Box.Get().GetCamera(), textBounds.min, textBounds.max);
		Color? searchColor = null;
		if (ServiceManager.Get<ITouchScreenService>().IsVirtualKeyboardVisible())
		{
			searchColor = m_altSearchColor;
		}
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = base.gameObject,
			m_rect = rect,
			m_updatedCallback = OnInputUpdated,
			m_completedCallback = OnInputComplete,
			m_canceledCallback = OnInputCanceled,
			m_unfocusedCallback = OnInputUnfocus,
			m_font = m_searchText.GetLocalizedFont(),
			m_text = m_text,
			m_color = searchColor,
			m_touchScreenKeyboardHideInput = false,
			m_gameLayer = GameLayer.UI,
			m_orderInLayer = -1
		};
		inputParams.m_showVirtualKeyboard = fromActivate;
		UniversalInputManager.Get().UseTextInput(inputParams);
	}

	private void HideInput()
	{
		UniversalInputManager.Get().CancelTextInput(base.gameObject);
		m_searchText.gameObject.SetActive(value: true);
	}

	private void ClearInput()
	{
		if (m_isActive)
		{
			SoundManager.Get().LoadAndPlay("text_box_delete_text.prefab:b4209934f760cc745b3dba5add912398");
			UniversalInputManager.Get().SetInputText("");
		}
	}

	private void OnInputUpdated(string input)
	{
		m_text = input;
		UpdateSearchText();
	}

	private void OnInputComplete(string input)
	{
		m_text = input;
		UpdateSearchText();
		SoundManager.Get().LoadAndPlay("text_commit.prefab:05a794ae046d3e842b87893629a826f1");
		Deactivate();
	}

	private void OnInputCanceled(bool userRequested, GameObject requester)
	{
		Cancel();
	}

	private void OnInputUnfocus()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			Deactivate();
		}
	}

	private void UpdateSearchText()
	{
		if (string.IsNullOrEmpty(m_text))
		{
			m_searchText.Text = GameStrings.Get("GLUE_COLLECTION_SEARCH");
			m_clearButton.gameObject.SetActive(value: false);
		}
		else
		{
			m_searchText.Text = m_text;
			m_clearButton.gameObject.SetActive(value: true);
		}
	}

	private void MoveToActiveLayer(bool saveOriginalLayer)
	{
		if (saveOriginalLayer)
		{
			m_originalLayer = (GameLayer)base.gameObject.layer;
		}
		LayerUtils.SetLayer(base.gameObject, m_activeLayer);
	}

	private void MoveToOriginalLayer()
	{
		LayerUtils.SetLayer(base.gameObject, m_originalLayer);
	}

	private void TouchKeyboardSearchDisplay(bool fromActivate = false)
	{
		if (!m_isTouchKeyboardDisplayMode)
		{
			m_isTouchKeyboardDisplayMode = true;
			CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
			if (collectibleDisplay != null)
			{
				base.transform.localPosition = collectibleDisplay.m_activeSearchBone_Win8.transform.localPosition;
			}
			HideInput();
			ShowInput(fromActivate || ServiceManager.Get<ITouchScreenService>().IsVirtualKeyboardVisible());
			m_xMesh.GetComponent<Renderer>().GetMaterial().SetColor("_Color", m_altSearchColor);
			UpdateBackground();
		}
	}

	private void ResetSearchDisplay()
	{
		if (m_isTouchKeyboardDisplayMode)
		{
			m_isTouchKeyboardDisplayMode = false;
			base.transform.localPosition = m_origSearchPos;
			HideInput();
			ShowInput(fromActivate: false);
			m_xMesh.GetComponent<Renderer>().GetMaterial().SetColor("_Color", Color.white);
			UpdateBackground();
		}
	}

	private void OnKeyboardShown()
	{
		if (m_isActive && !m_isTouchKeyboardDisplayMode)
		{
			TouchKeyboardSearchDisplay();
		}
	}

	private void OnKeyboardHidden()
	{
		if (m_isActive && m_isTouchKeyboardDisplayMode)
		{
			ResetSearchDisplay();
		}
	}

	private void UpdateBackground()
	{
		bool anyTopVariantExists = m_backgroundWhenAtTopNormal != null || m_backgroundWhenAtTopWild != null;
		GameObject bottomBackground = ((SceneMgr.Get().IsInTavernBrawlMode() && m_backgroundWhenAtBottomTavernBrawl != null) ? m_backgroundWhenAtBottomTavernBrawl : m_backgroundWhenAtBottom);
		if (bottomBackground != null)
		{
			bottomBackground.gameObject.SetActive(!m_isTouchKeyboardDisplayMode || !anyTopVariantExists);
		}
		if (m_backgroundWhenAtTopNormal != null)
		{
			m_backgroundWhenAtTopNormal.gameObject.SetActive(m_isTouchKeyboardDisplayMode && (m_backgroundWhenAtTopWild == null || !m_wildModeActive));
		}
		if (m_backgroundWhenAtTopWild != null)
		{
			m_backgroundWhenAtTopWild.gameObject.SetActive(m_isTouchKeyboardDisplayMode && m_wildModeActive);
		}
	}
}
