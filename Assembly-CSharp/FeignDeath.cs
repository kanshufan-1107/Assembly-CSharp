using System.Collections;
using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

public class FeignDeath : SuperSpell
{
	public GameObject m_RootObject;

	public GameObject m_Glow;

	public float m_Height = 1f;

	protected override void Awake()
	{
		base.Awake();
		m_RootObject.SetActive(value: false);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		if (!m_taskList.IsStartOfBlock())
		{
			base.OnAction(prevStateType);
			return;
		}
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		m_targets.Clear();
		for (PowerTaskList taskList = m_taskList; taskList != null; taskList = taskList.GetNext())
		{
			foreach (PowerTask task in taskList.GetTaskList())
			{
				if (!(task.GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.TARGET } metaData))
				{
					continue;
				}
				foreach (int entityId in metaData.Info)
				{
					Card targetCard = GameState.Get().GetEntity(entityId).GetCard();
					m_targets.Add(targetCard.gameObject);
				}
			}
		}
		StartCoroutine(ActionVisual());
	}

	private IEnumerator ActionVisual()
	{
		List<GameObject> fxObjects = new List<GameObject>();
		foreach (GameObject target in m_targets)
		{
			GameObject fx = Object.Instantiate(m_RootObject);
			fx.SetActive(value: true);
			fxObjects.Add(fx);
			fx.transform.position = target.transform.position;
			fx.transform.position = new Vector3(fx.transform.position.x, fx.transform.position.y + m_Height, fx.transform.position.z);
			ParticleSystem[] componentsInChildren = fx.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
		yield return new WaitForSeconds(1f);
		foreach (GameObject item in fxObjects)
		{
			Object.Destroy(item);
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
