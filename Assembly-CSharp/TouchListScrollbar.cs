using System.Collections;
using UnityEngine;

public class TouchListScrollbar : PegUIElement
{
	public enum ScrollDirection
	{
		X,
		Y,
		Z
	}

	public TouchList list;

	public PegUIElement thumb;

	public Transform thumbMin;

	public Transform thumbMax;

	public GameObject cover;

	public PegUIElement track;

	public ScrollDirection scrollPlane = ScrollDirection.Y;

	private bool isActive;

	protected override void Awake()
	{
		if (list.orientation == TouchList.Orientation.Horizontal)
		{
			Debug.LogError("Horizontal TouchListScrollbar not implemented");
			Object.Destroy(this);
			return;
		}
		base.Awake();
		ShowThumb(isActive);
		list.ClipSizeChanged += UpdateLayout;
		list.ScrollingEnabledChanged += UpdateActive;
		list.Scrolled += UpdateThumb;
		thumb.AddEventListener(UIEventType.PRESS, ThumbPressed);
		track.AddEventListener(UIEventType.PRESS, TrackPressed);
		UpdateLayout();
	}

	private void UpdateActive(bool canScroll)
	{
		if (isActive != canScroll)
		{
			isActive = canScroll;
			thumb.GetComponent<Collider>().enabled = isActive;
			if (isActive)
			{
				UpdateThumb();
			}
			ShowThumb(isActive);
		}
	}

	private void UpdateLayout()
	{
		TransformUtil.SetPosX(thumb, thumbMin.position.x);
		UpdateThumb();
	}

	private void ShowThumb(bool show)
	{
		Transform mesh = thumb.transform.Find("Mesh");
		if (mesh != null)
		{
			mesh.gameObject.SetActive(show);
		}
		if (cover != null)
		{
			cover.SetActive(!show);
		}
	}

	private void UpdateThumb()
	{
		if (isActive)
		{
			Collider touchListCollider = GetComponent<Collider>();
			if (list.layoutPlane == TouchList.LayoutPlane.XZ)
			{
				TransformUtil.SetPosY(thumb, touchListCollider.bounds.min.y);
			}
			else
			{
				TransformUtil.SetPosZ(thumb, touchListCollider.bounds.min.z);
			}
			float scrollValue = list.ScrollValue;
			float scrollPlanePos = thumbMin.position[(int)scrollPlane] + (thumbMax.position[(int)scrollPlane] - thumbMin.position[(int)scrollPlane]) * Mathf.Clamp01(scrollValue);
			Vector3 thumbPos = thumb.transform.position;
			thumbPos[(int)scrollPlane] = scrollPlanePos;
			thumb.transform.position = thumbPos;
			thumb.transform.localScale = Vector3.one;
			if (scrollValue < 0f || scrollValue > 1f)
			{
				float scale = 1f / ((scrollValue < 0f) ? (0f - scrollValue + 1f) : scrollValue);
				Collider thumbCollider = thumb.GetComponent<Collider>();
				float end = ((scrollValue < 0f) ? thumbCollider.bounds.max : thumbCollider.bounds.min)[(int)scrollPlane];
				float delta = (thumb.transform.position[(int)scrollPlane] - end) * (scale - 1f);
				thumbPos = thumb.transform.position;
				thumbPos[(int)scrollPlane] += delta;
				thumb.transform.position = thumbPos;
			}
		}
	}

	private void ThumbPressed(UIEvent e)
	{
		StartCoroutine(UpdateThumbDrag());
	}

	private void TrackPressed(UIEvent e)
	{
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		Plane trackPlane = new Plane(-camera.transform.forward, track.transform.position);
		float scrollValue = GetTouchPoint(trackPlane, camera)[(int)scrollPlane];
		scrollValue = Mathf.Clamp(scrollValue, thumbMax.position[(int)scrollPlane], thumbMin.position[(int)scrollPlane]);
		list.ScrollValue = (scrollValue - thumbMin.position[(int)scrollPlane]) / (thumbMax.position[(int)scrollPlane] - thumbMin.position[(int)scrollPlane]);
	}

	private IEnumerator UpdateThumbDrag()
	{
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		Plane dragPlane = new Plane(-camera.transform.forward, thumb.transform.position);
		float dragOffset = (thumb.transform.position - GetTouchPoint(dragPlane, camera))[(int)scrollPlane];
		while (!InputCollection.GetMouseButtonUp(0))
		{
			float thumbPosScrollPlane = GetTouchPoint(dragPlane, camera)[(int)scrollPlane] + dragOffset;
			thumbPosScrollPlane = Mathf.Clamp(thumbPosScrollPlane, thumbMax.position[(int)scrollPlane], thumbMin.position[(int)scrollPlane]);
			list.ScrollValue = (thumbPosScrollPlane - thumbMin.position[(int)scrollPlane]) / (thumbMax.position[(int)scrollPlane] - thumbMin.position[(int)scrollPlane]);
			yield return null;
		}
	}

	private Vector3 GetTouchPoint(Plane dragPlane, Camera camera)
	{
		Ray touchRay = camera.ScreenPointToRay(InputCollection.GetMousePosition());
		dragPlane.Raycast(touchRay, out var dist);
		return touchRay.GetPoint(dist);
	}
}
