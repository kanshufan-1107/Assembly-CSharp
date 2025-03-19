using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureDungeonCrawlHeroPowerOption : AdventureOptionWidget
{
	public delegate void HeroPowerSelectedOptionCallback(long heroPowerDbId, bool isLocked);

	public delegate void HeroPowerHoverOptionCallback(long heroPowerDbId, GameObject bigCardBone);

	[CustomEditField(Sections = "Bones")]
	public GameObject m_BigCardBone;

	private bool m_isDefLoaded;

	public override bool IsReady
	{
		get
		{
			if (base.IsReady)
			{
				return m_isDefLoaded;
			}
			return false;
		}
	}

	protected override void OnWidgetInstanceReady(WidgetInstance widgetInstance)
	{
		base.OnWidgetInstanceReady(widgetInstance);
		if (!(m_widgetInstance == null) && m_databaseId != 0L)
		{
			m_isDefLoaded = false;
			string cardId = GameUtils.TranslateDbIdToCardId((int)m_databaseId);
			DefLoader.Get().LoadFullDef(cardId, OnFullDefLoaded);
		}
	}

	protected override void OnClickableReady(Clickable clickable)
	{
		base.OnClickableReady(clickable);
		if (!(m_clickable == null))
		{
			m_clickable.AddEventListener(UIEventType.RELEASE, delegate
			{
				Select();
			});
		}
	}

	protected override void Rollover()
	{
		base.Rollover();
		if (!(m_rolloverCallback is HeroPowerHoverOptionCallback rolloverCallback))
		{
			Log.Adventures.PrintError("rollover callback was null or was not a HeroPowerHoverOptionCallback!");
		}
		else if (m_dataModel.Locked)
		{
			rolloverCallback(m_databaseId, m_BigCardBone);
		}
	}

	protected override void Rollout()
	{
		base.Rollout();
		if (!(m_rolloutCallback is HeroPowerHoverOptionCallback rolloutCallback))
		{
			Log.Adventures.PrintError("rollout callback was null or was not a HeroPowerHoverOptionCallback!");
		}
		else if (m_dataModel.Locked)
		{
			rolloutCallback(m_databaseId, m_BigCardBone);
		}
	}

	private void OnFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		using (def)
		{
			if (def == null)
			{
				Debug.LogErrorFormat("Unable to load FullDef for cardID={0}", cardID);
				return;
			}
			m_isDefLoaded = true;
			Actor componentInChildren = m_widgetInstance.GetComponentInChildren<Actor>(includeInactive: true);
			componentInChildren.SetFullDef(def);
			componentInChildren.UpdateAllComponents();
			if ((bool)m_widgetInstance)
			{
				m_widgetInstance.TriggerEvent("SetUpState");
			}
		}
	}

	public void Init(long heroPowerDbId, bool locked, string lockedText, bool completed, bool newlyUnlocked, OptionAcknowledgedCallback acknowledgedCallback)
	{
		m_databaseId = heroPowerDbId;
		string heroPowerName = null;
		InitWidget(heroPowerName, locked, lockedText, upgraded: false, completed, newlyUnlocked, acknowledgedCallback);
		if (m_widgetInstance != null)
		{
			string cardId = GameUtils.TranslateDbIdToCardId((int)m_databaseId);
			DefLoader.Get().LoadFullDef(cardId, OnFullDefLoaded);
		}
	}

	public override void Select()
	{
		base.Select();
		if (m_dataModel == null)
		{
			Log.Adventures.PrintError("Attempting to set deck pouch option clickable events but data model was null!");
		}
		else if (!(m_selectedCallback is HeroPowerSelectedOptionCallback selectedCallback))
		{
			Log.Adventures.PrintError("Attempting to set a callback for the AdventureDungeonCrawlHeroPowerOption, but no callback was provided!");
		}
		else
		{
			selectedCallback(m_databaseId, m_dataModel.Locked);
		}
	}

	public override void SetVisible(bool isVisible)
	{
		if (isVisible != m_isVisible)
		{
			base.SetVisible(isVisible);
			Actor heroPowerActor = m_widgetInstance.GetComponentInChildren<Actor>(includeInactive: true);
			if (heroPowerActor == null)
			{
				Log.Adventures.PrintError("Tried to set hero power actor visibility but hero power actor was not found!");
			}
			else if (isVisible)
			{
				m_widgetInstance.Show();
				heroPowerActor.Show();
			}
			else
			{
				m_widgetInstance.Hide();
				heroPowerActor.Hide();
			}
		}
	}
}
