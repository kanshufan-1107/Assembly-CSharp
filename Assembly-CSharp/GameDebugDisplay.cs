using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone;
using UnityEngine;

public class GameDebugDisplay : MonoBehaviour
{
	private static GameDebugDisplay s_instance;

	private bool m_showEntities;

	private bool m_hideZeroTags;

	private List<GAME_TAG> m_tagsToDisplay = new List<GAME_TAG>();

	public static GameDebugDisplay Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<GameDebugDisplay>();
			obj.name = "GameDebugDisplay (Dynamically created)";
		}
		return s_instance;
	}

	public bool ToggleEntityCount(string func, string[] args, string rawArgs)
	{
		m_showEntities = !m_showEntities;
		return true;
	}

	public bool ToggleHideZeroTags(string func, string[] args, string rawArgs)
	{
		m_hideZeroTags = !m_hideZeroTags;
		return true;
	}

	public bool AddTagToDisplay(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		for (int i = 0; i < args.Length; i++)
		{
			int parameter = 0;
			if (!int.TryParse(args[i], out parameter))
			{
				string tagName = args[i].Trim();
				if (tagName.Length > 0)
				{
					foreach (int enumValue in Enum.GetValues(typeof(GAME_TAG)))
					{
						GAME_TAG gAME_TAG = (GAME_TAG)enumValue;
						if (gAME_TAG.ToString().ToLower().CompareTo(tagName.ToLower()) == 0)
						{
							parameter = enumValue;
							break;
						}
					}
				}
			}
			if (parameter != 0 && !m_tagsToDisplay.Contains((GAME_TAG)parameter))
			{
				m_tagsToDisplay.Add((GAME_TAG)parameter);
			}
		}
		return true;
	}

	public bool RemoveTagToDisplay(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		for (int i = 0; i < args.Length; i++)
		{
			int parameter = int.Parse(args[i]);
			if (m_tagsToDisplay.Contains((GAME_TAG)parameter))
			{
				m_tagsToDisplay.Remove((GAME_TAG)parameter);
			}
		}
		return true;
	}

	public bool RemoveAllTags(string func, string[] args, string rawArgs)
	{
		m_tagsToDisplay.Clear();
		return true;
	}

	private void Update()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		GameState currentGame = GameState.Get();
		if (currentGame == null)
		{
			return;
		}
		Map<int, Entity> entityMap = currentGame.GetEntityMap();
		string topRightString = "";
		string bottomRightString = "";
		foreach (KeyValuePair<int, Entity> item in entityMap)
		{
			Entity ent = item.Value;
			if (ent != null)
			{
				topRightString = HandleCornerDisplay(ent, GAME_TAG.DEBUG_DISPLAY_TAG_TOP_RIGHT, topRightString);
				bottomRightString = HandleCornerDisplay(ent, GAME_TAG.DEBUG_DISPLAY_TAG_BOTTOM_RIGHT, bottomRightString);
			}
		}
		if (topRightString != "")
		{
			DebugTextManager.Get().DrawDebugText(topRightString, new Vector3((float)Screen.width - 150f, (float)Screen.height - 100f, 0f), 0f, screenSpace: true);
		}
		if (bottomRightString != "")
		{
			DebugTextManager.Get().DrawDebugText(bottomRightString, new Vector3((float)Screen.width - 150f, 100f, 0f), 0f, screenSpace: true);
		}
		if (m_showEntities)
		{
			string bottomLeftString = "Entities: " + entityMap.Count;
			DebugTextManager.Get().DrawDebugText(bottomLeftString, new Vector3(100f, 100f, 0f), 0f, screenSpace: true);
		}
		if (m_tagsToDisplay.Count == 0)
		{
			return;
		}
		Card mousedOverCard = InputManager.Get().GetMousedOverCard();
		Entity mousedOverEntity = null;
		RaycastHit leftClickRayInfo;
		if (mousedOverCard != null && mousedOverCard.GetEntity() != null)
		{
			mousedOverEntity = mousedOverCard.GetEntity();
		}
		else if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out leftClickRayInfo))
		{
			GameObject hitObject = leftClickRayInfo.collider.gameObject;
			if (hitObject.GetComponent<EndTurnButton>() != null || hitObject.GetComponent<EndTurnButtonReminder>() != null)
			{
				mousedOverEntity = currentGame.GetGameEntity();
			}
		}
		List<Zone> zones = ZoneMgr.Get().GetZones();
		for (int i = 0; i < zones.Count; i++)
		{
			Zone zone = zones[i];
			if (zone.m_ServerTag != TAG_ZONE.HAND && zone.m_ServerTag != TAG_ZONE.PLAY && zone.m_ServerTag != TAG_ZONE.SECRET && zone.m_ServerTag != TAG_ZONE.LETTUCE_ABILITY)
			{
				continue;
			}
			foreach (Card card in zone.GetCards())
			{
				Entity ent2 = card.GetEntity();
				if (mousedOverEntity != null && mousedOverEntity != ent2)
				{
					continue;
				}
				Vector3 drawPos = card.transform.position;
				if (zone.m_ServerTag == TAG_ZONE.HAND)
				{
					Vector3 offset = card.transform.forward;
					if (card.GetControllerSide() == Player.Side.OPPOSING)
					{
						offset *= -1.5f;
						if (card.GetController().IsRevealed())
						{
							offset = -offset;
						}
					}
					drawPos += offset;
				}
				else if (zone.m_ServerTag == TAG_ZONE.LETTUCE_ABILITY)
				{
					if (ent2.GetLettuceAbilityOwner() != ZoneMgr.Get().GetLettuceAbilitiesSourceEntity())
					{
						continue;
					}
					Actor actor = card.GetActor();
					if (actor == null)
					{
						continue;
					}
					drawPos = actor.transform.position;
				}
				if (mousedOverEntity != null)
				{
					DrawDebugTextForHighlightedCard(ent2, DebugTextManager.WorldPosToScreenPos(drawPos));
				}
				else
				{
					DrawDebugTextForCard(ent2, drawPos);
				}
				if (mousedOverEntity != null)
				{
					if (mousedOverEntity.IsHero())
					{
						Entity player = ((!mousedOverEntity.IsControlledByFriendlySidePlayer()) ? GameState.Get().GetOpposingSidePlayer() : GameState.Get().GetFriendlySidePlayer());
						Vector2 heroScreenPos = DebugTextManager.WorldPosToScreenPos(player.GetHeroCard().transform.position) + new Vector2(-300f, 0f);
						DrawDebugTextForHighlightedCard(player, heroScreenPos);
					}
					return;
				}
			}
		}
		if (mousedOverEntity == currentGame.GetGameEntity())
		{
			DrawDebugTextForHighlightedCard(mousedOverEntity, DebugTextManager.WorldPosToScreenPos(EndTurnButton.Get().transform.position));
			return;
		}
		DrawDebugTextForCard(currentGame.GetGameEntity(), EndTurnButton.Get().transform.position);
		foreach (Player player2 in GameState.Get().GetPlayerMap().Values)
		{
			if (player2 != null && !(player2.GetHeroCard() == null))
			{
				Vector2 heroScreenPos2 = DebugTextManager.WorldPosToScreenPos(player2.GetHeroCard().transform.position) + new Vector2(-300f, 0f);
				DrawDebugTextForCard(player2, heroScreenPos2, screenSpace: true);
			}
		}
	}

	private string HandleCornerDisplay(Entity ent, GAME_TAG tag, string currentString)
	{
		if (ent.HasTag(tag))
		{
			if (currentString != "")
			{
				currentString += "\n";
			}
			GAME_TAG tagToPrint = (GAME_TAG)ent.GetTag(tag);
			string tagName = tagToPrint.ToString();
			tagName = (int.TryParse(tagName, out var _) ? "" : $"{tagName}: ");
			currentString = $"{currentString}{ent.GetName()}\n{tagName}{ent.GetTag(tagToPrint)}";
		}
		return currentString;
	}

	private void DrawDebugTextForHighlightedCard(Entity ent, Vector3 pos)
	{
		string resultingText = DrawDebugTextForCard(ent, pos, screenSpace: true, forceShowZeroTags: true);
		Vector3 increment = new Vector3(0f, DebugTextManager.Get().TextSize(resultingText).y + 5f, 0f);
		if (ent.IsGame())
		{
			List<Entity> enchantments = ent.GetAttachments();
			for (int i = 0; i < enchantments.Count; i++)
			{
				Vector3 wantedPos = pos;
				DrawDebugTextForCard(pos: (i % 2 != 0) ? (pos - increment * (i / 2 + 1)) : (pos + increment * (i / 2 + 1)), ent: enchantments[i], screenSpace: true);
			}
			return;
		}
		if (ent.IsControlledByOpposingSidePlayer())
		{
			increment.y = 0f - increment.y;
		}
		foreach (Entity enchant in ent.GetAttachments())
		{
			pos += increment;
			DrawDebugTextForCard(enchant, pos, screenSpace: true);
		}
	}

	private string DrawDebugTextForCard(Entity ent, Vector3 pos, bool screenSpace = false, bool forceShowZeroTags = false)
	{
		string displayString = "";
		for (int j = 0; j < m_tagsToDisplay.Count; j++)
		{
			GAME_TAG tag = m_tagsToDisplay[j];
			int value = ent.GetTag(tag);
			if (forceShowZeroTags || !m_hideZeroTags || value != 0)
			{
				displayString = $"{displayString}\n{tag.ToString()}: {value}";
			}
		}
		if (!string.IsNullOrEmpty(displayString))
		{
			displayString = ent.GetName() + displayString;
			DebugTextManager.Get().DrawDebugText(displayString, pos, 0f, screenSpace);
		}
		return displayString;
	}
}
