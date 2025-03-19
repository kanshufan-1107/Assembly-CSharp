using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public abstract class ButtonListMenu : MonoBehaviour
{
	protected ButtonListMenuDef m_menu;

	protected GameLayer m_targetLayer = GameLayer.UI;

	protected bool m_useTextSizesInSlices;

	private bool m_isShown;

	private List<UIBButton> m_allButtons = new List<UIBButton>();

	private List<GameObject> m_horizontalDividers = new List<GameObject>();

	protected PegUIElement m_blocker;

	protected Transform m_menuParent;

	protected float PUNCH_SCALE = 1.08f;

	protected Vector3 NORMAL_SCALE = Vector3.one;

	protected static readonly Vector3 HIDDEN_SCALE = 0.01f * Vector3.one;

	protected string m_menuDefPrefab = "ButtonListMenuDef.prefab:1ab57b5c429373a4b8b4e0c0c706ca3e";

	protected bool m_showAnimation = true;

	protected virtual void Awake()
	{
		GameObject go = (GameObject)GameUtils.InstantiateGameObject(m_menuDefPrefab);
		m_menu = go.GetComponent<ButtonListMenuDef>();
		OverlayUI.Get().AddGameObject(base.gameObject);
		SetTransform();
		GameObject inputBlocker = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(go.layer), "GameMenuInputBlocker", this, go.transform, 10f);
		m_blocker = inputBlocker.AddComponent<PegUIElement>();
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		m_blocker.AddEventListener(UIEventType.RELEASE, OnBlockerRelease);
	}

	protected virtual void OnDestroy()
	{
		FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
	}

	public virtual void Show(bool playSound = true)
	{
		UniversalInputManager.Get().CancelTextInput(base.gameObject, force: true);
		SetTransform();
		if (playSound)
		{
			Log.Sound.PrintDebug("ButtonListMenu Playing sound Small_Click");
			SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		}
		base.gameObject.SetActive(value: true);
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		HideAllButtons();
		LayoutMenu();
		m_isShown = true;
		Bounds titleBounds = m_menu.m_headerText.GetTextBounds();
		TransformUtil.SetLocalScaleToWorldDimension(m_menu.m_headerMiddle, new WorldDimensionIndex(titleBounds.size.x, 0));
		m_menu.m_header.UpdateSlices();
		if (m_showAnimation)
		{
			AnimationUtil.ShowWithPunch(m_menu.gameObject, HIDDEN_SCALE, PUNCH_SCALE * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
		}
	}

	public virtual void Hide()
	{
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value: false);
		}
		UniversalInputManager.Get().SetGameDialogActive(active: false);
		m_isShown = false;
	}

	public bool IsShown()
	{
		return m_isShown;
	}

	public UIBButton CreateMenuButton(string name, string buttonTextString, UIEvent.Handler releaseHandler)
	{
		return CreateMenuButton(name, buttonTextString, releaseHandler, m_menu.m_templateButton);
	}

	public UIBButton CreateMenuButton(string name, string buttonTextString, UIEvent.Handler releaseHandler, UIBButton buttonTemplate)
	{
		UIBButton button = (UIBButton)GameUtils.Instantiate(buttonTemplate, m_menu.m_buttonContainer.gameObject);
		button.SetText(GameStrings.Get(buttonTextString));
		if (name != null)
		{
			button.gameObject.name = name;
		}
		button.AddEventListener(UIEventType.RELEASE, releaseHandler);
		button.transform.localRotation = buttonTemplate.transform.localRotation;
		m_allButtons.Add(button);
		return button;
	}

	public void DisableInputBlocker()
	{
		m_blocker.RemoveEventListener(UIEventType.RELEASE, OnBlockerRelease);
	}

	protected abstract List<UIBButton> GetButtons();

	protected void SetTransform()
	{
		if (m_menuParent == null)
		{
			m_menuParent = base.transform;
		}
		TransformUtil.AttachAndPreserveLocalTransform(m_menu.transform, m_menuParent);
		if (m_blocker != null)
		{
			m_blocker.transform.localPosition = new Vector3(0f, -5f, 0f);
			m_blocker.transform.eulerAngles = new Vector3(90f, 0f, 0f);
		}
		LayerUtils.SetLayer(this, m_targetLayer);
		m_menu.gameObject.transform.localScale = NORMAL_SCALE;
	}

	protected virtual void LayoutMenu()
	{
		LayoutMenuButtons();
		m_menu.m_buttonContainer.UpdateSlices();
		LayoutMenuBackground();
	}

	protected void LayoutMenuButtons()
	{
		List<UIBButton> buttons = GetButtons();
		m_menu.m_buttonContainer.ClearSlices();
		int i = 0;
		int dividerCount = 0;
		for (; i < buttons.Count; i++)
		{
			GameObject menuObj = null;
			UIBButton button = buttons[i];
			Vector3 minPadding = Vector3.zero;
			bool reverse = false;
			if (button == null)
			{
				GameObject divider;
				if (dividerCount >= m_horizontalDividers.Count)
				{
					divider = (GameObject)GameUtils.Instantiate(m_menu.m_templateHorizontalDivider, m_menu.m_buttonContainer.gameObject);
					divider.transform.localRotation = m_menu.m_templateHorizontalDivider.transform.localRotation;
					m_horizontalDividers.Add(divider);
				}
				else
				{
					divider = m_horizontalDividers[dividerCount];
				}
				dividerCount++;
				menuObj = divider;
				minPadding = m_menu.m_horizontalDividerMinPadding;
			}
			else
			{
				menuObj = button.gameObject;
			}
			MultiSliceElement buttonContainer = m_menu.m_buttonContainer;
			GameObject obj = menuObj;
			Vector3 minLocalPadding = minPadding;
			bool reverse2 = reverse;
			bool useTextSizesInSlices = m_useTextSizesInSlices;
			buttonContainer.AddSlice(obj, minLocalPadding, default(Vector3), reverse2, useMeshRenderers: true, useWidgetTransform: false, useTextSizesInSlices);
			menuObj.SetActive(value: true);
		}
	}

	protected void LayoutMenuBackground()
	{
		OrientedBounds orientedBounds = TransformUtil.ComputeOrientedWorldBounds(m_menu.m_buttonContainer.gameObject, default(Vector3), default(Vector3), null, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: true);
		float width = orientedBounds.Extents[0].magnitude * 2f;
		float height = orientedBounds.Extents[2].magnitude * 2f;
		m_menu.m_background.SetSize(width, height);
		m_menu.m_border.SetSize(width, height);
	}

	protected static void MakeButtonRed(UIBButton button, Material materialOverride)
	{
		MultiSliceElement multiSliceElement = button.m_RootObject.GetComponent<MultiSliceElement>();
		if (multiSliceElement == null)
		{
			Error.AddDevFatal("ButtonListMenu.MakeButtonRed() - Attempting to make button red, but the button does not have a multi slice element component!");
			return;
		}
		foreach (MultiSliceElement.Slice slice in multiSliceElement.m_slices)
		{
			GameObject sliceObject = slice.m_slice;
			if (sliceObject != null)
			{
				sliceObject.GetComponent<Renderer>().SetMaterial(materialOverride);
			}
		}
		if (button.m_ButtonText != null)
		{
			button.m_ButtonText.TextColor = Color.white;
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		if (SceneMgr.Get().GetNextMode() == SceneMgr.Mode.FATAL_ERROR)
		{
			Hide();
		}
	}

	private void HideAllButtons()
	{
		for (int i = 0; i < m_allButtons.Count; i++)
		{
			m_allButtons[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < m_horizontalDividers.Count; j++)
		{
			m_horizontalDividers[j].SetActive(value: false);
		}
	}

	protected virtual void OnBlockerRelease(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		Hide();
	}
}
