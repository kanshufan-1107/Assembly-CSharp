using Blizzard.T5.Core;
using UnityEngine;

public class TutorialEntity : MissionEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static readonly float TUTORIAL_DIALOG_SCALE_PHONE = 1.4f;

	private static readonly Vector3 HELP_POPUP_SCALE = 16f * Vector3.one;

	protected TutorialNotification m_preTutorialNotification;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool>
		{
			{
				GameEntityOption.HANDLE_COIN,
				false
			},
			{
				GameEntityOption.DO_OPENING_TAUNTS,
				false
			}
		};
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public static Vector3 GetTextScale()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return HELP_POPUP_SCALE * TUTORIAL_DIALOG_SCALE_PHONE;
		}
		return HELP_POPUP_SCALE;
	}

	public virtual bool IsCustomIntroFinished()
	{
		return true;
	}

	public void ClearPreTutorialNotification()
	{
		if (!(m_preTutorialNotification == null))
		{
			m_preTutorialNotification.gameObject.SetActive(value: false);
		}
	}
}
