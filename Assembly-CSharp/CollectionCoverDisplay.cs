using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CollectionCoverDisplay : PegUIElement
{
	public delegate void DelOnOpened();

	public GameObject m_bookCoverLatch;

	public GameObject m_bookCoverLatchJoint;

	public GameObject m_bookCover;

	public Material m_latchFadeMaterial;

	public Material m_latchOpaqueMaterial;

	private readonly string CRACK_LATCH_OPEN_ANIM_COROUTINE = "AnimateLatchCrackOpen";

	private readonly string LATCH_OPEN_ANIM_NAME = "CollectionManagerCoverV2_Lock_edit";

	private readonly float LATCH_OPEN_ANIM_SPEED = 4f;

	private readonly float LATCH_FADE_TIME = 0.1f;

	private readonly float LATCH_FADE_DELAY = 0.15f;

	private readonly float BOOK_COVER_FULLY_CLOSED_Z_ROTATION;

	private readonly float BOOK_COVER_FULLY_OPEN_Z_ROTATION = 280f;

	private readonly float BOOK_COVER_FULL_ANIM_TIME = 0.75f;

	private bool m_isAnimating;

	private BoxCollider m_boxCollider;

	protected override void Awake()
	{
		base.Awake();
		m_boxCollider = base.transform.GetComponent<BoxCollider>();
	}

	public bool IsAnimating()
	{
		return m_isAnimating;
	}

	public void Open(DelOnOpened callback)
	{
		if (m_bookCover.transform.localEulerAngles.z != BOOK_COVER_FULLY_OPEN_Z_ROTATION)
		{
			EnableCollider(enabled: false);
			SetIsAnimating(animating: true);
			AnimateLatchOpening();
			AnimateCoverOpening(callback);
			SoundManager.Get().LoadAndPlay("collection_manager_book_open.prefab:e32dc00de806ee1478b67810b89947bb");
		}
	}

	public void SetOpenState()
	{
		if (m_bookCover.activeSelf)
		{
			EnableCollider(enabled: false);
			SetIsAnimating(animating: false);
			m_bookCover.SetActive(value: false);
			m_bookCoverLatchJoint.GetComponent<Renderer>().enabled = false;
		}
	}

	public void Close()
	{
		m_bookCover.SetActive(value: true);
		if (m_bookCover.transform.localEulerAngles.z != BOOK_COVER_FULLY_CLOSED_Z_ROTATION)
		{
			SetIsAnimating(animating: true);
			AnimateCoverClosing();
			SoundManager.Get().LoadAndPlay("collection_manager_book_close.prefab:872608cda202ca440aa60cd0918be9ad");
		}
	}

	public void DisplayCover()
	{
		m_bookCover.SetActive(value: true);
		m_bookCoverLatch.SetActive(value: true);
	}

	private void SetIsAnimating(bool animating)
	{
		m_isAnimating = animating;
	}

	private void EnableCollider(bool enabled)
	{
		SetEnabled(enabled);
		m_boxCollider.enabled = enabled;
	}

	private void AnimateLatchOpening()
	{
		Animation bookCoverLatchAnimation = m_bookCoverLatch.GetComponent<Animation>();
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].speed = LATCH_OPEN_ANIM_SPEED;
		if (bookCoverLatchAnimation.IsPlaying(LATCH_OPEN_ANIM_NAME))
		{
			StopCoroutine(CRACK_LATCH_OPEN_ANIM_COROUTINE);
		}
		else
		{
			bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].time = 0f;
			bookCoverLatchAnimation.Play(LATCH_OPEN_ANIM_NAME);
		}
		Hashtable fadeArgs = iTweenManager.Get().GetTweenHashTable();
		fadeArgs.Add("amount", 0f);
		fadeArgs.Add("delay", LATCH_FADE_DELAY);
		fadeArgs.Add("time", LATCH_FADE_TIME);
		fadeArgs.Add("easetype", iTween.EaseType.linear);
		fadeArgs.Add("oncomplete", "OnLatchOpened");
		fadeArgs.Add("oncompletetarget", base.gameObject);
		iTween.FadeTo(m_bookCoverLatchJoint, fadeArgs);
	}

	private void AnimateCoverOpening(DelOnOpened callback)
	{
		m_bookCoverLatchJoint.GetComponent<Renderer>().SetMaterial(m_latchFadeMaterial);
		Vector3 targetRotation = m_bookCover.transform.localEulerAngles;
		targetRotation.z = BOOK_COVER_FULLY_OPEN_Z_ROTATION;
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("rotation", targetRotation);
		rotateArgs.Add("islocal", true);
		rotateArgs.Add("time", BOOK_COVER_FULL_ANIM_TIME);
		rotateArgs.Add("easetype", iTween.EaseType.easeInCubic);
		rotateArgs.Add("oncomplete", "OnCoverOpened");
		rotateArgs.Add("oncompletetarget", base.gameObject);
		rotateArgs.Add("oncompleteparams", callback);
		rotateArgs.Add("name", "rotation");
		iTween.StopByName(m_bookCover.gameObject, "rotation");
		iTween.RotateTo(m_bookCover.gameObject, rotateArgs);
	}

	private void AnimateCoverClosing()
	{
		Vector3 targetRotation = m_bookCover.transform.localEulerAngles;
		targetRotation.z = BOOK_COVER_FULLY_CLOSED_Z_ROTATION;
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("rotation", targetRotation);
		rotateArgs.Add("islocal", true);
		rotateArgs.Add("time", BOOK_COVER_FULL_ANIM_TIME);
		rotateArgs.Add("easetype", iTween.EaseType.easeInCubic);
		rotateArgs.Add("oncomplete", "AnimateLatchClosing");
		rotateArgs.Add("oncompletetarget", base.gameObject);
		rotateArgs.Add("name", "rotation");
		iTween.StopByName(m_bookCover.gameObject, "rotation");
		iTween.RotateTo(m_bookCover.gameObject, rotateArgs);
	}

	private void AnimateLatchClosing()
	{
		Animation bookCoverLatchAnimation = m_bookCoverLatch.GetComponent<Animation>();
		Renderer component = m_bookCoverLatchJoint.GetComponent<Renderer>();
		component.enabled = true;
		component.SetMaterial(m_latchFadeMaterial);
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].time = bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].length;
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].speed = (0f - LATCH_OPEN_ANIM_SPEED) * 2f;
		Hashtable fadeArgs = iTweenManager.Get().GetTweenHashTable();
		fadeArgs.Add("amount", 1f);
		fadeArgs.Add("time", LATCH_FADE_TIME);
		fadeArgs.Add("easetype", iTween.EaseType.linear);
		fadeArgs.Add("oncomplete", "OnLatchClosed");
		fadeArgs.Add("oncompletetarget", base.gameObject);
		bookCoverLatchAnimation.Play(LATCH_OPEN_ANIM_NAME);
		iTween.FadeTo(m_bookCoverLatchJoint, fadeArgs);
	}

	private void OnCoverOpened(DelOnOpened callback)
	{
		m_bookCover.SetActive(value: false);
		SetIsAnimating(animating: false);
		callback?.Invoke();
	}

	private void OnLatchOpened()
	{
		m_bookCoverLatchJoint.GetComponent<Renderer>().enabled = false;
	}

	private void OnLatchClosed()
	{
		EnableCollider(enabled: true);
		SetIsAnimating(animating: false);
	}

	private void CrackOpen()
	{
		if (!IsAnimating())
		{
			StopCoroutine(CRACK_LATCH_OPEN_ANIM_COROUTINE);
			StartCoroutine(CRACK_LATCH_OPEN_ANIM_COROUTINE);
		}
	}

	private IEnumerator AnimateLatchCrackOpen()
	{
		Animation bookCoverLatchAnimation = m_bookCoverLatch.GetComponent<Animation>();
		m_bookCoverLatchJoint.GetComponent<Renderer>().SetMaterial(m_latchOpaqueMaterial);
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].time = 0f;
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].speed = LATCH_OPEN_ANIM_SPEED;
		SoundManager.Get().LoadAndPlay("collection_manager_book_latch_jiggle.prefab:45ddcdb304889ac48b14478fc78991ba");
		bookCoverLatchAnimation.Play(LATCH_OPEN_ANIM_NAME);
		while (bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].time < 0.75f)
		{
			yield return null;
		}
		bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].speed = 0f;
	}

	private void CrackClose()
	{
		if (!IsAnimating())
		{
			Animation bookCoverLatchAnimation = m_bookCoverLatch.GetComponent<Animation>();
			if (bookCoverLatchAnimation.IsPlaying(LATCH_OPEN_ANIM_NAME))
			{
				StopCoroutine(CRACK_LATCH_OPEN_ANIM_COROUTINE);
				bookCoverLatchAnimation[LATCH_OPEN_ANIM_NAME].speed = 0f - LATCH_OPEN_ANIM_SPEED;
			}
		}
	}
}
