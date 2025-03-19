using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class BnetBarFriendButton : FriendListUIElement
{
	public UberText m_OnlineCountText;

	public Color m_AnyOnlineColor;

	public Color m_AllOfflineColor;

	public Color m_FSGColor;

	public GameObject m_PendingInvitesIcon;

	public GameObject m_FSGSocialBar;

	public GameObject m_FSGGlow;

	public GameObject m_Background;

	private static BnetBarFriendButton s_instance;

	private static bool m_hasClickedWhileFSGGlowing;

	private Material m_backgroundMaterial;

	private float m_originalLightingBlend;

	protected override void Awake()
	{
		s_instance = this;
		base.Awake();
		SetBackground();
		UpdateOnlineCount();
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		ShowPendingInvitesIcon(show: false);
	}

	private void SetBackground()
	{
		if (m_Background != null)
		{
			MeshRenderer backgroundRenderer = m_Background.GetComponent<MeshRenderer>();
			if (backgroundRenderer != null)
			{
				m_backgroundMaterial = backgroundRenderer.GetMaterial();
				m_originalLightingBlend = m_backgroundMaterial.GetFloat("_LightingBlend");
			}
		}
	}

	protected override void OnDestroy()
	{
		if (BnetFriendMgr.Get() != null)
		{
			BnetFriendMgr.Get().RemoveChangeListener(OnFriendsChanged);
		}
		if (BnetPresenceMgr.Get() != null)
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
		}
		if (FatalErrorMgr.Get() != null)
		{
			FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		}
		s_instance = null;
		base.OnDestroy();
	}

	public static BnetBarFriendButton Get()
	{
		return s_instance;
	}

	public void HideTooltip()
	{
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (tooltipZone != null)
		{
			tooltipZone.HideTooltip();
		}
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		UpdateOnlineCount();
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		UpdateOnlineCount();
	}

	private void OnJoinFSG(FSGConfig gathering)
	{
		m_Background.SetActive(value: false);
		m_FSGSocialBar.SetActive(value: true);
		UpdateOnlineCount();
	}

	private void OnLeaveFSG(FSGConfig gathering)
	{
		m_Background.SetActive(value: true);
		m_FSGSocialBar.SetActive(value: false);
		UpdateOnlineCount();
	}

	private void OnNearbyFSGs()
	{
		if (!m_hasClickedWhileFSGGlowing)
		{
			m_FSGGlow.SetActive(value: true);
		}
	}

	private void OnFSGPatronsUpdated(List<BnetPlayer> addedList, List<BnetPlayer> removedList)
	{
		UpdateOnlineCount();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		UpdateOnlineCount();
	}

	public void UpdateOnlineCount()
	{
		int count = BnetFriendMgr.Get().GetOnlineFriendCount();
		GameMgr gameMgr = GameMgr.Get();
		if (gameMgr == null || !gameMgr.IsTraditionalTutorial())
		{
			Color color = ((count == 0) ? m_AllOfflineColor : m_AnyOnlineColor);
			m_OnlineCountText.TextColor = color;
		}
		m_OnlineCountText.Text = count.ToString();
	}

	public void ShowPendingInvitesIcon(bool show)
	{
		if (m_PendingInvitesIcon != null && m_PendingInvitesIcon.activeInHierarchy != show)
		{
			m_PendingInvitesIcon.SetActive(show);
			m_OnlineCountText.gameObject.SetActive(!show);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9");
		UpdateHighlight();
	}

	protected override void OnOut(InteractionState oldState)
	{
		base.OnOut(oldState);
	}

	public override void SetEnabled(bool enabled, bool isInternal = false)
	{
		base.SetEnabled(enabled, isInternal);
		if (!enabled)
		{
			UpdateHighlight();
		}
		if (m_backgroundMaterial != null)
		{
			m_backgroundMaterial.SetFloat("_LightingBlend", enabled ? m_originalLightingBlend : 0.8f);
		}
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		if (!m_hasClickedWhileFSGGlowing && m_FSGGlow.activeInHierarchy)
		{
			m_FSGGlow.SetActive(value: false);
			m_hasClickedWhileFSGGlowing = true;
		}
	}

	public void SetColor(Color matColor)
	{
		m_OnlineCountText.TextColor = matColor;
		if (m_backgroundMaterial == null)
		{
			SetBackground();
		}
		m_backgroundMaterial.SetFloat("_LightingBlend", m_originalLightingBlend);
	}
}
