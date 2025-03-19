using UnityEngine;

public class Scrollbar : MonoBehaviour
{
	public ScrollBarThumb m_thumb;

	public GameObject m_track;

	public Vector3 m_sliderStartLocalPos;

	public Vector3 m_sliderEndLocalPos;

	public GameObject m_scrollArea;

	public BoxCollider m_scrollWindow;

	protected bool m_isActive = true;

	protected bool m_isDragging;

	protected float m_scrollValue;

	protected Vector3 m_scrollAreaStartPos;

	protected Vector3 m_scrollAreaEndPos;

	protected float m_stepSize;

	protected Vector3 m_thumbPosition;

	protected Bounds m_childrenBounds;

	protected float m_scrollWindowHeight;

	public float ScrollValue => m_scrollValue;

	protected virtual void Awake()
	{
		m_scrollWindowHeight = m_scrollWindow.size.z;
		m_scrollWindow.enabled = false;
	}

	public bool IsActive()
	{
		return m_isActive;
	}

	private void Update()
	{
		if (!m_isActive)
		{
			return;
		}
		if (InputIsOver())
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				ScrollDown();
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				ScrollUp();
			}
		}
		if (m_thumb.IsDragging())
		{
			Drag();
		}
	}

	public void Drag()
	{
		Vector3 planeOrigin = m_track.GetComponent<BoxCollider>().bounds.min;
		Camera camera = CameraUtils.FindFirstByLayer(m_track.layer);
		Plane trackPlane = new Plane(-camera.transform.forward, planeOrigin);
		Ray mouseRay = camera.ScreenPointToRay(InputCollection.GetMousePosition());
		if (trackPlane.Raycast(mouseRay, out var dist))
		{
			Vector3 intersectPoint = base.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
			TransformUtil.SetLocalPosZ(m_thumb.gameObject, Mathf.Clamp(intersectPoint.z, m_sliderStartLocalPos.z, m_sliderEndLocalPos.z));
			m_scrollValue = Mathf.Clamp01((intersectPoint.z - m_sliderStartLocalPos.z) / (m_sliderEndLocalPos.z - m_sliderStartLocalPos.z));
			UpdateScrollAreaPosition(tween: false);
		}
	}

	public virtual void Show()
	{
		m_isActive = true;
		ShowThumb(show: true);
		base.gameObject.SetActive(value: true);
	}

	public virtual void Hide()
	{
		m_isActive = false;
		ShowThumb(show: false);
		base.gameObject.SetActive(value: false);
	}

	public void Init()
	{
		m_scrollValue = 0f;
		m_stepSize = 1f;
		m_thumb.transform.localPosition = m_sliderStartLocalPos;
		m_scrollAreaStartPos = m_scrollArea.transform.position;
		UpdateScrollAreaBounds();
	}

	public void UpdateScrollAreaBounds()
	{
		GetBoundsOfChildren(m_scrollArea);
		float scrollHeightNeeded = m_childrenBounds.size.z - m_scrollWindowHeight;
		m_scrollAreaEndPos = m_scrollAreaStartPos;
		if (scrollHeightNeeded <= 0f)
		{
			m_scrollValue = 0f;
			Hide();
		}
		else
		{
			int numSteps = (int)Mathf.Ceil(scrollHeightNeeded / 5f);
			m_stepSize = 1f / (float)numSteps;
			m_scrollAreaEndPos.z += scrollHeightNeeded;
			Show();
		}
		UpdateThumbPosition();
		UpdateScrollAreaPosition(tween: false);
	}

	public virtual bool InputIsOver()
	{
		return UniversalInputManager.Get().InputIsOver(base.gameObject);
	}

	protected virtual void GetBoundsOfChildren(GameObject go)
	{
		m_childrenBounds = TransformUtil.GetBoundsOfChildren(go);
	}

	public void OverrideScrollWindowHeight(float scrollWindowHeight)
	{
		m_scrollWindowHeight = scrollWindowHeight;
	}

	protected void ShowThumb(bool show)
	{
		if (m_thumb != null)
		{
			m_thumb.gameObject.SetActive(show);
		}
	}

	private void UpdateThumbPosition()
	{
		m_thumbPosition = Vector3.Lerp(m_sliderStartLocalPos, m_sliderEndLocalPos, Mathf.Clamp01(m_scrollValue));
		m_thumb.transform.localPosition = m_thumbPosition;
		m_thumb.transform.localScale = Vector3.one;
		if (m_scrollValue < 0f || m_scrollValue > 1f)
		{
			float scale = 1f / ((m_scrollValue < 0f) ? (0f - m_scrollValue + 1f) : m_scrollValue);
			Renderer thumbRenderer = m_thumb.GetComponent<Renderer>();
			float end = m_thumb.transform.parent.InverseTransformPoint((m_scrollValue < 0f) ? thumbRenderer.bounds.max : thumbRenderer.bounds.min).z;
			float delta = (m_thumbPosition.z - end) * (scale - 1f);
			TransformUtil.SetLocalPosZ(m_thumb, m_thumbPosition.z + delta);
			TransformUtil.SetLocalScaleZ(m_thumb, scale);
		}
	}

	private void UpdateScrollAreaPosition(bool tween)
	{
		if (!(m_scrollArea == null))
		{
			Vector3 position = m_scrollAreaStartPos + m_scrollValue * (m_scrollAreaEndPos - m_scrollAreaStartPos);
			if (tween)
			{
				iTween.MoveTo(m_scrollArea, iTween.Hash("position", position, "time", 0.2f, "islocal", false));
			}
			else
			{
				m_scrollArea.transform.position = position;
			}
		}
	}

	public void ScrollTo(float value, bool clamp = true, bool lerp = true)
	{
		m_scrollValue = (clamp ? Mathf.Clamp01(value) : value);
		UpdateThumbPosition();
		UpdateScrollAreaPosition(lerp);
	}

	private void ScrollUp()
	{
		Scroll(0f - m_stepSize);
	}

	public void Scroll(float amount, bool lerp = true)
	{
		m_scrollValue = Mathf.Clamp01(m_scrollValue + amount);
		UpdateThumbPosition();
		UpdateScrollAreaPosition(lerp);
	}

	private void ScrollDown()
	{
		Scroll(m_stepSize);
	}
}
