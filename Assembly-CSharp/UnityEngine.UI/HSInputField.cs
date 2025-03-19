using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI;

[AddComponentMenu("UI/HS Input Field", 31)]
public class HSInputField : Selectable, IUpdateSelectedHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, ISubmitHandler, ICanvasElement, ILayoutElement
{
	public enum ContentType
	{
		Standard,
		Autocorrected,
		IntegerNumber,
		DecimalNumber,
		Alphanumeric,
		Name,
		EmailAddress,
		Password,
		Pin,
		Custom
	}

	public enum InputType
	{
		Standard,
		AutoCorrect,
		Password
	}

	public enum CharacterValidation
	{
		None,
		Integer,
		Decimal,
		Alphanumeric,
		Name,
		EmailAddress
	}

	public enum LineType
	{
		SingleLine,
		MultiLineSubmit,
		MultiLineNewline
	}

	public delegate char OnValidateInput(string text, int charIndex, char addedChar);

	[Serializable]
	public class SubmitEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class OnChangeEvent : UnityEvent<string>
	{
	}

	protected enum EditState
	{
		Continue,
		Finish
	}

	protected TouchScreenKeyboard m_Keyboard;

	private static readonly char[] kSeparators = new char[6] { ' ', '.', ',', '\t', '\r', '\n' };

	[SerializeField]
	[FormerlySerializedAs("text")]
	protected Text m_TextComponent;

	[SerializeField]
	protected Graphic m_Placeholder;

	[SerializeField]
	private ContentType m_ContentType;

	[FormerlySerializedAs("inputType")]
	[SerializeField]
	private InputType m_InputType;

	[FormerlySerializedAs("asteriskChar")]
	[SerializeField]
	private char m_AsteriskChar = '*';

	[FormerlySerializedAs("keyboardType")]
	[SerializeField]
	private TouchScreenKeyboardType m_KeyboardType;

	[SerializeField]
	private LineType m_LineType;

	[SerializeField]
	[FormerlySerializedAs("hideMobileInput")]
	private bool m_HideMobileInput;

	[FormerlySerializedAs("validation")]
	[SerializeField]
	private CharacterValidation m_CharacterValidation;

	[FormerlySerializedAs("characterLimit")]
	[SerializeField]
	private int m_CharacterLimit;

	[FormerlySerializedAs("onSubmit")]
	[SerializeField]
	[FormerlySerializedAs("m_EndEdit")]
	[FormerlySerializedAs("m_OnSubmit")]
	private SubmitEvent m_OnEndEdit = new SubmitEvent();

	[FormerlySerializedAs("m_OnValueChange")]
	[FormerlySerializedAs("onValueChange")]
	[SerializeField]
	private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

	[FormerlySerializedAs("onValidateInput")]
	[SerializeField]
	private OnValidateInput m_OnValidateInput;

	[SerializeField]
	[FormerlySerializedAs("selectionColor")]
	private Color m_CaretColor = new Color(10f / 51f, 10f / 51f, 10f / 51f, 1f);

	[SerializeField]
	private bool m_CustomCaretColor;

	[SerializeField]
	private Color m_SelectionColor = new Color(56f / 85f, 0.80784315f, 1f, 64f / 85f);

	[SerializeField]
	[FormerlySerializedAs("mValue")]
	protected string m_Text = string.Empty;

	[Range(0f, 4f)]
	[SerializeField]
	private float m_CaretBlinkRate = 0.85f;

	[SerializeField]
	[Range(1f, 5f)]
	private int m_CaretWidth = 1;

	[SerializeField]
	private bool m_ReadOnly;

	protected int m_CaretPosition;

	protected int m_CaretSelectPosition;

	private RectTransform caretRectTrans;

	protected UIVertex[] m_CursorVerts;

	private TextGenerator m_InputTextCache;

	private CanvasRenderer m_CachedInputRenderer;

	private bool m_PreventFontCallback;

	[NonSerialized]
	protected Mesh m_Mesh;

	private bool m_AllowInput;

	private bool m_ShouldActivateNextUpdate;

	private bool m_UpdateDrag;

	private bool m_DragPositionOutOfBounds;

	private const float kHScrollSpeed = 0.05f;

	private const float kVScrollSpeed = 0.1f;

	protected bool m_CaretVisible;

	private Coroutine m_BlinkCoroutine;

	private float m_BlinkStartTime;

	protected int m_DrawStart;

	protected int m_DrawEnd;

	private Coroutine m_DragCoroutine;

	private string m_OriginalText = "";

	private bool m_WasCanceled;

	private bool m_HasDoneFocusTransition;

	private WaitForSecondsRealtime m_WaitForSecondsRealtime;

	private bool m_TouchKeyboardAllowsInPlaceEditing;

	private const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";

	private Event m_ProcessingEvent = new Event();

	private const int k_MaxTextLength = 16382;

	private BaseInput input
	{
		get
		{
			if ((bool)EventSystem.current && (bool)EventSystem.current.currentInputModule)
			{
				return EventSystem.current.currentInputModule.input;
			}
			return null;
		}
	}

	private string compositionString
	{
		get
		{
			if (!(input != null))
			{
				return Input.compositionString;
			}
			return input.compositionString;
		}
	}

	protected Mesh mesh
	{
		get
		{
			if (m_Mesh == null)
			{
				m_Mesh = new Mesh();
			}
			return m_Mesh;
		}
	}

	protected TextGenerator cachedInputTextGenerator
	{
		get
		{
			if (m_InputTextCache == null)
			{
				m_InputTextCache = new TextGenerator();
			}
			return m_InputTextCache;
		}
	}

	public bool shouldHideMobileInput
	{
		get
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android || platform == RuntimePlatform.tvOS)
			{
				return m_HideMobileInput;
			}
			return true;
		}
		set
		{
			SetPropertyUtil.SetStruct(ref m_HideMobileInput, value);
		}
	}

	private bool shouldActivateOnSelect => Application.platform != RuntimePlatform.tvOS;

	public string text
	{
		get
		{
			return m_Text;
		}
		set
		{
			SetText(value);
		}
	}

	public bool isFocused => m_AllowInput;

	public float caretBlinkRate
	{
		get
		{
			return m_CaretBlinkRate;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_CaretBlinkRate, value) && m_AllowInput)
			{
				SetCaretActive();
			}
		}
	}

	public int caretWidth
	{
		get
		{
			return m_CaretWidth;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_CaretWidth, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public Text textComponent
	{
		get
		{
			return m_TextComponent;
		}
		set
		{
			if (m_TextComponent != null)
			{
				m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
				m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
				m_TextComponent.UnregisterDirtyMaterialCallback(UpdateCaretMaterial);
			}
			if (SetPropertyUtil.SetClass(ref m_TextComponent, value))
			{
				EnforceTextHOverflow();
				if (m_TextComponent != null)
				{
					m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
					m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
					m_TextComponent.RegisterDirtyMaterialCallback(UpdateCaretMaterial);
				}
			}
		}
	}

	public Graphic placeholder
	{
		get
		{
			return m_Placeholder;
		}
		set
		{
			SetPropertyUtil.SetClass(ref m_Placeholder, value);
		}
	}

	public Color caretColor
	{
		get
		{
			if (!customCaretColor)
			{
				return textComponent.color;
			}
			return m_CaretColor;
		}
		set
		{
			if (SetPropertyUtil.SetColor(ref m_CaretColor, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public bool customCaretColor
	{
		get
		{
			return m_CustomCaretColor;
		}
		set
		{
			if (m_CustomCaretColor != value)
			{
				m_CustomCaretColor = value;
				MarkGeometryAsDirty();
			}
		}
	}

	public Color selectionColor
	{
		get
		{
			return m_SelectionColor;
		}
		set
		{
			if (SetPropertyUtil.SetColor(ref m_SelectionColor, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public SubmitEvent onEndEdit
	{
		get
		{
			return m_OnEndEdit;
		}
		set
		{
			SetPropertyUtil.SetClass(ref m_OnEndEdit, value);
		}
	}

	[Obsolete("onValueChange has been renamed to onValueChanged")]
	public OnChangeEvent onValueChange
	{
		get
		{
			return onValueChanged;
		}
		set
		{
			onValueChanged = value;
		}
	}

	public OnChangeEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			SetPropertyUtil.SetClass(ref m_OnValueChanged, value);
		}
	}

	public OnValidateInput onValidateInput
	{
		get
		{
			return m_OnValidateInput;
		}
		set
		{
			SetPropertyUtil.SetClass(ref m_OnValidateInput, value);
		}
	}

	public int characterLimit
	{
		get
		{
			return m_CharacterLimit;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_CharacterLimit, Math.Max(0, value)))
			{
				UpdateLabel();
				if (m_Keyboard != null)
				{
					m_Keyboard.characterLimit = value;
				}
			}
		}
	}

	public ContentType contentType
	{
		get
		{
			return m_ContentType;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_ContentType, value))
			{
				EnforceContentType();
			}
		}
	}

	public LineType lineType
	{
		get
		{
			return m_LineType;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_LineType, value))
			{
				SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected);
				EnforceTextHOverflow();
			}
		}
	}

	public InputType inputType
	{
		get
		{
			return m_InputType;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_InputType, value))
			{
				SetToCustom();
			}
		}
	}

	public TouchScreenKeyboard touchScreenKeyboard => m_Keyboard;

	public TouchScreenKeyboardType keyboardType
	{
		get
		{
			return m_KeyboardType;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_KeyboardType, value))
			{
				SetToCustom();
			}
		}
	}

	public CharacterValidation characterValidation
	{
		get
		{
			return m_CharacterValidation;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_CharacterValidation, value))
			{
				SetToCustom();
			}
		}
	}

	public bool readOnly
	{
		get
		{
			return m_ReadOnly;
		}
		set
		{
			m_ReadOnly = value;
		}
	}

	public bool multiLine
	{
		get
		{
			if (m_LineType != LineType.MultiLineNewline)
			{
				return lineType == LineType.MultiLineSubmit;
			}
			return true;
		}
	}

	public char asteriskChar
	{
		get
		{
			return m_AsteriskChar;
		}
		set
		{
			if (SetPropertyUtil.SetStruct(ref m_AsteriskChar, value))
			{
				UpdateLabel();
			}
		}
	}

	public bool wasCanceled => m_WasCanceled;

	protected int caretPositionInternal
	{
		get
		{
			return m_CaretPosition + compositionString.Length;
		}
		set
		{
			m_CaretPosition = value;
			ClampPos(ref m_CaretPosition);
		}
	}

	protected int caretSelectPositionInternal
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			m_CaretSelectPosition = value;
			ClampPos(ref m_CaretSelectPosition);
		}
	}

	private bool hasSelection => caretPositionInternal != caretSelectPositionInternal;

	public int caretPosition
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			selectionAnchorPosition = value;
			selectionFocusPosition = value;
		}
	}

	public int selectionAnchorPosition
	{
		get
		{
			return m_CaretPosition + compositionString.Length;
		}
		set
		{
			if (compositionString.Length == 0)
			{
				m_CaretPosition = value;
				ClampPos(ref m_CaretPosition);
			}
		}
	}

	public int selectionFocusPosition
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			if (compositionString.Length == 0)
			{
				m_CaretSelectPosition = value;
				ClampPos(ref m_CaretSelectPosition);
			}
		}
	}

	private static string clipboard
	{
		get
		{
			return GUIUtility.systemCopyBuffer;
		}
		set
		{
			GUIUtility.systemCopyBuffer = value;
		}
	}

	public virtual float minWidth => 0f;

	public virtual float preferredWidth
	{
		get
		{
			if (textComponent == null)
			{
				return 0f;
			}
			TextGenerationSettings settings = textComponent.GetGenerationSettings(Vector2.zero);
			return textComponent.cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / textComponent.pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => 0f;

	public virtual float preferredHeight
	{
		get
		{
			if (textComponent == null)
			{
				return 0f;
			}
			TextGenerationSettings settings = textComponent.GetGenerationSettings(new Vector2(textComponent.rectTransform.rect.size.x, 0f));
			return textComponent.cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / textComponent.pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => 1;

	protected HSInputField()
	{
		EnforceTextHOverflow();
	}

	public void SetTextWithoutNotify(string input)
	{
		SetText(input, sendCallback: false);
	}

	private void SetText(string value, bool sendCallback = true)
	{
		if (text == value)
		{
			return;
		}
		if (value == null)
		{
			value = "";
		}
		value = value.Replace("\0", string.Empty);
		if (m_LineType == LineType.SingleLine)
		{
			value = value.Replace("\n", "").Replace("\t", "");
		}
		if (onValidateInput != null || characterValidation != 0)
		{
			m_Text = "";
			OnValidateInput validatorMethod = onValidateInput ?? new OnValidateInput(Validate);
			m_CaretPosition = (m_CaretSelectPosition = value.Length);
			int charactersToCheck = ((characterLimit > 0) ? Math.Min(characterLimit, value.Length) : value.Length);
			for (int i = 0; i < charactersToCheck; i++)
			{
				char c = validatorMethod(m_Text, m_Text.Length, value[i]);
				if (c != 0)
				{
					m_Text += c;
				}
			}
		}
		else
		{
			m_Text = ((characterLimit > 0 && value.Length > characterLimit) ? value.Substring(0, characterLimit) : value);
		}
		if (m_Keyboard != null)
		{
			m_Keyboard.text = m_Text;
		}
		if (m_CaretPosition > m_Text.Length)
		{
			m_CaretPosition = (m_CaretSelectPosition = m_Text.Length);
		}
		else if (m_CaretSelectPosition > m_Text.Length)
		{
			m_CaretSelectPosition = m_Text.Length;
		}
		if (sendCallback)
		{
			SendOnValueChanged();
		}
		UpdateLabel();
	}

	protected void ClampPos(ref int pos)
	{
		if (pos < 0)
		{
			pos = 0;
		}
		else if (pos > text.Length)
		{
			pos = text.Length;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_Text == null)
		{
			m_Text = string.Empty;
		}
		m_DrawStart = 0;
		m_DrawEnd = m_Text.Length;
		if (m_CachedInputRenderer != null)
		{
			m_CachedInputRenderer.SetMaterial(m_TextComponent.GetModifiedMaterial(Graphic.defaultGraphicMaterial), Texture2D.whiteTexture);
		}
		if (m_TextComponent != null)
		{
			m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
			m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
			m_TextComponent.RegisterDirtyMaterialCallback(UpdateCaretMaterial);
			UpdateLabel();
		}
	}

	protected override void OnDisable()
	{
		m_BlinkCoroutine = null;
		DeactivateInputField();
		if (m_TextComponent != null)
		{
			m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
			m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
			m_TextComponent.UnregisterDirtyMaterialCallback(UpdateCaretMaterial);
		}
		CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
		if (m_CachedInputRenderer != null)
		{
			m_CachedInputRenderer.Clear();
		}
		if (m_Mesh != null)
		{
			Object.DestroyImmediate(m_Mesh);
		}
		m_Mesh = null;
		base.OnDisable();
	}

	private IEnumerator CaretBlink()
	{
		m_CaretVisible = true;
		yield return null;
		while (isFocused && m_CaretBlinkRate > 0f)
		{
			float blinkPeriod = 1f / m_CaretBlinkRate;
			bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2f;
			if (m_CaretVisible != blinkState)
			{
				m_CaretVisible = blinkState;
				if (!hasSelection)
				{
					MarkGeometryAsDirty();
				}
			}
			yield return null;
		}
		m_BlinkCoroutine = null;
	}

	private void SetCaretVisible()
	{
		if (m_AllowInput)
		{
			m_CaretVisible = true;
			m_BlinkStartTime = Time.unscaledTime;
			SetCaretActive();
		}
	}

	private void SetCaretActive()
	{
		if (!m_AllowInput)
		{
			return;
		}
		if (m_CaretBlinkRate > 0f)
		{
			if (m_BlinkCoroutine == null)
			{
				m_BlinkCoroutine = StartCoroutine(CaretBlink());
			}
		}
		else
		{
			m_CaretVisible = true;
		}
	}

	private void UpdateCaretMaterial()
	{
		if (m_TextComponent != null && m_CachedInputRenderer != null)
		{
			m_CachedInputRenderer.SetMaterial(m_TextComponent.GetModifiedMaterial(Graphic.defaultGraphicMaterial), Texture2D.whiteTexture);
		}
	}

	protected void OnFocus()
	{
		SelectAll();
	}

	protected void SelectAll()
	{
		caretPositionInternal = text.Length;
		caretSelectPositionInternal = 0;
	}

	public void MoveTextEnd(bool shift)
	{
		int position = text.Length;
		if (shift)
		{
			caretSelectPositionInternal = position;
		}
		else
		{
			caretPositionInternal = position;
			caretSelectPositionInternal = caretPositionInternal;
		}
		UpdateLabel();
	}

	public void MoveTextStart(bool shift)
	{
		int position = 0;
		if (shift)
		{
			caretSelectPositionInternal = position;
		}
		else
		{
			caretPositionInternal = position;
			caretSelectPositionInternal = caretPositionInternal;
		}
		UpdateLabel();
	}

	private bool InPlaceEditing()
	{
		if (TouchScreenKeyboard.isSupported)
		{
			return m_TouchKeyboardAllowsInPlaceEditing;
		}
		return true;
	}

	private void UpdateCaretFromKeyboard()
	{
		RangeInt selectionRange = m_Keyboard.selection;
		int selectionStart = selectionRange.start;
		int selectionEnd = selectionRange.end;
		bool caretChanged = false;
		if (caretPositionInternal != selectionStart)
		{
			caretChanged = true;
			caretPositionInternal = selectionStart;
		}
		if (caretSelectPositionInternal != selectionEnd)
		{
			caretSelectPositionInternal = selectionEnd;
			caretChanged = true;
		}
		if (caretChanged)
		{
			m_BlinkStartTime = Time.unscaledTime;
			UpdateLabel();
		}
	}

	protected virtual void LateUpdate()
	{
		if (m_ShouldActivateNextUpdate)
		{
			if (!isFocused)
			{
				ActivateInputFieldInternal();
				m_ShouldActivateNextUpdate = false;
				return;
			}
			m_ShouldActivateNextUpdate = false;
		}
		AssignPositioningIfNeeded();
		if (!isFocused || InPlaceEditing())
		{
			return;
		}
		if (m_Keyboard == null || m_Keyboard.status != 0)
		{
			if (m_Keyboard != null)
			{
				if (!m_ReadOnly)
				{
					text = m_Keyboard.text;
				}
				if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled)
				{
					m_WasCanceled = true;
				}
			}
			OnDeselect(null);
			return;
		}
		string val = m_Keyboard.text;
		if (m_Text != val)
		{
			if (m_ReadOnly)
			{
				m_Keyboard.text = m_Text;
			}
			else
			{
				m_Text = "";
				for (int i = 0; i < val.Length; i++)
				{
					char c = val[i];
					if (c == '\r' || c == '\u0003')
					{
						c = '\n';
					}
					if (onValidateInput != null)
					{
						c = onValidateInput(m_Text, m_Text.Length, c);
					}
					else if (characterValidation != 0)
					{
						c = Validate(m_Text, m_Text.Length, c);
					}
					if (lineType == LineType.MultiLineSubmit && c == '\n')
					{
						m_Keyboard.text = m_Text;
						OnDeselect(null);
						return;
					}
					if (c != 0)
					{
						m_Text += c;
					}
				}
				if (characterLimit > 0 && m_Text.Length > characterLimit)
				{
					m_Text = m_Text.Substring(0, characterLimit);
				}
				if (m_Keyboard.canGetSelection)
				{
					UpdateCaretFromKeyboard();
				}
				else
				{
					int num = (caretSelectPositionInternal = m_Text.Length);
					caretPositionInternal = num;
				}
				if (m_Text != val)
				{
					m_Keyboard.text = m_Text;
				}
				SendOnValueChangedAndUpdateLabel();
			}
		}
		else if (m_HideMobileInput && m_Keyboard.canSetSelection)
		{
			m_Keyboard.selection = new RangeInt(caretPositionInternal, caretSelectPositionInternal - caretPositionInternal);
		}
		else if (m_Keyboard.canGetSelection && !m_HideMobileInput)
		{
			UpdateCaretFromKeyboard();
		}
		if (m_Keyboard.status != 0)
		{
			if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled)
			{
				m_WasCanceled = true;
			}
			OnDeselect(null);
		}
	}

	[Obsolete("This function is no longer used. Please use RectTransformUtility.ScreenPointToLocalPointInRectangle() instead.")]
	public Vector2 ScreenToLocal(Vector2 screen)
	{
		Canvas theCanvas = m_TextComponent.canvas;
		if (theCanvas == null)
		{
			return screen;
		}
		Vector3 pos = Vector3.zero;
		if (theCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			pos = m_TextComponent.transform.InverseTransformPoint(screen);
		}
		else if (theCanvas.worldCamera != null)
		{
			Ray mouseRay = theCanvas.worldCamera.ScreenPointToRay(screen);
			new Plane(m_TextComponent.transform.forward, m_TextComponent.transform.position).Raycast(mouseRay, out var dist);
			pos = m_TextComponent.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
		}
		return new Vector2(pos.x, pos.y);
	}

	private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator)
	{
		if (!multiLine)
		{
			return 0;
		}
		float y = pos.y * m_TextComponent.pixelsPerUnit;
		float lastBottomY = 0f;
		for (int i = 0; i < generator.lineCount; i++)
		{
			float topY = generator.lines[i].topY;
			float bottomY = topY - (float)generator.lines[i].height;
			if (y > topY)
			{
				float leading = topY - lastBottomY;
				if (y > topY - 0.5f * leading)
				{
					return i - 1;
				}
				return i;
			}
			if (y > bottomY)
			{
				return i;
			}
			lastBottomY = bottomY;
		}
		return generator.lineCount;
	}

	protected int GetCharacterIndexFromPosition(Vector2 pos)
	{
		TextGenerator gen = m_TextComponent.cachedTextGenerator;
		if (gen.lineCount == 0)
		{
			return 0;
		}
		int line = GetUnclampedCharacterLineFromPosition(pos, gen);
		if (line < 0)
		{
			return 0;
		}
		if (line >= gen.lineCount)
		{
			return gen.characterCountVisible;
		}
		int startCharIdx = gen.lines[line].startCharIdx;
		int endCharIndex = GetLineEndPosition(gen, line);
		for (int i = startCharIdx; i < endCharIndex && i < gen.characterCountVisible; i++)
		{
			UICharInfo charInfo = gen.characters[i];
			Vector2 charPos = charInfo.cursorPos / m_TextComponent.pixelsPerUnit;
			float num = pos.x - charPos.x;
			float distToCharEnd = charPos.x + charInfo.charWidth / m_TextComponent.pixelsPerUnit - pos.x;
			if (num < distToCharEnd)
			{
				return i;
			}
		}
		return endCharIndex;
	}

	private bool MayDrag(PointerEventData eventData)
	{
		if (IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left && m_TextComponent != null)
		{
			if (!InPlaceEditing())
			{
				return m_HideMobileInput;
			}
			return true;
		}
		return false;
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			m_UpdateDrag = true;
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePos);
			caretSelectPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;
			MarkGeometryAsDirty();
			m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textComponent.rectTransform, eventData.position, eventData.pressEventCamera);
			if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
			{
				m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));
			}
			eventData.Use();
		}
	}

	private IEnumerator MouseDragOutsideRect(PointerEventData eventData)
	{
		while (m_UpdateDrag && m_DragPositionOutOfBounds)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePos);
			Rect rect = textComponent.rectTransform.rect;
			if (multiLine)
			{
				if (localMousePos.y > rect.yMax)
				{
					MoveUp(shift: true, goToFirstChar: true);
				}
				else if (localMousePos.y < rect.yMin)
				{
					MoveDown(shift: true, goToLastChar: true);
				}
			}
			else if (localMousePos.x < rect.xMin)
			{
				MoveLeft(shift: true, ctrl: false);
			}
			else if (localMousePos.x > rect.xMax)
			{
				MoveRight(shift: true, ctrl: false);
			}
			UpdateLabel();
			float delay = (multiLine ? 0.1f : 0.05f);
			if (m_WaitForSecondsRealtime == null)
			{
				m_WaitForSecondsRealtime = new WaitForSecondsRealtime(delay);
			}
			else
			{
				m_WaitForSecondsRealtime.waitTime = delay;
			}
			yield return m_WaitForSecondsRealtime;
		}
		m_DragCoroutine = null;
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			m_UpdateDrag = false;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!MayDrag(eventData))
		{
			return;
		}
		EventSystem.current.SetSelectedGameObject(base.gameObject, eventData);
		bool hadFocusBefore = m_AllowInput;
		base.OnPointerDown(eventData);
		if (!InPlaceEditing() && (m_Keyboard == null || !m_Keyboard.active))
		{
			OnSelect(eventData);
			return;
		}
		if (hadFocusBefore)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePos);
			int num2 = (caretPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart);
			caretSelectPositionInternal = num2;
		}
		UpdateLabel();
		eventData.Use();
	}

	protected EditState KeyPressed(Event evt)
	{
		EventModifiers currentEventModifiers = evt.modifiers;
		bool ctrl = ((SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX) ? ((currentEventModifiers & EventModifiers.Command) != 0) : ((currentEventModifiers & EventModifiers.Control) != 0));
		bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
		bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
		bool ctrlOnly = ctrl && !alt && !shift;
		switch (evt.keyCode)
		{
		case KeyCode.Backspace:
			Backspace();
			return EditState.Continue;
		case KeyCode.Delete:
			ForwardSpace();
			return EditState.Continue;
		case KeyCode.Home:
			MoveTextStart(shift);
			return EditState.Continue;
		case KeyCode.End:
			MoveTextEnd(shift);
			return EditState.Continue;
		case KeyCode.A:
			if (ctrlOnly)
			{
				SelectAll();
				return EditState.Continue;
			}
			break;
		case KeyCode.C:
			if (ctrlOnly)
			{
				if (inputType != InputType.Password)
				{
					clipboard = GetSelectedString();
				}
				else
				{
					clipboard = "";
				}
				return EditState.Continue;
			}
			break;
		case KeyCode.V:
			if (ctrlOnly)
			{
				Append(clipboard);
				UpdateLabel();
				return EditState.Continue;
			}
			break;
		case KeyCode.X:
			if (ctrlOnly)
			{
				if (inputType != InputType.Password)
				{
					clipboard = GetSelectedString();
				}
				else
				{
					clipboard = "";
				}
				Delete();
				UpdateTouchKeyboardFromEditChanges();
				SendOnValueChangedAndUpdateLabel();
				return EditState.Continue;
			}
			break;
		case KeyCode.LeftArrow:
			MoveLeft(shift, ctrl);
			return EditState.Continue;
		case KeyCode.RightArrow:
			MoveRight(shift, ctrl);
			return EditState.Continue;
		case KeyCode.UpArrow:
			MoveUp(shift);
			return EditState.Continue;
		case KeyCode.DownArrow:
			MoveDown(shift);
			return EditState.Continue;
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			if (lineType != LineType.MultiLineNewline)
			{
				return EditState.Finish;
			}
			break;
		case KeyCode.Escape:
			m_WasCanceled = true;
			return EditState.Finish;
		}
		char c = evt.character;
		if (!multiLine && (c == '\t' || c == '\r' || c == '\n'))
		{
			return EditState.Continue;
		}
		if (c == '\r' || c == '\u0003')
		{
			c = '\n';
		}
		if (compositionString.Length > 0 && hasSelection)
		{
			Delete();
		}
		if (IsValidChar(c))
		{
			Append(c);
		}
		if (c == '\0' && compositionString.Length > 0)
		{
			UpdateLabel();
		}
		return EditState.Continue;
	}

	private bool IsValidChar(char c)
	{
		switch (c)
		{
		case '\u007f':
			return false;
		case '\t':
		case '\n':
			return true;
		default:
			return m_TextComponent.font.HasCharacter(c);
		}
	}

	public void ProcessEvent(Event e)
	{
		KeyPressed(e);
	}

	public virtual void OnUpdateSelected(BaseEventData eventData)
	{
		if (!isFocused)
		{
			return;
		}
		bool consumedEvent = false;
		while (Event.PopEvent(m_ProcessingEvent))
		{
			if (m_ProcessingEvent.rawType == EventType.KeyDown)
			{
				consumedEvent = true;
				if (KeyPressed(m_ProcessingEvent) == EditState.Finish)
				{
					DeactivateInputField();
					break;
				}
			}
			else if (m_ProcessingEvent.rawType == EventType.KeyUp)
			{
				consumedEvent = true;
			}
			EventType type = m_ProcessingEvent.type;
			if ((uint)(type - 13) <= 1u && m_ProcessingEvent.commandName == "SelectAll")
			{
				SelectAll();
				consumedEvent = true;
			}
		}
		if (consumedEvent)
		{
			UpdateLabel();
		}
		eventData.Use();
	}

	private string GetSelectedString()
	{
		if (!hasSelection)
		{
			return "";
		}
		int startPos = caretPositionInternal;
		int endPos = caretSelectPositionInternal;
		if (startPos > endPos)
		{
			int num = startPos;
			startPos = endPos;
			endPos = num;
		}
		return text.Substring(startPos, endPos - startPos);
	}

	private int FindtNextWordBegin()
	{
		if (caretSelectPositionInternal + 1 >= text.Length)
		{
			return text.Length;
		}
		int spaceLoc = text.IndexOfAny(kSeparators, caretSelectPositionInternal + 1);
		if (spaceLoc == -1)
		{
			return text.Length;
		}
		return spaceLoc + 1;
	}

	private void MoveRight(bool shift, bool ctrl)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
			return;
		}
		int position = ((!ctrl) ? (caretSelectPositionInternal + 1) : FindtNextWordBegin());
		if (shift)
		{
			caretSelectPositionInternal = position;
			return;
		}
		num2 = (caretPositionInternal = position);
		caretSelectPositionInternal = num2;
	}

	private int FindtPrevWordBegin()
	{
		if (caretSelectPositionInternal - 2 < 0)
		{
			return 0;
		}
		int spaceLoc = text.LastIndexOfAny(kSeparators, caretSelectPositionInternal - 2);
		if (spaceLoc == -1)
		{
			return 0;
		}
		return spaceLoc + 1;
	}

	private void MoveLeft(bool shift, bool ctrl)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
			return;
		}
		int position = ((!ctrl) ? (caretSelectPositionInternal - 1) : FindtPrevWordBegin());
		if (shift)
		{
			caretSelectPositionInternal = position;
			return;
		}
		num2 = (caretPositionInternal = position);
		caretSelectPositionInternal = num2;
	}

	private int DetermineCharacterLine(int charPos, TextGenerator generator)
	{
		for (int i = 0; i < generator.lineCount - 1; i++)
		{
			if (generator.lines[i + 1].startCharIdx > charPos)
			{
				return i;
			}
		}
		return generator.lineCount - 1;
	}

	private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
	{
		if (originalPos >= cachedInputTextGenerator.characters.Count)
		{
			return 0;
		}
		UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
		int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);
		if (originLine <= 0)
		{
			if (!goToFirstChar)
			{
				return originalPos;
			}
			return 0;
		}
		int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;
		for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; i++)
		{
			if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
			{
				return i;
			}
		}
		return endCharIdx;
	}

	private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
	{
		if (originalPos >= cachedInputTextGenerator.characterCountVisible)
		{
			return text.Length;
		}
		UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
		int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);
		if (originLine + 1 >= cachedInputTextGenerator.lineCount)
		{
			if (!goToLastChar)
			{
				return originalPos;
			}
			return text.Length;
		}
		int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);
		for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; i++)
		{
			if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
			{
				return i;
			}
		}
		return endCharIdx;
	}

	private void MoveDown(bool shift)
	{
		MoveDown(shift, goToLastChar: true);
	}

	private void MoveDown(bool shift, bool goToLastChar)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
		}
		int position = (multiLine ? LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : text.Length);
		if (shift)
		{
			caretSelectPositionInternal = position;
			return;
		}
		num2 = (caretSelectPositionInternal = position);
		caretPositionInternal = num2;
	}

	private void MoveUp(bool shift)
	{
		MoveUp(shift, goToFirstChar: true);
	}

	private void MoveUp(bool shift, bool goToFirstChar)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
		}
		int position = (multiLine ? LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0);
		if (shift)
		{
			caretSelectPositionInternal = position;
			return;
		}
		num2 = (caretPositionInternal = position);
		caretSelectPositionInternal = num2;
	}

	private void Delete()
	{
		if (m_ReadOnly || caretPositionInternal == caretSelectPositionInternal)
		{
			return;
		}
		if (caretPositionInternal < caretSelectPositionInternal)
		{
			if (compositionString.Length > 0)
			{
				m_Text = text.Substring(0, m_CaretPosition) + text.Substring(m_CaretSelectPosition, text.Length - m_CaretSelectPosition);
				caretPositionInternal = m_CaretPosition;
				caretSelectPositionInternal = m_CaretPosition;
			}
			else
			{
				m_Text = text.Substring(0, caretPositionInternal) + text.Substring(caretSelectPositionInternal, text.Length - caretSelectPositionInternal);
				caretSelectPositionInternal = caretPositionInternal;
			}
		}
		else if (compositionString.Length > 0)
		{
			m_Text = text.Substring(0, m_CaretSelectPosition) + text.Substring(m_CaretPosition, text.Length - m_CaretPosition);
			caretSelectPositionInternal = m_CaretSelectPosition;
			caretPositionInternal = m_CaretSelectPosition;
		}
		else
		{
			m_Text = text.Substring(0, caretSelectPositionInternal) + text.Substring(caretPositionInternal, text.Length - caretPositionInternal);
			caretPositionInternal = caretSelectPositionInternal;
		}
	}

	private void ForwardSpace()
	{
		if (!m_ReadOnly)
		{
			if (hasSelection)
			{
				Delete();
				UpdateTouchKeyboardFromEditChanges();
				SendOnValueChangedAndUpdateLabel();
			}
			else if (caretPositionInternal < text.Length)
			{
				m_Text = text.Remove(caretPositionInternal, 1);
				UpdateTouchKeyboardFromEditChanges();
				SendOnValueChangedAndUpdateLabel();
			}
		}
	}

	private void Backspace()
	{
		if (!m_ReadOnly)
		{
			if (hasSelection)
			{
				Delete();
				UpdateTouchKeyboardFromEditChanges();
				SendOnValueChangedAndUpdateLabel();
			}
			else if (caretPositionInternal > 0)
			{
				m_Text = text.Remove(caretPositionInternal - 1, 1);
				caretSelectPositionInternal = --caretPositionInternal;
				UpdateTouchKeyboardFromEditChanges();
				SendOnValueChangedAndUpdateLabel();
			}
		}
	}

	private void Insert(char c)
	{
		if (m_ReadOnly)
		{
			return;
		}
		string replaceString = c.ToString();
		Delete();
		if (characterLimit <= 0 || text.Length < characterLimit)
		{
			m_Text = text.Insert(m_CaretPosition, replaceString);
			if (compositionString.Length > 0)
			{
				int num2 = (caretPositionInternal = caretPositionInternal);
				caretSelectPositionInternal = num2;
			}
			else
			{
				caretSelectPositionInternal = (caretPositionInternal += replaceString.Length);
			}
			UpdateTouchKeyboardFromEditChanges();
			SendOnValueChanged();
		}
	}

	private void UpdateTouchKeyboardFromEditChanges()
	{
		if (m_Keyboard != null && InPlaceEditing())
		{
			m_Keyboard.text = m_Text;
		}
	}

	private void SendOnValueChangedAndUpdateLabel()
	{
		SendOnValueChanged();
		UpdateLabel();
	}

	private void SendOnValueChanged()
	{
		UISystemProfilerApi.AddMarker("InputField.value", this);
		if (onValueChanged != null)
		{
			onValueChanged.Invoke(text);
		}
	}

	protected void SendOnSubmit()
	{
		UISystemProfilerApi.AddMarker("InputField.onSubmit", this);
		if (onEndEdit != null)
		{
			onEndEdit.Invoke(m_Text);
		}
	}

	protected virtual void Append(string input)
	{
		if (m_ReadOnly || !InPlaceEditing())
		{
			return;
		}
		int i = 0;
		for (int imax = input.Length; i < imax; i++)
		{
			char c = input[i];
			if (c >= ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\n')
			{
				Append(c);
			}
		}
	}

	protected virtual void Append(char input)
	{
		if (!char.IsSurrogate(input) && !m_ReadOnly && text.Length < 16382 && InPlaceEditing())
		{
			int insertionPoint = Math.Min(selectionFocusPosition, selectionAnchorPosition);
			if (onValidateInput != null)
			{
				input = onValidateInput(text, insertionPoint, input);
			}
			else if (characterValidation != 0)
			{
				input = Validate(text, insertionPoint, input);
			}
			if (input != 0)
			{
				Insert(input);
			}
		}
	}

	protected void UpdateLabel()
	{
		if (m_TextComponent != null && m_TextComponent.font != null && !m_PreventFontCallback)
		{
			m_PreventFontCallback = true;
			string fullText = ((compositionString.Length <= 0) ? text : (text.Substring(0, m_CaretPosition) + compositionString + text.Substring(m_CaretPosition)));
			string processed = ((inputType != InputType.Password) ? fullText : new string(asteriskChar, fullText.Length));
			bool isEmpty = string.IsNullOrEmpty(fullText);
			if (m_Placeholder != null)
			{
				m_Placeholder.enabled = isEmpty;
			}
			if (!m_AllowInput)
			{
				m_DrawStart = 0;
				m_DrawEnd = m_Text.Length;
			}
			if (!isEmpty)
			{
				Vector2 extents = m_TextComponent.rectTransform.rect.size;
				TextGenerationSettings settings = m_TextComponent.GetGenerationSettings(extents);
				settings.generateOutOfBounds = true;
				cachedInputTextGenerator.PopulateWithErrors(processed, settings, base.gameObject);
				SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);
				processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);
				SetCaretVisible();
			}
			m_TextComponent.text = processed;
			MarkGeometryAsDirty();
			m_PreventFontCallback = false;
		}
	}

	private bool IsSelectionVisible()
	{
		if (m_DrawStart > caretPositionInternal || m_DrawStart > caretSelectPositionInternal)
		{
			return false;
		}
		if (m_DrawEnd < caretPositionInternal || m_DrawEnd < caretSelectPositionInternal)
		{
			return false;
		}
		return true;
	}

	private static int GetLineStartPosition(TextGenerator gen, int line)
	{
		line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
		return gen.lines[line].startCharIdx;
	}

	private static int GetLineEndPosition(TextGenerator gen, int line)
	{
		line = Mathf.Max(line, 0);
		if (line + 1 < gen.lines.Count)
		{
			return gen.lines[line + 1].startCharIdx - 1;
		}
		return gen.characterCountVisible;
	}

	private void SetDrawRangeToContainCaretPosition(int caretPos)
	{
		if (cachedInputTextGenerator.lineCount <= 0)
		{
			return;
		}
		Vector2 extents = cachedInputTextGenerator.rectExtents.size;
		if (multiLine)
		{
			IList<UILineInfo> lines = cachedInputTextGenerator.lines;
			int caretLine = DetermineCharacterLine(caretPos, cachedInputTextGenerator);
			if (caretPos > m_DrawEnd)
			{
				m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, caretLine);
				float bottomY = lines[caretLine].topY - (float)lines[caretLine].height;
				if (caretLine == lines.Count - 1)
				{
					bottomY += lines[caretLine].leading;
				}
				int startLine = caretLine;
				while (startLine > 0 && !(lines[startLine - 1].topY - bottomY > extents.y))
				{
					startLine--;
				}
				m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
				return;
			}
			if (caretPos < m_DrawStart)
			{
				m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, caretLine);
			}
			int startLine2 = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
			int endLine = startLine2;
			float topY = lines[startLine2].topY;
			float bottomY2 = lines[endLine].topY - (float)lines[endLine].height;
			if (endLine == lines.Count - 1)
			{
				bottomY2 += lines[endLine].leading;
			}
			for (; endLine < lines.Count - 1; endLine++)
			{
				bottomY2 = lines[endLine + 1].topY - (float)lines[endLine + 1].height;
				if (endLine + 1 == lines.Count - 1)
				{
					bottomY2 += lines[endLine + 1].leading;
				}
				if (topY - bottomY2 > extents.y)
				{
					break;
				}
			}
			m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);
			while (startLine2 > 0)
			{
				topY = lines[startLine2 - 1].topY;
				if (topY - bottomY2 > extents.y)
				{
					break;
				}
				startLine2--;
			}
			m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine2);
			return;
		}
		IList<UICharInfo> characters = cachedInputTextGenerator.characters;
		if (m_DrawEnd > cachedInputTextGenerator.characterCountVisible)
		{
			m_DrawEnd = cachedInputTextGenerator.characterCountVisible;
		}
		float width = 0f;
		if (caretPos > m_DrawEnd || (caretPos == m_DrawEnd && m_DrawStart > 0))
		{
			m_DrawEnd = caretPos;
			m_DrawStart = m_DrawEnd - 1;
			while (m_DrawStart >= 0 && !(width + characters[m_DrawStart].charWidth > extents.x))
			{
				width += characters[m_DrawStart].charWidth;
				m_DrawStart--;
			}
			m_DrawStart++;
		}
		else
		{
			if (caretPos < m_DrawStart)
			{
				m_DrawStart = caretPos;
			}
			m_DrawEnd = m_DrawStart;
		}
		while (m_DrawEnd < cachedInputTextGenerator.characterCountVisible)
		{
			width += characters[m_DrawEnd].charWidth;
			if (!(width > extents.x))
			{
				m_DrawEnd++;
				continue;
			}
			break;
		}
	}

	public void ForceLabelUpdate()
	{
		UpdateLabel();
	}

	private void MarkGeometryAsDirty()
	{
		CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
	}

	public virtual void Rebuild(CanvasUpdate update)
	{
		if (update == CanvasUpdate.LatePreRender)
		{
			UpdateGeometry();
		}
	}

	public virtual void LayoutComplete()
	{
	}

	public virtual void GraphicUpdateComplete()
	{
	}

	private void UpdateGeometry()
	{
		if (shouldHideMobileInput)
		{
			if (m_CachedInputRenderer == null && m_TextComponent != null)
			{
				GameObject go = new GameObject(base.transform.name + " Input Caret", typeof(RectTransform), typeof(CanvasRenderer));
				go.hideFlags = HideFlags.DontSave;
				go.transform.SetParent(m_TextComponent.transform.parent);
				go.transform.SetAsFirstSibling();
				go.layer = base.gameObject.layer;
				caretRectTrans = go.GetComponent<RectTransform>();
				m_CachedInputRenderer = go.GetComponent<CanvasRenderer>();
				m_CachedInputRenderer.SetMaterial(m_TextComponent.GetModifiedMaterial(Graphic.defaultGraphicMaterial), Texture2D.whiteTexture);
				go.AddComponent<LayoutElement>().ignoreLayout = true;
				AssignPositioningIfNeeded();
			}
			if (!(m_CachedInputRenderer == null))
			{
				OnFillVBO(mesh);
				m_CachedInputRenderer.SetMesh(mesh);
			}
		}
	}

	private void AssignPositioningIfNeeded()
	{
		if (m_TextComponent != null && caretRectTrans != null && (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition || caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation || caretRectTrans.localScale != m_TextComponent.rectTransform.localScale || caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin || caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax || caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition || caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta || caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
		{
			caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
			caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
			caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
			caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
			caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
			caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
			caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
			caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
		}
	}

	private void OnFillVBO(Mesh vbo)
	{
		using VertexHelper helper = new VertexHelper();
		if (!isFocused)
		{
			helper.FillMesh(vbo);
			return;
		}
		Vector2 roundingOffset = m_TextComponent.PixelAdjustPoint(Vector2.zero);
		if (!hasSelection)
		{
			GenerateCaret(helper, roundingOffset);
		}
		else
		{
			GenerateHighlight(helper, roundingOffset);
		}
		helper.FillMesh(vbo);
	}

	private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
	{
		if (!m_CaretVisible)
		{
			return;
		}
		if (m_CursorVerts == null)
		{
			CreateCursorVerts();
		}
		float width = m_CaretWidth;
		int adjustedPos = Mathf.Max(0, caretPositionInternal - m_DrawStart);
		TextGenerator gen = m_TextComponent.cachedTextGenerator;
		if (gen == null || gen.lineCount == 0)
		{
			return;
		}
		Vector2 startPosition = Vector2.zero;
		if (adjustedPos < gen.characters.Count)
		{
			startPosition.x = gen.characters[adjustedPos].cursorPos.x;
		}
		startPosition.x /= m_TextComponent.pixelsPerUnit;
		if (startPosition.x > m_TextComponent.rectTransform.rect.xMax)
		{
			startPosition.x = m_TextComponent.rectTransform.rect.xMax;
		}
		int characterLine = DetermineCharacterLine(adjustedPos, gen);
		startPosition.y = gen.lines[characterLine].topY / m_TextComponent.pixelsPerUnit;
		float height = (float)gen.lines[characterLine].height / m_TextComponent.pixelsPerUnit;
		for (int i = 0; i < m_CursorVerts.Length; i++)
		{
			m_CursorVerts[i].color = caretColor;
		}
		m_CursorVerts[0].position = new Vector3(startPosition.x, startPosition.y - height, 0f);
		m_CursorVerts[1].position = new Vector3(startPosition.x + width, startPosition.y - height, 0f);
		m_CursorVerts[2].position = new Vector3(startPosition.x + width, startPosition.y, 0f);
		m_CursorVerts[3].position = new Vector3(startPosition.x, startPosition.y, 0f);
		if (roundingOffset != Vector2.zero)
		{
			for (int j = 0; j < m_CursorVerts.Length; j++)
			{
				UIVertex uiv = m_CursorVerts[j];
				uiv.position.x += roundingOffset.x;
				uiv.position.y += roundingOffset.y;
			}
		}
		vbo.AddUIVertexQuad(m_CursorVerts);
		int screenHeight = Screen.height;
		int displayIndex = m_TextComponent.canvas.targetDisplay;
		if (displayIndex > 0 && displayIndex < Display.displays.Length)
		{
			screenHeight = Display.displays[displayIndex].renderingHeight;
		}
		startPosition.y = (float)screenHeight - startPosition.y;
		input.compositionCursorPos = startPosition;
	}

	private void CreateCursorVerts()
	{
		m_CursorVerts = new UIVertex[4];
		for (int i = 0; i < m_CursorVerts.Length; i++)
		{
			m_CursorVerts[i] = UIVertex.simpleVert;
			m_CursorVerts[i].uv0 = Vector2.zero;
		}
	}

	private void GenerateHighlight(VertexHelper vbo, Vector2 roundingOffset)
	{
		int startChar = Mathf.Max(0, caretPositionInternal - m_DrawStart);
		int endChar = Mathf.Max(0, caretSelectPositionInternal - m_DrawStart);
		if (startChar > endChar)
		{
			int num = startChar;
			startChar = endChar;
			endChar = num;
		}
		endChar--;
		TextGenerator gen = m_TextComponent.cachedTextGenerator;
		if (gen.lineCount <= 0)
		{
			return;
		}
		int currentLineIndex = DetermineCharacterLine(startChar, gen);
		int lastCharInLineIndex = GetLineEndPosition(gen, currentLineIndex);
		UIVertex vert = UIVertex.simpleVert;
		vert.uv0 = Vector2.zero;
		vert.color = selectionColor;
		for (int currentChar = startChar; currentChar <= endChar && currentChar < gen.characterCount; currentChar++)
		{
			if (currentChar == lastCharInLineIndex || currentChar == endChar)
			{
				UICharInfo startCharInfo = gen.characters[startChar];
				UICharInfo endCharInfo = gen.characters[currentChar];
				Vector2 startPosition = new Vector2(startCharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit, gen.lines[currentLineIndex].topY / m_TextComponent.pixelsPerUnit);
				Vector2 endPosition = new Vector2((endCharInfo.cursorPos.x + endCharInfo.charWidth) / m_TextComponent.pixelsPerUnit, startPosition.y - (float)gen.lines[currentLineIndex].height / m_TextComponent.pixelsPerUnit);
				if (endPosition.x > m_TextComponent.rectTransform.rect.xMax || endPosition.x < m_TextComponent.rectTransform.rect.xMin)
				{
					endPosition.x = m_TextComponent.rectTransform.rect.xMax;
				}
				int startIndex = vbo.currentVertCount;
				vert.position = new Vector3(startPosition.x, endPosition.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(vert);
				vert.position = new Vector3(endPosition.x, endPosition.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(vert);
				vert.position = new Vector3(endPosition.x, startPosition.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(vert);
				vert.position = new Vector3(startPosition.x, startPosition.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(vert);
				vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
				vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
				startChar = currentChar + 1;
				currentLineIndex++;
				lastCharInLineIndex = GetLineEndPosition(gen, currentLineIndex);
			}
		}
	}

	protected char Validate(string text, int pos, char ch)
	{
		if (characterValidation == CharacterValidation.None || !base.enabled)
		{
			return ch;
		}
		if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
		{
			bool num = pos == 0 && text.Length > 0 && text[0] == '-';
			bool dashInSelection = text.Length > 0 && text[0] == '-' && ((caretPositionInternal == 0 && caretSelectPositionInternal > 0) || (caretSelectPositionInternal == 0 && caretPositionInternal > 0));
			bool selectionAtStart = caretPositionInternal == 0 || caretSelectPositionInternal == 0;
			if (!num || dashInSelection)
			{
				if (ch >= '0' && ch <= '9')
				{
					return ch;
				}
				if (ch == '-' && (pos == 0 || selectionAtStart))
				{
					return ch;
				}
				if ((ch == '.' || ch == ',') && characterValidation == CharacterValidation.Decimal && text.IndexOfAny(new char[2] { '.', ',' }) == -1)
				{
					return ch;
				}
			}
		}
		else if (characterValidation == CharacterValidation.Alphanumeric)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (characterValidation == CharacterValidation.Name)
		{
			if (char.IsLetter(ch))
			{
				if (char.IsLower(ch) && (pos == 0 || text[pos - 1] == ' '))
				{
					return char.ToUpper(ch);
				}
				if (char.IsUpper(ch) && pos > 0 && text[pos - 1] != ' ' && text[pos - 1] != '\'')
				{
					return char.ToLower(ch);
				}
				return ch;
			}
			if (ch == '\'' && !text.Contains("'") && (pos <= 0 || (text[pos - 1] != ' ' && text[pos - 1] != '\'')) && (pos >= text.Length || (text[pos] != ' ' && text[pos] != '\'')))
			{
				return ch;
			}
			if (ch == ' ' && pos != 0 && (pos <= 0 || (text[pos - 1] != ' ' && text[pos - 1] != '\'')) && (pos >= text.Length || (text[pos] != ' ' && text[pos] != '\'')))
			{
				return ch;
			}
		}
		else if (characterValidation == CharacterValidation.EmailAddress)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '@' && text.IndexOf('@') == -1)
			{
				return ch;
			}
			if ("!#$%&'*+-/=?^_`{|}~".IndexOf(ch) != -1)
			{
				return ch;
			}
			if (ch == '.')
			{
				char num2 = ((text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ');
				char nextChar = ((text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n');
				if (num2 != '.' && nextChar != '.')
				{
					return ch;
				}
			}
		}
		return '\0';
	}

	public void ActivateInputField()
	{
		if (!(m_TextComponent == null) && !(m_TextComponent.font == null) && IsActive() && IsInteractable())
		{
			if (isFocused && m_Keyboard != null && !m_Keyboard.active)
			{
				m_Keyboard.active = true;
				m_Keyboard.text = m_Text;
			}
			m_ShouldActivateNextUpdate = true;
		}
	}

	private void ActivateInputFieldInternal()
	{
		if (EventSystem.current == null)
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
		if (TouchScreenKeyboard.isSupported)
		{
			if (input.touchSupported)
			{
				TouchScreenKeyboard.hideInput = shouldHideMobileInput;
			}
			m_Keyboard = ((inputType == InputType.Password) ? TouchScreenKeyboard.Open(m_Text, keyboardType, autocorrection: false, multiLine, secure: true, alert: false, "", characterLimit) : TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputType.AutoCorrect, multiLine, secure: false, alert: false, "", characterLimit));
			m_TouchKeyboardAllowsInPlaceEditing = TouchScreenKeyboard.isInPlaceEditingAllowed;
			MoveTextEnd(shift: false);
		}
		else
		{
			input.imeCompositionMode = IMECompositionMode.On;
			OnFocus();
		}
		m_AllowInput = true;
		m_OriginalText = text;
		m_WasCanceled = false;
		SetCaretVisible();
		UpdateLabel();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (shouldActivateOnSelect)
		{
			ActivateInputField();
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ActivateInputField();
		}
	}

	public void DeactivateInputField()
	{
		if (!m_AllowInput)
		{
			return;
		}
		m_HasDoneFocusTransition = false;
		m_AllowInput = false;
		if (m_Placeholder != null)
		{
			m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);
		}
		if (m_TextComponent != null && IsInteractable())
		{
			if (m_WasCanceled)
			{
				text = m_OriginalText;
			}
			SendOnSubmit();
			if (m_Keyboard != null)
			{
				m_Keyboard.active = false;
				m_Keyboard = null;
			}
			m_CaretPosition = (m_CaretSelectPosition = 0);
			input.imeCompositionMode = IMECompositionMode.Auto;
		}
		MarkGeometryAsDirty();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		DeactivateInputField();
		base.OnDeselect(eventData);
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		if (IsActive() && IsInteractable() && !isFocused)
		{
			m_ShouldActivateNextUpdate = true;
		}
	}

	private void EnforceContentType()
	{
		switch (contentType)
		{
		case ContentType.Standard:
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = CharacterValidation.None;
			break;
		case ContentType.Autocorrected:
			m_InputType = InputType.AutoCorrect;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = CharacterValidation.None;
			break;
		case ContentType.IntegerNumber:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.NumberPad;
			m_CharacterValidation = CharacterValidation.Integer;
			break;
		case ContentType.DecimalNumber:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
			m_CharacterValidation = CharacterValidation.Decimal;
			break;
		case ContentType.Alphanumeric:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
			m_CharacterValidation = CharacterValidation.Alphanumeric;
			break;
		case ContentType.Name:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.NamePhonePad;
			m_CharacterValidation = CharacterValidation.Name;
			break;
		case ContentType.EmailAddress:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
			m_CharacterValidation = CharacterValidation.EmailAddress;
			break;
		case ContentType.Password:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Password;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = CharacterValidation.None;
			break;
		case ContentType.Pin:
			m_LineType = LineType.SingleLine;
			m_InputType = InputType.Password;
			m_KeyboardType = TouchScreenKeyboardType.NumberPad;
			m_CharacterValidation = CharacterValidation.Integer;
			break;
		}
		EnforceTextHOverflow();
	}

	private void EnforceTextHOverflow()
	{
		if (m_TextComponent != null)
		{
			if (multiLine)
			{
				m_TextComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
			}
			else
			{
				m_TextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
			}
		}
	}

	private void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
	{
		if (contentType == ContentType.Custom)
		{
			return;
		}
		for (int i = 0; i < allowedContentTypes.Length; i++)
		{
			if (contentType == allowedContentTypes[i])
			{
				return;
			}
		}
		contentType = ContentType.Custom;
	}

	private void SetToCustom()
	{
		if (contentType != ContentType.Custom)
		{
			contentType = ContentType.Custom;
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (m_HasDoneFocusTransition)
		{
			state = SelectionState.Highlighted;
		}
		else if (state == SelectionState.Pressed)
		{
			m_HasDoneFocusTransition = true;
		}
		base.DoStateTransition(state, instant);
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	Transform ICanvasElement.get_transform()
	{
		return base.transform;
	}
}
