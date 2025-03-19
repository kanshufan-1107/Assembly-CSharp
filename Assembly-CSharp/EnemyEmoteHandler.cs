using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class EnemyEmoteHandler : MonoBehaviour
{
	[Serializable]
	private class EnemyEmote
	{
		public GameObject m_SquelchEmote;

		public MeshRenderer m_SquelchEmoteBackplate;

		public UberText m_SquelchEmoteText;

		public bool m_SquelchTeammate;

		private bool m_squelchMousedOver;

		public bool IsMousedOver()
		{
			return m_squelchMousedOver;
		}

		public void SetMousedOver(bool mousedOver)
		{
			m_squelchMousedOver = mousedOver;
		}

		public void MouseOverSquelch(Vector3 startingScale)
		{
			iTween.ScaleTo(m_SquelchEmote, iTween.Hash("scale", startingScale * 1.1f, "time", 0.2f, "ignoretimescale", true));
		}

		public void MouseOutSquelch(Vector3 startingScale)
		{
			iTween.ScaleTo(m_SquelchEmote, iTween.Hash("scale", startingScale, "time", 0.2f, "ignoretimescale", true));
		}
	}

	[SerializeField]
	private EnemyEmote m_emote1;

	[SerializeField]
	private EnemyEmote m_emote2;

	[SerializeField]
	private string m_SquelchStringTag;

	[SerializeField]
	private string m_UnsquelchStringTag;

	[SerializeField]
	private string m_squelchAllStringTag;

	[SerializeField]
	private string m_unsquelchAllStringTag;

	[SerializeField]
	private string m_squelchTeammateStringTag;

	[SerializeField]
	private string m_unsquelchTeamamteStringTag;

	[SerializeField]
	private string m_squelchOpponentsStringTag;

	[SerializeField]
	private string m_unsquelchOpponentsStringTag;

	private static EnemyEmoteHandler s_instance;

	private Vector3 m_squelchEmoteStartingScale;

	private bool m_emotesShown;

	private int m_shownAtFrame;

	private Map<int, bool> m_squelched;

	private const int PLAYERS_IN_BATTLEGROUNDS = 8;

	private void Awake()
	{
		s_instance = this;
		GetComponent<Collider>().enabled = false;
		m_squelchEmoteStartingScale = m_emote1.m_SquelchEmote.transform.localScale;
		m_squelched = new Map<int, bool>(8);
		for (int i = 0; i < 8; i++)
		{
			m_squelched.Add(i + 1, value: false);
		}
		InitEnemyEmote(m_emote1);
		InitEnemyEmote(m_emote2);
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static EnemyEmoteHandler Get()
	{
		return s_instance;
	}

	public bool AreEmotesActive()
	{
		return m_emotesShown;
	}

	public bool IsSquelched(int playerId)
	{
		if (m_squelched.ContainsKey(playerId))
		{
			return m_squelched[playerId];
		}
		return false;
	}

	private bool AnySquelched()
	{
		foreach (bool value in m_squelched.Values)
		{
			if (value)
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyOpponentSquelched()
	{
		int currentPlayerId = GameState.Get().GetCurrentPlayer().GetPlayerId();
		int teamMateID = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		foreach (KeyValuePair<int, bool> squelched in m_squelched)
		{
			if (squelched.Value && teamMateID != squelched.Key && currentPlayerId != squelched.Key)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTeamamteSquelched()
	{
		int teamMateID = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		if (m_squelched.TryGetValue(teamMateID, out var isSquelched))
		{
			return isSquelched;
		}
		return false;
	}

	private string GetEmoteString()
	{
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			if (!AnyOpponentSquelched())
			{
				return m_squelchOpponentsStringTag;
			}
			return m_unsquelchOpponentsStringTag;
		}
		if (GameMgr.Get().IsBattlegrounds())
		{
			if (!AnySquelched())
			{
				return m_squelchAllStringTag;
			}
			return m_unsquelchAllStringTag;
		}
		if (!AnySquelched())
		{
			return m_SquelchStringTag;
		}
		return m_UnsquelchStringTag;
	}

	public void ShowEmotes()
	{
		if (!m_emotesShown)
		{
			m_emotesShown = true;
			GetComponent<Collider>().enabled = true;
			m_shownAtFrame = Time.frameCount;
			ShowEnemyEmote(m_emote1, GetEmoteString());
			if (GameMgr.Get().IsBattlegroundDuoGame())
			{
				ShowEnemyEmote(m_emote2, IsTeamamteSquelched() ? m_unsquelchTeamamteStringTag : m_squelchTeammateStringTag);
			}
		}
	}

	public void HideEmotes()
	{
		if (m_emotesShown)
		{
			m_emotesShown = false;
			GetComponent<Collider>().enabled = false;
			HideEnemyEmote(m_emote1);
			if (GameMgr.Get().IsBattlegroundDuoGame())
			{
				HideEnemyEmote(m_emote2);
			}
		}
	}

	public void HandleInput()
	{
		if (!HitTestEmotes(out var hitInfo))
		{
			HideEmotes();
			return;
		}
		GameObject mousedOverObject = hitInfo.transform.gameObject;
		UpdateEnemyEmoteInput(m_emote1, mousedOverObject);
		UpdateEnemyEmoteInput(m_emote2, mousedOverObject);
	}

	public bool IsMouseOverEmoteOption()
	{
		if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.Default.LayerBit(), out var hitInfo) && (hitInfo.transform.gameObject == m_emote1.m_SquelchEmote || hitInfo.transform.gameObject == m_emote2.m_SquelchEmote))
		{
			return true;
		}
		return false;
	}

	private void DoSquelchClick(EnemyEmote enemyEmote)
	{
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			int currentPlayerId = GameState.Get().GetCurrentPlayer().GetPlayerId();
			int teamMateID = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
			if (enemyEmote.m_SquelchTeammate)
			{
				foreach (int playerId in m_squelched.Keys.ToList())
				{
					if (playerId == teamMateID)
					{
						m_squelched[playerId] = !m_squelched[playerId];
					}
				}
			}
			else
			{
				foreach (int playerId2 in m_squelched.Keys.ToList())
				{
					if (playerId2 != currentPlayerId && playerId2 != teamMateID)
					{
						m_squelched[playerId2] = !m_squelched[playerId2];
					}
				}
			}
		}
		else if (GameMgr.Get().IsBattlegrounds())
		{
			int currentPlayerId2 = GameState.Get().GetCurrentPlayer().GetPlayerId();
			foreach (int playerId3 in m_squelched.Keys.ToList())
			{
				if (playerId3 != currentPlayerId2)
				{
					m_squelched[playerId3] = !m_squelched[playerId3];
				}
			}
		}
		else
		{
			m_squelched[GameState.Get().GetOpposingPlayerId()] = !m_squelched[GameState.Get().GetOpposingPlayerId()];
		}
		HideEmotes();
	}

	private bool HitTestEmotes(out RaycastHit hitInfo)
	{
		if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out hitInfo))
		{
			return false;
		}
		if (IsMousedOverHero(hitInfo))
		{
			return true;
		}
		if (IsMousedOverSelf(hitInfo))
		{
			return true;
		}
		if (IsMousedOverEmote(hitInfo))
		{
			return true;
		}
		return false;
	}

	private bool IsMousedOverHero(RaycastHit cardHitInfo)
	{
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(cardHitInfo.transform);
		if (actor == null)
		{
			return false;
		}
		Card card = actor.GetCard();
		if (card == null)
		{
			return false;
		}
		if (card.GetEntity().IsHero())
		{
			return true;
		}
		return false;
	}

	private bool IsMousedOverSelf(RaycastHit cardHitInfo)
	{
		return GetComponent<Collider>() == cardHitInfo.collider;
	}

	private bool IsMousedOverEmote(RaycastHit cardHitInfo)
	{
		if (cardHitInfo.transform == m_emote1.m_SquelchEmote.transform || cardHitInfo.transform == m_emote2.m_SquelchEmote.transform)
		{
			return true;
		}
		return false;
	}

	private void InitEnemyEmote(EnemyEmote emote)
	{
		emote.m_SquelchEmoteText.gameObject.SetActive(value: false);
		emote.m_SquelchEmoteBackplate.enabled = false;
		emote.m_SquelchEmote.transform.localScale = Vector3.zero;
	}

	private void ShowEnemyEmote(EnemyEmote emote, string stringTag)
	{
		emote.m_SquelchEmoteText.Text = GameStrings.Get(stringTag);
		emote.m_SquelchEmoteBackplate.enabled = true;
		emote.m_SquelchEmoteText.gameObject.SetActive(value: true);
		emote.m_SquelchEmote.GetComponent<Collider>().enabled = true;
		iTween.Stop(emote.m_SquelchEmote);
		iTween.ScaleTo(emote.m_SquelchEmote, iTween.Hash("scale", m_squelchEmoteStartingScale, "time", 0.5f, "ignoretimescale", true, "easetype", iTween.EaseType.easeOutElastic));
	}

	private void HideEnemyEmote(EnemyEmote emote)
	{
		emote.SetMousedOver(mousedOver: false);
		emote.m_SquelchEmote.GetComponent<Collider>().enabled = false;
		iTween.Stop(emote.m_SquelchEmote);
		iTween.ScaleTo(emote.m_SquelchEmote, iTween.Hash("scale", Vector3.zero, "time", 0.1f, "ignoretimescale", true, "easetype", iTween.EaseType.linear, "oncompletetarget", base.gameObject, "oncomplete", "FinishDisable"));
	}

	private void UpdateEnemyEmoteInput(EnemyEmote enemyEmote, GameObject mousedOverObject)
	{
		if (mousedOverObject != enemyEmote.m_SquelchEmote)
		{
			if (enemyEmote.IsMousedOver())
			{
				enemyEmote.MouseOutSquelch(m_squelchEmoteStartingScale);
				enemyEmote.SetMousedOver(mousedOver: false);
			}
		}
		else if (!enemyEmote.IsMousedOver())
		{
			enemyEmote.SetMousedOver(mousedOver: true);
			enemyEmote.MouseOverSquelch(m_squelchEmoteStartingScale);
		}
		if (InputCollection.GetMouseButtonUp(0))
		{
			if (enemyEmote.IsMousedOver())
			{
				DoSquelchClick(enemyEmote);
			}
			else if (UniversalInputManager.Get().IsTouchMode() && Time.frameCount != m_shownAtFrame)
			{
				HideEnemyEmote(enemyEmote);
			}
		}
	}
}
