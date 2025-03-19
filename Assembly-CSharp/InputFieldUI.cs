using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldUI : MonoBehaviour
{
	[SerializeField]
	private Image m_inputFieldBackgroundImage;

	[SerializeField]
	private Font m_defaultInputFont;

	[SerializeField]
	private TextAnchor m_defaultInputTextAlignment = TextAnchor.MiddleLeft;

	[SerializeField]
	private RectTransform m_inputFieldRect;

	[SerializeField]
	private Canvas m_inputFieldCanvas;

	[SerializeField]
	private HSInputField m_inputField;

	[SerializeField]
	private float m_inputFieldPadding = 5f;

	[SerializeField]
	private GameLayer m_defaultCanvasLayer = GameLayer.HighPriorityUI;

	[SerializeField]
	private int m_defaultOrderInLayer;

	private Canvas m_masterCanvas;

	public string Text
	{
		get
		{
			return m_inputField.text;
		}
		set
		{
			m_inputField.text = value;
		}
	}

	public string RawText
	{
		get
		{
			return m_inputField.textComponent.text;
		}
		set
		{
			m_inputField.textComponent.text = value;
		}
	}

	public bool IsFocused => m_inputField.isFocused;

	public float RectHeight => m_inputFieldRect.rect.height;

	public void SetTextInputParams(UniversalInputManager.TextInputParams parms)
	{
		m_inputField.contentType = (parms.m_number ? HSInputField.ContentType.IntegerNumber : HSInputField.ContentType.Standard);
		m_inputField.contentType = (parms.m_password ? HSInputField.ContentType.Password : m_inputField.contentType);
		m_inputField.lineType = (parms.m_multiLine ? HSInputField.LineType.MultiLineNewline : HSInputField.LineType.SingleLine);
		m_inputField.characterLimit = parms.m_maxCharacters;
		m_inputField.textComponent.color = parms.m_color ?? m_inputField.textComponent.color;
		m_inputField.textComponent.font = parms.m_font ?? m_defaultInputFont;
		m_inputField.textComponent.alignment = parms.m_alignment ?? m_defaultInputTextAlignment;
		m_inputField.text = parms.m_text ?? string.Empty;
		m_inputFieldBackgroundImage.enabled = parms.m_showBackground;
		m_inputFieldCanvas.gameObject.layer = (int)(parms.m_gameLayer ?? m_defaultCanvasLayer);
		if (m_masterCanvas != null)
		{
			m_masterCanvas.sortingOrder = parms.m_orderInLayer ?? m_defaultOrderInLayer;
		}
	}

	public void SetCanvasActive(bool active, bool showBackground = false)
	{
		m_inputFieldCanvas.enabled = active;
	}

	public void SetInputRect(Rect r)
	{
		m_inputFieldRect.anchorMin = Vector3.zero;
		m_inputFieldRect.anchorMax = Vector3.one;
		m_inputFieldRect.sizeDelta = new Vector2(r.xMax - r.xMin - m_inputFieldPadding, r.yMax - r.yMin);
		m_inputFieldRect.anchoredPosition = new Vector2((r.xMin + r.xMax) / 2f, (0f - (r.yMin + r.yMax)) / 2f);
	}

	public void SetupTextProperties(int fontSize, Color? inputColor, TextAnchor? inputAlignment)
	{
		m_inputField.textComponent.fontSize = fontSize;
		if (inputColor.HasValue)
		{
			m_inputField.textComponent.color = inputColor.Value;
		}
		m_inputField.textComponent.alignment = inputAlignment ?? m_defaultInputTextAlignment;
	}

	public void ActivateInputField()
	{
		m_inputField.ActivateInputField();
	}

	public void MoveCursorToEnd()
	{
		m_inputField.caretPosition = m_inputField.text.Length;
	}

	public void SetEndEditFunction(UnityAction<string> endEditFunc)
	{
		m_inputField.onEndEdit.AddListener(endEditFunc);
	}

	private void Awake()
	{
		m_masterCanvas = GetComponent<Canvas>();
	}
}
