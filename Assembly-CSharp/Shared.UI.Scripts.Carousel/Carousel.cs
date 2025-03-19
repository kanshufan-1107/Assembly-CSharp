using System;
using System.Linq;
using UnityEngine;

namespace Shared.UI.Scripts.Carousel;

public class Carousel : MonoBehaviour
{
	public interface Item
	{
		void Show(Carousel parent);

		void Hide();

		void Clear();

		GameObject GetGameObject();

		bool IsLoaded();
	}

	public delegate void SettledEventHandler();

	public delegate void StartedScrollingEventHandler();

	public delegate void ItemPulledEventHandler(Item item, int index);

	public delegate void ItemClickedEventHandler(Item item, int index);

	public delegate void ItemReleasedHandler();

	private const float MIN_VELOCITY = 0.03f;

	private const float DRAG = 0.015f;

	private const float SCROLL_THRESHOLD = 10f;

	private const float MOVE_DISTANCE = 4.5f;

	private const float POSITION_ESPILON = 0.01f;

	private readonly Vector3 m_offScreenPosition = new Vector3(81f, 9.4f, 4f);

	public Transform[] m_bones;

	public Transform m_centerBone;

	public Vector3 m_centerBoneOffset;

	public bool m_useFlyIn;

	public bool m_trackItemHit;

	public bool m_noMouseMovement;

	public int m_maxPosition = 4;

	public float m_minSpacing = 1f;

	private float m_position;

	private float m_targetPosition;

	private bool m_touchActive;

	private Vector2 m_touchStart;

	private float m_startX;

	private float m_touchX;

	private float m_velocity;

	private int m_hitIndex;

	private Item m_hitItem;

	private Vector3 m_hitWorldPosition;

	private float m_hitCarouselPosition;

	private float m_totalMove;

	private float m_moveAdjustment;

	private bool m_doFlyIn;

	private float m_flyInState;

	private bool m_settleCallbackCalled;

	private readonly ItemCollection m_items = new ItemCollection();

	private readonly MomentumHistory m_momentumHistory = new MomentumHistory(5);

	public int CurrentIndex => Mathf.RoundToInt(m_position);

	public Item CurrentItem => m_items.GetClampedItemAtIndex(CurrentIndex);

	public bool IsScrolling { get; private set; }

	private int Radius => m_bones.Length / 2;

	private bool IsAtTargetPosition => Math.Abs(m_position - m_targetPosition) < 0.01f;

	private float PullDistance => (float)Screen.height * 0.1f;

	private float MinPullY => (float)Screen.height * 0.275f;

	private float Speed => Mathf.Abs(m_velocity);

	private bool ShouldSettle
	{
		get
		{
			if (!m_touchActive && !IsAtTargetPosition)
			{
				return Speed < Mathf.Epsilon;
			}
			return false;
		}
	}

	public event SettledEventHandler OnSettled;

	public event StartedScrollingEventHandler OnStartedScrolling;

	public event ItemPulledEventHandler OnItemPulled;

	public event ItemClickedEventHandler OnItemClicked;

	public event ItemReleasedHandler OnItemReleased;

	public event ItemPulledEventHandler OnItemCrossedCenterPosition;

	private void Start()
	{
		m_trackItemHit = true;
	}

	public void UpdateUI(bool mouseDown)
	{
		bool outOfBounds = m_position < 0f || m_position > (float)m_maxPosition;
		Vector3 mousePosition = InputCollection.GetMousePosition();
		bool isOverWindow = mousePosition.x >= 0f && mousePosition.y >= 0f && mousePosition.x < (float)Screen.width && mousePosition.y < (float)Screen.height;
		if (m_touchActive)
		{
			if (isOverWindow)
			{
				float newTouchX = mousePosition.x;
				float num = newTouchX - m_touchX;
				if (!IsScrolling && Math.Abs(m_touchStart.x - newTouchX) >= 10f)
				{
					StartScrolling();
				}
				float moveDistance = num * 4.5f / (float)Screen.width;
				if (m_position < 0f)
				{
					moveDistance /= 1f + Math.Abs(m_position) * 5f;
				}
				if (!m_noMouseMovement)
				{
					if (m_trackItemHit)
					{
						UniversalInputManager.Get().GetInputPointOnPlane(m_hitWorldPosition, out var currentWorldPosition);
						m_position = m_startX + m_hitCarouselPosition - GetCarouselPosition(currentWorldPosition.x);
					}
					else
					{
						m_position -= moveDistance;
					}
				}
				m_momentumHistory.Put(moveDistance);
				outOfBounds = m_position < 0f || m_position > (float)m_maxPosition;
				m_touchX = newTouchX;
				if (mousePosition.y - m_touchStart.y > PullDistance && mousePosition.y > MinPullY)
				{
					this.OnItemPulled?.Invoke(m_hitItem, m_hitIndex);
					m_touchActive = false;
					m_velocity = 0f;
					SettlePosition();
				}
			}
			if (!InputCollection.GetMouseButton(0))
			{
				if (!m_noMouseMovement && IsScrolling)
				{
					m_velocity = m_momentumHistory.CalculateVelocity() * -0.9f;
					if (m_position < 0f)
					{
						m_targetPosition = 0f;
						m_velocity = 0f;
					}
					else if (m_position >= (float)m_maxPosition)
					{
						m_targetPosition = m_maxPosition;
						m_velocity = 0f;
					}
					else if (Math.Abs(m_velocity) < 0.03f)
					{
						SettlePosition();
						m_velocity = 0f;
					}
				}
				this.OnItemReleased?.Invoke();
				m_touchActive = false;
			}
		}
		Item itemHit;
		int indexHit = m_items.MouseHit(out itemHit);
		if (mouseDown && indexHit >= 0)
		{
			m_touchActive = true;
			if (isOverWindow)
			{
				m_touchStart = mousePosition;
			}
			m_touchX = m_touchStart.x;
			m_velocity = 0f;
			m_hitIndex = indexHit;
			m_hitItem = itemHit;
			IsScrolling = false;
			m_settleCallbackCalled = false;
			if (m_trackItemHit)
			{
				UniversalInputManager.Get().GetInputHitInfo(out var cardHitInfo);
				m_hitWorldPosition = cardHitInfo.point;
				m_hitCarouselPosition = GetCarouselPosition(m_hitWorldPosition.x);
				m_startX = m_position;
			}
			InitVelocity();
			this.OnItemClicked?.Invoke(m_hitItem, indexHit);
		}
		if (!m_touchActive && m_velocity != 0f)
		{
			if (Math.Abs(m_velocity) < 0.03f || outOfBounds)
			{
				SettlePosition();
				m_velocity = 0f;
			}
			else
			{
				m_position += m_velocity;
				m_velocity -= 0.015f * (float)Math.Sign(m_velocity);
			}
		}
		if (ShouldSettle)
		{
			Settle();
		}
		UpdateVisibleItems();
		if (m_doFlyIn)
		{
			float delta = Math.Min(0.03f, Time.deltaTime);
			m_flyInState += delta * 8f;
			if (m_flyInState > (float)m_bones.Length)
			{
				m_doFlyIn = false;
			}
		}
	}

	public void Initialize(Item[] items)
	{
		m_items.SetItems(items);
		m_position = 0f;
		m_targetPosition = 0f;
		DoFlyIn();
	}

	public void SetPosition(int position, bool animate = false)
	{
		m_targetPosition = position;
		if (!animate)
		{
			m_position = m_targetPosition;
		}
		else
		{
			StartScrolling();
			m_settleCallbackCalled = false;
		}
		DoFlyIn();
	}

	private void DoFlyIn()
	{
		if (m_useFlyIn)
		{
			m_doFlyIn = true;
			m_flyInState = 0f;
		}
	}

	private void Settle()
	{
		m_position = Mathf.Lerp(m_targetPosition, m_position, 0.85f);
		if (IsAtTargetPosition)
		{
			if (!m_settleCallbackCalled)
			{
				this.OnSettled?.Invoke();
				m_settleCallbackCalled = true;
			}
			m_position = m_targetPosition;
			IsScrolling = false;
		}
	}

	private float GetCarouselPosition(float x)
	{
		Transform firstBone = m_bones.First();
		if (x < firstBone.position.x)
		{
			return 0f;
		}
		Transform lastBone = m_bones.Last();
		if (x > lastBone.position.x)
		{
			return (float)m_bones.Length - 1f;
		}
		float lastX = firstBone.position.x;
		foreach (var (position, index2) in m_bones.Select((Transform bone, int index) => (x: bone.position.x, index: index)))
		{
			if (x >= lastX && x <= position)
			{
				return (float)index2 + (x - lastX) / (position - lastX);
			}
			lastX = position;
		}
		return 0f;
	}

	private void InitVelocity()
	{
		m_momentumHistory.Clear();
	}

	private void SettlePosition()
	{
		float nearest = ((m_velocity > 0.001f) ? Mathf.Ceil(m_position) : ((!(m_velocity < -0.001f)) ? Mathf.Round(m_position) : Mathf.Floor(m_position)));
		m_targetPosition = Mathf.Clamp(nearest, 0f, m_maxPosition);
	}

	public void UpdateVisibleItems(int startingIndex = 0, bool shouldTriggerOnCrossedCenterPosition = true)
	{
		float adjustedPosition = m_position;
		int itemPosition = Mathf.FloorToInt(adjustedPosition);
		float progress = 1f - adjustedPosition + (float)itemPosition;
		int currentPosition = 0;
		m_items.ForEach(delegate(Item item, int index)
		{
			if (index >= startingIndex)
			{
				UpdateItemPosition(item, index, progress, itemPosition, currentPosition, shouldTriggerOnCrossedCenterPosition);
			}
			currentPosition++;
		});
	}

	private void UpdateItemPosition(Item item, int index, float progress, int itemPosition, int currentPosition, bool shouldTriggerOnCrossedCenterPosition)
	{
		int boneIndex1 = index - itemPosition + Radius - 1;
		int boneIndex2 = index - itemPosition + Radius;
		if (boneIndex1 < 0 || boneIndex2 >= m_bones.Length)
		{
			item.Hide();
			if (item.IsLoaded())
			{
				Transform boneToUse = m_bones[(boneIndex1 >= 0) ? (m_bones.Length - 1) : 0];
				Transform obj = item.GetGameObject().transform;
				obj.localPosition = boneToUse.localPosition;
				obj.localScale = boneToUse.localScale;
				obj.localRotation = boneToUse.localRotation;
			}
			return;
		}
		item.Show(this);
		Transform itemTransform = item.GetGameObject().transform;
		if (!item.IsLoaded())
		{
			return;
		}
		Transform obj2 = m_bones[boneIndex1];
		Transform bone2 = m_bones[boneIndex2];
		Vector3 position = Vector3.Lerp(obj2.localPosition, bone2.localPosition, progress);
		Vector3 scale = Vector3.Lerp(obj2.localScale, bone2.localScale, progress);
		Quaternion rotate = Quaternion.Lerp(obj2.localRotation, bone2.localRotation, progress);
		if (m_doFlyIn)
		{
			itemTransform.localPosition = ComputeFlyInPosition(currentPosition, position);
		}
		else
		{
			Vector3 prevLocalPos = itemTransform.localPosition;
			itemTransform.localPosition = position;
			if (shouldTriggerOnCrossedCenterPosition && m_centerBone != null && CrossedCenterPosition(position, prevLocalPos))
			{
				this.OnItemCrossedCenterPosition?.Invoke(item, index);
			}
		}
		itemTransform.localScale = scale;
		itemTransform.localRotation = rotate;
	}

	private Vector3 ComputeFlyInPosition(int positionIndex, Vector3 position)
	{
		float flyInVal = 1f;
		if (positionIndex >= (int)m_flyInState + 1)
		{
			flyInVal = 0f;
		}
		else if (positionIndex >= (int)m_flyInState)
		{
			flyInVal = m_flyInState - Mathf.Floor(m_flyInState);
		}
		return Vector3.Lerp(position, m_offScreenPosition, 1f - flyInVal);
	}

	private bool CrossedCenterPosition(Vector3 position, Vector3 prevLocalPos)
	{
		Vector3 centerPos = m_centerBone.localPosition + m_centerBoneOffset;
		if (position.x - centerPos.x <= Mathf.Epsilon)
		{
			return prevLocalPos.x - position.x >= Mathf.Epsilon;
		}
		return (prevLocalPos.x <= centerPos.x && position.x >= centerPos.x) || (prevLocalPos.x >= centerPos.x && position.x <= centerPos.x);
	}

	private void StartScrolling()
	{
		IsScrolling = true;
		this.OnStartedScrolling?.Invoke();
	}

	private static Vector2 GetMousePosition()
	{
		return ClampToScreen(InputCollection.GetMousePosition());
	}

	private static Vector2 ClampToScreen(Vector2 position)
	{
		return new Vector2(Mathf.Clamp(position.x, 0f, Screen.width), Mathf.Clamp(position.y, 0f, Screen.height));
	}
}
