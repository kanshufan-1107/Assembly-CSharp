using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone;
using UnityEngine;

public class DebugTextManager : MonoBehaviour
{
	private class DebugTextRequest
	{
		public string m_text;

		public Vector3 m_position;

		public float m_remainingDuration;

		public bool m_screenSpace;

		public bool m_fitOnScreen;

		public string m_requestIdentifier;

		public GUIStyle m_textStyle;

		public DebugTextRequest(string text, Vector3 position, float duration, bool screenSpace, string requestIdentifier, GUIStyle textStyle = null)
		{
			m_text = text;
			m_position = position;
			m_remainingDuration = duration;
			m_screenSpace = screenSpace;
			m_fitOnScreen = true;
			m_requestIdentifier = requestIdentifier;
			m_textStyle = textStyle;
		}
	}

	private static DebugTextManager s_instance;

	private GUIStyle debugTextStyle;

	private List<DebugTextRequest> m_textRequests = new List<DebugTextRequest>();

	private Map<int, float> m_scrollBarValues = new Map<int, float>();

	public static DebugTextManager Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<DebugTextManager>();
			obj.name = "DebugTextManager (Dynamically created)";
			s_instance.debugTextStyle = new GUIStyle("box");
			s_instance.debugTextStyle.fontSize = 12;
			s_instance.debugTextStyle.fontStyle = FontStyle.Bold;
			s_instance.debugTextStyle.normal.textColor = Color.white;
			s_instance.debugTextStyle.alignment = TextAnchor.MiddleCenter;
		}
		return s_instance;
	}

	public static Vector2 WorldPosToScreenPos(Vector3 position)
	{
		return Camera.main.WorldToScreenPoint(position);
	}

	public Vector2 TextSize(string text)
	{
		return debugTextStyle.CalcSize(new GUIContent(text));
	}

	public void DrawDebugText(string text, Vector3 position, float duration = 5f, bool screenSpace = false, string requestIdentifier = "", GUIStyle textStyle = null)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			m_textRequests.Add(new DebugTextRequest(text, position, duration, screenSpace, requestIdentifier, textStyle));
		}
	}

	private void LateUpdate()
	{
		if (!HearthstoneApplication.IsPublic())
		{
			m_textRequests.RemoveAll((DebugTextRequest x) => x.m_remainingDuration < 0f);
			m_textRequests.ForEach(delegate(DebugTextRequest x)
			{
				x.m_remainingDuration -= Time.deltaTime;
			});
		}
	}

	private void OnGUI()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		foreach (DebugTextRequest request in m_textRequests)
		{
			Vector3 screenPos = (request.m_screenSpace ? request.m_position : Camera.main.WorldToScreenPoint(request.m_position));
			Vector2 textSize = ((request.m_textStyle != null) ? request.m_textStyle.CalcSize(new GUIContent(request.m_text)) : debugTextStyle.CalcSize(new GUIContent(request.m_text)));
			Rect rect = new Rect(screenPos.x - textSize.x / 2f, (float)Screen.height - screenPos.y - textSize.y / 2f, textSize.x, textSize.y);
			if (request.m_fitOnScreen)
			{
				if (rect.x < 0f)
				{
					rect.x = 0f;
				}
				else if (rect.x + textSize.x > (float)Screen.width)
				{
					rect.x = (float)Screen.width - textSize.x;
				}
				if (rect.y < 0f)
				{
					rect.y = 0f;
				}
				else if (rect.y + textSize.y > (float)Screen.height)
				{
					rect.y = (float)Screen.height - textSize.y;
				}
				if (textSize.y > (float)Screen.height)
				{
					float currentValue = 0f;
					int textHash = 0;
					textHash = ((!string.IsNullOrEmpty(request.m_requestIdentifier)) ? request.m_requestIdentifier.GetHashCode() : request.m_text.GetHashCode());
					if (m_scrollBarValues.ContainsKey(textHash))
					{
						currentValue = m_scrollBarValues[textHash];
					}
					int scrollbarXPos = (int)rect.x - 50;
					if (scrollbarXPos <= 0)
					{
						scrollbarXPos = (int)rect.x + (int)textSize.x + 50;
					}
					m_scrollBarValues[textHash] = GUI.VerticalSlider(new Rect(scrollbarXPos, rect.y + 10f, 100f, Screen.height - 100), currentValue, 0f, 1f);
					float excessSize = textSize.y - (float)Screen.height;
					rect.y -= excessSize * m_scrollBarValues[textHash];
				}
			}
			if (request.m_textStyle == null)
			{
				GUI.Box(rect, request.m_text, debugTextStyle);
			}
			else
			{
				GUI.Box(rect, request.m_text, request.m_textStyle);
			}
		}
	}
}
