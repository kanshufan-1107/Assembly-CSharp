using UnityEngine;

public class FramedRadioButton : MonoBehaviour
{
	public enum FrameType
	{
		SINGLE,
		MULTI_LEFT_END,
		MULTI_RIGHT_END,
		MULTI_MIDDLE
	}

	public GameObject m_root;

	public GameObject m_frameEndLeft;

	public GameObject m_frameEndRight;

	public GameObject m_frameLeft;

	public GameObject m_frameFill;

	public RadioButton m_radioButton;

	public UberText m_text;

	private float m_leftEdgeOffset;

	public int GetButtonID()
	{
		return m_radioButton.GetButtonID();
	}

	public float GetLeftEdgeOffset()
	{
		return m_leftEdgeOffset;
	}

	public virtual void Init(FrameType frameType, string text, int buttonID, object userData)
	{
		m_radioButton.SetButtonID(buttonID);
		m_radioButton.SetUserData(userData);
		m_text.Text = text;
		m_text.UpdateNow();
		m_frameFill.SetActive(value: true);
		bool isEndLeft = false;
		bool isEndRight = false;
		switch (frameType)
		{
		case FrameType.SINGLE:
			isEndLeft = true;
			isEndRight = true;
			break;
		case FrameType.MULTI_LEFT_END:
			isEndLeft = true;
			isEndRight = false;
			break;
		case FrameType.MULTI_RIGHT_END:
			isEndLeft = false;
			isEndRight = true;
			break;
		case FrameType.MULTI_MIDDLE:
			isEndLeft = false;
			isEndRight = false;
			break;
		}
		m_frameEndLeft.SetActive(isEndLeft);
		m_frameLeft.SetActive(!isEndLeft);
		m_frameEndRight.SetActive(isEndRight);
		Transform leftTransform = (isEndLeft ? m_frameEndLeft.transform : m_frameLeft.transform);
		m_leftEdgeOffset = leftTransform.position.x - base.transform.position.x;
	}

	public void Show()
	{
		m_root.SetActive(value: true);
	}

	public void Hide()
	{
		m_root.SetActive(value: false);
	}

	public Bounds GetBounds()
	{
		Bounds bounds = m_frameFill.GetComponent<Renderer>().bounds;
		IncludeBoundsOfGameObject(m_frameEndLeft, ref bounds);
		IncludeBoundsOfGameObject(m_frameEndRight, ref bounds);
		IncludeBoundsOfGameObject(m_frameLeft, ref bounds);
		return bounds;
	}

	private void IncludeBoundsOfGameObject(GameObject go, ref Bounds bounds)
	{
		if (go.activeSelf)
		{
			Bounds goBounds = go.GetComponent<Renderer>().bounds;
			Vector3 maxPoint = Vector3.Max(goBounds.max, bounds.max);
			Vector3 minPoint = Vector3.Min(goBounds.min, bounds.min);
			bounds.SetMinMax(minPoint, maxPoint);
		}
	}
}
