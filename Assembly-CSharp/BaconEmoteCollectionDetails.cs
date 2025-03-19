using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconEmoteCollectionDetails : BaconCollectionDetails
{
	[Tooltip("Reference to the GameObject on the image widget that contains the AnimationOverrideWidgetBehavior component")]
	[SerializeField]
	private AsyncReference m_asyncAnimationOverrideReference;

	[Tooltip("Number of seconds to wait after the details view has fully scaled-up, before playing the emote")]
	[Min(0f)]
	[SerializeField]
	private float m_entranceDelaySeconds;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Number of seconds to wait in between consecutive loops of the emote")]
	private float m_intervalDelaySeconds;

	[SerializeField]
	[Tooltip("If true, emote will pause on the first frame before starting. If false, emote will pause on the animation's configured display frame before starting")]
	private bool m_shouldStartOnFirstFrame;

	[Tooltip("If true, emote will transition to the first frame before starting the interval delay between loops (if one exists). If false, stay on last frame")]
	[SerializeField]
	private bool m_shouldFinishOnFirstFrame;

	private BattlegroundsEmoteDataModel m_dataModel;

	private Animator m_animator;

	private AnimationOverrideWidgetBehaviour m_animationOverrideWidgetBehaviour;

	private WaitUntil m_waitUntilAnimationReady;

	private WaitForSeconds m_entranceWaitForSeconds;

	private WaitForSeconds m_intervalWaitForSeconds;

	private bool m_isAnimationReady;

	private int m_dataVersion;

	private float m_animationLength;

	private int m_animationHash;

	private float m_displayFrameNormalizedTime;

	private const string SetSpeechBubbleEventName = "DEFAULT_BOTTOM_LEFT";

	protected override string DebugTextValue => $"Emote ID: {m_dataModel?.EmoteDbiId}";

	private void Awake()
	{
		if (m_asyncAnimationOverrideReference == null)
		{
			Debug.LogError("BaconEmoteCollectionDetails: Missing required async AnimationOverrideWidgetBehaviour reference");
		}
	}

	protected override void Start()
	{
		base.Start();
		m_asyncAnimationOverrideReference.RegisterReadyListener<AnimationOverrideWidgetBehaviour>(OnAnimationOverrideReady);
	}

	protected void OnEnable()
	{
		if (m_animationOverrideWidgetBehaviour != null && m_animator != null && m_animationOverrideWidgetBehaviour.GetLocalDataVersion() == m_dataVersion)
		{
			m_isAnimationReady = true;
		}
	}

	public override void AssignDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		m_dataModel = dataModel as BattlegroundsEmoteDataModel;
		m_widget.BindDataModel(dataModel);
		m_widget.TriggerEvent("DEFAULT_BOTTOM_LEFT");
		if (m_animationOverrideWidgetBehaviour != null && m_animator != null && m_animationOverrideWidgetBehaviour.GetLocalDataVersion() == m_dataVersion)
		{
			m_animator.enabled = true;
			m_isAnimationReady = true;
		}
	}

	private void OnAnimationOverrideReady(AnimationOverrideWidgetBehaviour animationOverride)
	{
		if (animationOverride == null || !animationOverride.TryGetComponent<Animator>(out m_animator))
		{
			Debug.LogError("BaconEmoteCollectionDetails: Failed to load animation reference, animation may not play correctly.");
			return;
		}
		m_animationOverrideWidgetBehaviour = animationOverride;
		m_animationOverrideWidgetBehaviour.RegisterDoneChangingStatesListener(delegate
		{
			OnAnimationLoaded();
		});
	}

	private void OnAnimationLoaded()
	{
		m_dataVersion = m_animationOverrideWidgetBehaviour.GetLocalDataVersion();
		AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
		m_animationLength = stateInfo.length;
		m_animationHash = stateInfo.fullPathHash;
		m_displayFrameNormalizedTime = stateInfo.normalizedTime;
		m_isAnimationReady = true;
		if (m_shouldStartOnFirstFrame)
		{
			PauseAtNormalizedTime(0f);
		}
	}

	protected override bool ValidateDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		if (dataModel is BattlegroundsEmoteDataModel)
		{
			return pageDataModel is BattlegroundsEmoteCollectionPageDataModel;
		}
		return false;
	}

	protected override void ClearDataModels()
	{
		m_dataModel = null;
	}

	protected override void DetailsEventListener(string eventName)
	{
		if (eventName == "OffDialogClick_code")
		{
			if (CanHide())
			{
				Hide();
			}
		}
		else
		{
			Debug.LogWarning("Unrecognized event handled in BaconEmoteCollectionDetails: " + eventName);
		}
	}

	protected override void OnShowAnimationComplete(object objectData)
	{
		base.OnShowAnimationComplete(objectData);
		StartCoroutine(PlayEmoteOnLoop());
	}

	protected override void OnHideAnimationComplete(object objectData)
	{
		PauseAtNormalizedTime(m_shouldStartOnFirstFrame ? 0f : m_displayFrameNormalizedTime);
		m_isAnimationReady = false;
		base.OnHideAnimationComplete(objectData);
	}

	private void PauseAtNormalizedTime(float normalizedTime)
	{
		if (!(m_animator == null) && m_isAnimationReady)
		{
			m_animator.Play(m_animationHash, -1, normalizedTime);
			m_animator.Update(0f);
			m_animator.enabled = false;
		}
	}

	private IEnumerator PlayEmoteOnLoop()
	{
		yield return m_entranceWaitForSeconds;
		yield return m_waitUntilAnimationReady;
		WaitForSeconds animationWaitForSeconds = new WaitForSeconds(m_animationLength);
		while (m_isShown)
		{
			m_animator.enabled = true;
			m_animator.Play(m_animationHash, -1, 0f);
			yield return animationWaitForSeconds;
			if (m_shouldFinishOnFirstFrame)
			{
				PauseAtNormalizedTime(0f);
			}
			yield return m_intervalWaitForSeconds;
		}
	}

	private void CreateYieldInstructions()
	{
		m_waitUntilAnimationReady = new WaitUntil(() => m_isAnimationReady);
		m_entranceWaitForSeconds = new WaitForSeconds(m_entranceDelaySeconds);
		m_intervalWaitForSeconds = new WaitForSeconds(m_intervalDelaySeconds);
	}
}
