using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get the Reward card actor's GameObjects for each Card in current friendly player choice in left-to-right order.")]
[ActionCategory("Pegasus")]
public class GetChoiceCardRewardActorsAction : FsmStateAction
{
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray m_ChoiceCardRewardActorGameObjects;

	public override void Reset()
	{
		m_ChoiceCardRewardActorGameObjects = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Player friendlyPlayer = GameState.Get()?.GetFriendlySidePlayer();
		if (friendlyPlayer == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Friendly player is nullptr", this);
			Finish();
			return;
		}
		ChoiceCardMgr.ChoiceState choiceState = ChoiceCardMgr.Get()?.GetChoiceStateForPlayer(friendlyPlayer.GetPlayerId());
		if (choiceState == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Choice Card Manager doesn't have a valid choice state for friendly player", this);
			Finish();
			return;
		}
		if (!m_ChoiceCardRewardActorGameObjects.IsNone)
		{
			List<Card> choiceCards = choiceState.m_cards;
			GameObject[] actorGameObjects = new GameObject[choiceCards.Count];
			for (int cardIndex = 0; cardIndex < choiceCards.Count; cardIndex++)
			{
				Entity choiceQuestEntity = choiceCards[cardIndex].GetEntity();
				if (choiceQuestEntity == null)
				{
					global::Log.Spells.PrintError("{0}.OnEnter(): Choice card {1} doesn't have an entity!", this, choiceCards[cardIndex]);
					Finish();
					return;
				}
				Actor rewardActor = choiceQuestEntity.GetCard()?.GetQuestRewardActor();
				if (rewardActor == null)
				{
					global::Log.Spells.PrintError("{0}.OnEnter(): Quest Entity {1} doesn't have a reward actor set!", this, choiceQuestEntity);
					Finish();
					return;
				}
				actorGameObjects[cardIndex] = rewardActor.gameObject;
			}
			FsmArray choiceCardRewardActorGameObjects = m_ChoiceCardRewardActorGameObjects;
			object[] values = actorGameObjects;
			choiceCardRewardActorGameObjects.Values = values;
		}
		Finish();
	}
}
