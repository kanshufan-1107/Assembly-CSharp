using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoH_Valeera_08 : BoH_Valeera_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8EmoteResponse_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8EmoteResponse_01.prefab:0723307cf76586348911e4261e586673");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeA_01.prefab:ef35cbf633307724cbe2f7ddebcf54da");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeB_01.prefab:eab885d89886d8446919a714a40d10d4");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeC_02 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeC_02.prefab:6b2374883d397c444a18a82f36e812a0");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_01.prefab:deb8713c125651d409bf412e056dd506");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_02 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_02.prefab:190187b6bb313974bbfedd59cc012f72");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_03 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_03.prefab:0bc78b3600f7e0347b9490d7f3948600");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Intro_02.prefab:709bd338ef18f964bbcb5f827e252c62");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Loss_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Loss_01.prefab:9c67722a24f784d4baaa83bd706f1fe2");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_01.prefab:f813048b715c4c642b80960a3c7dd6b9");

	private static readonly AssetReference VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_03 = new AssetReference("VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_03.prefab:692bd7580a9819d468287265230f8b95");

	private static readonly AssetReference VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeA_02 = new AssetReference("VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeA_02.prefab:5bad61ef61d7b9e4eb89daee515e1b60");

	private static readonly AssetReference VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeC_01.prefab:13aaf782442e9d640bbff9b51220295c");

	private static readonly AssetReference VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Intro_01.prefab:a7233807d400b874cbef3a042fb0993c");

	private static readonly AssetReference VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Victory_02.prefab:480a3cbda32cba048a7b9f84a022c433");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOH_VALEERA_08b" }
	} };

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_02, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>
		{
			VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8EmoteResponse_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeA_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeB_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeC_02, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_02, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Idle_03, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Intro_02, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Loss_01, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_01,
			VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_03, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeA_02, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeC_01, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Intro_01, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Victory_02
		};
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().SetBusy(busy: true);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Intro_01);
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Intro_02);
		GameState.Get().SetBusy(busy: false);
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_GILFinalBoss;
		m_standardEmoteResponseLine = VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8EmoteResponse_01;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 507:
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Loss_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_01);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8Victory_02);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
		{
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return new WaitForSeconds(3.5f);
			NotificationManager.Get().DestroyNotification(popup, 0f);
			break;
		}
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return base.RespondToPlayedCardWithTiming(entity);
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeA_01);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeA_02);
			break;
		case 7:
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeB_01);
			break;
		case 11:
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Valeera_Female_BloodElf_Story_Valeera_Mission8ExchangeC_01);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Jorach_Male_Human_Story_Valeera_Mission8ExchangeC_02);
			break;
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
		m_turnCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmBool("RunningMan").Value = true;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = false;
		component.SendEvent("Birth");
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_turnCounter.transform.parent = enemyActor.gameObject.transform;
		m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
		m_turnCounter.transform.localScale = Vector3.one * 0.52f;
		UpdateTurnCounterText(cost);
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOH_VALEERA_08", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
