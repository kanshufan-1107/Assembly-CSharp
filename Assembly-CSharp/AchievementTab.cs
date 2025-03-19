using System.Collections;
using Hearthstone.Progression;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementTab : MonoBehaviour
{
	[SerializeField]
	private UberText m_PointsText;

	private const string START_POINT_HUD_ANIMATION = "CODE_START_POINT_HUD_ANIMATION";

	private const int ROLL_UP_TIME = 1;

	private WidgetTemplate m_widget;

	private int m_currentPointsValue;

	private Coroutine m_rollUpRoutine;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.keepAnimatorStateOnDisable = true;
		}
		AchievementManager.Get().OnPointsChanged += OnPointsChanged;
	}

	private void OnEnable()
	{
		m_currentPointsValue = AchievementManager.Get().TotalPoints;
		m_PointsText.Text = m_currentPointsValue.ToString();
	}

	private void OnDestroy()
	{
		if (AchievementManager.Get() != null)
		{
			AchievementManager.Get().OnPointsChanged -= OnPointsChanged;
		}
	}

	private void OnPointsChanged()
	{
		m_widget.TriggerEvent("CODE_START_POINT_HUD_ANIMATION");
		if (m_rollUpRoutine != null)
		{
			StopCoroutine(m_rollUpRoutine);
		}
		m_rollUpRoutine = StartCoroutine(RollUpPoints());
	}

	private IEnumerator RollUpPoints()
	{
		int targetPointsValue = AchievementManager.Get().TotalPoints;
		float time = 0f;
		float rollupPoints = m_currentPointsValue;
		while (time < 1f)
		{
			rollupPoints = Mathf.Lerp(rollupPoints, targetPointsValue, time / 1f);
			time += Time.deltaTime;
			m_currentPointsValue = Mathf.FloorToInt(rollupPoints);
			m_PointsText.Text = m_currentPointsValue.ToString();
			yield return null;
		}
		m_currentPointsValue = targetPointsValue;
		m_PointsText.Text = targetPointsValue.ToString();
	}
}
