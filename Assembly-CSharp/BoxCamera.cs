using System.Collections;
using Blizzard.T5.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BoxCamera : MonoBehaviour
{
	public enum State
	{
		UNKNOWN = -1,
		CLOSED,
		CLOSED_TUTORIAL,
		CLOSED_TUTORIAL_VIDEO_PREVIEW,
		CLOSED_WITH_DRAWER,
		OPENED,
		SET_ROTATION_OPENED
	}

	private Box m_parent;

	private BoxCameraStateInfo m_info;

	private State m_state;

	private bool m_disableAccelerometer = true;

	private bool m_applyAccelerometer;

	private Vector2 m_currentAngle;

	private Vector3 m_basePosition;

	private Vector2 m_gyroRotation;

	private float m_offset;

	private float MAX_GYRO_RANGE = 2.1f;

	private float ROTATION_SCALE = 0.085f;

	private Vector3 m_lookAtPoint;

	private Camera m_camera;

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		if (!m_camera)
		{
			Log.All.PrintError("BoxCamera: m_camera is null.");
		}
		Camera c = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
		if (c != null)
		{
			UniversalAdditionalCameraData data = m_camera.GetUniversalAdditionalCameraData();
			if (!data.cameraStack.Contains(c))
			{
				data.cameraStack.Add(c);
			}
		}
		if (m_camera != null)
		{
			m_camera.allowMSAA = ServiceManager.Get<IGraphicsManager>().AllowMSAA();
			CameraManager.Get().BaseCamera = m_camera;
		}
	}

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public Box GetParent()
	{
		return m_parent;
	}

	public BoxCameraStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxCameraStateInfo info)
	{
		m_info = info;
	}

	public Vector3 GetCameraPosition(State state)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			Vector3 min;
			Vector3 wide;
			Vector3 extraWide;
			switch (state)
			{
			case State.CLOSED:
				min = m_info.m_ClosedMinAspectRatioBone.transform.position;
				wide = m_info.m_ClosedBone.transform.position;
				extraWide = m_info.m_ClosedExtraWideAspectRatioBone.transform.position;
				break;
			case State.CLOSED_WITH_DRAWER:
				min = m_info.m_ClosedWithDrawerMinAspectRatioBone.transform.position;
				wide = m_info.m_ClosedWithDrawerBone.transform.position;
				extraWide = m_info.m_ClosedWithDrawerExtraWideAspectRatioBone.transform.position;
				break;
			case State.CLOSED_TUTORIAL_VIDEO_PREVIEW:
				min = m_info.m_ClosedTutorialPreviewRightMinAspectRatioBone.transform.position;
				wide = m_info.m_ClosedTutorialPreviewRightBone.transform.position;
				extraWide = m_info.m_ClosedTutorialPreviewRightExtraWideAspectRatioBone.transform.position;
				break;
			default:
				min = m_info.m_OpenedMinAspectRatioBone.transform.position;
				wide = m_info.m_OpenedBone.transform.position;
				extraWide = m_info.m_OpenedExtraWideAspectRatioBone.transform.position;
				break;
			}
			return TransformUtil.GetAspectRatioDependentPosition(min, wide, extraWide);
		}
		return state switch
		{
			State.CLOSED => m_info.m_ClosedBone.transform.position, 
			State.CLOSED_TUTORIAL => m_info.m_ClosedTutorialBone.transform.position, 
			State.CLOSED_TUTORIAL_VIDEO_PREVIEW => m_info.m_ClosedTutorialPreviewRightBone.transform.position, 
			State.CLOSED_WITH_DRAWER => m_info.m_ClosedWithDrawerBone.transform.position, 
			_ => m_info.m_OpenedBone.transform.position, 
		};
	}

	public State GetState()
	{
		return m_state;
	}

	public bool ChangeState(State state, bool force = false)
	{
		if (!force && m_state == state)
		{
			return false;
		}
		Vector3 position = GetCameraPosition(state);
		m_parent.OnAnimStarted();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_applyAccelerometer = false;
			m_basePosition = base.transform.parent.InverseTransformPoint(position);
			m_lookAtPoint = base.transform.parent.InverseTransformPoint(new Vector3(position.x, 1.5f, position.z));
			if (position == base.gameObject.transform.position)
			{
				OnAnimFinished(state);
				return true;
			}
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		switch (state)
		{
		case State.CLOSED:
		case State.CLOSED_TUTORIAL:
		case State.CLOSED_TUTORIAL_VIDEO_PREVIEW:
			args.Add("position", position);
			args.Add("delay", m_info.m_ClosedDelaySec);
			args.Add("time", m_info.m_ClosedMoveSec);
			args.Add("easetype", m_info.m_ClosedMoveEaseType);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompleteparams", state);
			args.Add("oncompletetarget", base.gameObject);
			break;
		case State.CLOSED_WITH_DRAWER:
			args.Add("position", position);
			args.Add("delay", m_info.m_ClosedWithDrawerDelaySec);
			args.Add("time", m_info.m_ClosedWithDrawerMoveSec);
			args.Add("easetype", m_info.m_ClosedWithDrawerMoveEaseType);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompleteparams", state);
			args.Add("oncompletetarget", base.gameObject);
			break;
		case State.OPENED:
			args.Add("position", position);
			args.Add("delay", m_info.m_OpenedDelaySec);
			args.Add("time", m_info.m_OpenedMoveSec);
			args.Add("easetype", m_info.m_OpenedMoveEaseType);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompleteparams", state);
			args.Add("oncompletetarget", base.gameObject);
			break;
		case State.SET_ROTATION_OPENED:
			args.Add("position", position);
			args.Add("delay", m_info.m_OpenedDelaySec);
			args.Add("time", 1.5f);
			args.Add("easetype", m_info.m_OpenedMoveEaseType);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompleteparams", state);
			args.Add("oncompletetarget", base.gameObject);
			break;
		}
		if (CameraShakeMgr.IsShaking(m_camera))
		{
			CameraShakeMgr.Stop(m_camera);
		}
		iTween.MoveTo(base.gameObject, args);
		return true;
	}

	public void EnableAccelerometer()
	{
	}

	public void Update()
	{
		if (!m_disableAccelerometer && !(base.transform.parent.gameObject.GetComponent<LoadingScreen>() != null) && (bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_applyAccelerometer)
			{
				m_gyroRotation.x = Input.gyro.rotationRateUnbiased.x;
				m_gyroRotation.y = 0f - Input.gyro.rotationRateUnbiased.y;
				m_currentAngle.x += m_gyroRotation.y * ROTATION_SCALE;
				m_currentAngle.y += m_gyroRotation.x * ROTATION_SCALE;
				m_currentAngle.x = Mathf.Clamp(m_currentAngle.x, 0f - MAX_GYRO_RANGE, MAX_GYRO_RANGE);
				m_currentAngle.y = Mathf.Clamp(m_currentAngle.y, 0f - MAX_GYRO_RANGE, MAX_GYRO_RANGE);
				base.gameObject.transform.localPosition = new Vector3(m_basePosition.x, m_basePosition.y, m_basePosition.z + m_currentAngle.y);
			}
			Vector3 up = new Vector3(0f, 0f, 1f);
			Vector3 lookAtPointWorld = base.gameObject.transform.parent.TransformPoint(m_lookAtPoint);
			base.gameObject.transform.LookAt(lookAtPointWorld, up);
		}
	}

	private void OnDestroy()
	{
		if (CameraManager.IsInitialized())
		{
			CameraManager.Get().BaseCamera = null;
		}
	}

	public void OnAnimFinished(State state)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_applyAccelerometer = m_state != State.OPENED;
			m_currentAngle = new Vector2(0f, 0f);
		}
		m_state = state;
		m_parent.OnAnimFinished();
	}

	public void UpdateState(State state)
	{
		m_state = state;
		base.transform.position = GetCameraPosition(state);
	}
}
