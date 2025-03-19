using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Faelin_08 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission8_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission8_01.prefab:7d8d790efe69a984ab1ba100fcc3b8ba");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeA_01.prefab:d5b0eaf00b54de6429b561a836800fca");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeF_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeF_01.prefab:46369b303279e28439b60ce90e9f2dfb");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Start_01.prefab:98e13ee59e7c21c42b3f880cff0f632b");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_01.prefab:5b28472ff57c86644b829dd15567a261");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_02.prefab:9f4ab1ffc6509d24ca8fbecfadeeddd6");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission8_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission8_01.prefab:09becc0bc2c6a3a4ebdab286f6b380c5");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_01.prefab:ec4afe4d893bf38438d61c7f38c4ef9d");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_02.prefab:5602b344563ad2149bda928a72ab1766");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01.prefab:3a5e940ad8c7aed4a9788c7336e3c57e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02.prefab:07a4ee4bfb204fd44a4c03656d53ed57");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03.prefab:4f95caa9522cbe14ea4e9f5c0c7a9f9c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_04 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_04.prefab:67293738c15302947a9c0c493d573ac7");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_05 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_05.prefab:a1868e7f968aa00469414f8edb9728fa");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeA_01.prefab:0e076cf0fbd990848a0affd995f48182");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeB_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeB_01.prefab:80c63c7cb4fd77d418a8bfccf57e20a7");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeC_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeC_01.prefab:2ec1ca0ee393a5c4b8d53a96fd795660");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeD_01.prefab:b49cbe405195b6148bfaaede3f8ae688");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeE_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeE_01.prefab:1c73daba59a173d4f8572b5a1d52dbe6");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeF_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeF_01.prefab:df4f25624a5ccdd4fb66a0e57fe50c1e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_01.prefab:9e383c3e57577d5478db79f9bd56044d");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_02.prefab:cb1488da7676d584eb25b18bf48c9e37");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_01.prefab:61fa161f66762304e98aed938672e75b");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_02.prefab:3b5c8b1fa064a0949b5b74c3acd10dc6");

	private static readonly AssetReference Story_11_Leviathan_001hb_Death_Sound = new AssetReference("Story_11_Leviathan_001hb_Death_Sound.prefab:d5051de62f48f4b49aa855220792f2f4");

	private static readonly AssetReference Story_11_Leviathan_001hb_EmoteResponse_Sound = new AssetReference("Story_11_Leviathan_001hb_EmoteResponse_Sound.prefab:90b2aae30ca06f648a8033e394c1fa22");

	private static readonly AssetReference Story_11_Leviathan_001hb_Loss_Sound = new AssetReference("Story_11_Leviathan_001hb_Loss_Sound.prefab:c8702d2dff96aa1468e091e19a0f99cb");

	private static readonly AssetReference Story_11_Leviathan_001hb_Rev_Sound = new AssetReference("Story_11_Leviathan_001hb_Rev_Sound.prefab:63e2a91d4276f1a429b25b7ddcddd53d");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private List<string> m_NegativeReactionLines = new List<string> { VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03 };

	private List<string> m_PositiveReactionLines = new List<string> { VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_04, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_05 };

	private List<string> m_LossLines = new List<string> { VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_02 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_08()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Start_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeB_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeC_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_04, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_05, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeD_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeE_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeF_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeF_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_02,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Loss_02, Story_11_Leviathan_001hb_Death_Sound, Story_11_Leviathan_001hb_EmoteResponse_Sound, Story_11_Leviathan_001hb_Loss_Sound, Story_11_Leviathan_001hb_Rev_Sound
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_Mission_EnemyPlayIdleLines = false;
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Leviathan;
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
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_EmoteResponse_Sound);
			break;
		case 107:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Start_02);
			break;
		case 520:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Loss_Sound);
			yield return MissionPlayVO(friendlyActor, m_LossLines);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8Victory_02);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeB_01);
			break;
		case 102:
			yield return PlayLineInOrderOnce(friendlyActor, m_NegativeReactionLines);
			break;
		case 103:
			yield return PlayLineInOrderOnce(friendlyActor, m_PositiveReactionLines);
			break;
		case 104:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeD_01);
			break;
		case 105:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeE_01);
			break;
		case 106:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeF_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeF_01);
			break;
		case 108:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission8ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8ExchangeA_01);
			break;
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}
}
