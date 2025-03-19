using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_Champs : MissionEntity
{
	public struct PopupMessage
	{
		public string Message;

		public float Delay;

		public string Champion;
	}

	private Notification m_popup;

	public bool doPopup;

	private static readonly Dictionary<int, PopupMessage> popupMsgs = new Dictionary<int, PopupMessage>
	{
		{
			1235,
			new PopupMessage
			{
				Message = "FB_CHAMPS_PAVEL_DRUID",
				Delay = 6f,
				Champion = "Pavel"
			}
		},
		{
			1236,
			new PopupMessage
			{
				Message = "FB_CHAMPS_PAVEL_MAGE",
				Delay = 6f,
				Champion = "Pavel"
			}
		},
		{
			1237,
			new PopupMessage
			{
				Message = "FB_CHAMPS_PAVEL_SHAMAN",
				Delay = 6f,
				Champion = "Pavel"
			}
		},
		{
			1238,
			new PopupMessage
			{
				Message = "FB_CHAMPS_PAVEL_ROGUE",
				Delay = 6f,
				Champion = "Pavel"
			}
		},
		{
			1239,
			new PopupMessage
			{
				Message = "FB_CHAMPS_PAVEL_WARRIOR",
				Delay = 6f,
				Champion = "Pavel"
			}
		},
		{
			1671,
			new PopupMessage
			{
				Message = "FB_CHAMPS_OSTKAKA_ROGUE",
				Delay = 6f,
				Champion = "Ostkaka"
			}
		},
		{
			1672,
			new PopupMessage
			{
				Message = "FB_CHAMPS_OSTKAKA_WARRIOR",
				Delay = 6f,
				Champion = "Ostkaka"
			}
		},
		{
			1673,
			new PopupMessage
			{
				Message = "FB_CHAMPS_OSTKAKA_MAGE",
				Delay = 6f,
				Champion = "Ostkaka"
			}
		},
		{
			1675,
			new PopupMessage
			{
				Message = "FB_CHAMPS_FIREBAT_ROGUE",
				Delay = 6f,
				Champion = "Firebat"
			}
		},
		{
			1676,
			new PopupMessage
			{
				Message = "FB_CHAMPS_FIREBAT_HUNTER",
				Delay = 6f,
				Champion = "Firebat"
			}
		},
		{
			1678,
			new PopupMessage
			{
				Message = "FB_CHAMPS_FIREBAT_DRUID",
				Delay = 6f,
				Champion = "Firebat"
			}
		},
		{
			1679,
			new PopupMessage
			{
				Message = "FB_CHAMPS_FIREBAT_WARLOCK",
				Delay = 6f,
				Champion = "Firebat"
			}
		},
		{
			2173,
			new PopupMessage
			{
				Message = "FB_CHAMPS_TOM60229_WARLOCK",
				Delay = 6f,
				Champion = "tom60229"
			}
		},
		{
			2174,
			new PopupMessage
			{
				Message = "FB_CHAMPS_TOM60229_DRUID",
				Delay = 6f,
				Champion = "tom60229"
			}
		},
		{
			2175,
			new PopupMessage
			{
				Message = "FB_CHAMPS_TOM60229_ROGUE",
				Delay = 6f,
				Champion = "tom60229"
			}
		},
		{
			2176,
			new PopupMessage
			{
				Message = "FB_CHAMPS_TOM60229_PRIEST",
				Delay = 6f,
				Champion = "tom60229"
			}
		},
		{
			2838,
			new PopupMessage
			{
				Message = "FB_CHAMPS_VKLIOOON_SHAMAN",
				Delay = 6f,
				Champion = "VKLiooon"
			}
		},
		{
			2839,
			new PopupMessage
			{
				Message = "FB_CHAMPS_VKLIOOON_HUNTER",
				Delay = 6f,
				Champion = "VKLiooon"
			}
		},
		{
			2840,
			new PopupMessage
			{
				Message = "FB_CHAMPS_VKLIOOON_PRIEST",
				Delay = 6f,
				Champion = "VKLiooon"
			}
		},
		{
			2841,
			new PopupMessage
			{
				Message = "FB_CHAMPS_VKLIOOON_DRUID",
				Delay = 6f,
				Champion = "VKLiooon"
			}
		},
		{
			2842,
			new PopupMessage
			{
				Message = "FB_CHAMPS_HUNTERACE_SHAMAN",
				Delay = 6f,
				Champion = "Hunterace"
			}
		},
		{
			2843,
			new PopupMessage
			{
				Message = "FB_CHAMPS_HUNTERACE_ROGUE",
				Delay = 6f,
				Champion = "Hunterace"
			}
		},
		{
			2844,
			new PopupMessage
			{
				Message = "FB_CHAMPS_HUNTERACE_MAGE",
				Delay = 6f,
				Champion = "Hunterace"
			}
		},
		{
			2845,
			new PopupMessage
			{
				Message = "FB_CHAMPS_HUNTERACE_WARRIOR",
				Delay = 6f,
				Champion = "Hunterace"
			}
		},
		{
			2847,
			new PopupMessage
			{
				Message = "FB_CHAMPS_MERC14",
				Delay = 6f,
				Champion = "Mercenaries 14"
			}
		}
	};

	public override void PreloadAssets()
	{
		PreloadSound("tutorial_mission_hero_coin_mouse_away.prefab:6266be3ca0b50a645915b9ea0a59d774");
	}

	public override string GetNameBannerOverride(Player.Side playerSide)
	{
		int tag = GameState.Get().GetPlayerBySide(playerSide).GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		return popupMsgs[tag].Champion;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		doPopup = true;
		if (missionEvent == 10000)
		{
			doPopup = false;
		}
		Vector3 popUpPos = default(Vector3);
		if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
		{
			popUpPos.z = (UniversalInputManager.UsePhoneUI ? 27f : 18f);
		}
		else
		{
			popUpPos.z = (UniversalInputManager.UsePhoneUI ? (-18f) : (-12f));
			yield return new WaitForSeconds(3f);
		}
		if (doPopup)
		{
			yield return ShowPopup(GameStrings.Get(popupMsgs[missionEvent].Message), popupMsgs[missionEvent].Delay, popUpPos);
		}
	}

	private IEnumerator ShowPopup(string stringID, float popupDuration, Vector3 popUpPos)
	{
		m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * 1.4f, GameStrings.Get(stringID), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
		NotificationManager.Get().DestroyNotification(m_popup, popupDuration);
		GameState.Get().SetBusy(busy: true);
		yield return new WaitForSeconds(5f);
		GameState.Get().SetBusy(busy: false);
	}
}
