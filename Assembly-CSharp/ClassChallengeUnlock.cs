using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class ClassChallengeUnlock : Reward
{
	[CustomEditField(Sections = "Container")]
	public UIBObjectSpacing m_classFrameContainer;

	[CustomEditField(Sections = "Text Settings")]
	public UberText m_headerText;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_appearSound;

	private List<GameObject> m_classFrames = new List<GameObject>();

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_rewardBanner.transform.localScale = m_rewardBanner.transform.localScale * 8f;
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public static List<AdventureMissionDbfRecord> AdventureMissionsUnlockedByWingId(int wingId)
	{
		List<AdventureMissionDbfRecord> missions = new List<AdventureMissionDbfRecord>();
		foreach (AdventureMissionDbfRecord record in GameDbf.AdventureMission.GetRecords())
		{
			if (record.ReqWingId == wingId)
			{
				int scenarioId = record.ScenarioId;
				ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(scenarioId);
				if (scenario == null)
				{
					Debug.LogError($"Unable to find Scenario record with ID: {scenarioId}");
				}
				else if (scenario.ModeId == 4)
				{
					missions.Add(record);
				}
			}
		}
		return missions;
	}

	protected override void InitData()
	{
		SetData(new ClassChallengeUnlockData(), updateVisuals: false);
	}

	protected override void PlayShowSounds()
	{
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		m_classFrameContainer.UpdatePositions();
		foreach (GameObject classFrame in m_classFrames)
		{
			classFrame.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", new Vector3(0f, 0f, 540f));
			args.Add("time", 1.5f);
			args.Add("easetype", iTween.EaseType.easeOutElastic);
			args.Add("space", Space.Self);
			iTween.RotateAdd(classFrame, args);
		}
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 1f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_screenEffectsHandle.StopEffect(DestroyClassChallengeUnlock);
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is ClassChallengeUnlockData unlockData))
		{
			Debug.LogWarning($"ClassChallengeUnlock.OnDataSet() - Data {base.Data} is not ClassChallengeUnlockData");
			return;
		}
		List<string> classPrefabsToShow = new List<string>();
		List<string> classNames = new List<string>();
		foreach (AdventureMissionDbfRecord record in AdventureMissionsUnlockedByWingId(unlockData.WingID))
		{
			int scenarioId = record.ScenarioId;
			ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(scenarioId);
			if (scenario == null)
			{
				Debug.LogError($"Unable to find Scenario record with ID: {scenarioId}");
			}
			else if (!string.IsNullOrEmpty(record.ClassChallengePrefabPopup))
			{
				DbfLocValue classChallengeName = scenario.ShortName;
				classPrefabsToShow.Add(record.ClassChallengePrefabPopup);
				classNames.Add(classChallengeName);
			}
			else
			{
				Debug.LogWarning($"CLASS_CHALLENGE_PREFAB_POPUP not define for AdventureMission SCENARIO_ID: {scenarioId}");
			}
		}
		if (classPrefabsToShow.Count == 0)
		{
			Debug.LogError($"Unable to find AdventureMission record with REQ_WING_ID: {unlockData.WingID}.");
			return;
		}
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = classPrefabsToShow.Count
			}
		};
		m_headerText.Text = GameStrings.FormatPlurals("GLOBAL_REWARD_CLASS_CHALLENGE_HEADLINE", pluralNumbers);
		string rewardDetails = ((classPrefabsToShow.Count <= 0) ? "" : string.Join(", ", classNames.ToArray()));
		string sourceText = GameDbf.Wing.GetRecord(unlockData.WingID).ClassChallengeRewardSource;
		SetRewardText(rewardDetails, string.Empty, sourceText);
		foreach (string prefabPath in classPrefabsToShow)
		{
			GameObject classFrame = AssetLoader.Get().InstantiatePrefab(prefabPath);
			if (!(classFrame == null))
			{
				GameUtils.SetParent(classFrame, m_classFrameContainer);
				classFrame.transform.localRotation = Quaternion.identity;
				m_classFrameContainer.AddObject(classFrame);
				m_classFrames.Add(classFrame);
			}
		}
		m_classFrameContainer.UpdatePositions();
		SetReady(ready: true);
		EnableClickCatcher(enabled: true);
		RegisterClickListener(OnClicked);
	}

	private void OnClicked(Reward reward, object userData)
	{
		HideReward();
	}

	private void DestroyClassChallengeUnlock()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
