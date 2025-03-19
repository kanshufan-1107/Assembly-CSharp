using System.Collections.Generic;
using PegasusGame;

public class PowerTask
{
	public delegate void TaskCompleteCallback();

	private Network.PowerHistory m_power;

	private bool m_completed;

	private TaskCompleteCallback m_onCompleted;

	public Network.PowerHistory GetPower()
	{
		return m_power;
	}

	public void SetPower(Network.PowerHistory power)
	{
		m_power = power;
	}

	public bool IsCompleted()
	{
		return m_completed;
	}

	public void SetCompleted(bool complete)
	{
		m_completed = complete;
		if (m_completed && m_onCompleted != null)
		{
			m_onCompleted();
		}
	}

	public void SetTaskCompleteCallback(TaskCompleteCallback onComplete)
	{
		m_onCompleted = onComplete;
	}

	private bool IsZoneTransition(TAG_ZONE fromZone, TAG_ZONE toZone)
	{
		if (IsCompleted())
		{
			return false;
		}
		Network.PowerHistory power = GetPower();
		if (power.Type == Network.PowerType.SHOW_ENTITY)
		{
			Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
			Entity entity = GameState.Get().GetEntity(showEntity.Entity.ID);
			Network.Entity.Tag tag = showEntity.Entity.Tags.Find((Network.Entity.Tag currTag) => currTag.Name == 49);
			if (entity != null && tag != null && entity.GetZone() == fromZone && tag.Value == (int)toZone)
			{
				return true;
			}
		}
		if (power.Type == Network.PowerType.TAG_CHANGE)
		{
			Network.HistTagChange tagChange = (Network.HistTagChange)power;
			Entity entity2 = GameState.Get().GetEntity(tagChange.Entity);
			if (entity2 != null && tagChange.Tag == 49 && entity2.GetZone() == fromZone && tagChange.Value == (int)toZone)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCardDraw()
	{
		return IsZoneTransition(TAG_ZONE.DECK, TAG_ZONE.HAND);
	}

	public bool IsCardMill()
	{
		return IsZoneTransition(TAG_ZONE.DECK, TAG_ZONE.GRAVEYARD);
	}

	public bool IsFatigue()
	{
		if (IsCompleted())
		{
			return false;
		}
		Network.PowerHistory power = GetPower();
		if (power.Type == Network.PowerType.BLOCK_START)
		{
			return ((Network.HistBlockStart)power).BlockType == HistoryBlock.Type.FATIGUE;
		}
		return false;
	}

	public void DoRealTimeTask(List<Network.PowerHistory> powerList, int index)
	{
		GameState state = GameState.Get();
		switch (m_power.Type)
		{
		case Network.PowerType.CREATE_GAME:
		{
			Network.HistCreateGame createGame = (Network.HistCreateGame)m_power;
			state.OnRealTimeCreateGame(powerList, index, createGame);
			break;
		}
		case Network.PowerType.FULL_ENTITY:
		{
			Network.HistFullEntity fullEntity = (Network.HistFullEntity)m_power;
			state.OnRealTimeFullEntity(fullEntity);
			break;
		}
		case Network.PowerType.SHOW_ENTITY:
		{
			Network.HistShowEntity showEntity = (Network.HistShowEntity)m_power;
			state.OnRealTimeShowEntity(showEntity);
			break;
		}
		case Network.PowerType.CHANGE_ENTITY:
		{
			Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)m_power;
			state.OnRealTimeChangeEntity(powerList, index, changeEntity);
			break;
		}
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange tagChange = (Network.HistTagChange)m_power;
			state.OnRealTimeTagChange(tagChange);
			break;
		}
		case Network.PowerType.RESET_GAME:
		{
			Network.HistResetGame resetGame = (Network.HistResetGame)m_power;
			state.OnRealTimeResetGame(resetGame);
			break;
		}
		case Network.PowerType.VO_SPELL:
		{
			Network.HistVoSpell voSpell = (Network.HistVoSpell)m_power;
			state.OnRealTimeVoSpell(voSpell);
			break;
		}
		case Network.PowerType.HIDE_ENTITY:
		case Network.PowerType.BLOCK_START:
		case Network.PowerType.BLOCK_END:
		case Network.PowerType.META_DATA:
		case Network.PowerType.SUB_SPELL_START:
		case Network.PowerType.SUB_SPELL_END:
			break;
		}
	}

	public void DoTask()
	{
		if (!m_completed)
		{
			GameState state = GameState.Get();
			switch (m_power.Type)
			{
			case Network.PowerType.FULL_ENTITY:
			{
				Network.HistFullEntity fullEntity = (Network.HistFullEntity)m_power;
				state.OnFullEntity(fullEntity);
				HistoryManager.Get().OnEntityRevealed();
				break;
			}
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)m_power;
				state.OnShowEntity(showEntity);
				HistoryManager.Get().OnEntityRevealed();
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)m_power;
				state.OnHideEntity(hideEntity);
				break;
			}
			case Network.PowerType.CHANGE_ENTITY:
			{
				Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)m_power;
				state.OnChangeEntity(changeEntity);
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)m_power;
				state.OnTagChange(tagChange);
				break;
			}
			case Network.PowerType.META_DATA:
			{
				Network.HistMetaData metaData = (Network.HistMetaData)m_power;
				state.OnMetaData(metaData);
				break;
			}
			case Network.PowerType.RESET_GAME:
			{
				Network.HistResetGame resetGame = (Network.HistResetGame)m_power;
				state.OnResetGame(resetGame);
				break;
			}
			case Network.PowerType.VO_SPELL:
			{
				Network.HistVoSpell voSpell = (Network.HistVoSpell)m_power;
				state.OnVoSpell(voSpell);
				break;
			}
			case Network.PowerType.VO_BANTER:
			{
				Network.HistVoBanter voBanter = (Network.HistVoBanter)m_power;
				state.OnVoBanter(voBanter);
				break;
			}
			case Network.PowerType.CACHED_TAG_FOR_DORMANT_CHANGE:
			{
				Network.HistCachedTagForDormantChange cachedTagForDormantChange = (Network.HistCachedTagForDormantChange)m_power;
				state.OnCachedTagForDormantChange(cachedTagForDormantChange);
				break;
			}
			case Network.PowerType.SHUFFLE_DECK:
			{
				Network.HistShuffleDeck shuffleDeck = (Network.HistShuffleDeck)m_power;
				state.OnShuffleDeck(shuffleDeck);
				break;
			}
			case Network.PowerType.TAG_LIST_CHANGE:
			{
				Network.HistTagListChange tagListChange = (Network.HistTagListChange)m_power;
				state.OnTagListChange(tagListChange);
				break;
			}
			}
			SetCompleted(complete: true);
		}
	}

	public void DoEarlyConcedeTask()
	{
		if (!m_completed)
		{
			GameState state = GameState.Get();
			switch (m_power.Type)
			{
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)m_power;
				state.OnEarlyConcedeShowEntity(showEntity);
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)m_power;
				state.OnEarlyConcedeHideEntity(hideEntity);
				break;
			}
			case Network.PowerType.CHANGE_ENTITY:
			{
				Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)m_power;
				state.OnEarlyConcedeChangeEntity(changeEntity);
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)m_power;
				state.OnEarlyConcedeTagChange(tagChange);
				break;
			}
			}
			m_completed = true;
		}
	}

	public override string ToString()
	{
		string powerString = "null";
		if (m_power != null)
		{
			switch (m_power.Type)
			{
			case Network.PowerType.CREATE_GAME:
				powerString = ((Network.HistCreateGame)m_power).ToString();
				break;
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)m_power;
				powerString = string.Format("type={0} entity={1} {2} {3}", m_power.Type, GetPrintableEntity(tagChange.Entity), Tags.DebugTag(tagChange.Tag, tagChange.Value), tagChange.ChangeDef ? "DEF CHANGE" : "");
				break;
			}
			case Network.PowerType.FULL_ENTITY:
			{
				Network.HistFullEntity fullEntity = (Network.HistFullEntity)m_power;
				powerString = $"type={m_power.Type} entity={GetPrintableEntity(fullEntity.Entity)} tags={fullEntity.Entity.Tags}";
				break;
			}
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)m_power;
				powerString = $"type={m_power.Type} entity={GetPrintableEntity(showEntity.Entity)} tags={showEntity.Entity.Tags}";
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)m_power;
				powerString = $"type={m_power.Type} entity={GetPrintableEntity(hideEntity.Entity)} zone={hideEntity.Zone}";
				break;
			}
			case Network.PowerType.CHANGE_ENTITY:
			{
				Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)m_power;
				powerString = $"type={m_power.Type} entity={GetPrintableEntity(changeEntity.Entity)} tags={changeEntity.Entity.Tags}";
				break;
			}
			case Network.PowerType.META_DATA:
				powerString = ((Network.HistMetaData)m_power).ToString();
				break;
			}
		}
		return $"power=[{powerString}] complete={m_completed}";
	}

	private string GetEntityLogName(Entity entity)
	{
		if (entity == null)
		{
			return null;
		}
		string name = entity.GetName();
		if (entity.IsPlayer())
		{
			BnetPlayer bnetPlayer = (entity as Player).GetBnetPlayer();
			if (bnetPlayer != null && bnetPlayer.GetBattleTag() != null)
			{
				name = bnetPlayer.GetBattleTag().GetName();
			}
		}
		return name;
	}

	private string GetPrintableEntity(int entityId)
	{
		Entity entity = GameState.Get().GetEntity(entityId);
		if (entity == null)
		{
			return entityId.ToString();
		}
		string name = GetEntityLogName(entity);
		if (name == null)
		{
			return $"[id={entityId} cardId={entity.GetCardId()}]";
		}
		return $"[id={entityId} cardId={entity.GetCardId()} name={name}]";
	}

	private string GetPrintableEntity(Network.Entity netEntity)
	{
		Entity entity = GameState.Get().GetEntity(netEntity.ID);
		string name = GetEntityLogName(entity);
		if (name == null)
		{
			return $"[id={netEntity.ID} cardId={netEntity.CardID}]";
		}
		return $"[id={netEntity.ID} cardId={netEntity.CardID} name={name}]";
	}
}
