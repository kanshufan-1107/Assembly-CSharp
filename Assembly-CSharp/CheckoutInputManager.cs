using System.Collections.Generic;
using Blizzard.Commerce;
using UnityEngine;

public class CheckoutInputManager : MonoBehaviour
{
	public delegate void KeyboardEventListener(bool isKeyDown);

	private const int INPUT_DELTA_DELTA_SCALE = 40;

	private Vec2D m_lastMousePosition;

	private HearthstoneCheckout m_checkout;

	private IScreenSpace screenSpace;

	private List<char> blockedCharacters = new List<char> { '\t', '\n' };

	private Dictionary<KeyCode, KeyboardEventListener> m_KeyboardEventHandlers = new Dictionary<KeyCode, KeyboardEventListener>();

	public bool IsActive { get; set; }

	private int GetModifiers(Event e)
	{
		int modifiers = 0;
		if (e.isKey)
		{
			if ((e.modifiers & EventModifiers.Shift) != 0)
			{
				modifiers |= 2;
			}
			if ((e.modifiers & EventModifiers.Control) != 0)
			{
				modifiers |= 1;
			}
			if ((e.modifiers & EventModifiers.Alt) != 0)
			{
				modifiers |= 4;
			}
		}
		return modifiers;
	}

	public void Setup(HearthstoneCheckout checkout, IScreenSpace screenSpace)
	{
		m_checkout = checkout;
		this.screenSpace = screenSpace;
	}

	public void AddKeyboardEventListener(KeyCode keyCode, KeyboardEventListener listener)
	{
		m_KeyboardEventHandlers[keyCode] = listener;
	}

	public void RemoveKeyboardEventListener(KeyCode keyCode)
	{
		m_KeyboardEventHandlers.Remove(keyCode);
	}

	private Vec2D GetMousePosition(Rect window, Vector3 mousePosition, float inputScale)
	{
		if (window.Contains(mousePosition))
		{
			return new Vec2D((int)((mousePosition.x - window.x) / inputScale), (int)(((float)Screen.height - mousePosition.y - window.y) / inputScale));
		}
		return null;
	}

	public void OnGUI()
	{
		if (!IsActive || m_checkout == null || !m_checkout.CheckoutIsReady)
		{
			return;
		}
		Event e = Event.current;
		if (e == null)
		{
			return;
		}
		int modifiers = 0;
		KeyCode keyCode = KeyCode.None;
		char character = '\0';
		bool isKeyDown = true;
		while (Event.PopEvent(e))
		{
			new Rect(Screen.width / 2 - m_checkout.CheckoutUi.BrowserWidth / 2, Screen.height / 2 - m_checkout.CheckoutUi.BrowserHeight / 2, m_checkout.CheckoutUi.BrowserWidth, m_checkout.CheckoutUi.BrowserHeight);
			if (e.isKey)
			{
				modifiers = GetModifiers(e);
				isKeyDown = e.type == EventType.KeyDown;
				if ((e.modifiers & EventModifiers.FunctionKey) != 0)
				{
					if (!CommerceWrapper.Instance.SendKeyboardInput(e.keyCode, isKeyDown, (uint)modifiers, e.character))
					{
						Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendKeyboardInput failed");
					}
					continue;
				}
				if (e.keyCode > KeyCode.None)
				{
					keyCode = e.keyCode;
				}
				if (e.character != 0)
				{
					character = SwapCharacter(e.character);
				}
			}
			Vector3 screenMousePosition = InputCollection.GetMousePosition();
			Vec2D sceneMousePosition = GetMousePosition(screenSpace.GetScreenRect(), screenMousePosition, screenSpace.GetScreenSpaceScale());
			if (sceneMousePosition == null)
			{
				continue;
			}
			if (e.isScrollWheel)
			{
				if (e.type == EventType.ScrollWheel)
				{
					int calcDelta = (int)((0f - e.delta.y) * 40f);
					if (!CommerceWrapper.Instance.SendMouseWheelEvent(calcDelta, sceneMousePosition, (uint)modifiers))
					{
						Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendMouseWheelEvent failed");
					}
				}
			}
			else if (e.type == EventType.MouseDown || e.type == EventType.MouseUp)
			{
				bool isDown = e.type == EventType.MouseDown;
				if (!CommerceWrapper.Instance.SendMouseInputEvent(isDown, e.button, sceneMousePosition, (uint)modifiers))
				{
					Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendMouseInputEvent failed");
				}
			}
			else if ((e.type == EventType.MouseEnterWindow || e.type == EventType.MouseMove || m_lastMousePosition == null || m_lastMousePosition.x != sceneMousePosition.x || m_lastMousePosition.y != sceneMousePosition.y) && !CommerceWrapper.Instance.SendMouseMoveEvent(sceneMousePosition, (uint)modifiers))
			{
				Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendMouseMoveEvent failed");
			}
			m_lastMousePosition = sceneMousePosition;
		}
		if (!m_KeyboardEventHandlers.TryGetValue(e.keyCode, out var callback))
		{
			if (keyCode <= KeyCode.None && character == '\0')
			{
				return;
			}
			if (character == '\0' || character == '\t')
			{
				int kCode = Helper.KeycodeToVK(keyCode);
				if (kCode == (int)keyCode)
				{
					kCode = ((character == '\0') ? char.ToUpper(keyCode.ToString()[0]) : character);
				}
				if (!CommerceWrapper.Instance.SendKeyboardInput(kCode, isKeyDown, (uint)modifiers, character))
				{
					Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendKeyboardInput failed");
				}
			}
			if (!blockedCharacters.Contains(character) && !CommerceWrapper.Instance.SendCharacterEvent(character, (uint)modifiers))
			{
				Log.Store.PrintWarning("[CheckoutInputManager.OnGui] SendCharacterEvent failed");
			}
		}
		else
		{
			callback(isKeyDown);
		}
	}

	private static char SwapCharacter(char character)
	{
		return (int)character switch
		{
			10 => '\r', 
			25 => '\t', 
			_ => character, 
		};
	}
}
