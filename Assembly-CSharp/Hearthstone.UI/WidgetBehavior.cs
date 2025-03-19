using System;
using System.Collections.Generic;
using Hearthstone.UI.Internal;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[HelpURL("https://confluence.blizzard.com/x/Im8zIQ")]
public abstract class WidgetBehavior : MonoBehaviour, IDataModelProvider, IStatefulWidgetComponent
{
	[HideInInspector]
	[SerializeField]
	private WidgetTemplate m_owner;

	private float m_initializationStartTime;

	protected FlagStateTracker m_startChangingStatesEvent;

	private FlagStateTracker m_doneChangingStatesEvent;

	private bool m_enabledInternally;

	private bool m_changingStatesInternally;

	protected const int InitialDataVersion = 1;

	private int m_dataVersion = 1;

	private bool m_hasOwner;

	protected bool EnabledInterally => m_enabledInternally;

	protected bool IsChangingStatesInternally => m_changingStatesInternally;

	public bool IsActive => m_enabledInternally;

	public bool IsWaitingForOwnerInit
	{
		get
		{
			if (!m_hasOwner)
			{
				return false;
			}
			if ((object)Owner == null)
			{
				Log.UIFramework.PrintError("WidgetBehavior missing template owner: '" + base.name + "'");
				return false;
			}
			return Owner.GetInitializationState() < Widget.InitializationState.InitializingWidgetBehaviors;
		}
	}

	public bool WillTickWhileInactive
	{
		get
		{
			if (m_hasOwner)
			{
				return Owner.WillTickWhileInactive;
			}
			return false;
		}
	}

	public bool CanTick
	{
		get
		{
			if (!IsWaitingForOwnerInit)
			{
				if (!IsActive)
				{
					return WillTickWhileInactive;
				}
				return true;
			}
			return false;
		}
	}

	protected bool CanSendStateChanges
	{
		get
		{
			if (!m_enabledInternally)
			{
				return WillTickWhileInactive;
			}
			return true;
		}
	}

	public float InitializationStartTime => m_initializationStartTime;

	public WidgetTemplate Owner => m_owner;

	public bool WillLoadSynchronously
	{
		get
		{
			if (m_hasOwner)
			{
				return Owner.WillLoadSynchronously;
			}
			return false;
		}
	}

	public abstract bool IsChangingStates { get; }

	protected virtual void Awake()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnEnable()
	{
		m_enabledInternally = true;
		if (m_changingStatesInternally && !WillTickWhileInactive && !m_startChangingStatesEvent.IsSet)
		{
			m_startChangingStatesEvent.SetAndDispatch();
		}
	}

	protected virtual void OnDisable()
	{
		m_enabledInternally = false;
		if (m_changingStatesInternally && !WillTickWhileInactive && m_startChangingStatesEvent.IsSet)
		{
			m_startChangingStatesEvent.Clear();
			m_doneChangingStatesEvent.SetAndDispatch();
		}
	}

	protected void IncrementLocalDataVersion()
	{
		m_dataVersion++;
	}

	public int GetLocalDataVersion()
	{
		return m_dataVersion;
	}

	public void BindDataModel(IDataModel dataModel, bool propagateToChildren = true, bool overrideChildren = false)
	{
		Owner.BindDataModel(dataModel, base.name, propagateToChildren, overrideChildren);
	}

	public bool GetDataModel(int id, out IDataModel dataModel)
	{
		if (!m_hasOwner)
		{
			dataModel = null;
			return false;
		}
		if (!Owner.GetDataModel(id, base.gameObject, out dataModel))
		{
			return GlobalDataContext.Get().GetDataModel(id, out dataModel);
		}
		return true;
	}

	public ICollection<IDataModel> GetDataModels()
	{
		if (m_hasOwner)
		{
			return Owner.GetDataModels();
		}
		return GlobalDataContext.Get().GetDataModels();
	}

	public void PreInitialize(WidgetTemplate owner)
	{
		if (m_owner == null)
		{
			m_owner = owner;
		}
		if (m_owner != null)
		{
			m_hasOwner = true;
		}
	}

	public void Initialize()
	{
		OnInitialize();
	}

	public abstract bool GetIsChangingStates(Func<GameObject, bool> includeGameObject);

	public void RegisterStartChangingStatesListener(Action<object> listener, object payload = null, bool callImmediatelyIfSet = true, bool doOnce = false)
	{
		m_startChangingStatesEvent.RegisterSetListener(listener, payload, callImmediatelyIfSet, doOnce);
	}

	public void RemoveStartChangingStatesListener(Action<object> listener)
	{
		m_startChangingStatesEvent.RemoveSetListener(listener);
	}

	public void RegisterDoneChangingStatesListener(Action<object> listener, object payload = null, bool callImmediatelyIfSet = true, bool doOnce = false)
	{
		m_doneChangingStatesEvent.RegisterSetListener(listener, payload, callImmediatelyIfSet, doOnce);
	}

	public void RemoveDoneChangingStatesListener(Action<object> listener)
	{
		m_doneChangingStatesEvent.RemoveSetListener(listener);
	}

	protected void HandleStartChangingStates()
	{
		if (!m_changingStatesInternally)
		{
			m_changingStatesInternally = true;
			if (CanSendStateChanges && !m_startChangingStatesEvent.IsSet)
			{
				m_doneChangingStatesEvent.Clear();
				m_startChangingStatesEvent.SetAndDispatch();
			}
		}
	}

	protected void HandleDoneChangingStates()
	{
		if (m_changingStatesInternally)
		{
			m_changingStatesInternally = false;
			if (CanSendStateChanges && m_startChangingStatesEvent.IsSet)
			{
				m_startChangingStatesEvent.Clear();
				m_doneChangingStatesEvent.SetAndDispatch();
			}
		}
	}

	protected abstract void OnInitialize();

	public virtual void OnUpdate()
	{
	}

	public virtual bool TryIncrementDataVersion(int id)
	{
		return false;
	}
}
