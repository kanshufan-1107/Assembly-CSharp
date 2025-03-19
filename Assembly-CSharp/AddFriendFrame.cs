using System;
using Blizzard.T5.Fonts;
using Blizzard.T5.Services;
using UnityEngine;

public class AddFriendFrame : MonoBehaviour
{
	public AddFriendFrameBones m_Bones;

	public UberText m_HeaderText;

	public UberText m_InstructionText;

	public TextField m_InputTextField;

	public Font m_InputFont;

	public RecentOpponent m_RecentOpponent;

	public UberText m_LastPlayedText;

	private PegUIElement m_inputBlocker;

	private string m_inputText = string.Empty;

	private BnetPlayer m_player;

	private bool m_usePlayer;

	private string m_playerDisplayName;

	private Font m_localizedInputFont;

	private float m_initialLastPlayedTextPositionX;

	private IFontTable m_fontTable;

	public event Action Closed;

	private void Awake()
	{
		m_fontTable = ServiceManager.Get<IFontTable>();
		InitItems();
		Layout();
		InitInput();
		InitInputTextField();
		DialogManager.Get().OnDialogShown += OnDialogShown;
		DialogManager.Get().OnDialogHidden += OnDialogHidden;
		m_RecentOpponent.button.AddEventListener(UIEventType.RELEASE, OnRecentOpponentButtonReleased);
	}

	private void Start()
	{
		InitInputBlocker();
		m_InputTextField.SetInputFont(m_localizedInputFont);
		m_InputTextField.Activate();
		UpdateRecentOpponent();
		if (!DialogManager.Get().ShowingDialog())
		{
			m_InputTextField.Text = m_inputText;
			UpdateInstructions();
		}
	}

	private void OnDestroy()
	{
		DialogManager.Get().OnDialogShown -= OnDialogShown;
		DialogManager.Get().OnDialogHidden -= OnDialogHidden;
	}

	private void InitInput()
	{
		FontDefinition inputFontDef = m_fontTable.GetFontDef(m_InputFont);
		if (inputFontDef == null)
		{
			m_localizedInputFont = m_InputFont;
		}
		else
		{
			m_localizedInputFont = inputFontDef.m_Font;
		}
	}

	public void UpdateLayout()
	{
		Layout();
	}

	public void Close()
	{
		if (m_inputBlocker != null)
		{
			UnityEngine.Object.Destroy(m_inputBlocker.gameObject);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetPlayer(BnetPlayer player)
	{
		m_player = player;
		if (player == null)
		{
			m_usePlayer = false;
			m_playerDisplayName = null;
		}
		else
		{
			m_usePlayer = true;
			m_playerDisplayName = FriendUtils.GetUniqueName(m_player);
		}
		if (DialogManager.Get().ShowingDialog())
		{
			SaveAndHideText(m_playerDisplayName);
			return;
		}
		m_inputText = m_playerDisplayName;
		m_InputTextField.Text = m_inputText;
		UpdateInstructions();
	}

	public void UpdateRecentOpponent()
	{
		bool recentOpponentDisplayDisabled = (NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>())?.RecentFriendListDisplayEnabled ?? false;
		BnetPlayer recentOpponentPlayer = FriendMgr.Get().GetRecentOpponent();
		if (recentOpponentPlayer == null || recentOpponentDisplayDisabled)
		{
			m_RecentOpponent.button.gameObject.SetActive(value: false);
			return;
		}
		m_RecentOpponent.button.gameObject.SetActive(value: true);
		m_RecentOpponent.nameText.Text = FriendUtils.GetUniqueNameWithColor(recentOpponentPlayer);
		AdjustHeaderTextPositionBasedOnBattletagLength();
	}

	private void OnRecentOpponentButtonReleased(UIEvent e)
	{
		if (!string.IsNullOrEmpty(m_RecentOpponent.nameText.Text))
		{
			BnetPlayer opponent = FriendMgr.Get().GetRecentOpponent();
			SetPlayer(opponent);
		}
	}

	private void InitItems()
	{
		m_HeaderText.Text = GameStrings.Get("GLOBAL_ADDFRIEND_HEADER");
		m_InstructionText.Text = GameStrings.Get("GLOBAL_ADDFRIEND_INSTRUCTION");
		m_initialLastPlayedTextPositionX = m_LastPlayedText.transform.localPosition.x;
	}

	private void Layout()
	{
		base.transform.parent = BaseUI.Get().transform;
		base.transform.position = BaseUI.Get().GetAddFriendBone().position;
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		if ((UniversalInputManager.Get().UseWindowsTouch() && touchScreenService.IsTouchSupported()) || touchScreenService.IsVirtualKeyboardVisible())
		{
			Vector3 touchPos = new Vector3(base.transform.position.x, base.transform.position.y + 100f, base.transform.position.z);
			base.transform.position = touchPos;
		}
	}

	private void UpdateInstructions()
	{
		if (m_InstructionText != null)
		{
			m_InstructionText.gameObject.SetActive(string.IsNullOrEmpty(m_inputText) && string.IsNullOrEmpty(Input.compositionString));
		}
	}

	private void AdjustHeaderTextPositionBasedOnBattletagLength()
	{
		float x = m_RecentOpponent.nameText.GetBounds().size.x;
		float textSize = m_RecentOpponent.nameText.GetTextBounds().size.x;
		float lastPlayedTextOffsetX = x - textSize;
		if (base.transform.lossyScale.x != 0f)
		{
			lastPlayedTextOffsetX /= base.transform.lossyScale.x;
		}
		m_LastPlayedText.transform.localPosition = new Vector3(m_initialLastPlayedTextPositionX + lastPlayedTextOffsetX, m_LastPlayedText.transform.localPosition.y, m_LastPlayedText.transform.localPosition.z);
	}

	private void InitInputBlocker()
	{
		GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "AddFriendInputBlocker", this);
		inputBlockerObject.layer = 26;
		m_inputBlocker = inputBlockerObject.AddComponent<PegUIElement>();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, OnInputBlockerReleased);
	}

	private void OnInputBlockerReleased(UIEvent e)
	{
		OnClosed();
	}

	private void InitInputTextField()
	{
		m_InputTextField.Preprocess += OnInputPreprocess;
		m_InputTextField.Changed += OnInputChanged;
		m_InputTextField.Submitted += OnInputSubmitted;
		m_InputTextField.Canceled += OnInputCanceled;
		m_InstructionText.gameObject.SetActive(value: true);
	}

	private void OnInputPreprocess()
	{
		if (Input.imeIsSelected)
		{
			UpdateInstructions();
		}
	}

	private void OnInputChanged(string text)
	{
		m_inputText = text;
		UpdateInstructions();
		m_usePlayer = string.Compare(m_playerDisplayName, text.Trim(), ignoreCase: true) == 0;
	}

	private void OnInputSubmitted(string input)
	{
		string name = (m_usePlayer ? m_player.GetBattleTag().ToString() : input.Trim());
		if (!BnetFriendMgr.Get().SendInvite(name))
		{
			string message = GameStrings.Get("GLOBAL_ADDFRIEND_ERROR_MALFORMED");
			UIStatus.Get().AddError(message);
		}
		OnClosed();
	}

	private void OnInputCanceled()
	{
		OnClosed();
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed();
		}
	}

	private void SaveAndHideText(string text)
	{
		m_inputText = text;
		m_InputTextField.Text = string.Empty;
	}

	private void ShowSavedText()
	{
		m_InputTextField.Text = m_inputText;
		UpdateInstructions();
	}

	private void OnDialogShown()
	{
		SaveAndHideText(m_inputText);
	}

	private void OnDialogHidden()
	{
		ShowSavedText();
	}
}
