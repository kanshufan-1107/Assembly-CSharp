using UnityEngine;

public class DragCardRow : MonoBehaviour
{
	private float m_CursorX;

	private Rect dragRect;

	private void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		Vector3 bottomLeft = component.bounds.min;
		Vector3 topRight = component.bounds.max;
		dragRect = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
	}

	private void OnMouseDown()
	{
		m_CursorX = InputCollection.GetMousePosition().x;
	}

	private void OnMouseDrag()
	{
		float mousePositionX = InputCollection.GetMousePosition().x;
		float mouseDiff = mousePositionX - m_CursorX;
		mouseDiff *= 0.01f;
		base.transform.Translate(mouseDiff, 0f, 0f);
		base.transform.position = new Vector3(Mathf.Clamp(base.transform.position.x, dragRect.xMin, dragRect.xMax), base.transform.position.y, base.transform.position.z);
		m_CursorX = mousePositionX;
	}
}
