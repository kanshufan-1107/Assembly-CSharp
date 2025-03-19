using System.Collections;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(PlayMakerFSM))]
public class BattlegroundsEmoteNotification : Notification
{
	[Header("-Battlegrounds Emote Parameters-")]
	[SerializeField]
	private WidgetInstance m_widget;

	[SerializeField]
	private PlayMakerFSM m_playMakerFsm;

	[SerializeField]
	private AsyncReference m_asyncAnimationOverrideReference;

	[SerializeField]
	private AsyncReference m_asyncImageWidgetColliderReference;

	[Min(0f)]
	[Header("-PlayMaker Parameters-")]
	[SerializeField]
	[Tooltip("Delay at the start of the animation loop, after scaling up, holding the first frame of animation")]
	private float m_entranceDelaySeconds;

	[SerializeField]
	[Tooltip("Delay after the final loop of animation is played, before scaling down, holding the last frame of animation")]
	[Min(0f)]
	private float m_exitDelaySeconds;

	[Tooltip("If true, emote will pause on the first frame before starting. If false, emote will pause on the animation's configured display frame before starting")]
	[SerializeField]
	private bool m_shouldStartOnFirstFrame = true;

	private Animator m_animator;

	private bool m_isAnimationReady;

	private const string BirthEventName = "Birth";

	private const float CheckIfReadyIntervalSeconds = 0.1f;

	private const string SetSpeechBubbleEventName = "GAMEPLAY_LEFT";

	public float EntranceDelaySeconds => m_entranceDelaySeconds;

	public float ExitDelaySeconds => m_exitDelaySeconds;

	private void Awake()
	{
		if (m_widget == null)
		{
			Debug.LogError("BattlegroundsEmoteNotification: Missing required widget reference");
		}
		if (m_playMakerFsm == null)
		{
			Debug.LogError("BattlegroundsEmoteNotification: Missing required PlayMakerFSM component");
		}
		if (m_asyncAnimationOverrideReference == null)
		{
			Debug.LogError("BattlegroundsEmoteNotification: Missing required async AnimationOverrideWidgetBehaviour reference");
		}
		if (m_asyncImageWidgetColliderReference == null)
		{
			Debug.LogError("BattlegroundsEmoteNotification: Missing a required async Collider reference");
		}
	}

	private void Start()
	{
		m_widget.Hide();
		m_asyncAnimationOverrideReference.RegisterReadyListener<AnimationOverrideWidgetBehaviour>(OnAnimationOverrideReady);
		m_asyncImageWidgetColliderReference.RegisterReadyListener<Collider>(OnImageWidgetColliderReady);
	}

	public void BindEmoteDataModel(int battlegroundsEmoteId)
	{
		BattlegroundsEmoteDbfRecord dbfRecord = GameDbf.BattlegroundsEmote.GetRecord(battlegroundsEmoteId);
		if (dbfRecord == null)
		{
			Debug.Log("BattlegroundsEmoteNotification: No emote DBF record found for binding to " + m_widget.name + ". Emote notification will be empty.");
			return;
		}
		BattlegroundsEmoteDataModel dataModel = new BattlegroundsEmoteDataModel
		{
			EmoteDbiId = dbfRecord.ID,
			Animation = dbfRecord.AnimationPath,
			IsAnimating = dbfRecord.IsAnimating,
			BorderType = dbfRecord.BorderType,
			XOffset = (float)dbfRecord.XOffset,
			ZOffset = (float)dbfRecord.ZOffset
		};
		m_widget.BindDataModel(dataModel);
		m_widget.TriggerEvent("GAMEPLAY_LEFT");
	}

	public override void PlayBirth()
	{
		Processor.RunCoroutine(WaitUntilReadyThenPlay());
	}

	public void DestroyNotification()
	{
		NotificationManager.Get().DestroyNotification(this, 0f);
	}

	public GameObject GetAnimatorGameObject()
	{
		if (!(m_animator != null))
		{
			return null;
		}
		return m_animator.gameObject;
	}

	public void EnableAnimatorComponent(bool isEnabled)
	{
		if (!(m_animator == null))
		{
			m_animator.enabled = isEnabled;
		}
	}

	private void OnImageWidgetColliderReady(Collider imageWidgetCollider)
	{
		if (imageWidgetCollider == null)
		{
			Debug.LogError("BattlegroundsEmoteNotification: Failed to load collider reference, notification will block raycasts.");
		}
		else
		{
			imageWidgetCollider.enabled = false;
		}
	}

	private void OnAnimationOverrideReady(AnimationOverrideWidgetBehaviour animationOverride)
	{
		if (animationOverride == null || !animationOverride.TryGetComponent<Animator>(out m_animator))
		{
			Debug.LogError("BattlegroundsEmoteNotification: Failed to load animation reference, animation may not play correctly.");
		}
		else
		{
			animationOverride.RegisterDoneChangingStatesListener(OnAnimationReady);
		}
	}

	private void OnAnimationReady(object _)
	{
		if (m_shouldStartOnFirstFrame)
		{
			m_animator.Play(0, -1, 0f);
			m_animator.Update(0f);
			m_animator.enabled = false;
		}
		m_isAnimationReady = true;
	}

	private IEnumerator WaitUntilReadyThenPlay()
	{
		while (!m_isAnimationReady || !m_asyncImageWidgetColliderReference.IsReady)
		{
			yield return new WaitForSeconds(0.1f);
		}
		m_playMakerFsm.SendEvent("Birth");
	}
}
