using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Get the GameObjects for each Card's Actor in current choice in left-to-right order. Requires the Spell to be extended from SpawnToDeckSpell.")]
public class SpellGetSpawnToDeckActorsAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray m_SpawnToDeckActorGameObjects;

	[Tooltip("Whether it appears or not after loading.")]
	public bool m_Show;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_SpawnToDeckActorGameObjects = null;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		Spell spell = GetSpell();
		if (spell == null)
		{
			global::Log.Spells.PrintError("{0}.OnUpdate(): Unable to find Spell.", this);
			Finish();
			return;
		}
		SpawnToDeckSpell spawnToDeckSpellSpell = spell as SpawnToDeckSpell;
		if (spawnToDeckSpellSpell == null)
		{
			global::Log.Spells.PrintError("{0}.OnUpdate(): Spell {1} is not extended from SpawnToDeckSpell.", this, spell);
			Finish();
		}
		else
		{
			if (!spawnToDeckSpellSpell.m_finishedLoading)
			{
				return;
			}
			if (!m_SpawnToDeckActorGameObjects.IsNone)
			{
				List<Actor> spawnedActors = spawnToDeckSpellSpell.GetLoadedActors();
				GameObject[] actorGameObjects = new GameObject[spawnedActors.Count];
				for (int cardIndex = 0; cardIndex < spawnedActors.Count; cardIndex++)
				{
					Actor actor = spawnedActors[cardIndex];
					if (actor == null)
					{
						global::Log.Spells.PrintError("{0}.OnUpdate(): Spawned card {1} doesn't have an actor!", this, spawnedActors[cardIndex]);
						Finish();
						return;
					}
					if (m_Show)
					{
						actor.Show();
					}
					else
					{
						actor.Hide();
					}
					actorGameObjects[cardIndex] = actor.gameObject;
				}
				FsmArray spawnToDeckActorGameObjects = m_SpawnToDeckActorGameObjects;
				object[] values = actorGameObjects;
				spawnToDeckActorGameObjects.Values = values;
			}
			Finish();
		}
	}
}
