using System;
using System.Collections.Generic;
using Assets;
using UnityEngine;

[CustomEditClass]
public class EventOverride : MonoBehaviour
{
	[Serializable]
	public class EventOverrideElement
	{
		public EventTimingType EventType;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string EventPrefab;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string EventPrefab_phone;

		public GameObject Parent;

		public bool showToReturningPlayers;

		public bool showToNewPlayers;
	}

	public List<EventOverrideElement> m_SpecialEvents;

	private List<Spell> m_ActiveSpells { get; set; }

	private void Start()
	{
		m_ActiveSpells = new List<Spell>();
		if (GameMgr.Get().IsTraditionalTutorial())
		{
			return;
		}
		foreach (EventOverrideElement specialEvent in m_SpecialEvents)
		{
			if (EventTimingManager.Get().IsEventActive(specialEvent.EventType))
			{
				LoadSpecialEvent(specialEvent);
			}
		}
	}

	public virtual void LoadSpecialEvent(EventOverrideElement specialEvent)
	{
		if (!EventTimingManager.Get().IsEventForcedActive(specialEvent.EventType) && ((!specialEvent.showToNewPlayers && !AchieveManager.Get().HasUnlockedFeature(Achieve.Unlocks.DAILY)) || (!specialEvent.showToReturningPlayers && ReturningPlayerMgr.Get().IsInReturningPlayerMode)))
		{
			return;
		}
		string prefabPath = specialEvent.EventPrefab;
		if (PlatformSettings.Screen == ScreenCategory.Phone && !string.IsNullOrEmpty(specialEvent.EventPrefab_phone))
		{
			prefabPath = specialEvent.EventPrefab_phone;
		}
		if (string.IsNullOrEmpty(prefabPath))
		{
			return;
		}
		GameObject eventGameObject = AssetLoader.Get().InstantiatePrefab(prefabPath);
		if (eventGameObject == null)
		{
			Debug.LogWarning($"Failed to load special event prefab: {prefabPath}");
			return;
		}
		eventGameObject.transform.SetParent(base.transform, worldPositionStays: false);
		if (specialEvent.Parent != null)
		{
			eventGameObject.transform.SetParent(specialEvent.Parent.transform, worldPositionStays: true);
		}
		Spell activeSpecialEvent = eventGameObject.GetComponent<Spell>();
		if (activeSpecialEvent != null)
		{
			activeSpecialEvent.ActivateState(SpellStateType.BIRTH);
			m_ActiveSpells.Add(activeSpecialEvent);
		}
	}

	private void OnDisable()
	{
		foreach (Spell activeSpell in m_ActiveSpells)
		{
			if (activeSpell != null && activeSpell.gameObject.activeSelf && activeSpell.IsActive())
			{
				activeSpell.ActivateState(SpellStateType.DEATH);
			}
		}
		m_ActiveSpells.Clear();
	}
}
