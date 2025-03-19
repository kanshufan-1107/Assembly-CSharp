using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FatalErrorDialog : MonoBehaviour
{
	private const float DialogWidth = 600f;

	private const float DialogHeight = 347f;

	private const float DialogPadding = 20f;

	private const float ButtonWidth = 100f;

	private const float ButtonHeight = 31f;

	private GUIStyle m_dialogStyle;

	private string m_text;

	private float DialogTop => ((float)Screen.height - 347f) / 2f;

	private float DialogLeft => ((float)Screen.width - 600f) / 2f;

	private Rect DialogRect => new Rect(DialogLeft, DialogTop, 600f, 347f);

	private float ButtonTop => DialogTop + 347f - 20f - 31f;

	private float ButtonLeft => ((float)Screen.width - 100f) / 2f;

	private Rect ButtonRect => new Rect(ButtonLeft, ButtonTop, 100f, 31f);

	private void Awake()
	{
		BuildText();
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
	}

	private void OnGUI()
	{
		InitGUIStyles();
		GUI.Box(DialogRect, string.Empty, m_dialogStyle);
		GUI.Box(DialogRect, m_text, m_dialogStyle);
		if (GUI.Button(ButtonRect, GameStrings.Get("GLOBAL_EXIT")))
		{
			FatalErrorMgr.Get().NotifyExitPressed();
		}
	}

	private void InitGUIStyles()
	{
		if (m_dialogStyle == null)
		{
			m_dialogStyle = new GUIStyle("box")
			{
				clipping = TextClipping.Overflow,
				stretchHeight = true,
				stretchWidth = true,
				wordWrap = true,
				fontSize = 16
			};
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		BuildText();
	}

	private void BuildText()
	{
		if (!FatalErrorMgr.Get().HasError())
		{
			m_text = string.Empty;
			return;
		}
		FatalErrorMessage[] errorMessages = FatalErrorMgr.Get().GetMessages();
		List<string> textMessages = new List<string>();
		for (int i = 0; i < errorMessages.Length; i++)
		{
			string text = errorMessages[i].m_text;
			if (!textMessages.Contains(text))
			{
				textMessages.Add(text);
			}
		}
		StringBuilder textBuilder = new StringBuilder();
		for (int j = 0; j < textMessages.Count; j++)
		{
			string text2 = textMessages[j];
			textBuilder.Append(text2);
			textBuilder.Append("\n");
		}
		textBuilder.Remove(textBuilder.Length - 1, 1);
		m_text = textBuilder.ToString();
	}
}
