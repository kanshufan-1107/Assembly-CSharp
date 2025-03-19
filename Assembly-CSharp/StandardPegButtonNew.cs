using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class StandardPegButtonNew : PegUIElement
{
	public UberText m_buttonText;

	public ThreeSliceElement m_button;

	public ThreeSliceElement m_border;

	public ThreeSliceElement m_highlight;

	public GameObject m_upBone;

	public GameObject m_downBone;

	public float m_buttonWidth;

	public bool m_ExecuteInEditMode;

	private bool m_highlightLocked;

	private const float HIGHLIGHT_SCALE = 1.2f;

	private const float GRAY_FRAME_SCALE = 0.88f;

	public void SetText(string t)
	{
		m_buttonText.Text = t;
	}

	public void SetWidth(float globalWidth)
	{
		m_button.SetWidth(globalWidth * 0.88f);
		if (m_border != null)
		{
			m_border.SetWidth(globalWidth);
		}
		Quaternion currentRotation = base.transform.rotation;
		base.transform.rotation = Quaternion.Euler(Vector3.zero);
		Vector3 buttonSize = m_button.GetSize();
		Vector3 scaleValue = TransformUtil.ComputeWorldScale(base.transform);
		Vector3 colliderSize = new Vector3(buttonSize.x / scaleValue.x, buttonSize.z / scaleValue.z, buttonSize.y / scaleValue.y);
		GetComponent<BoxCollider>().size = colliderSize;
		base.transform.rotation = currentRotation;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Disable()
	{
		m_button.transform.localRotation = Quaternion.Euler(new Vector3(180f, 180f, 0f));
		SetEnabled(enabled: false);
	}

	public void Enable()
	{
		m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		SetEnabled(enabled: true);
	}

	public void Reset()
	{
		iTween.StopByName(m_button.gameObject, "rotation");
		m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
	}

	public void LockHighlight()
	{
		m_highlight.gameObject.SetActive(value: true);
		m_highlightLocked = true;
	}

	public void UnlockHighlight()
	{
		m_highlight.gameObject.SetActive(value: false);
		m_highlightLocked = false;
	}

	protected override void OnOver(InteractionState oldState)
	{
		if (!m_highlightLocked)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", new Vector3(90f, 0f, 0f));
			args.Add("time", 0.5f);
			args.Add("name", "rotation");
			iTween.StopByName(m_button.gameObject, "rotation");
			m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
			iTween.PunchRotation(m_button.gameObject, args);
			m_highlight.gameObject.SetActive(value: true);
			SoundManager soundManager = SoundManager.Get();
			if (soundManager != null && soundManager.GetConfig() != null)
			{
				soundManager.LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9");
			}
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		m_button.transform.localPosition = m_upBone.transform.localPosition;
		if (!m_highlightLocked)
		{
			m_highlight.gameObject.SetActive(value: false);
		}
	}

	protected override void OnPress()
	{
		m_button.transform.localPosition = m_downBone.transform.localPosition;
		if (SoundManager.Get() != null && SoundManager.Get().GetConfig() != null)
		{
			SoundManager.Get().LoadAndPlay("Back_Click.prefab:f7df4bfeab7ccff4198e670ca516da2e");
		}
	}

	protected override void OnRelease()
	{
		m_button.transform.localPosition = m_upBone.transform.localPosition;
	}
}
