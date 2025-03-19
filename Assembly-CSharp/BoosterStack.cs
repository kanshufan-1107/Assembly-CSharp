using System;
using System.Collections.Generic;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class BoosterStack : MonoBehaviour
{
	protected enum BoosterEvent
	{
		INTRO,
		OUTRO,
		INSTANT_INTRO,
		INSTANT_OUTRO
	}

	[SerializeField]
	protected Vector3 m_incrementalDisplacement;

	[SerializeField]
	protected int m_stackSize;

	[SerializeField]
	protected GameObject m_boosterContainer;

	[SerializeField]
	protected float m_stackingDelay;

	[SerializeField]
	protected float m_stackingBaseDuration = 0.1f;

	[SerializeField]
	protected float m_stackingIncrementalDuration = 0.02f;

	[SerializeField]
	protected string m_boosterIntroEvent;

	[SerializeField]
	protected string m_boosterOutroEvent;

	[SerializeField]
	protected string m_boosterInstantIntroEvent;

	[SerializeField]
	protected string m_boosterInstantOutroEvent;

	private float m_playTime;

	private float m_startTime;

	private float m_endTime;

	private int m_startingStackSize;

	private int m_currentStackSize;

	private int m_targetStackSize;

	private List<GameObject> m_boosters = new List<GameObject>();

	private bool m_instantIntro;

	[Overridable]
	public float StackingDelay
	{
		get
		{
			return m_stackingDelay;
		}
		set
		{
			m_stackingDelay = value;
		}
	}

	[Overridable]
	public float StackingBaseDuration
	{
		get
		{
			return m_stackingBaseDuration;
		}
		set
		{
			m_stackingBaseDuration = value;
		}
	}

	[Overridable]
	public float StackingIncrementalDuration
	{
		get
		{
			return m_stackingIncrementalDuration;
		}
		set
		{
			m_stackingIncrementalDuration = value;
		}
	}

	public int CurrentStackSize => m_currentStackSize;

	private void Start()
	{
		foreach (Transform child in m_boosterContainer.transform)
		{
			int idx = m_boosters.Count;
			m_boosters.Add(child.gameObject);
			Widget widget = child.GetComponent<Widget>();
			if (widget != null && !widget.IsReady)
			{
				widget.RegisterReadyListener(delegate
				{
					OnBoosterReady(idx);
				});
			}
			else
			{
				OnBoosterReady(idx);
			}
		}
	}

	private void Update()
	{
		if (IsSettled())
		{
			return;
		}
		m_playTime += Time.deltaTime;
		int newStackSize = m_currentStackSize;
		if (!(m_playTime < m_startTime))
		{
			if (m_playTime < m_endTime && !Mathf.Approximately(m_endTime, m_startTime))
			{
				int firstStep = m_startingStackSize + Math.Sign(m_targetStackSize - m_startingStackSize);
				float progress = (m_playTime - m_startTime) / (m_endTime - m_startTime);
				newStackSize = firstStep + (int)(progress * (float)(m_targetStackSize - firstStep));
			}
			else
			{
				newStackSize = m_targetStackSize;
				m_playTime = (m_endTime = (m_startTime = 0f));
			}
			if (newStackSize > m_currentStackSize)
			{
				PlayEventAcrossRange(BoosterEvent.INTRO, m_currentStackSize, newStackSize - 1);
			}
			else if (newStackSize < m_currentStackSize)
			{
				PlayEventAcrossRange(BoosterEvent.OUTRO, m_currentStackSize - 1, newStackSize);
			}
			m_currentStackSize = newStackSize;
		}
	}

	private void Awake()
	{
		SetStacks(m_targetStackSize, m_instantIntro);
	}

	public bool IsSettled()
	{
		return m_currentStackSize == m_targetStackSize;
	}

	public void SetStacks(int stackSize, bool instantaneous = true)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			m_targetStackSize = stackSize;
			m_instantIntro = instantaneous;
		}
		else if (instantaneous)
		{
			m_targetStackSize = stackSize;
			if (stackSize > m_currentStackSize)
			{
				PlayEventAcrossRange(BoosterEvent.INSTANT_INTRO, m_currentStackSize, stackSize - 1);
			}
			else if (stackSize < m_currentStackSize)
			{
				PlayEventAcrossRange(BoosterEvent.INSTANT_OUTRO, m_currentStackSize - 1, stackSize);
			}
			m_currentStackSize = m_targetStackSize;
		}
		else
		{
			AddStacks(stackSize - m_targetStackSize);
		}
	}

	public void AddStacks(int deltaStacks)
	{
		deltaStacks = Math.Max(deltaStacks, -m_targetStackSize);
		float stackingTime = (float)Math.Abs(deltaStacks) * StackingIncrementalDuration;
		if (IsSettled())
		{
			m_playTime = 0f;
			m_startTime = StackingDelay;
			m_endTime = m_startTime + StackingBaseDuration + stackingTime;
			m_startingStackSize = m_currentStackSize;
		}
		else
		{
			if (deltaStacks > 0 != m_targetStackSize > m_currentStackSize)
			{
				m_endTime = (m_startTime = (m_playTime = 0f));
				m_startingStackSize = m_currentStackSize;
				stackingTime = (float)Math.Abs(deltaStacks + m_targetStackSize - m_currentStackSize) * StackingIncrementalDuration;
			}
			m_endTime += stackingTime;
		}
		m_targetStackSize += deltaStacks;
		if (m_currentStackSize == m_targetStackSize)
		{
			m_endTime = (m_startTime = 0f);
		}
	}

	protected void PlayEventAcrossRange(BoosterEvent ev, int startIdx, int endIdx)
	{
		int i = Math.Min(startIdx, endIdx);
		for (int max = Math.Max(startIdx, endIdx); i <= max; i++)
		{
			PlayEvent(ev, i);
		}
	}

	protected void PlayEvent(BoosterEvent ev, int atIndex)
	{
		if (atIndex >= m_boosters.Count)
		{
			Log.Store.PrintError("BoosterStack::PlayEvent index {0} out of range (max: {1})", atIndex, m_boosters.Count - 1);
			return;
		}
		GameObject booster = m_boosters[atIndex];
		booster.transform.localPosition = m_incrementalDisplacement * (atIndex - 1);
		Widget widget = booster.GetComponent<Widget>();
		if (widget != null && !widget.IsReady)
		{
			booster.SetActive(value: true);
			return;
		}
		PlayMakerFSM playMaker = booster.GetComponentInChildren<PlayMakerFSM>();
		if (playMaker == null)
		{
			Log.Store.PrintError("No PlayMakerFSM found on booster {0} in BoosterStack {1}!", booster, this);
			return;
		}
		switch (ev)
		{
		case BoosterEvent.INTRO:
			playMaker.SendEvent(m_boosterIntroEvent);
			break;
		case BoosterEvent.OUTRO:
			playMaker.SendEvent(m_boosterOutroEvent);
			break;
		case BoosterEvent.INSTANT_INTRO:
			playMaker.SendEvent(m_boosterInstantIntroEvent);
			break;
		case BoosterEvent.INSTANT_OUTRO:
			playMaker.SendEvent(m_boosterInstantOutroEvent);
			break;
		}
	}

	protected void OnBoosterReady(int idx)
	{
		if (idx >= m_currentStackSize)
		{
			PlayEvent(BoosterEvent.INSTANT_OUTRO, idx);
		}
		else if (idx == m_currentStackSize - 1)
		{
			PlayEvent(BoosterEvent.INTRO, idx);
		}
		else
		{
			PlayEvent(BoosterEvent.INSTANT_INTRO, idx);
		}
	}
}
