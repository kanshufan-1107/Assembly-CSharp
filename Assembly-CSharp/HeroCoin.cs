using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class HeroCoin : PegUIElement
{
	public delegate void CoinPressCallback();

	public enum CoinStatus
	{
		ACTIVE,
		DEFEATED,
		UNREVEALED,
		UNREVEALED_TO_ACTIVE,
		ACTIVE_TO_DEFEATED
	}

	public GameObject m_coin;

	public GameObject m_coinX;

	public GameObject m_cracks;

	public HighlightState m_highlight;

	public UberText m_coinLabel;

	public GameObject m_tooltip;

	public UberText m_tooltipText;

	public GameObject m_leftCap;

	public GameObject m_rightCap;

	public GameObject m_middle;

	public GameObject m_explosionPrefab;

	public bool m_inputEnabled;

	private UnityEngine.Vector2 m_goldTexture;

	private UnityEngine.Vector2 m_grayTexture;

	private UnityEngine.Vector2 m_crackTexture;

	private string m_lessonAsset;

	private UnityEngine.Vector2 m_lessonCoords;

	private int m_missionID;

	private CoinStatus m_currentStatus;

	private float m_originalMiddleWidth;

	private Vector3 m_originalPosition;

	private Material m_material;

	private Material m_grayMaterial;

	private Vector3 m_originalXPosition;

	private bool m_nextTutorialStarted;

	private CoinPressCallback m_coinPressCallback;

	protected override void Awake()
	{
		base.Awake();
		m_tooltip.SetActive(value: false);
	}

	private void Start()
	{
		m_coinLabel.Text = GameStrings.Get("GLOBAL_PLAY");
	}

	protected override void OnDestroy()
	{
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameLoaded);
		}
		base.OnDestroy();
	}

	public int GetMissionId()
	{
		return m_missionID;
	}

	public void SetCoinInfo(UnityEngine.Vector2 goldTexture, UnityEngine.Vector2 grayTexture, UnityEngine.Vector2 crackTexture, int missionID)
	{
		List<Material> materials = GetComponent<Renderer>().GetMaterials();
		m_material = materials[0];
		m_grayMaterial = materials[1];
		m_goldTexture = goldTexture;
		m_material.mainTextureOffset = m_goldTexture;
		m_grayTexture = grayTexture;
		m_grayMaterial.mainTextureOffset = m_grayTexture;
		m_material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
		m_grayMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
		m_crackTexture = crackTexture;
		m_cracks.GetComponent<Renderer>().GetMaterial().mainTextureOffset = m_crackTexture;
		m_missionID = missionID;
		m_tooltipText.Text = GameUtils.GetMissionHeroName(missionID);
		m_originalPosition = base.transform.localPosition;
		m_originalXPosition = m_coinX.transform.localPosition;
	}

	public void SetCoinPressCallback(CoinPressCallback callback)
	{
		m_coinPressCallback = callback;
	}

	public void SetLessonAsset(string lessonAsset)
	{
		m_lessonAsset = lessonAsset;
	}

	public string GetLessonAsset()
	{
		return m_lessonAsset;
	}

	public void SetProgress(CoinStatus status)
	{
		base.gameObject.SetActive(value: true);
		m_currentStatus = status;
		switch (status)
		{
		case CoinStatus.DEFEATED:
			m_material.mainTextureOffset = m_grayTexture;
			m_cracks.SetActive(value: true);
			m_coinX.SetActive(value: true);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			m_inputEnabled = false;
			m_coinLabel.gameObject.SetActive(value: false);
			break;
		case CoinStatus.ACTIVE:
			m_cracks.SetActive(value: false);
			m_coinX.SetActive(value: false);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			m_inputEnabled = true;
			break;
		case CoinStatus.UNREVEALED_TO_ACTIVE:
		{
			m_material.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
			m_grayMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
			Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
			moveArgs2.Add("y", 3f);
			moveArgs2.Add("z", m_originalPosition.z - 0.2f);
			moveArgs2.Add("time", 0.5f);
			moveArgs2.Add("islocal", true);
			moveArgs2.Add("easetype", iTween.EaseType.easeOutCubic);
			iTween.MoveTo(base.gameObject, moveArgs2);
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
			rotateArgs.Add("time", 1f);
			rotateArgs.Add("islocal", true);
			rotateArgs.Add("delay", 0.5f);
			rotateArgs.Add("easetype", iTween.EaseType.easeOutElastic);
			iTween.RotateTo(base.gameObject, rotateArgs);
			Hashtable moveArgs3 = iTweenManager.Get().GetTweenHashTable();
			moveArgs3.Add("y", m_originalPosition.y);
			moveArgs3.Add("z", m_originalPosition.z);
			moveArgs3.Add("time", 0.5f);
			moveArgs3.Add("islocal", true);
			moveArgs3.Add("delay", 1.75f);
			moveArgs3.Add("easetype", iTween.EaseType.easeOutCubic);
			iTween.MoveTo(base.gameObject, moveArgs3);
			Hashtable valueArgs = iTweenManager.Get().GetTweenHashTable();
			valueArgs.Add("from", 0f);
			valueArgs.Add("to", 1f);
			valueArgs.Add("time", 0.25f);
			valueArgs.Add("delay", 1.5f);
			valueArgs.Add("easetype", iTween.EaseType.easeOutCirc);
			valueArgs.Add("onupdate", "OnUpdateAlphaVal");
			valueArgs.Add("oncomplete", "EnableInput");
			valueArgs.Add("oncompletetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, valueArgs);
			SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_rises.prefab:42026163dad364742abef7e15cb2e6cc", base.gameObject);
			StartCoroutine(ShowCoinText());
			m_inputEnabled = false;
			break;
		}
		case CoinStatus.ACTIVE_TO_DEFEATED:
		{
			m_coinX.transform.localPosition = new Vector3(0f, 10f, UniversalInputManager.UsePhoneUI ? 0f : (-0.23f));
			m_cracks.SetActive(value: true);
			m_coinX.SetActive(value: true);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			m_inputEnabled = false;
			RenderUtils.SetAlpha(m_coinX, 0f);
			RenderUtils.SetAlpha(m_cracks, 0f);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("y", m_originalXPosition.y);
			moveArgs.Add("z", m_originalXPosition.z);
			moveArgs.Add("time", 0.25f);
			moveArgs.Add("islocal", true);
			moveArgs.Add("easetype", iTween.EaseType.easeInCirc);
			iTween.MoveTo(m_coinX, moveArgs);
			Hashtable alphaArgs = iTweenManager.Get().GetTweenHashTable();
			alphaArgs.Add("amount", 1f);
			alphaArgs.Add("delay", 0f);
			alphaArgs.Add("time", 0.25f);
			alphaArgs.Add("easetype", iTween.EaseType.easeInCirc);
			iTween.FadeTo(m_coinX, alphaArgs);
			Hashtable alphaArgs2 = iTweenManager.Get().GetTweenHashTable();
			alphaArgs2.Add("amount", 1f);
			alphaArgs2.Add("delay", 0.15f);
			alphaArgs2.Add("time", 0.25f);
			alphaArgs2.Add("easetype", iTween.EaseType.easeInCirc);
			iTween.FadeTo(m_cracks, alphaArgs2);
			SoundManager.Get().LoadAndPlay("tutorial_mission_x_descend.prefab:079781bfd4ce602448860b74693d61bd", base.gameObject);
			StartCoroutine(SwitchCoinToGray());
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
			{
				GameState.Get().GetGameEntity().NotifyOfDefeatCoinAnimation();
			}
			break;
		}
		default:
			base.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
			m_coinLabel.gameObject.SetActive(value: false);
			m_cracks.SetActive(value: false);
			m_coinX.SetActive(value: false);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			m_inputEnabled = false;
			break;
		}
	}

	public IEnumerator ShowCoinText()
	{
		yield return new WaitForSeconds(1.5f);
		m_coinLabel.gameObject.SetActive(value: true);
	}

	public IEnumerator SwitchCoinToGray()
	{
		yield return new WaitForSeconds(0.25f);
		m_material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
		m_grayMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
		m_coinLabel.gameObject.SetActive(value: false);
		iTween.ShakePosition(Camera.main.gameObject, iTween.Hash("amount", new Vector3(0.2f, 0.2f, 0.2f), "name", "HeroCoin", "time", 0.5f, "delay", 0f, "axis", "none"));
	}

	public void HideTooltip()
	{
		m_tooltip.SetActive(value: false);
	}

	public void EnableInput()
	{
		m_inputEnabled = true;
		m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
	}

	public void FinishIntroScaling()
	{
		AddEventListener(UIEventType.ROLLOVER, OnOver);
		AddEventListener(UIEventType.ROLLOUT, OnOut);
		AddEventListener(UIEventType.RELEASE, OnPress);
		m_originalMiddleWidth = m_middle.GetComponent<Renderer>().bounds.size.x;
	}

	private void ShowTooltip()
	{
		if (m_currentStatus != CoinStatus.UNREVEALED)
		{
			m_tooltip.SetActive(value: true);
			float padding = 0f;
			float paddedWidthScale = (m_tooltipText.GetTextWorldSpaceBounds().size.x + padding) / m_originalMiddleWidth;
			TransformUtil.SetLocalScaleX(m_middle, paddedWidthScale);
			float overlapX = m_originalMiddleWidth * 0.223f;
			float adjustZ = m_originalMiddleWidth * 0.01f;
			TransformUtil.SetPoint(m_leftCap, Anchor.RIGHT, m_middle, Anchor.LEFT, new Vector3(overlapX, 0f, adjustZ));
			TransformUtil.SetPoint(m_rightCap, Anchor.LEFT, m_middle, Anchor.RIGHT, new Vector3(0f - overlapX, 0f, adjustZ));
		}
	}

	private void OnOver(UIEvent e)
	{
		if (!m_nextTutorialStarted && !iTween.HasTween(base.gameObject))
		{
			ShowTooltip();
			if (m_inputEnabled)
			{
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_MOUSE_OVER);
				SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_mouse_over.prefab:1fd7d833d7e2ffa469d6572bb08c4582", base.gameObject);
			}
		}
	}

	private void OnOut(UIEvent e)
	{
		HideTooltip();
		if (!m_nextTutorialStarted && m_inputEnabled)
		{
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
	}

	private void OnPress(UIEvent e)
	{
		if (m_inputEnabled && !m_nextTutorialStarted)
		{
			m_inputEnabled = false;
			SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_play_select.prefab:781c9e3f238e74b4894b5ad9a05c4e35", base.gameObject);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			if (m_coinPressCallback != null)
			{
				m_coinPressCallback();
				return;
			}
			LoadingScreen.Get().AddTransitionBlocker();
			LoadingScreen.Get().AddTransitionObject(base.gameObject);
			SceneMgr.Get().RegisterSceneLoadedEvent(OnGameLoaded);
			StartNextTutorial();
			GetComponentInChildren<PlayMakerFSM>().SendEvent("Action");
		}
	}

	private void OnGameLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		GetComponentInChildren<PlayMakerFSM>().SendEvent("Death");
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameLoaded);
		StartCoroutine(WaitThenTransition());
	}

	private IEnumerator WaitThenTransition()
	{
		yield return new WaitForSeconds(1.25f);
		LoadingScreen.Get().NotifyTransitionBlockerComplete();
	}

	private void StartNextTutorial()
	{
		m_nextTutorialStarted = true;
		GameMgr.Get().FindGame(GameType.GT_TUTORIAL, FormatType.FT_WILD, m_missionID, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}
}
