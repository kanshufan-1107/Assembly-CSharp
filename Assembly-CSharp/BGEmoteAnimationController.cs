using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BGEmoteAnimationController : MonoBehaviour
{
	[SerializeField]
	[Min(0f)]
	[Tooltip("Number of seconds to wait between animating each emote in sequence (e.g. animate emote 1, pause, animate emote 2, pause...). If only 1 emote is shown, this wait is ignored")]
	private float m_pauseBetweenEmoteAnimations;

	[Min(0f)]
	[SerializeField]
	[Tooltip("Number of seconds to wait after animating each emote in sequence (e.g. if 3 emotes are shown, this pause happens after emote 3 finishes animating). If only 1 emote is shown, this wait happens after a single loop.")]
	private float m_pauseBetweenEmoteCycles;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Number of seconds to wait after the page is loaded and before playing an emote")]
	private float m_emoteEntranceDelaySeconds;

	[Tooltip("If true, emote will pause on the first frame before starting. If false, emote will pause on the animation's configured display frame before starting")]
	[SerializeField]
	private bool m_shouldStartEmoteOnFirstFrame;

	private Widget m_widget;

	private bool m_isProductPage;

	private List<Animator> m_bgEmoteAnimators = new List<Animator>();

	private List<(Animator animator, float normalizedDisplayTime)> m_bgEmotesWithDisplayTimes = new List<(Animator, float)>();

	private Coroutine m_emoteAnimationCoroutine;

	private int m_animatedEmoteIndex;

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		m_isProductPage = GetComponent<ProductPage>() != null;
		if (m_widget != null && !m_isProductPage)
		{
			m_widget.RegisterEventListener(OnWidgetEvent);
		}
	}

	private void OnDestroy()
	{
		Reset();
		StopEmoteAnimation();
	}

	private void OnEnable()
	{
		StartEmoteAnimation();
	}

	private void OnDisable()
	{
		StopEmoteAnimation();
	}

	public void SetBGEmoteAnimators(List<Animator> animators)
	{
		if (animators == null || animators.Count == 0)
		{
			return;
		}
		StopEmoteAnimation();
		Reset();
		m_bgEmoteAnimators = new List<Animator>(animators);
		m_bgEmotesWithDisplayTimes.Clear();
		foreach (Animator bgEmoteAnimator in m_bgEmoteAnimators)
		{
			if (bgEmoteAnimator != null)
			{
				float duration = bgEmoteAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
				m_bgEmotesWithDisplayTimes.Add((bgEmoteAnimator, duration));
			}
		}
	}

	public void StartEmoteAnimation()
	{
		if (m_bgEmotesWithDisplayTimes.Count > 0)
		{
			m_emoteAnimationCoroutine = StartCoroutine(AnimateEmotes());
		}
	}

	private void Reset()
	{
		m_bgEmoteAnimators.Clear();
		m_bgEmotesWithDisplayTimes.Clear();
		m_animatedEmoteIndex = 0;
	}

	private void OnWidgetEvent(string eventName)
	{
		if (eventName == "StartEmoteAnimation_code")
		{
			SetBGEmoteAnimators(GetShopSlotBGEmotes());
			StartEmoteAnimation();
		}
	}

	private void StopEmoteAnimation()
	{
		if (m_emoteAnimationCoroutine != null)
		{
			StopCoroutine(m_emoteAnimationCoroutine);
			m_emoteAnimationCoroutine = null;
		}
		foreach (var (animator, normalizedDisplayTime) in m_bgEmotesWithDisplayTimes)
		{
			PauseEmoteAtNormalizedTime(animator, normalizedDisplayTime);
		}
	}

	private void PauseEmoteAtNormalizedTime(Animator animator, float normalizedDisplayTime)
	{
		if (animator != null)
		{
			int shortNameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			animator.Play(shortNameHash, -1, normalizedDisplayTime);
			animator.Update(0f);
			animator.enabled = false;
		}
	}

	private List<Animator> GetShopSlotBGEmotes()
	{
		WidgetInstance[] productItems = GetComponentsInChildren<WidgetInstance>(includeInactive: false);
		if (productItems == null || productItems.Count() == 0)
		{
			return null;
		}
		HashSet<Animator> bgEmotes = new HashSet<Animator>();
		WidgetInstance[] array = productItems;
		foreach (WidgetInstance item in array)
		{
			RewardItemDataModel rewardItem = item.GetDataModel<RewardItemDataModel>();
			if (item.gameObject.activeInHierarchy && rewardItem != null && rewardItem.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE)
			{
				Animator animator = item.GetComponentInChildren<Animator>();
				if (animator != null)
				{
					bgEmotes.Add(animator);
				}
			}
		}
		return bgEmotes.ToList();
	}

	private IEnumerator AnimateEmotes()
	{
		if (m_shouldStartEmoteOnFirstFrame && m_bgEmotesWithDisplayTimes.Count > 0)
		{
			foreach (var bgEmotesWithDisplayTime in m_bgEmotesWithDisplayTimes)
			{
				PauseEmoteAtNormalizedTime(bgEmotesWithDisplayTime.animator, 0f);
			}
		}
		yield return new WaitForSeconds(m_emoteEntranceDelaySeconds);
		while (m_bgEmotesWithDisplayTimes.Count > 0)
		{
			if (m_animatedEmoteIndex >= m_bgEmotesWithDisplayTimes.Count)
			{
				m_animatedEmoteIndex = 0;
				yield return new WaitForSeconds(m_pauseBetweenEmoteCycles);
			}
			var (animator, normalizedDisplayTime) = m_bgEmotesWithDisplayTimes[m_animatedEmoteIndex];
			if (animator == null)
			{
				yield return new WaitForSeconds(0f);
				continue;
			}
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			animator.enabled = true;
			animator.Play(stateInfo.shortNameHash, -1, 0f);
			yield return new WaitForSeconds(stateInfo.length);
			if (m_bgEmotesWithDisplayTimes.Count > 1)
			{
				PauseEmoteAtNormalizedTime(animator, normalizedDisplayTime);
				if (m_animatedEmoteIndex < m_bgEmotesWithDisplayTimes.Count - 1)
				{
					yield return new WaitForSeconds(m_pauseBetweenEmoteAnimations);
				}
			}
			m_animatedEmoteIndex++;
		}
	}
}
