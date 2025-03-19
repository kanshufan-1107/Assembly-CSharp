using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class TGTArcheryTarget : MonoBehaviour
{
	public int m_BullseyePercent = 5;

	public int m_TargetDummyPercent = 1;

	public float m_MaxRandomOffset = 0.3f;

	public int m_Levelup = 50;

	public GameObject m_Collider01;

	public Animation m_Animation;

	public GameObject m_TargetRoot;

	public GameObject m_Arrow;

	public GameObject m_SplitArrow;

	public int m_MaxArrows;

	public List<TGTArrow> m_TargetDummyArrows;

	public GameObject m_ArrowBone01;

	public GameObject m_ArrowBone02;

	public BoxCollider m_BoxCollider01;

	public BoxCollider m_BoxCollider02;

	public BoxCollider m_BoxColliderBullseye;

	public Transform m_CenterBone;

	public Transform m_OuterRadiusBone;

	public Transform m_BullseyeCenterBone;

	public Transform m_BullseyeRadiusBone;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitTargetSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitBullseyeSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitTargetDummySoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_SplitArrowSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_RemoveArrowSoundPrefab;

	private GameObject[] m_arrows;

	private int m_lastArrow = 1;

	private float m_targetRadius;

	private float m_bullseyeRadius;

	private int m_ArrowCount;

	private List<int> m_AvailableTargetDummyArrows;

	private GameObject m_lastBullseyeArrow;

	private bool m_lastArrowWasBullseye;

	private bool m_clearingArrows;

	private float m_lastClickTime;

	private void Start()
	{
		m_arrows = new GameObject[m_MaxArrows];
		m_arrows[0] = Object.Instantiate(m_Arrow, m_ArrowBone01.transform.position, m_ArrowBone01.transform.rotation, m_TargetRoot.transform);
		m_arrows[1] = Object.Instantiate(m_Arrow, m_ArrowBone02.transform.position, m_ArrowBone02.transform.rotation, m_TargetRoot.transform);
		m_lastArrow = 2;
		for (int i = m_lastArrow; i < m_MaxArrows; i++)
		{
			GameObject newArrow = Object.Instantiate(m_Arrow, new Vector3(-15f, -15f, -15f), Quaternion.identity, m_TargetRoot.transform);
			newArrow.SetActive(value: false);
			m_arrows[i] = newArrow;
		}
		m_targetRadius = Vector3.Distance(m_CenterBone.position, m_OuterRadiusBone.position);
		m_bullseyeRadius = Vector3.Distance(m_BullseyeCenterBone.position, m_BullseyeRadiusBone.position);
		m_AvailableTargetDummyArrows = new List<int>();
		for (int j = 0; j < m_TargetDummyArrows.Count; j++)
		{
			m_AvailableTargetDummyArrows.Add(j);
		}
		m_SplitArrow.SetActive(value: false);
	}

	private void Update()
	{
		HandleHits();
	}

	private void HandleHits()
	{
		if (InputCollection.GetMouseButtonDown(0) && IsOver(m_Collider01))
		{
			HandleFireArrow();
		}
	}

	private void HandleFireArrow()
	{
		if (m_clearingArrows)
		{
			return;
		}
		m_ArrowCount++;
		if (m_ArrowCount > m_Levelup)
		{
			m_ArrowCount = 0;
			m_MaxRandomOffset *= 0.95f;
			m_BullseyePercent += 4;
		}
		if (Random.Range(0, 100) < m_TargetDummyPercent && m_AvailableTargetDummyArrows.Count > 0)
		{
			HitTargetDummy();
			return;
		}
		Ray cameraRay = Camera.main.ScreenPointToRay(InputCollection.GetMousePosition());
		bool bullseye = false;
		if (m_BoxColliderBullseye.Raycast(cameraRay, out var hitInfo, 100f))
		{
			bullseye = true;
		}
		if (m_BoxCollider02.Raycast(cameraRay, out hitInfo, 100f))
		{
			m_lastArrow++;
			if (m_lastArrow >= m_MaxArrows)
			{
				m_lastArrow = 0;
				StartCoroutine(ClearArrows());
				return;
			}
			GameObject obj = m_arrows[m_lastArrow];
			TGTArrow tgtArrow = obj.GetComponent<TGTArrow>();
			FireArrow(tgtArrow, hitInfo.point, bullseye);
			obj.transform.eulerAngles = hitInfo.normal;
			ImpactTarget();
		}
	}

	private IEnumerator ClearArrows()
	{
		m_clearingArrows = true;
		GameObject[] arrows = m_arrows;
		foreach (GameObject arrowGO in arrows)
		{
			if (arrowGO.activeSelf)
			{
				arrowGO.SetActive(value: false);
				m_Animation.Stop();
				m_Animation.Play("TGT_GrandStand_ArcheryTarget_Remove");
				PlaySound(m_RemoveArrowSoundPrefab);
				yield return new WaitForSeconds(0.2f);
			}
		}
		yield return new WaitForSeconds(0.2f);
		if (m_SplitArrow.activeSelf)
		{
			m_SplitArrow.SetActive(value: false);
			m_Animation.Stop();
			m_Animation.Play("TGT_GrandStand_ArcheryTarget_Remove");
			PlaySound(m_RemoveArrowSoundPrefab);
		}
		m_lastArrowWasBullseye = false;
		m_lastBullseyeArrow = null;
		m_clearingArrows = false;
	}

	private void FireArrow(TGTArrow arrow, Vector3 hitPosition, bool bullseye)
	{
		arrow.transform.position = hitPosition;
		bool clickBonus = false;
		if (Time.timeSinceLevelLoad > m_lastClickTime + 0.8f)
		{
			clickBonus = true;
		}
		m_lastClickTime = Time.timeSinceLevelLoad;
		int bullseyePercent = m_BullseyePercent;
		if (clickBonus)
		{
			bullseyePercent *= 2;
		}
		if (bullseyePercent > 80)
		{
			bullseyePercent = 80;
		}
		if (bullseye && Random.Range(0, 100) < bullseyePercent)
		{
			int splitArrowChance = 2;
			if (clickBonus)
			{
				splitArrowChance = 8;
			}
			if (m_lastArrowWasBullseye && !m_SplitArrow.activeSelf && bullseye && Random.Range(0, 100) < splitArrowChance)
			{
				m_SplitArrow.transform.position = m_lastBullseyeArrow.transform.position;
				m_SplitArrow.transform.rotation = m_lastBullseyeArrow.transform.rotation;
				TGTArrow component = m_SplitArrow.GetComponent<TGTArrow>();
				TGTArrow lastTGTArrow = m_lastBullseyeArrow.GetComponent<TGTArrow>();
				m_SplitArrow.SetActive(value: true);
				component.FireArrow(randomRotation: false);
				component.Bullseye();
				PlaySound(m_SplitArrowSoundPrefab);
				component.m_ArrowRoot.transform.position = lastTGTArrow.m_ArrowRoot.transform.position;
				component.m_ArrowRoot.transform.rotation = lastTGTArrow.m_ArrowRoot.transform.rotation;
				m_lastBullseyeArrow.SetActive(value: false);
				m_lastArrowWasBullseye = false;
				m_lastBullseyeArrow = null;
			}
			else
			{
				arrow.gameObject.SetActive(value: true);
				arrow.Bullseye();
				PlaySound(m_HitBullseyeSoundPrefab);
				arrow.m_ArrowRoot.transform.localPosition = Vector3.zero;
				m_lastBullseyeArrow = arrow.gameObject;
				m_lastArrowWasBullseye = true;
			}
			return;
		}
		m_lastArrowWasBullseye = false;
		m_lastBullseyeArrow = null;
		arrow.gameObject.SetActive(value: true);
		if (bullseye)
		{
			Vector2 bullseyeOffset = Random.insideUnitCircle.normalized * m_bullseyeRadius * 2f;
			arrow.m_ArrowRoot.transform.localPosition = new Vector3(bullseyeOffset.x, bullseyeOffset.y, 0f);
			arrow.FireArrow(randomRotation: true);
			PlaySound(m_HitTargetSoundPrefab);
			return;
		}
		Vector2 offset = Random.insideUnitCircle * Random.Range(0f, m_MaxRandomOffset);
		Transform arrowRootTransform = arrow.m_ArrowRoot.transform;
		arrowRootTransform.localPosition = new Vector3(offset.x, offset.y, 0f);
		if (Vector3.Distance(arrowRootTransform.position, m_CenterBone.position) > m_targetRadius)
		{
			arrowRootTransform.localPosition = Vector3.zero;
		}
		if (Vector3.Distance(arrowRootTransform.position, m_BullseyeCenterBone.position) < m_bullseyeRadius)
		{
			arrowRootTransform.localPosition = Vector3.zero;
		}
		arrow.FireArrow(randomRotation: true);
		PlaySound(m_HitTargetSoundPrefab);
	}

	private void HitTargetDummy()
	{
		int randomIdx = 0;
		if (m_AvailableTargetDummyArrows.Count > 1)
		{
			randomIdx = Random.Range(0, m_AvailableTargetDummyArrows.Count);
		}
		TGTArrow tGTArrow = m_TargetDummyArrows[m_AvailableTargetDummyArrows[randomIdx]];
		tGTArrow.gameObject.SetActive(value: true);
		tGTArrow.FireArrow(randomRotation: false);
		TGTTargetDummy.Get().ArrowHit();
		PlaySound(m_HitTargetDummySoundPrefab);
		if (m_AvailableTargetDummyArrows.Count > 1)
		{
			m_AvailableTargetDummyArrows.RemoveAt(randomIdx);
		}
		else
		{
			m_AvailableTargetDummyArrows.Clear();
		}
	}

	private void ImpactTarget()
	{
		m_Animation.Stop();
		m_Animation.Play("TGT_GrandStand_ArcheryTarget_Hit");
	}

	private void PlaySound(string soundPrefab)
	{
		if (!string.IsNullOrEmpty(soundPrefab))
		{
			SoundManager.Get().LoadAndPlay(soundPrefab, base.gameObject);
		}
	}

	private bool IsOver(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
		{
			return false;
		}
		if (!UniversalInputManager.Get().InputIsOver(go))
		{
			return false;
		}
		return true;
	}
}
