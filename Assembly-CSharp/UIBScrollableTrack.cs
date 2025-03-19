using System.Collections;
using UnityEngine;

[CustomEditClass]
[RequireComponent(typeof(BoxCollider), typeof(PegUIElement))]
public class UIBScrollableTrack : MonoBehaviour
{
	public UIBScrollable m_parentScrollbar;

	public GameObject m_scrollTrack;

	public Vector3 m_showRotation = Vector3.zero;

	public Vector3 m_hideRotation = new Vector3(0f, 0f, 180f);

	public float m_rotateAnimationTime = 0.5f;

	private bool m_lastEnabled;

	private void Awake()
	{
		if (m_parentScrollbar == null)
		{
			Debug.LogError("Parent scroll bar not set!");
		}
		else
		{
			m_parentScrollbar.AddEnableScrollListener(OnScrollEnabled);
		}
	}

	private void OnEnable()
	{
		if (m_scrollTrack != null)
		{
			m_lastEnabled = m_parentScrollbar.IsEnabled();
			m_scrollTrack.transform.localEulerAngles = (m_lastEnabled ? m_showRotation : m_hideRotation);
		}
	}

	private void OnScrollEnabled(bool enabled)
	{
		if (!(m_scrollTrack == null) && m_scrollTrack.activeInHierarchy && m_lastEnabled != enabled)
		{
			m_lastEnabled = enabled;
			Vector3 start;
			Vector3 end;
			if (enabled)
			{
				start = m_hideRotation;
				end = m_showRotation;
			}
			else
			{
				start = m_showRotation;
				end = m_hideRotation;
			}
			m_scrollTrack.transform.localEulerAngles = start;
			iTween.StopByName(m_scrollTrack, "rotate");
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("rotation", end);
			args.Add("islocal", true);
			args.Add("time", m_rotateAnimationTime);
			args.Add("name", "rotate");
			iTween.RotateTo(m_scrollTrack, args);
		}
	}
}
