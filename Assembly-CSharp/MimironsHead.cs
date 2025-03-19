using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class MimironsHead : SuperSpell
{
	public GameObject m_root;

	public GameObject m_highPosBone;

	public GameObject m_minionPosBone;

	public GameObject m_background;

	public GameObject m_minionElectricity;

	public GameObject m_minionGlow;

	public GameObject m_mimironNegative;

	public GameObject m_mimironFlare;

	public GameObject m_mimironGlow;

	public GameObject m_mimironElectricity;

	public Spell m_voltSpawnOverrideSpell;

	public string m_perMinionSound;

	public string[] m_startSounds;

	private Card m_volt;

	private Card m_mimiron;

	private List<Card> m_mechMinions = new List<Card>();

	private Transform m_voltParent;

	private Color m_clear = new Color(1f, 1f, 1f, 0f);

	private Map<GameObject, List<GameObject>> m_cleanup = new Map<GameObject, List<GameObject>>();

	private bool m_isNegFlash;

	private float m_flashDelay = 0.15f;

	private float m_mimironHighTime = 1.5f;

	private float m_minionHighTime = 2f;

	private float m_sparkDelay = 0.3f;

	private float m_absorbTime = 1f;

	private float m_glowTime = 0.5f;

	private PowerTaskList m_waitForTaskList;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		Card sourceCard = m_taskList.GetSourceEntity().GetCard();
		if (m_taskList.IsOrigin())
		{
			List<PowerTaskList> allTaskLists = new List<PowerTaskList>();
			for (PowerTaskList list = m_taskList; list != null; list = list.GetNext())
			{
				allTaskLists.Add(list);
			}
			foreach (PowerTaskList taskList in allTaskLists)
			{
				foreach (PowerTask task in taskList.GetTaskList())
				{
					Network.PowerHistory power = task.GetPower();
					if (power.Type == Network.PowerType.TAG_CHANGE)
					{
						Network.HistTagChange tagChange = power as Network.HistTagChange;
						if (tagChange.Tag == 360 && tagChange.Value == 1)
						{
							Entity entity = GameState.Get().GetEntity(tagChange.Entity);
							if (entity == null)
							{
								Debug.LogWarning($"{this}.AddPowerTargets() - WARNING trying to target entity with id {tagChange.Entity} but there is no entity with that id");
								continue;
							}
							Card targetCard = entity.GetCard();
							if (targetCard != sourceCard)
							{
								m_mechMinions.Add(targetCard);
							}
							else
							{
								m_mimiron = targetCard;
							}
							m_waitForTaskList = taskList;
						}
					}
					if (power.Type == Network.PowerType.FULL_ENTITY)
					{
						Network.Entity netEnt = (power as Network.HistFullEntity).Entity;
						Entity entity2 = GameState.Get().GetEntity(netEnt.ID);
						if (entity2 == null)
						{
							Debug.LogWarning($"{this}.AddPowerTargets() - WARNING trying to target entity with id {netEnt.ID} but there is no entity with that id");
						}
						else if (!(entity2.GetCardId() != "GVG_111t"))
						{
							Card targetCard2 = entity2.GetCard();
							m_volt = targetCard2;
							m_waitForTaskList = taskList;
						}
					}
				}
			}
			if (m_volt != null && m_mimiron != null && m_mechMinions.Count > 0)
			{
				m_mimiron.IgnoreDeath(ignore: true);
				foreach (Card mechMinion in m_mechMinions)
				{
					mechMinion.IgnoreDeath(ignore: true);
				}
				foreach (Card card in sourceCard.GetController().GetBattlefieldZone().GetCards())
				{
					card.SetDoNotSort(on: true);
				}
			}
			else
			{
				m_volt = null;
				m_mimiron = null;
				m_mechMinions.Clear();
			}
		}
		if (m_volt == null || m_mimiron == null || m_mechMinions.Count == 0 || m_taskList != m_waitForTaskList)
		{
			return false;
		}
		foreach (Card card2 in sourceCard.GetController().GetBattlefieldZone().GetCards())
		{
			card2.SetDoNotSort(on: true);
		}
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		if ((bool)m_voltSpawnOverrideSpell)
		{
			m_volt.OverrideCustomSpawnSpell(SpellManager.Get().GetSpell(m_voltSpawnOverrideSpell));
		}
		StartCoroutine(TransformEffect());
	}

	private IEnumerator TransformEffect()
	{
		string[] startSounds = m_startSounds;
		foreach (string sound in startSounds)
		{
			SoundManager.Get().LoadAndPlay(sound);
		}
		m_volt.SetDoNotSort(on: true);
		m_taskList.DoAllTasks();
		while (!m_taskList.IsComplete())
		{
			yield return null;
		}
		m_volt.GetActor().Hide();
		GameObject volt = m_volt.GetActor().gameObject;
		m_voltParent = volt.transform.parent;
		volt.transform.parent = m_highPosBone.transform;
		volt.transform.localPosition = new Vector3(0f, -0.1f, 0f);
		m_root.transform.parent = null;
		m_root.transform.localPosition = Vector3.zero;
		iTween.MoveTo(m_mimiron.gameObject, iTween.Hash("position", m_highPosBone.transform.localPosition, "easetype", iTween.EaseType.easeOutQuart, "time", m_mimironHighTime, "delay", 0.5f));
		yield return new WaitForSeconds(0.5f + m_mimironHighTime / 5f);
		TransformMinions();
	}

	private void TransformMinions()
	{
		float delay = 1f;
		Vector3 angleOffset = new Vector3(0f, 0f, 2.3f);
		List<int> availableIndexes = new List<int>();
		for (int i = 0; i < m_mechMinions.Count; i++)
		{
			availableIndexes.Add(i);
		}
		List<int> randomIndexes = new List<int>();
		for (int j = 0; j < m_mechMinions.Count; j++)
		{
			int availableIndex = UnityEngine.Random.Range(0, availableIndexes.Count);
			randomIndexes.Add(availableIndexes[availableIndex]);
			availableIndexes.RemoveAt(availableIndex);
		}
		for (int k = 0; k < m_mechMinions.Count; k++)
		{
			Vector3 roundOffset = Quaternion.Euler(0f, 360 / m_mechMinions.Count * randomIndexes[k] + 60, 0f) * angleOffset;
			m_minionPosBone.transform.localPosition = m_highPosBone.transform.localPosition + roundOffset;
			GameObject minion = m_mechMinions[k].GetActor().gameObject;
			float partOfDelay = delay / (float)m_mechMinions.Count * (float)k;
			StartCoroutine(MinionPlayFX(minion, m_minionElectricity, partOfDelay / 2f));
			List<Vector3> curvePoints = new List<Vector3>();
			Vector3 ranOffset = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0f, UnityEngine.Random.Range(-2f, 2f));
			curvePoints.Add(minion.transform.position + (m_minionPosBone.transform.localPosition - minion.transform.position) / 4f + ranOffset);
			curvePoints.Add(m_minionPosBone.transform.localPosition);
			if (k < m_mechMinions.Count - 1)
			{
				iTween.MoveTo(minion, iTween.Hash("path", curvePoints.ToArray(), "easetype", iTween.EaseType.easeInOutSine, "delay", partOfDelay, "time", m_minionHighTime / (float)m_mechMinions.Count));
				continue;
			}
			iTween.MoveTo(minion, iTween.Hash("path", curvePoints.ToArray(), "easetype", iTween.EaseType.easeInOutSine, "delay", partOfDelay, "time", m_minionHighTime / (float)m_mechMinions.Count, "oncomplete", (Action<object>)delegate
			{
				FadeInBackground();
			}));
		}
	}

	private IEnumerator MinionPlayFX(GameObject minion, GameObject FX, float delay)
	{
		GameObject minionFX = UnityEngine.Object.Instantiate(FX);
		minionFX.transform.parent = minion.transform;
		minionFX.transform.localPosition = new Vector3(0f, 0.5f, 0f);
		if (!m_cleanup.ContainsKey(minion))
		{
			m_cleanup.Add(minion, new List<GameObject>());
		}
		m_cleanup[minion].Add(minionFX);
		yield return new WaitForSeconds(delay);
		minionFX.GetComponent<ParticleSystem>().Play();
	}

	private IEnumerator MimironNegativeFX()
	{
		while (m_isNegFlash)
		{
			yield return new WaitForSeconds(m_flashDelay);
			m_mimironNegative.SetActive(!m_mimironNegative.activeSelf);
			if (m_flashDelay > 0.05f)
			{
				m_flashDelay -= 0.01f;
			}
		}
		m_mimironNegative.SetActive(value: false);
	}

	private void MinionCleanup(GameObject minion)
	{
		if (!m_cleanup.ContainsKey(minion))
		{
			return;
		}
		foreach (GameObject m in m_cleanup[minion])
		{
			if (m != null)
			{
				UnityEngine.Object.Destroy(m);
			}
		}
	}

	private void FadeInBackground()
	{
		m_background.SetActive(value: true);
		m_background.GetComponent<Renderer>().GetMaterial().SetColor("_Color", m_clear);
		HighlightState hi = m_volt.GetActor().gameObject.GetComponentInChildren<HighlightState>();
		if (hi != null)
		{
			hi.Hide();
		}
		iTween.ColorTo(m_background, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "a", 1f, "time", 0.5f, "oncomplete", (Action<object>)delegate
		{
			MimironPowerUp();
		}));
	}

	private void SetGlow(Material glowMat, float newVal, string colorVal = "_TintColor")
	{
		glowMat.SetColor(colorVal, Color.Lerp(m_clear, Color.white, newVal));
	}

	private void MimironPowerUp()
	{
		m_mimironElectricity.GetComponent<ParticleSystem>().Play();
		for (int i = 0; i < m_mechMinions.Count; i++)
		{
			GameObject minion = m_mechMinions[i].GetActor().gameObject;
			GameObject minionGlow = UnityEngine.Object.Instantiate(m_minionGlow);
			if (!m_cleanup.ContainsKey(minion))
			{
				m_cleanup.Add(minion, new List<GameObject>());
			}
			m_cleanup[minion].Add(minionGlow);
			minionGlow.transform.parent = minion.transform;
			minionGlow.transform.localPosition = new Vector3(0f, 0.5f, 0f);
			float partOfDelay = m_absorbTime / (float)m_mechMinions.Count * (float)i;
			Material rendererMaterial = minionGlow.GetComponent<Renderer>().GetMaterial();
			rendererMaterial.SetColor("_TintColor", m_clear);
			RenderUtils.EnableRenderers(minionGlow, enable: true);
			if (i < m_mechMinions.Count - 1)
			{
				iTween.ValueTo(minionGlow, iTween.Hash("from", 0f, "to", 1f, "time", m_glowTime, "delay", 0.1f + partOfDelay + m_sparkDelay, "onstart", (Action<object>)delegate
				{
					SoundManager.Get().LoadAndPlay(m_perMinionSound);
				}, "onupdate", (Action<object>)delegate(object newVal)
				{
					SetGlow(rendererMaterial, (float)newVal);
				}));
				iTween.ValueTo(minionGlow, iTween.Hash("from", 1f, "to", 0f, "time", m_glowTime, "delay", 0.1f + partOfDelay + m_sparkDelay + m_glowTime, "onupdate", (Action<object>)delegate(object newVal)
				{
					SetGlow(rendererMaterial, (float)newVal);
				}));
				continue;
			}
			iTween.ValueTo(minionGlow, iTween.Hash("from", 0f, "to", 1f, "time", m_glowTime, "delay", 0.1f + partOfDelay + m_sparkDelay, "onstart", (Action<object>)delegate
			{
				SoundManager.Get().LoadAndPlay(m_perMinionSound);
			}, "onupdate", (Action<object>)delegate(object newVal)
			{
				SetGlow(rendererMaterial, (float)newVal);
			}, "oncomplete", (Action<object>)delegate
			{
				AbsorbMinions();
			}));
			iTween.ValueTo(minionGlow, iTween.Hash("from", 1f, "to", 0f, "time", m_glowTime, "delay", 0.1f + partOfDelay + m_sparkDelay + m_glowTime, "onupdate", (Action<object>)delegate(object newVal)
			{
				SetGlow(rendererMaterial, (float)newVal);
			}));
		}
	}

	private void AbsorbMinions()
	{
		Vector3 mimironOffset = new Vector3(0f, -1f, 0f);
		for (int i = 0; i < m_mechMinions.Count; i++)
		{
			float partOfDelay = m_absorbTime / (float)m_mechMinions.Count * (float)i;
			GameObject minion = m_mechMinions[i].GetActor().gameObject;
			if (i < m_mechMinions.Count - 1)
			{
				iTween.MoveTo(minion, iTween.Hash("position", m_highPosBone.transform.localPosition + mimironOffset, "easetype", iTween.EaseType.easeInOutSine, "delay", m_glowTime + partOfDelay + m_sparkDelay, "time", 0.5f, "oncomplete", (Action<object>)delegate
				{
					MinionCleanup(minion);
				}));
				continue;
			}
			iTween.MoveTo(minion, iTween.Hash("position", m_highPosBone.transform.localPosition + mimironOffset, "easetype", iTween.EaseType.easeInOutSine, "delay", m_glowTime + partOfDelay + m_sparkDelay, "time", 0.5f, "oncomplete", (Action<object>)delegate
			{
				MinionCleanup(minion);
				FlareMimiron();
			}));
		}
		m_isNegFlash = true;
		StartCoroutine(MimironNegativeFX());
	}

	private void FlareMimiron()
	{
		Material mimironGlowMaterial = m_mimironGlow.GetComponent<Renderer>().GetMaterial();
		Material mimironFlareMaterial = m_mimironFlare.GetComponent<Renderer>().GetMaterial();
		mimironGlowMaterial.SetColor("_TintColor", m_clear);
		mimironFlareMaterial.SetColor("_TintColor", m_clear);
		m_mimironGlow.SetActive(value: true);
		m_mimironFlare.SetActive(value: true);
		iTween.ValueTo(m_mimironGlow, iTween.Hash("from", 0f, "to", 0.7f, "time", 0.3f, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGlow(mimironGlowMaterial, (float)newVal);
		}));
		iTween.ValueTo(m_mimironFlare, iTween.Hash("from", 0f, "to", 2.5f, "time", 0.3f, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGlow(mimironFlareMaterial, (float)newVal, "_Intensity");
		}, "oncomplete", (Action<object>)delegate
		{
			UnflareMimiron();
		}));
	}

	private void UnflareMimiron()
	{
		m_volt.SetDoNotSort(on: false);
		ZonePlay playZone = m_volt.GetController().GetBattlefieldZone();
		foreach (Card card in playZone.GetCards())
		{
			card.SetDoNotSort(on: false);
		}
		playZone.UpdateLayout();
		DestroyMinions();
		m_volt.GetActor().Show();
		Material mimironGlowMaterial = m_mimironGlow.GetComponent<Renderer>().GetMaterial();
		Material mimironFlareMaterial = m_mimironFlare.GetComponent<Renderer>().GetMaterial();
		mimironGlowMaterial.SetColor("_TintColor", m_clear);
		mimironFlareMaterial.SetColor("_TintColor", m_clear);
		m_mimironGlow.SetActive(value: true);
		m_mimironFlare.SetActive(value: true);
		iTween.ValueTo(m_mimironGlow, iTween.Hash("from", 0.7f, "to", 0f, "time", 0.3f, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGlow(mimironGlowMaterial, (float)newVal);
		}));
		iTween.ValueTo(m_mimironFlare, iTween.Hash("from", 2.5f, "to", 0f, "time", 0.3f, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGlow(mimironFlareMaterial, (float)newVal, "_Intensity");
		}, "oncomplete", (Action<object>)delegate
		{
			FadeOutBackground();
		}));
		m_isNegFlash = false;
		OnSpellFinished();
	}

	private void FadeOutBackground()
	{
		m_mimironGlow.SetActive(value: false);
		m_mimironFlare.SetActive(value: false);
		iTween.ColorTo(m_background, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "a", 0f, "time", 0.5f, "oncomplete", (Action<object>)delegate
		{
			RaiseVolt();
		}));
	}

	private void DestroyMinions()
	{
		foreach (Card mechMinion in m_mechMinions)
		{
			mechMinion.IgnoreDeath(ignore: false);
			mechMinion.SetDoNotSort(on: false);
			mechMinion.GetActor().Destroy();
		}
		m_mimiron.IgnoreDeath(ignore: false);
		m_mimiron.SetDoNotSort(on: false);
		m_mimiron.GetActor().Destroy();
	}

	private void RaiseVolt()
	{
		m_mimironElectricity.GetComponent<ParticleSystem>().Stop();
		m_background.GetComponent<Renderer>().GetMaterial().SetColor("_Color", m_clear);
		m_background.SetActive(value: false);
		GameObject volt = m_volt.GetActor().gameObject;
		volt.transform.parent = m_voltParent;
		iTween.MoveTo(volt, iTween.Hash("position", volt.transform.localPosition + new Vector3(0f, 3f, 0f), "time", 0.2f, "islocal", true, "oncomplete", (Action<object>)delegate
		{
			DropV07tron();
		}));
	}

	private void DropV07tron()
	{
		iTween.MoveTo(m_volt.GetActor().gameObject, iTween.Hash("position", Vector3.zero, "time", 0.3f, "islocal", true));
		Finish();
	}

	private void Finish()
	{
		m_volt = null;
		m_mimiron = null;
		m_mechMinions.Clear();
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
