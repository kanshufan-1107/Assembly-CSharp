using System;
using System.Collections.Generic;
using UnityEngine;

public class TeammatePingOptions : MonoBehaviour
{
	[Serializable]
	public class PingOption
	{
		public TEAMMATE_PING_TYPE type;

		[Tooltip("Show option when on friendly side")]
		public bool friendlySide;

		[Tooltip("Show option when on opposing side")]
		public bool opposingSide;

		[Tooltip("Show option when card is in your teammates gamestate")]
		public bool teammatesCards;

		[Tooltip("Show option when card is in your gamestate")]
		public bool myCards;

		[Tooltip("Only show during mulligan/hero slecet")]
		public bool onlyMulligan;
	}

	public List<PingOption> m_pingOptions = new List<PingOption>();

	private bool m_viewing;

	private PingOption m_currrentOption;

	private Actor m_actor;

	private bool m_isTeammatesCard;

	private PlayMakerFSM m_fsm;

	private bool m_mousedOver;

	private const string EXCLAMATION_SPRITE = "TeammatePing_Exclamation.png:63f21db6e902a6f45a23f66d9cb024d6";

	private const string CHECK_SPRITE = "TeammatePing_Check.png:7abbfd09067553343ad25ddd3d954830";

	private const string CROSS_SPRITE = "TeammatePing_X.png:85f9a6fc96185be4f9277afa1d296476";

	private const string WARP_SPRITE = "TeammatePing_Warp.png:11b6bd4333f59034dade71f6e85d1a38";

	private const string QUESTION_SPRITE = "TeammatePing_Question.png:b83549f046fc1fe4a8538653d3e365fe";

	private void Awake()
	{
		m_fsm = GetComponent<PlayMakerFSM>();
	}

	private void Update()
	{
		if (base.gameObject.layer != 8)
		{
			LayerUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
		}
	}

	private static string GetSpriteForPingType(TEAMMATE_PING_TYPE pingType)
	{
		return pingType switch
		{
			TEAMMATE_PING_TYPE.EXCLAMATION => "TeammatePing_Exclamation.png:63f21db6e902a6f45a23f66d9cb024d6", 
			TEAMMATE_PING_TYPE.CHECK => "TeammatePing_Check.png:7abbfd09067553343ad25ddd3d954830", 
			TEAMMATE_PING_TYPE.CROSS => "TeammatePing_X.png:85f9a6fc96185be4f9277afa1d296476", 
			TEAMMATE_PING_TYPE.WARP => "TeammatePing_Warp.png:11b6bd4333f59034dade71f6e85d1a38", 
			TEAMMATE_PING_TYPE.QUESTION => "TeammatePing_Question.png:b83549f046fc1fe4a8538653d3e365fe", 
			_ => "", 
		};
	}

	public static void SetSpriteFromPingType(SpriteRenderer spriteRenderer, TEAMMATE_PING_TYPE pingType)
	{
		string spritePath = GetSpriteForPingType(pingType);
		if (!string.IsNullOrEmpty(spritePath))
		{
			Sprite sprite = AssetLoader.Get().LoadAsset<Sprite>(spritePath);
			spriteRenderer.sprite = sprite;
		}
	}

	public void ShowPingOption(Actor actor)
	{
		bool cardIsFriendly = actor.GetEntity().IsControlledByFriendlySidePlayer();
		bool cardIsTeammates = TeammateBoardViewer.Get().IsActorTeammates(actor);
		bool mulliganActive = MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive();
		foreach (PingOption pingOption in m_pingOptions)
		{
			if ((!cardIsTeammates || !pingOption.teammatesCards) && (cardIsTeammates || !pingOption.myCards))
			{
				HidePingOption();
			}
			else if (pingOption.onlyMulligan && !mulliganActive)
			{
				HidePingOption();
			}
			else if ((cardIsFriendly && pingOption.friendlySide) || (!cardIsFriendly && pingOption.opposingSide))
			{
				SetSpriteFromPingType(base.transform.Find("PingIcon").GetComponent<SpriteRenderer>(), pingOption.type);
				base.gameObject.SetActive(value: true);
				LayerUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
				m_currrentOption = pingOption;
				m_actor = actor;
				m_isTeammatesCard = cardIsTeammates;
				break;
			}
		}
	}

	public void HidePingOption()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OptionSelected()
	{
		if (!(m_actor == null) && m_currrentOption != null)
		{
			m_actor.GetSpell(SpellType.TEAMMATE_PING);
			m_actor.PingSelected(m_currrentOption.type);
			Network.Get().SendPingTeammateEntity(m_actor.GetEntity().GetEntityId(), (int)m_currrentOption.type, m_isTeammatesCard);
			m_fsm.SendEvent("Clicked");
		}
	}

	public void MousedOver()
	{
		if (!m_mousedOver)
		{
			m_fsm.SendEvent("HoverOn");
			m_mousedOver = true;
		}
	}

	public void MousedOut()
	{
		if (m_mousedOver)
		{
			m_fsm.SendEvent("HoverOff");
			m_mousedOver = false;
		}
	}
}
