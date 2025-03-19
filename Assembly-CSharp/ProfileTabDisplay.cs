using Hearthstone.UI;
using UnityEngine;

public class ProfileTabDisplay : MonoBehaviour
{
	public const string TURN_ARROW_GLOW_ON = "CODE_TURN_GLOW_ON";

	public const string TURN_ARROW_GLOW_OFF = "CODE_TURN_GLOW_OFF";

	public const string PROFILE_PAGE_ARROW_PRESSED = "PAGE_FLIP01";

	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "PAGE_FLIP01")
			{
				RightArrowPressed();
			}
		});
		if (GetProfilePageArrowGlow() <= 0)
		{
			m_widget.TriggerEvent("CODE_TURN_GLOW_ON");
		}
		else
		{
			m_widget.TriggerEvent("CODE_TURN_GLOW_OFF");
		}
	}

	private void RightArrowPressed()
	{
		SetProfilePageArrowGlow(1);
		m_widget.TriggerEvent("CODE_TURN_GLOW_OFF");
	}

	public bool SetProfilePageArrowGlow(int profilePageArrowGlow)
	{
		if (GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_PROFILE_PAGE_HAS_SEEN_ARROW_GLOW, profilePageArrowGlow)))
		{
			return true;
		}
		return false;
	}

	public int GetProfilePageArrowGlow()
	{
		long pageArrowGlow = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_PROFILE_PAGE_HAS_SEEN_ARROW_GLOW, out pageArrowGlow);
		return (int)pageArrowGlow;
	}
}
