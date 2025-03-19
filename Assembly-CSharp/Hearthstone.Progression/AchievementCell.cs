using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementCell : MonoBehaviour
{
	public const string CLAIM_ACHIEVEMENT = "CODE_CLAIM_ACHIEVEMENT";

	public const string ACHIEVEMENT_CHANGED = "CODE_ACHIEVEMENT_CHANGED";

	public const string CLAIM_RESPONSE_RECEIVED = "CODE_CLAIM_RESPONSE_RECEIVED";

	public const string CLAIM_ANIMATION_COMPLETED = "CODE_CLAIM_ANIMATION_COMPLETED";

	public const string SHOW_ACHIEVEMENT_TILE_DELAY = "CODE_SHOW_ACHIEVEMENT_TILE_DELAY";

	public const string HIDE_ACHIEVEMENT_TILE = "CODE_HIDE_ACHIEVEMENT_TILE";

	public const string SHOW_ACHIEVEMENT_TILE = "CODE_SHOW_ACHIEVEMENT_TILE";

	public static float ACHIEVEMENT_SHOW_DELAY = 0.1f;

	[SerializeField]
	private Widget m_achievementTile;

	private Widget m_widget;

	private bool m_waitingForClaimResponse;

	private bool m_waitingForClaimAnimation;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		AchievementSection section = GetComponentInParent<AchievementSection>();
		if (section != null)
		{
			section.OnSectionChanged += HandleSectionChanged;
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		m_waitingForClaimResponse = false;
		m_waitingForClaimAnimation = false;
		AchievementSection section = GetComponentInParent<AchievementSection>();
		if (section != null)
		{
			section.OnSectionChanged -= HandleSectionChanged;
		}
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CLAIM_ACHIEVEMENT":
			HandleClaimAchievement();
			break;
		case "CODE_ACHIEVEMENT_CHANGED":
			HandleCellAchievementChanged();
			break;
		case "CODE_CLAIM_RESPONSE_RECEIVED":
			HandleClaimResponseReceived();
			break;
		case "CODE_CLAIM_ANIMATION_COMPLETED":
			HandleClaimAnimationCompleted();
			break;
		case "CODE_SHOW_ACHIEVEMENT_TILE_DELAY":
			HandleShowAchievementTile();
			break;
		case "CODE_HIDE_ACHIEVEMENT_TILE":
			m_waitingForClaimResponse = false;
			m_waitingForClaimAnimation = false;
			break;
		}
	}

	private void HandleClaimAchievement()
	{
		if (!Network.IsLoggedIn())
		{
			ProgressUtils.ShowOfflinePopup();
			return;
		}
		AchievementDataModel achievement = m_widget.GetDataModel<AchievementDataModel>();
		if (achievement == null)
		{
			Debug.LogWarning("Unexpected state: no bound achievement.");
		}
		else
		{
			StartClaimSequence(achievement.ID);
		}
	}

	private void StartClaimSequence(int achievementId)
	{
		if (AchievementManager.Get().ClaimAchievementReward(achievementId))
		{
			m_waitingForClaimResponse = true;
			m_waitingForClaimAnimation = true;
		}
	}

	private void HandleSectionChanged(AchievementSectionDataModel section)
	{
		StopAllCoroutines();
		m_waitingForClaimResponse = false;
		m_waitingForClaimAnimation = false;
		HideTile();
	}

	private void HandleCellAchievementChanged()
	{
		m_waitingForClaimResponse = false;
		UpdateIfReady();
	}

	private void HandleClaimResponseReceived()
	{
		m_waitingForClaimResponse = false;
		UpdateIfReady();
	}

	private void HandleClaimAnimationCompleted()
	{
		m_waitingForClaimAnimation = false;
		UpdateIfReady();
	}

	private void UpdateIfReady()
	{
		if (!m_waitingForClaimResponse && !m_waitingForClaimAnimation)
		{
			UpdateTile();
		}
	}

	private void UpdateTile()
	{
		AchievementDataModel achievement = m_widget.GetDataModel<AchievementDataModel>();
		if (achievement == null)
		{
			Debug.LogWarning("Unexpected state: no bound achievement");
		}
		else
		{
			m_achievementTile.BindDataModel(achievement);
		}
	}

	private void HideTile()
	{
		m_widget.TriggerEvent("CODE_HIDE_ACHIEVEMENT_TILE");
		m_waitingForClaimResponse = false;
		m_waitingForClaimAnimation = false;
	}

	private void HandleShowAchievementTile()
	{
		StartCoroutine(WaitAndShowTile());
	}

	private IEnumerator WaitAndShowTile()
	{
		AchievementDataModel achievement = m_widget.GetDataModel<AchievementDataModel>();
		while (achievement == null)
		{
			if (m_achievementTile == null || m_widget == null)
			{
				yield break;
			}
			achievement = m_widget.GetDataModel<AchievementDataModel>();
			yield return null;
		}
		yield return new WaitForSeconds(achievement.DisplayDelay);
		if (!(m_achievementTile == null) && !(m_widget == null))
		{
			m_achievementTile.BindDataModel(achievement);
			m_achievementTile.TriggerEvent("CODE_SHOW_ACHIEVEMENT_TILE");
		}
	}
}
