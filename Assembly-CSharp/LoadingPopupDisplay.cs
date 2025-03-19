using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LoadingPopupDisplay : TransitionPopup
{
	[Serializable]
	public class LoadingbarTexture
	{
		public AdventureDbId adventureID;

		public ScenarioDbId scenarioId;

		public Texture texture;

		public float m_barIntensity = 1.2f;

		public float m_barIntensityIncreaseMax = 3f;
	}

	public UberText m_tipOfTheDay;

	public ProgressBar m_progressBar;

	public GameObject m_loadingTile;

	public GameObject m_cancelButtonParent;

	public List<LoadingbarTexture> m_barTextures = new List<LoadingbarTexture>();

	private Map<AdventureDbId, List<string>> m_adventureTaskNameMap = new Map<AdventureDbId, List<string>>();

	private List<string> m_mercenariesTaskNameList = new List<string>();

	private Map<AdventureDbId, List<string>> m_adventureTipOfTheDayNameMap = new Map<AdventureDbId, List<string>>();

	private List<string> m_spectatorTaskNameMap = new List<string>();

	private bool m_stopAnimating;

	private bool m_animationStopped;

	private AudioSource m_loopSound;

	private bool m_barAnimating;

	public static readonly Vector3 START_POS = new Vector3(-0.0152f, -0.0894f, -0.0837f);

	public static readonly Vector3 MID_POS = new Vector3(-0.0152f, -0.0894f, 0.0226f);

	public static readonly Vector3 END_POS = new Vector3(-0.0152f, 0.0368f, 0.0226f);

	public static readonly Vector3 OFFSCREEN_POS = new Vector3(-0.0152f, -0.0894f, 0.13f);

	private const int TASK_DURATION_VARIATION = 2;

	private const float ROTATION_DURATION = 0.5f;

	private const float ROTATION_DELAY = 0.5f;

	private const float SLIDE_IN_TIME = 0.5f;

	private const float SLIDE_OUT_TIME = 0.25f;

	private const float RAISE_TIME = 0.5f;

	private const float LOWER_TIME = 0.25f;

	private const string SHOW_CANCEL_BUTTON_TWEEN_NAME = "ShowCancelButton";

	private const float SHOW_CANCEL_BUTTON_THRESHOLD = 30f;

	protected override void Awake()
	{
		base.Awake();
		GenerateStringNameMaps();
		m_title.Text = GameStrings.Get("GLUE_STARTING_GAME");
		base.gameObject.transform.localPosition = new Vector3(-0.05f, 9f, 3.908f);
		SoundManager.Get().Load("StartGame_window_expand_up.prefab:1989383da054858489f420f8e2ac43d4");
		SoundManager.Get().Load("StartGame_window_shrink_down.prefab:07b3273ed29d9df479442d93caa07799");
		SoundManager.Get().Load("StartGame_window_loading_bar_move_down_and_forward.prefab:0b04e30939289024dadd8292dbfd7fef");
		SoundManager.Get().Load("StartGame_window_loading_bar_flip.prefab:fa63e6a9075dba24fae4fddc2fd32a39");
		SoundManager.Get().Load("StartGame_window_bar_filling_loop.prefab:4e8350e37c1218a4cbbd9e93b394cd48");
		SoundManager.Get().Load("StartGame_window_loading_bar_drop.prefab:899774ec312ca8241a5a5e4e300c5d93");
		DisableCancelButton();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (SoundManager.Get() != null)
		{
			StopLoopingSound();
		}
	}

	public override void Hide()
	{
		if (m_shown)
		{
			Navigation.RemoveHandler(OnNavigateBack);
			base.Hide();
		}
	}

	protected override bool EnableCancelButtonIfPossible()
	{
		if (!base.EnableCancelButtonIfPossible())
		{
			return false;
		}
		TransformUtil.SetLocalPosX(m_queueTab, -0.3234057f);
		return true;
	}

	protected override void EnableCancelButton()
	{
		m_cancelButtonParent.SetActive(value: true);
		base.EnableCancelButton();
	}

	protected override void DisableCancelButton()
	{
		base.DisableCancelButton();
		m_cancelButtonParent.SetActive(value: false);
	}

	protected override void AnimateShow()
	{
		Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
		tweenArgs.Add("name", "ShowCancelButton");
		tweenArgs.Add("time", 30f);
		tweenArgs.Add("ignoretimescale", true);
		tweenArgs.Add("oncomplete", new Action<object>(OnCancelButtonShowTimerCompleted));
		tweenArgs.Add("oncompletetarget", base.gameObject);
		iTween.Timer(base.gameObject, tweenArgs);
		SetTipOfTheDay();
		SetLoadingBarTexture();
		SoundManager.Get().LoadAndPlay("StartGame_window_expand_up.prefab:1989383da054858489f420f8e2ac43d4");
		base.AnimateShow();
		m_stopAnimating = false;
		Navigation.Push(OnNavigateBack);
	}

	protected void OnCancelButtonShowTimerCompleted(object userData)
	{
		EnableCancelButtonIfPossible();
	}

	protected override void OnGameEntered(FindGameEventData eventData)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			base.OnGameEntered(eventData);
		}
	}

	protected override void OnGameUpdated(FindGameEventData eventData)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			base.OnGameUpdated(eventData);
		}
	}

	protected override void AnimateHide()
	{
		SoundManager.Get().LoadAndPlay("StartGame_window_shrink_down.prefab:07b3273ed29d9df479442d93caa07799");
		iTween.StopByName(base.gameObject, "ShowCancelButton");
		if (m_barAnimating)
		{
			StopCoroutine("AnimateBar");
			m_barAnimating = false;
			StopLoopingSound();
		}
		base.AnimateHide();
	}

	protected override void OnAnimateShowFinished()
	{
		base.OnAnimateShowFinished();
		AnimateInLoadingTile();
	}

	private void AnimateInLoadingTile()
	{
		if (m_stopAnimating)
		{
			m_animationStopped = true;
			return;
		}
		m_loadingTile.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
		m_loadingTile.transform.localPosition = START_POS;
		m_progressBar.SetProgressBar(0f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", MID_POS);
		args.Add("islocal", true);
		args.Add("time", 0.5f);
		args.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.MoveTo(m_loadingTile, args);
		SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_move_down_and_forward.prefab:0b04e30939289024dadd8292dbfd7fef");
		Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
		args2.Add("position", END_POS);
		args2.Add("islocal", true);
		args2.Add("time", 0.5f);
		args2.Add("delay", 0.5f);
		args2.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.MoveTo(m_loadingTile, args2);
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("amount", new Vector3(180f, 0f, 0f));
		rotateArgs.Add("time", 0.5f);
		rotateArgs.Add("delay", 0.8f);
		rotateArgs.Add("easetype", iTween.EaseType.easeOutElastic);
		rotateArgs.Add("space", Space.Self);
		rotateArgs.Add("name", "flip");
		iTween.RotateAdd(m_loadingTile, rotateArgs);
		m_progressBar.SetLabel(GetRandomTaskName());
		StartCoroutine("AnimateBar");
	}

	private void AnimateOutLoadingTile()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", MID_POS);
		args.Add("islocal", true);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.MoveTo(m_loadingTile, args);
		SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_drop.prefab:899774ec312ca8241a5a5e4e300c5d93");
		Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
		args2.Add("position", OFFSCREEN_POS);
		args2.Add("islocal", true);
		args2.Add("time", 0.25f);
		args2.Add("delay", 0.25f);
		args2.Add("easetype", iTween.EaseType.easeOutCubic);
		args2.Add("oncomplete", "AnimateInLoadingTile");
		args2.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_loadingTile, args2);
	}

	private float GetRandomTaskDuration()
	{
		return 1f + UnityEngine.Random.value * 2f;
	}

	private string GetRandomTaskName()
	{
		List<string> taskNames;
		if (GameMgr.Get().IsSpectator())
		{
			taskNames = m_spectatorTaskNameMap;
		}
		else if (!m_adventureTaskNameMap.TryGetValue(m_adventureId, out taskNames))
		{
			taskNames = ((!GameUtils.IsMercenariesMission(m_scenarioId)) ? m_adventureTaskNameMap[AdventureDbId.INVALID] : m_mercenariesTaskNameList);
		}
		if (taskNames.Count == 0)
		{
			return "ERROR - OUT OF TASK NAMES!!!";
		}
		int index = UnityEngine.Random.Range(0, taskNames.Count);
		return taskNames[index];
	}

	private IEnumerator AnimateBar()
	{
		m_barAnimating = true;
		yield return new WaitForSeconds(0.8f);
		SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_flip.prefab:fa63e6a9075dba24fae4fddc2fd32a39");
		yield return new WaitForSeconds(0.19999999f);
		float timeToAnimate = GetRandomTaskDuration();
		m_progressBar.m_increaseAnimTime = timeToAnimate;
		m_progressBar.AnimateProgress(0f, 1f);
		SoundManager.Get().LoadAndPlay("StartGame_window_bar_filling_loop.prefab:4e8350e37c1218a4cbbd9e93b394cd48", null, 1f, LoopingSoundLoadedCallback);
		yield return new WaitForSeconds(timeToAnimate);
		StopLoopingSound();
		AnimateOutLoadingTile();
		m_barAnimating = false;
	}

	private void LoopingSoundLoadedCallback(AudioSource source, object userData)
	{
		StopLoopingSound();
		if (m_barAnimating)
		{
			m_loopSound = source;
		}
		else
		{
			SoundManager.Get().Stop(source);
		}
	}

	protected override void OnGameplaySceneLoaded()
	{
		StartCoroutine(StopLoading());
		Navigation.Clear();
	}

	private IEnumerator StopLoading()
	{
		m_stopAnimating = true;
		while (!m_animationStopped)
		{
			yield return null;
		}
		if (m_adventureId == AdventureDbId.PRACTICE)
		{
			int practiceProgress = Options.Get().GetInt(Option.TIP_PRACTICE_PROGRESS, 0);
			Options.Get().SetInt(Option.TIP_PRACTICE_PROGRESS, practiceProgress + 1);
		}
		Hide();
	}

	private void StopLoopingSound()
	{
		SoundManager.Get().Stop(m_loopSound);
		m_loopSound = null;
	}

	private bool OnNavigateBack()
	{
		if (!m_cancelButtonParent.gameObject.activeSelf)
		{
			return false;
		}
		StartCoroutine(StopLoading());
		FireMatchCanceledEvent();
		return true;
	}

	protected override void OnCancelButtonReleased(UIEvent e)
	{
		base.OnCancelButtonReleased(e);
		Navigation.GoBack();
	}

	private void GenerateStringNameMaps()
	{
		GenerateTaskNamesForAdventure(AdventureDbId.INVALID, "GLUE_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.NAXXRAMAS, "GLUE_NAXX_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.BRM, "GLUE_BRM_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.LOE, "GLUE_LOE_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.KARA, "GLUE_KARA_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.ICC, "GLUE_ICC_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.LOOT, "GLUE_LOOT_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.GIL, "GLUE_GIL_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.BOT, "GLUE_BOT_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.TRL, "GLUE_TRL_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.DALARAN, "GLUE_DAL_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.ULDUM, "GLUE_ULD_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.DRAGONS, "GLUE_DRG_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.BTA, "GLUE_BTA_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.BOH, "GLUE_BOH_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.BOM, "GLUE_BOH_LOADING_BAR_TASK_");
		GenerateTaskNamesForAdventure(AdventureDbId.ROTLK, "GLUE_BOH_LOADING_BAR_TASK_");
		GenerateTaskNamesForPrefix(m_mercenariesTaskNameList, "GLUE_LET_LOADING_BAR_TASK_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.LOOT, "GLUE_TIP_ADVENTURE_LOOT_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.GIL, "GLUE_TIP_ADVENTURE_GIL_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.BOT, "GLUE_TIP_ADVENTURE_BOT_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.TRL, "GLUE_TIP_ADVENTURE_TRL_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.DALARAN, "GLUE_TIP_ADVENTURE_DAL_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.ULDUM, "GLUE_TIP_ADVENTURE_ULD_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.DRAGONS, "GLUE_TIP_ADVENTURE_DRG_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.BTP, "GLUE_TIP_ADVENTURE_BTP_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.BTA, "GLUE_TIP_ADVENTURE_BTA_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.BOH, "GLUE_TIP_ADVENTURE_BOH_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.BOM, "GLUE_TIP_ADVENTURE_BOM_");
		GenerateTipOfTheDayNamesForAdventure(AdventureDbId.ROTLK, "GLUE_TIP_ADVENTURE_RLK_");
		GenerateTaskNamesForPrefix(m_spectatorTaskNameMap, "GLUE_SPECTATOR_LOADING_BAR_TASK_");
	}

	private void GenerateTaskNamesForAdventure(AdventureDbId adventureId, string prefix)
	{
		List<string> taskNames = new List<string>();
		GenerateTaskNamesForPrefix(taskNames, prefix);
		m_adventureTaskNameMap[adventureId] = taskNames;
	}

	private void GenerateTipOfTheDayNamesForAdventure(AdventureDbId adventureId, string prefix)
	{
		List<string> tipNames = new List<string>();
		GenerateTaskNamesForPrefix(tipNames, prefix);
		m_adventureTipOfTheDayNameMap[adventureId] = tipNames;
	}

	private void GenerateTaskNamesForPrefix(List<string> taskNames, string prefix)
	{
		taskNames.Clear();
		for (int i = 1; i < 100; i++)
		{
			string key = prefix + i;
			string name = GameStrings.Get(key);
			if (!(name == key))
			{
				taskNames.Add(name);
				continue;
			}
			break;
		}
	}

	private void SetTipOfTheDay()
	{
		if (GameUtils.IsMercenariesMission(m_scenarioId))
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.LETTUCE);
		}
		else if (m_adventureId == AdventureDbId.PRACTICE)
		{
			m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.PRACTICE, Options.Get().GetInt(Option.TIP_PRACTICE_PROGRESS, 0));
		}
		else if (GameUtils.IsExpansionAdventure(m_adventureId))
		{
			if (m_adventureTipOfTheDayNameMap.TryGetValue(m_adventureId, out var tips) && tips != null && tips.Count > 0)
			{
				int randomIndex = UnityEngine.Random.Range(0, tips.Count);
				m_tipOfTheDay.Text = tips[randomIndex];
			}
			else
			{
				m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.ADVENTURE);
			}
		}
		else if (m_scenarioId == 3539 || m_scenarioId == 3459)
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(new List<TipCategory>
			{
				TipCategory.BACON,
				TipCategory.BACON_SOLO
			});
		}
		else if (m_scenarioId == 5173)
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(new List<TipCategory>
			{
				TipCategory.BACON,
				TipCategory.BACON_DUO
			});
		}
		else
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.DEFAULT);
		}
	}

	private void SetLoadingBarTexture()
	{
		Texture barTexture = m_barTextures[0].texture;
		foreach (LoadingbarTexture loadingBarTexture in m_barTextures)
		{
			if (loadingBarTexture.adventureID == m_adventureId || loadingBarTexture.scenarioId == (ScenarioDbId)m_scenarioId)
			{
				barTexture = loadingBarTexture.texture;
				m_progressBar.m_barIntensity = loadingBarTexture.m_barIntensity;
				m_progressBar.m_barIntensityIncreaseMax = loadingBarTexture.m_barIntensityIncreaseMax;
				break;
			}
		}
		m_progressBar.SetBarTexture(barTexture);
	}
}
