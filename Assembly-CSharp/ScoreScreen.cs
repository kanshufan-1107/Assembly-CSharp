using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class ScoreScreen : MonoBehaviour
{
	public GameObject m_BackgroundCenter;

	public NestedPrefab m_ScoreBoxLeft;

	public NestedPrefab m_ScoreBoxCenter;

	public NestedPrefab m_ScoreBoxRight;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_ShowSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HideSoundPrefab;

	public UberText m_FooterText;

	public const float SHOW_SEC = 0.65f;

	public const float HIDE_SEC = 0.25f;

	private const float SHOW_INTERMED_SEC = 0.5f;

	private const float SHOW_FINAL_SEC = 0.15f;

	private const float DRIFT_CYCLE_SEC = 10f;

	private static readonly Vector3 START_SCALE = new Vector3(0.001f, 0.001f, 0.001f);

	private static ScoreScreen s_instance;

	private Vector3 m_initialScale;

	private void Awake()
	{
		s_instance = this;
		Init();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static ScoreScreen Get()
	{
		return s_instance;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Vector3 intermedScale = 1.03f * m_initialScale;
		Vector3 finalScale = m_initialScale;
		base.transform.localScale = START_SCALE;
		Hashtable intermedScaleArgs = iTweenManager.Get().GetTweenHashTable();
		intermedScaleArgs.Add("scale", intermedScale);
		intermedScaleArgs.Add("time", 0.5f);
		iTween.ScaleTo(base.gameObject, intermedScaleArgs);
		Action<object> onComplete = delegate
		{
			Drift();
		};
		Hashtable finalScaleArgs = iTweenManager.Get().GetTweenHashTable();
		finalScaleArgs.Add("scale", finalScale);
		finalScaleArgs.Add("delay", 0.5f);
		finalScaleArgs.Add("time", 0.15f);
		finalScaleArgs.Add("oncomplete", onComplete);
		iTween.ScaleTo(base.gameObject, finalScaleArgs);
		if (!string.IsNullOrEmpty(m_ShowSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_ShowSoundPrefab);
		}
	}

	public void Hide()
	{
		Action<object> onComplete = delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		};
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", START_SCALE);
		scaleArgs.Add("time", 0.25f);
		scaleArgs.Add("oncomplete", onComplete);
		iTween.ScaleTo(base.gameObject, scaleArgs);
		if (!string.IsNullOrEmpty(m_HideSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HideSoundPrefab);
		}
	}

	private void Init()
	{
		m_initialScale = base.transform.localScale;
		UpdateScoreBoxes();
		LayoutScoreBoxes();
		UpdateFooterText();
		base.gameObject.SetActive(value: false);
	}

	private void UpdateScoreBoxes()
	{
		UpdateScoreBox(m_ScoreBoxLeft, GAME_TAG.SCORE_LABELID_1, GAME_TAG.SCORE_VALUE_1);
		UpdateScoreBox(m_ScoreBoxCenter, GAME_TAG.SCORE_LABELID_2, GAME_TAG.SCORE_VALUE_2);
		UpdateScoreBox(m_ScoreBoxRight, GAME_TAG.SCORE_LABELID_3, GAME_TAG.SCORE_VALUE_3);
	}

	private void UpdateFooterText()
	{
		int labelId = 0;
		int dummyValue = -1;
		GetLabelAndScore(GAME_TAG.SCORE_FOOTERID, GAME_TAG.SCORE_FOOTERID, out labelId, out dummyValue);
		if (labelId <= 0)
		{
			m_FooterText.gameObject.SetActive(value: false);
			return;
		}
		ScoreLabelDbfRecord rec = GameDbf.ScoreLabel.GetRecord(labelId);
		if (rec == null)
		{
			Error.AddDevWarning("Error", "ScoreScreen.UpdateFooterText() - There is no ScoreLabel record for id {0}.", labelId);
		}
		else
		{
			m_FooterText.Text = rec.Text.GetString();
			m_FooterText.gameObject.SetActive(value: true);
		}
	}

	private void UpdateScoreBox(NestedPrefab scoreBoxPrefab, GAME_TAG labelTag, GAME_TAG valueTag)
	{
		ScoreBox scoreBox = scoreBoxPrefab.PrefabGameObject(instantiateIfNeeded: true).GetComponent<ScoreBox>();
		int labelId = 0;
		int value = 0;
		GetLabelAndScore(labelTag, valueTag, out labelId, out value);
		bool showBox = labelId != 0;
		scoreBoxPrefab.gameObject.SetActive(showBox);
		if (showBox)
		{
			ScoreLabelDbfRecord rec = GameDbf.ScoreLabel.GetRecord(labelId);
			if (rec == null)
			{
				Error.AddDevWarning("Error", "ScoreScreen.UpdateScoreBox() - There is no ScoreLabel record for id {0}.", labelId);
			}
			else
			{
				scoreBox.m_Label.Text = rec.Text.GetString();
			}
			scoreBox.m_Value.Text = value.ToString();
		}
	}

	private void GetLabelAndScore(GAME_TAG labelTag, GAME_TAG valueTag, out int labelId, out int value)
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		labelId = player.GetTag(labelTag);
		if (labelId != 0)
		{
			value = player.GetTag(valueTag);
			return;
		}
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		labelId = gameEntity.GetTag(labelTag);
		value = gameEntity.GetTag(valueTag);
	}

	private void LayoutScoreBoxes()
	{
		NestedPrefab[] scoreBoxes = new NestedPrefab[3] { m_ScoreBoxLeft, m_ScoreBoxCenter, m_ScoreBoxRight };
		int firstActiveScoreBoxIndex = Array.FindIndex(scoreBoxes, 0, (NestedPrefab scoreBox) => scoreBox.gameObject.activeInHierarchy);
		int secondActiveScoreBoxIndex = Array.FindIndex(scoreBoxes, firstActiveScoreBoxIndex + 1, (NestedPrefab scoreBox) => scoreBox.gameObject.activeInHierarchy);
		int thirdActiveScoreBoxIndex = Array.FindIndex(scoreBoxes, secondActiveScoreBoxIndex + 1, (NestedPrefab scoreBox) => scoreBox.gameObject.activeInHierarchy);
		NestedPrefab firstScoreBox = scoreBoxes[firstActiveScoreBoxIndex];
		if (secondActiveScoreBoxIndex < 0)
		{
			firstScoreBox.transform.position = m_ScoreBoxCenter.transform.position;
		}
		else if (thirdActiveScoreBoxIndex < 0)
		{
			NestedPrefab obj = scoreBoxes[secondActiveScoreBoxIndex];
			Vector3 firstScoreBoxPos = 0.5f * (m_ScoreBoxLeft.transform.position + m_ScoreBoxCenter.transform.position);
			Vector3 secondScoreBoxPos = 0.5f * (m_ScoreBoxCenter.transform.position + m_ScoreBoxRight.transform.position);
			firstScoreBox.transform.position = firstScoreBoxPos;
			obj.transform.position = secondScoreBoxPos;
		}
	}

	private void Drift()
	{
		Vector3 center = base.transform.position;
		float relativeWidth = m_BackgroundCenter.GetComponent<Renderer>().bounds.size.x;
		Vector3 offsetDir = 0.02f * relativeWidth * base.transform.up;
		Vector3 frontPoint = center + offsetDir;
		Vector3 backPoint = center - offsetDir;
		List<Vector3> path = new List<Vector3>();
		path.Add(center);
		path.Add(frontPoint);
		path.Add(center);
		path.Add(backPoint);
		path.Add(center);
		Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
		tweenArgs.Add("path", path.ToArray());
		tweenArgs.Add("time", 10f);
		tweenArgs.Add("easetype", iTween.EaseType.linear);
		tweenArgs.Add("looptype", iTween.LoopType.loop);
		iTween.MoveTo(base.gameObject, tweenArgs);
	}
}
