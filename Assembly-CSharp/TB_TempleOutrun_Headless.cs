using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TB_TempleOutrun_Headless : ULDA_Dungeon
{
	public struct PopupMessage
	{
		public string Message;

		public float Delay;
	}

	private static readonly AssetReference VO_HeadlessHorseman_Male_Human_HHIntro_02 = new AssetReference("VO_HeadlessHorseman_Male_Human_HHIntro_02.prefab:0dc446d089c1c6142819ecd89009e9bf");

	private static readonly AssetReference VO_HeadlessHorseman_Male_Human_HHReaction1_01 = new AssetReference("VO_HeadlessHorseman_Male_Human_HHReaction1_01.prefab:8443a7874cc9cbb48a30f57d69e1b431");

	private static readonly AssetReference VO_HeadlessHorseman_Male_Human_HallowsEve_19 = new AssetReference("VO_HeadlessHorseman_Male_Human_HallowsEve_19.prefab:74a92ec2af554f94fb8c6e205c561bde");

	private List<string> m_HeroPowerLines = new List<string>();

	private List<string> m_IdleLines = new List<string>();

	private Notification m_popup;

	private float popupScale = 1.4f;

	private static readonly Dictionary<int, PopupMessage> popupMsgs = new Dictionary<int, PopupMessage>
	{
		{
			1000,
			new PopupMessage
			{
				Message = "TB_EVILBRM_CURRENT_BEST_SCORE",
				Delay = 5f
			}
		},
		{
			2000,
			new PopupMessage
			{
				Message = "TB_EVILBRM_NEW_BEST_SCORE",
				Delay = 5f
			}
		}
	};

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_HeadlessHorseman_Male_Human_HHIntro_02, VO_HeadlessHorseman_Male_Human_HHReaction1_01, VO_HeadlessHorseman_Male_Human_HallowsEve_19 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override List<string> GetIdleLines()
	{
		return m_IdleLines;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_HeroPowerLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_standardEmoteResponseLine = VO_HeadlessHorseman_Male_Human_HallowsEve_19;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.None, enemyActor));
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		while (m_enemySpeaking)
		{
			yield return null;
		}
		switch (missionEvent)
		{
		case 1010:
		{
			Debug.Log("Got Case 1010");
			int scriptDataNum1 = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
			string msgString;
			if (scriptDataNum1 == 0)
			{
				msgString = GameStrings.Get(popupMsgs[2000].Message);
			}
			else
			{
				string hoursBuffer = "";
				string minutesBuffer = "";
				string secondsBuffer = "";
				int hours = scriptDataNum1 / 3600;
				int minutes = scriptDataNum1 % 3600 / 60;
				int seconds = scriptDataNum1 % 60;
				if (hours < 10)
				{
					hoursBuffer = "0";
				}
				if (minutes < 10)
				{
					minutesBuffer = "0";
				}
				if (seconds < 10)
				{
					secondsBuffer = "0";
				}
				msgString = GameStrings.Get(popupMsgs[missionEvent].Message) + "\n" + hoursBuffer + hours + ":" + minutesBuffer + minutes + ":" + secondsBuffer + seconds;
				popupScale = 1.7f;
			}
			Vector3 popUpPos = default(Vector3);
			if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
			{
				popUpPos.z = (UniversalInputManager.UsePhoneUI ? 27f : 18f);
			}
			else
			{
				popUpPos.z = (UniversalInputManager.UsePhoneUI ? (-40f) : (-40f));
			}
			yield return new WaitForSeconds(4f);
			yield return ShowPopup(msgString, popupMsgs[missionEvent].Delay, popUpPos, popupScale);
			break;
		}
		case 100:
			Debug.Log("Got Case 100");
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_HeadlessHorseman_Male_Human_HHIntro_02, Notification.SpeechBubbleDirection.TopRight, enemyActor));
			break;
		case 101:
			Debug.Log("Got Case 101");
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_HeadlessHorseman_Male_Human_HHReaction1_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
			break;
		}
	}

	private IEnumerator ShowPopup(string stringID, float popupDuration, Vector3 popUpPos, float popupScale)
	{
		m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popupScale, GameStrings.Get(stringID), convertLegacyPosition: false);
		NotificationManager.Get().DestroyNotification(m_popup, popupDuration);
		yield return new WaitForSeconds(0f);
	}
}
