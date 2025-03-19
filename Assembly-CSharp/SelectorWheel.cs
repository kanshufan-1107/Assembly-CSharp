using System;
using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class SelectorWheel : MonoBehaviour
{
	private struct Tile
	{
		public GameObject gameObject;

		public int indexOffset;
	}

	[SerializeField]
	protected PegUIElement m_dragRegion;

	[SerializeField]
	protected PegUIElement m_cruiseUpRegion;

	[SerializeField]
	protected PegUIElement m_cruiseDownRegion;

	[SerializeField]
	protected GameObject m_tileBase;

	[SerializeField]
	protected float m_radius = 2f;

	[SerializeField]
	protected float m_flyBraking = 4f;

	[SerializeField]
	protected float m_flyDamping = 1f;

	[SerializeField]
	protected float m_snapDamping = 8f;

	[SerializeField]
	protected float m_snapForce = 40f;

	[SerializeField]
	protected float m_tileSpacing = 1f;

	[SerializeField]
	protected int m_maxTiles = 7;

	[SerializeField]
	protected bool m_globalScrolling = true;

	[SerializeField]
	protected bool m_invertScrolling = true;

	[SerializeField]
	protected float m_scrollingSpeed = 15f;

	[SerializeField]
	protected int m_numTiles = 1;

	[SerializeField]
	protected bool m_topToBottom = true;

	[SerializeField]
	protected float m_cruiseStartSpeed = 4f;

	[SerializeField]
	protected float m_cruiseEndSpeed = 24f;

	[SerializeField]
	protected float m_exponentialCruising = 0.5f;

	private Tile[] m_tiles;

	private float m_velocity;

	private float m_snapInfluence = 1f;

	private float m_wheelPosition;

	private int m_lastSelection;

	private Vector3? m_lastDragPos;

	private float? m_scrollTargetPosition;

	private int m_cruiseDir;

	private List<IDataModel> m_dataModels = new List<IDataModel>();

	public int TileCount
	{
		get
		{
			return m_numTiles;
		}
		protected set
		{
			m_numTiles = value;
			if (m_wheelPosition >= (float)m_numTiles)
			{
				SetIndex((m_numTiles > 0) ? (m_numTiles - 1) : 0);
			}
			LayoutTiles();
		}
	}

	public event Action OnSelectionChanged;

	private void Start()
	{
		m_tiles = new Tile[m_maxTiles];
		for (int i = 0; i < m_maxTiles; i++)
		{
			m_tiles[i] = default(Tile);
			m_tiles[i].indexOffset = i;
			if (i == 0)
			{
				m_tiles[i].gameObject = m_tileBase;
			}
			else
			{
				m_tiles[i].gameObject = UnityEngine.Object.Instantiate(m_tileBase, m_tileBase.transform.parent);
			}
		}
		if (m_dragRegion != null)
		{
			m_dragRegion.AddEventListener(UIEventType.PRESS, delegate
			{
				m_lastDragPos = GetLocalMousePos();
				PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
			});
		}
		InitializeCruiseClicker(m_cruiseUpRegion, 1);
		InitializeCruiseClicker(m_cruiseDownRegion, -1);
		SetIndex(0);
	}

	private void Update()
	{
		float dTime = Time.deltaTime;
		UpdateInput(dTime);
		float lastWheelPos = m_wheelPosition;
		SimulateWheel(dTime);
		if (!(Mathf.Abs(m_wheelPosition - lastWheelPos) < 0.001f))
		{
			UpdateSelection();
			LayoutTiles();
		}
	}

	public void SetIndex(int index, bool instant = true)
	{
		float position = GetPositionFromIndex(index);
		if (instant)
		{
			m_wheelPosition = position;
			m_velocity = 0f;
			LayoutTiles();
		}
		else
		{
			m_scrollTargetPosition = position;
		}
	}

	public int GetSelectedIndex()
	{
		return GetIndexFromPosition(Mathf.RoundToInt(m_wheelPosition));
	}

	public void SetDataModels(List<IDataModel> dataModels)
	{
		m_dataModels = dataModels;
		TileCount = m_dataModels.Count;
	}

	public IDataModel GetDataModel(int index)
	{
		if (index < 0 || index >= m_dataModels.Count)
		{
			return null;
		}
		return m_dataModels[index];
	}

	private void UpdateInput(float deltaTime)
	{
		Camera camera = GetCamera();
		bool canScroll = false;
		if (m_globalScrolling)
		{
			canScroll = true;
		}
		else if (UniversalInputManager.Get() != null && camera != null)
		{
			canScroll = UniversalInputManager.Get().ForcedInputIsOver(camera, m_dragRegion.gameObject);
		}
		if (canScroll)
		{
			float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			if (scrollDelta != 0f)
			{
				float scrollDir = Mathf.Sign(scrollDelta);
				if (m_invertScrolling)
				{
					scrollDir = 0f - scrollDir;
				}
				if (!m_scrollTargetPosition.HasValue)
				{
					m_scrollTargetPosition = Mathf.Round(m_wheelPosition);
				}
				float distance = 1f;
				m_scrollTargetPosition += scrollDir * distance;
				m_scrollTargetPosition = Mathf.Clamp(m_scrollTargetPosition.Value, 0f, m_numTiles - 1);
				m_snapInfluence = 1f;
				AbortCruise();
			}
		}
		if (m_lastDragPos.HasValue)
		{
			if (InputCollection.GetMouseButtonUp(0) || UniversalInputManager.Get().WasTouchCanceled())
			{
				m_lastDragPos = null;
				PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
			}
			else
			{
				Vector3 dragPos = GetLocalMousePos();
				Vector3 vector = dragPos - m_lastDragPos.Value;
				m_lastDragPos = dragPos;
				float dragDistance = vector.y;
				m_velocity = (0f - dragDistance) / m_tileSpacing / deltaTime;
				m_snapInfluence = 0f;
			}
		}
		if (m_cruiseDir != 0)
		{
			if (InputCollection.GetMouseButtonUp(0) || UniversalInputManager.Get().WasTouchCanceled())
			{
				AbortCruise();
			}
			if (Mathf.Sign(m_velocity) == Mathf.Sign(m_cruiseDir))
			{
				float speed = Mathf.Abs(m_velocity) * Mathf.Exp(m_exponentialCruising * deltaTime);
				m_velocity = (float)m_cruiseDir * Mathf.Clamp(speed, m_cruiseStartSpeed, m_cruiseEndSpeed);
			}
			m_snapInfluence = 1f;
		}
	}

	private void SimulateWheel(float deltaTime)
	{
		float minPosition = 0f;
		float maxPosition = m_numTiles - 1;
		bool isHolding = m_lastDragPos.HasValue || m_cruiseDir != 0;
		if (m_scrollTargetPosition.HasValue)
		{
			float delta = m_scrollTargetPosition.Value - m_wheelPosition;
			float distance = Mathf.Abs(delta);
			if (distance >= 0.5f)
			{
				float maxSpeed = 0.5f + distance / 2f;
				m_velocity = Mathf.Clamp(delta, 0f - maxSpeed, maxSpeed) * m_scrollingSpeed;
			}
			else
			{
				maxPosition = (minPosition = m_scrollTargetPosition.Value);
				float slowDown = 0.5f * (0.5f - distance);
				if (Mathf.Sign(m_velocity) != Mathf.Sign(delta))
				{
					slowDown = 4f / (0.51f - distance);
				}
				m_velocity *= Mathf.Exp((0f - deltaTime) * slowDown);
				if (Mathf.Abs(m_velocity) < 0.1f)
				{
					m_scrollTargetPosition = null;
				}
			}
		}
		else
		{
			float snapInfluence = Mathf.Max(m_snapInfluence, 1f / (1f + 4f * Mathf.Abs(m_velocity)));
			float lerp = (isHolding ? 1f : (1f - Mathf.Exp((0f - deltaTime) * m_flyBraking)));
			m_snapInfluence = Mathf.Lerp(m_snapInfluence, snapInfluence, lerp);
		}
		if (!isHolding)
		{
			float num = (m_wheelPosition % 1f + 1.5f) % 1f - 0.5f;
			float distance2 = Mathf.Abs(num);
			float guidance = -Math.Sign(num);
			guidance = ((!(distance2 <= 0.45f)) ? (guidance * (1f - (distance2 - 0.45f) / 0.050000012f)) : (guidance * (distance2 / 0.45f)));
			m_velocity += guidance * deltaTime * m_snapForce * m_snapInfluence;
			m_velocity *= Mathf.Exp((0f - Mathf.Lerp(m_flyDamping, m_snapDamping, m_snapInfluence)) * deltaTime);
		}
		m_wheelPosition += m_velocity * deltaTime;
		minPosition -= 0.49f;
		maxPosition += 0.49f;
		if ((m_wheelPosition <= minPosition && m_velocity < 0f) || (m_wheelPosition >= maxPosition && m_velocity > 0f))
		{
			m_wheelPosition = Mathf.Clamp(m_wheelPosition, minPosition, maxPosition);
			m_velocity = 0f;
			AbortCruise();
		}
	}

	private void UpdateSelection()
	{
		int selection = Mathf.RoundToInt(m_wheelPosition);
		if (m_lastSelection != selection)
		{
			m_lastSelection = selection;
			if (this.OnSelectionChanged != null)
			{
				this.OnSelectionChanged();
			}
		}
	}

	private void LayoutTiles()
	{
		Tile[] tiles = m_tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			Tile tile = tiles[i];
			int tileCycle = Mathf.FloorToInt((m_wheelPosition - (float)tile.indexOffset) / (float)m_maxTiles + 0.5f);
			int tilePos = tile.indexOffset + tileCycle * m_maxTiles;
			if (tilePos < 0 || tilePos >= m_numTiles)
			{
				tile.gameObject.SetActive(value: false);
				continue;
			}
			tile.gameObject.SetActive(value: true);
			float tileAngle = ((float)tilePos - m_wheelPosition) * m_tileSpacing / m_radius;
			Vector3 localPosition = tile.gameObject.transform.localPosition;
			localPosition.y = m_radius * Mathf.Sin(tileAngle);
			localPosition.z = m_radius * (1f - Mathf.Cos(tileAngle));
			tile.gameObject.transform.localPosition = localPosition;
			tile.gameObject.transform.localEulerAngles = new Vector3(tileAngle * 180f / (float)Math.PI, 0f, 0f);
			int index = GetIndexFromPosition(tilePos);
			AssignIndexToTile(tile.gameObject, index);
		}
	}

	protected void AssignIndexToTile(GameObject tile, int index)
	{
		Widget widget = tile.GetComponentInChildren<Widget>();
		IDataModel dataModel = GetDataModel(index);
		if (widget != null && dataModel != null)
		{
			widget.BindDataModel(dataModel);
		}
	}

	private Camera GetCamera()
	{
		return CameraUtils.FindFirstByLayer(base.gameObject.layer);
	}

	private Vector3 GetLocalMousePos()
	{
		Camera cam = GetCamera();
		Vector3 planeOrigin = m_dragRegion.GetComponent<BoxCollider>().bounds.min;
		Plane trackPlane = new Plane(-cam.transform.forward, planeOrigin);
		Ray mouseRay = cam.ScreenPointToRay(InputCollection.GetMousePosition());
		if (trackPlane.Raycast(mouseRay, out var dist))
		{
			return base.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
		}
		return Vector3.zero;
	}

	private void InitializeCruiseClicker(PegUIElement cruiser, int dir)
	{
		if (cruiser == null)
		{
			return;
		}
		cruiser.AddEventListener(UIEventType.PRESS, delegate
		{
			m_cruiseDir = (m_invertScrolling ? (-dir) : dir);
			float a = ((Mathf.Sign(m_velocity) == Mathf.Sign(m_cruiseDir)) ? Mathf.Abs(m_velocity) : 0f);
			m_velocity = (float)m_cruiseDir * Mathf.Max(a, m_cruiseStartSpeed);
			m_scrollTargetPosition = null;
		});
		cruiser.AddEventListener(UIEventType.RELEASE, delegate
		{
			if (m_cruiseDir != 0)
			{
				float value = Mathf.Round(m_wheelPosition + (float)m_cruiseDir * 0.51f);
				m_scrollTargetPosition = Mathf.Clamp(value, 0f, m_numTiles - 1);
				m_cruiseDir = 0;
			}
		});
		PegCursor.Mode cursor = ((dir > 0) ? PegCursor.Mode.UPARROW : PegCursor.Mode.DOWNARROW);
		cruiser.SetCursorOver(cursor);
		cruiser.SetCursorDown(cursor);
	}

	private int GetIndexFromPosition(int position)
	{
		if (!m_topToBottom)
		{
			return position;
		}
		return m_numTiles - 1 - position;
	}

	private float GetPositionFromIndex(int index)
	{
		return m_topToBottom ? (m_numTiles - 1 - index) : index;
	}

	private void AbortCruise()
	{
		m_cruiseDir = 0;
	}
}
