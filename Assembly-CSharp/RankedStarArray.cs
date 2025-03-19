using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using HutongGames.PlayMaker;
using UnityEngine;

[CustomEditClass]
public class RankedStarArray : MonoBehaviour
{
	public enum LayoutStyle
	{
		Horizontal,
		Vertical,
		Arc
	}

	[CustomEditField(Sections = "General")]
	public int m_starCount;

	[CustomEditField(Sections = "General")]
	public int m_starCountDarkened;

	[CustomEditField(Sections = "General")]
	public LayoutStyle m_layout;

	[CustomEditField(Sections = "Linear Layout")]
	public float m_xOffsetPerStar;

	[CustomEditField(Sections = "Linear Layout")]
	public float m_zOffsetPerStar;

	[CustomEditField(Sections = "Arc Layout")]
	public float m_arcRadius;

	[CustomEditField(Sections = "Arc Layout")]
	public float m_arcDegreesPerStar;

	[CustomEditField(Sections = "Arc Layout")]
	public float m_centerStarsAtDegrees;

	[CustomEditField(Sections = "Arc Layout")]
	public bool m_arcAlignEdge;

	private static readonly string s_starPrefab = "Star_Ranked.prefab:48d5a18072eff2445a3de1c9f7348bea";

	private List<RankChangeStar> m_stars = new List<RankChangeStar>();

	private Coroutine m_showCoroutine;

	private Coroutine m_loadCoroutine;

	private bool IsShowing { get; set; }

	private void Awake()
	{
		LoadStars();
	}

	public void Show()
	{
		if (m_showCoroutine != null)
		{
			StopCoroutine(m_showCoroutine);
		}
		m_showCoroutine = StartCoroutine(ShowWhenReady());
	}

	public void Hide()
	{
		if (!IsShowing)
		{
			return;
		}
		foreach (RankChangeStar star in m_stars)
		{
			star.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator ShowWhenReady()
	{
		while (IsLoading())
		{
			yield return null;
		}
		foreach (RankChangeStar star in m_stars)
		{
			star.gameObject.SetActive(value: true);
		}
		IsShowing = true;
	}

	public void Init(int starCount, int starCountDarkened)
	{
		m_starCount = starCount;
		m_starCountDarkened = starCountDarkened;
		Reset();
	}

	public bool PopulateFsmArrayWithStars(PlayMakerFSM fsm, string varName, int startIndex = 0, int count = 0)
	{
		if (fsm == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(varName))
		{
			return false;
		}
		FsmArray fsmArray = fsm.FsmVariables.GetFsmArray(varName);
		if (fsmArray == null)
		{
			return false;
		}
		if (count <= 0)
		{
			count = m_stars.Count;
		}
		fsmArray.objectReferences = m_stars.Skip(startIndex).Take(count).Select((Func<RankChangeStar, UnityEngine.Object>)((RankChangeStar star) => star.gameObject))
			.ToArray();
		return true;
	}

	public bool IsLoading()
	{
		return m_stars.Count < m_starCount;
	}

	private void LoadStars()
	{
		if (m_loadCoroutine != null)
		{
			StopCoroutine(m_loadCoroutine);
		}
		m_loadCoroutine = StartCoroutine(LoadStarsWhenReady());
	}

	private IEnumerator LoadStarsWhenReady()
	{
		if (m_starCount > 0)
		{
			while (!ServiceManager.IsAvailable<IAssetLoader>())
			{
				yield return null;
			}
			for (int i = 0; i < m_starCount; i++)
			{
				AssetLoader.Get().InstantiatePrefab(s_starPrefab, OnStarLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	private void OnStarLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.localScale = Vector3.one;
		GameUtils.SetParent(go, base.gameObject);
		go.SetActive(value: false);
		RankChangeStar star = go.GetComponent<RankChangeStar>();
		m_stars.Add(star);
		if (m_stars.Count == m_starCount)
		{
			int numStarsToDarken = m_starCountDarkened;
			int idx = m_stars.Count - 1;
			while (idx >= 0 && numStarsToDarken > 0)
			{
				m_stars[idx].BlackOut();
				idx--;
				numStarsToDarken--;
			}
			if (m_layout == LayoutStyle.Arc)
			{
				LayoutStarsArc();
			}
			else
			{
				LayoutStarsLinear();
			}
		}
	}

	private void LayoutStarsArc()
	{
		float centerStarsAtAngle = m_centerStarsAtDegrees * ((float)Math.PI / 180f);
		float offsetAnglePerStar = m_arcDegreesPerStar * ((float)Math.PI / 180f);
		float totalArc = offsetAnglePerStar * (float)(m_stars.Count - 1);
		float angle = centerStarsAtAngle + totalArc / 2f;
		Vector3 centerStarPos = base.transform.position;
		centerStarPos.x += m_arcRadius * Mathf.Cos(centerStarsAtAngle);
		centerStarPos.z += m_arcRadius * Mathf.Sin(centerStarsAtAngle);
		foreach (RankChangeStar star in m_stars)
		{
			Vector3 pos = base.transform.position;
			pos.x += m_arcRadius * Mathf.Cos(angle);
			pos.z += m_arcRadius * Mathf.Sin(angle);
			if (m_arcAlignEdge)
			{
				Vector3 centerStarToThis = base.transform.position - centerStarPos;
				pos += centerStarToThis;
			}
			star.transform.position = pos;
			angle -= offsetAnglePerStar;
		}
	}

	private void LayoutStarsLinear()
	{
		int idx1 = m_stars.Count / 2 - 1;
		int idx2 = idx1 + 1;
		float mirrorDirX = ((m_layout == LayoutStyle.Vertical) ? 1f : (-1f));
		float mirrorDirZ = ((m_layout == LayoutStyle.Vertical) ? (-1f) : 1f);
		float offsetX = 0f;
		float offsetZ = 0f;
		if (GeneralUtils.IsOdd(m_stars.Count))
		{
			if (m_stars.Count < 3)
			{
				return;
			}
			idx2++;
		}
		else
		{
			if (m_stars.Count < 2)
			{
				return;
			}
			if (m_layout == LayoutStyle.Vertical)
			{
				offsetZ += m_zOffsetPerStar / 2f;
				TransformUtil.SetLocalPosZ(m_stars[idx1], offsetZ * -1f);
				TransformUtil.SetLocalPosZ(m_stars[idx2], offsetZ);
			}
			else
			{
				offsetX += m_xOffsetPerStar / 2f;
				TransformUtil.SetLocalPosX(m_stars[idx1], offsetX * -1f);
				TransformUtil.SetLocalPosX(m_stars[idx2], offsetX);
			}
			idx1--;
			idx2++;
		}
		while (idx1 >= 0)
		{
			offsetX += m_xOffsetPerStar;
			offsetZ += m_zOffsetPerStar;
			TransformUtil.SetLocalPosX(m_stars[idx1], offsetX * mirrorDirX);
			TransformUtil.SetLocalPosZ(m_stars[idx1], offsetZ * mirrorDirZ);
			idx1--;
			TransformUtil.SetLocalPosX(m_stars[idx2], offsetX);
			TransformUtil.SetLocalPosZ(m_stars[idx2], offsetZ);
			idx2++;
		}
	}

	[ContextMenu("Show")]
	private void ResetAndShow()
	{
		Reset();
		Show();
	}

	[ContextMenu("Reset")]
	private void Reset()
	{
		foreach (RankChangeStar star in m_stars)
		{
			UnityEngine.Object.Destroy(star.gameObject);
		}
		m_stars.Clear();
		IsShowing = false;
		LoadStars();
	}
}
