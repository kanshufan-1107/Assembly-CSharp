using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.Progression;
using UnityEngine;

public class VictoryScreenICCFinale : VictoryScreen
{
	public Animation m_BurnAwayAnimation;

	public Renderer m_LichPortraitRenderer;

	public string m_PortraitTextureName;

	public AudioSource m_PreLichTransformationAudio;

	public AudioSource m_MidLichTransformationAudio;

	public AudioSource m_HideTwoScoopsAudio;

	public List<AudioSource> m_HeroPostcardShowAudio;

	public AudioSource m_HeroPostcardHideAudio;

	public UberText m_HeroNameText;

	public UberText m_HeroDescriptionText;

	public UIBButton m_ContinueButton;

	public GameObject m_HeroPostcard;

	public MeshRenderer m_HeroPostcardRenderer;

	public Material m_DruidPostcardMaterial;

	public Material m_HunterPostcardMaterial;

	public Material m_MagePostcardMaterial;

	public Material m_PaladinPostcardMaterial;

	public Material m_PriestPostcardMaterial;

	public Material m_RoguePostcardMaterial;

	public Material m_ShamanPostcardMaterial;

	public Material m_WarlockPostcardMaterial;

	public Material m_WarriorPostcardMaterial;

	private Map<TAG_CLASS, string> m_heroDesriptions = new Map<TAG_CLASS, string>
	{
		{
			TAG_CLASS.DRUID,
			"ICCDruidVictory_01"
		},
		{
			TAG_CLASS.HUNTER,
			"ICCHunterVictory_01"
		},
		{
			TAG_CLASS.MAGE,
			"ICCMageVictory_01"
		},
		{
			TAG_CLASS.PALADIN,
			"ICCPaladinVictory_01"
		},
		{
			TAG_CLASS.PRIEST,
			"ICCPriestVictory_01"
		},
		{
			TAG_CLASS.ROGUE,
			"ICCRogueVictory_01"
		},
		{
			TAG_CLASS.SHAMAN,
			"ICCShamanVictory_01"
		},
		{
			TAG_CLASS.WARLOCK,
			"ICCWarlockVictory_01"
		},
		{
			TAG_CLASS.WARRIOR,
			"ICCWarriorVictory_01"
		}
	};

	private Map<TAG_CLASS, string> m_heroMap = new Map<TAG_CLASS, string>
	{
		{
			TAG_CLASS.DRUID,
			"HERO_06"
		},
		{
			TAG_CLASS.HUNTER,
			"HERO_05"
		},
		{
			TAG_CLASS.MAGE,
			"HERO_08"
		},
		{
			TAG_CLASS.PALADIN,
			"HERO_04"
		},
		{
			TAG_CLASS.PRIEST,
			"HERO_09"
		},
		{
			TAG_CLASS.ROGUE,
			"HERO_03"
		},
		{
			TAG_CLASS.SHAMAN,
			"HERO_02"
		},
		{
			TAG_CLASS.WARLOCK,
			"HERO_07"
		},
		{
			TAG_CLASS.WARRIOR,
			"HERO_01"
		}
	};

	private Map<TAG_CLASS, string> m_dkHeroMap = new Map<TAG_CLASS, string>
	{
		{
			TAG_CLASS.DRUID,
			"ICC_832"
		},
		{
			TAG_CLASS.HUNTER,
			"ICC_828"
		},
		{
			TAG_CLASS.MAGE,
			"ICC_833"
		},
		{
			TAG_CLASS.PALADIN,
			"ICC_829"
		},
		{
			TAG_CLASS.PRIEST,
			"ICC_830"
		},
		{
			TAG_CLASS.ROGUE,
			"ICC_827"
		},
		{
			TAG_CLASS.SHAMAN,
			"ICC_481"
		},
		{
			TAG_CLASS.WARLOCK,
			"ICC_831"
		},
		{
			TAG_CLASS.WARRIOR,
			"ICC_834"
		}
	};

	private bool m_dismissedTwoScoops;

	private DefLoader.DisposableCardDef m_heroSkinCardDef;

	private static readonly float TIRION_VO_DELAY = 5f;

	private static readonly float POSTCARD_DELAY = 2f;

	protected override void Awake()
	{
		base.Awake();
		Card friendlyStartingHero = GameState.Get().GetFriendlySidePlayer().GetStartingHero()
			.GetCard();
		Entity friendlyStartingHeroEntity = friendlyStartingHero.GetEntity();
		TAG_CLASS friendlyStartingHeroClass = friendlyStartingHero.GetEntity().GetClass();
		m_heroMap.TryGetValue(friendlyStartingHeroClass, out var heroCardID);
		if (!string.IsNullOrEmpty(heroCardID))
		{
			if (heroCardID == friendlyStartingHeroEntity.GetCardId())
			{
				m_LichPortraitRenderer.GetMaterial().SetTexture(m_PortraitTextureName, friendlyStartingHero.GetPortraitTexture());
			}
			else
			{
				m_heroSkinCardDef?.Dispose();
				m_heroSkinCardDef = DefLoader.Get().GetCardDef(heroCardID);
				if (m_heroSkinCardDef?.CardDef != null)
				{
					m_LichPortraitRenderer.GetMaterial().SetTexture(m_PortraitTextureName, m_heroSkinCardDef.CardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL));
				}
			}
		}
		VictoryTwoScoop victoryTwoScoop = m_twoScoop as VictoryTwoScoop;
		if (victoryTwoScoop == null)
		{
			Log.Gameplay.PrintError("VictoryScreenICCPrologue.Awake() - m_twoScoop is not an instance of VictoryTwoScoop!");
		}
		m_dkHeroMap.TryGetValue(friendlyStartingHeroClass, out var dkCardID);
		if (!string.IsNullOrEmpty(dkCardID))
		{
			EntityDef dkEntityDef = DefLoader.Get().GetEntityDef(dkCardID);
			if (dkEntityDef != null)
			{
				m_HeroNameText.Text = dkEntityDef.GetName();
				victoryTwoScoop.SetOverrideHero(dkEntityDef);
			}
		}
		m_heroDesriptions.TryGetValue(friendlyStartingHeroClass, out var dkHeroDescriptionGameString);
		if (!string.IsNullOrEmpty(dkHeroDescriptionGameString))
		{
			m_HeroDescriptionText.Text = GameStrings.Get(dkHeroDescriptionGameString);
		}
	}

	protected override void OnDestroy()
	{
		m_heroSkinCardDef?.Dispose();
		m_heroSkinCardDef = null;
		base.OnDestroy();
	}

	protected override void ShowStandardFlow()
	{
		ShowTwoScoop();
		ShowRewardsXpGains();
	}

	protected override void OnTwoScoopShown()
	{
		base.OnTwoScoopShown();
		StartCoroutine(PlayAnim());
	}

	private IEnumerator PlayAnim()
	{
		yield return new WaitForSeconds(TIRION_VO_DELAY);
		while (NotificationManager.Get().IsQuotePlaying)
		{
			yield return null;
		}
		GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor()
			.GetSpell(SpellType.ENDGAME_LOSE_ENEMY)
			.ActivateState(SpellStateType.DEATH);
		VictoryTwoScoop victoryTwoScoop = m_twoScoop as VictoryTwoScoop;
		if (victoryTwoScoop != null)
		{
			victoryTwoScoop.StopFireworksAudio();
		}
		else
		{
			Log.Gameplay.PrintError("VictoryScreenICCFinale.PlayAnim(): m_twoScoop is not of type VictoryTwoScoop!");
		}
		if (GameState.Get().GetGameEntity() is ICC_08_Finale missionEntity)
		{
			yield return StartCoroutine(missionEntity.PlayTirionVictoryVO());
			Coroutine enumerator = StartCoroutine(missionEntity.PlayFriendlyHeroVictoryVO(m_twoScoop.m_heroActor, m_PreLichTransformationAudio, m_MidLichTransformationAudio));
			yield return new WaitForSeconds(1f);
			m_BurnAwayAnimation["LichHeroBurnAway"].speed = 0.4f;
			m_BurnAwayAnimation.Play("LichHeroBurnAway");
			yield return new WaitForSeconds(2f);
			if (m_twoScoop.m_heroBone != null)
			{
				m_twoScoop.m_heroBone.SetActive(value: false);
			}
			yield return enumerator;
		}
		else
		{
			Log.Gameplay.PrintError("VictoryScreenICCEpilogue.PlayAnim(): GameEntity is not an instance of ICC_08_Finale!.");
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_continueText.gameObject.SetActive(value: true);
		}
		m_hitbox.AddEventListener(UIEventType.RELEASE, ContinueButtonPress_HideTwoScoop);
		while (!m_dismissedTwoScoops)
		{
			yield return null;
		}
		m_hitbox.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_HideTwoScoop);
		m_hitbox.gameObject.SetActive(value: false);
		RewardXpNotificationManager.Get()?.TerminateEarly();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_continueText.gameObject.SetActive(value: false);
		}
		yield return new WaitForSeconds(POSTCARD_DELAY);
		if (m_HeroPostcard != null)
		{
			if (m_HeroPostcardRenderer != null)
			{
				Material material = GetPostcardMaterialForClass(GameState.Get().GetFriendlySidePlayer().GetStartingHero()
					.GetClass());
				m_HeroPostcardRenderer.SetMaterial(material);
			}
			m_HeroPostcard.SetActive(value: true);
			foreach (AudioSource audioSource in m_HeroPostcardShowAudio)
			{
				if (audioSource != null)
				{
					SoundManager.Get().Play(audioSource);
				}
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("scale", new Vector3(0.01f, 0.01f, 0.01f));
			args.Add("time", 0.5f);
			args.Add("easetype", iTween.EaseType.easeOutBounce);
			iTween.ScaleFrom(m_HeroPostcard, args);
		}
		else
		{
			Log.Gameplay.PrintError("VictoryScreenICCFinale.PlayAnim(): m_HeroPostcard is null!");
		}
		m_ContinueButton.AddEventListener(UIEventType.RELEASE, ContinueButtonPress_DismissPostcard);
	}

	private void ContinueButtonPress_HideTwoScoop(UIEvent e)
	{
		if (m_HideTwoScoopsAudio != null)
		{
			SoundManager.Get().Play(m_HideTwoScoopsAudio);
		}
		m_twoScoop.Hide();
		m_dismissedTwoScoops = true;
	}

	private void ContinueButtonPress_DismissPostcard(UIEvent e)
	{
		m_ContinueButton.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_DismissPostcard);
		StartCoroutine(DismissPostcard());
	}

	private IEnumerator DismissPostcard()
	{
		if (m_HeroPostcardHideAudio != null)
		{
			SoundManager.Get().Play(m_HeroPostcardHideAudio);
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", new Vector3(0.01f, 0.01f, 0.01f));
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.linear);
		iTween.ScaleTo(m_HeroPostcard, args);
		while (iTween.HasTween(m_HeroPostcard))
		{
			yield return null;
		}
		m_HeroPostcard.SetActive(value: false);
		ContinueEvents();
	}

	private Material GetPostcardMaterialForClass(TAG_CLASS classType)
	{
		return classType switch
		{
			TAG_CLASS.DRUID => m_DruidPostcardMaterial, 
			TAG_CLASS.HUNTER => m_HunterPostcardMaterial, 
			TAG_CLASS.MAGE => m_MagePostcardMaterial, 
			TAG_CLASS.PALADIN => m_PaladinPostcardMaterial, 
			TAG_CLASS.PRIEST => m_PriestPostcardMaterial, 
			TAG_CLASS.ROGUE => m_RoguePostcardMaterial, 
			TAG_CLASS.SHAMAN => m_ShamanPostcardMaterial, 
			TAG_CLASS.WARLOCK => m_WarlockPostcardMaterial, 
			TAG_CLASS.WARRIOR => m_WarriorPostcardMaterial, 
			_ => m_MagePostcardMaterial, 
		};
	}
}
