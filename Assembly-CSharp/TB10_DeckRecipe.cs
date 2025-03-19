using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TB10_DeckRecipe : MissionEntity
{
	public struct RecipeMessage
	{
		public string Message;

		public float Delay;
	}

	private Notification DeckRecipePopup;

	private Vector3 popUpPos;

	private string textID;

	private bool doPopup;

	private bool doLeftArrow;

	private bool doUpArrow;

	private bool doDownArrow;

	private float delayTime = 2.5f;

	private float popupDuration = 7f;

	private float popupScale = 2.5f;

	private HashSet<int> seen = new HashSet<int>();

	private static readonly Dictionary<int, RecipeMessage> popupMsgs = new Dictionary<int, RecipeMessage>
	{
		{
			939,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_DRUID",
				Delay = 7f
			}
		},
		{
			946,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_HUNTER",
				Delay = 7f
			}
		},
		{
			947,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_MAGE",
				Delay = 7f
			}
		},
		{
			938,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_PALADIN",
				Delay = 7f
			}
		},
		{
			945,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_PRIEST",
				Delay = 7f
			}
		},
		{
			944,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_ROGUE",
				Delay = 7f
			}
		},
		{
			937,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_SHAMAN",
				Delay = 7f
			}
		},
		{
			940,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_WARLOCK",
				Delay = 7f
			}
		},
		{
			936,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_OG_WARRIOR",
				Delay = 7f
			}
		},
		{
			1125,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_DRUID",
				Delay = 2.5f
			}
		},
		{
			1130,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_HUNTER",
				Delay = 2.5f
			}
		},
		{
			1131,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_MAGE",
				Delay = 2.5f
			}
		},
		{
			1124,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_PALADIN",
				Delay = 2.5f
			}
		},
		{
			1129,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_PRIEST",
				Delay = 2.5f
			}
		},
		{
			1128,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_ROGUE",
				Delay = 2.5f
			}
		},
		{
			1123,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_SHAMAN",
				Delay = 2.5f
			}
		},
		{
			1126,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_WARLOCK",
				Delay = 2.5f
			}
		},
		{
			1122,
			new RecipeMessage
			{
				Message = "TB_DECKRECIPE_MSG_WARRIOR",
				Delay = 2.5f
			}
		}
	};

	public override void PreloadAssets()
	{
		PreloadSound("tutorial_mission_hero_coin_mouse_away.prefab:6266be3ca0b50a645915b9ea0a59d774");
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
		{
			yield return null;
		}
		if (seen.Contains(missionEvent))
		{
			yield break;
		}
		seen.Add(missionEvent);
		doPopup = false;
		doLeftArrow = false;
		doUpArrow = false;
		doDownArrow = false;
		if (missionEvent == 11)
		{
			NotificationManager.Get().DestroyNotification(DeckRecipePopup, 0f);
			doPopup = false;
		}
		else if (missionEvent > 900)
		{
			doPopup = true;
			textID = popupMsgs[missionEvent].Message;
			popupDuration = popupMsgs[missionEvent].Delay;
			popUpPos.x = 0f;
			popUpPos.z = 10f;
			_ = (bool)UniversalInputManager.UsePhoneUI;
		}
		if (doPopup)
		{
			yield return new WaitForSeconds(delayTime);
			DeckRecipePopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popupScale, GameStrings.Get(textID), convertLegacyPosition: false);
			if (doLeftArrow)
			{
				DeckRecipePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			}
			if (doUpArrow)
			{
				DeckRecipePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
			}
			if (doDownArrow)
			{
				DeckRecipePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
			}
			PlaySound("tutorial_mission_hero_coin_mouse_away.prefab:6266be3ca0b50a645915b9ea0a59d774");
			NotificationManager.Get().DestroyNotification(DeckRecipePopup, popupDuration);
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(popupDuration);
			GameState.Get().SetBusy(busy: false);
		}
	}
}
