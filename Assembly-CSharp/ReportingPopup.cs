using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class ReportingPopup : MonoBehaviour
{
	public const string ReportHeaderText = "GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER";

	public const string ReportHeaderFriendText = "GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_FRIEND";

	public const string ReportHeaderNonfriendText = "GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_NONFRIEND";

	public const string ReportReasonDropdownDefaultText = "GLOBAL_FRIENDLIST_REPORT_SELECT_REASON";

	public const string ShowReportReasonEvent = "SHOW_REPORT_REASON";

	public const string ShowReportReasonCheckboxesEvent = "SHOW_REPORT_REASON_CHECKBOXES";

	public const string ShowReportCompleteEvent = "SHOW_REPORT_COMPLETE";

	public const string DismissPopupEvent = "DISMISS_POPUP";

	public const string ReportReasonNextReleasedEvent = "SELECT_REASON_NEXT_BUTTON_CLICKED";

	public const string ReportDetailsSubmitReleasedEvent = "SELECT_DETAILS_SUBMIT_BUTTON_CLICKED";

	private static Dictionary<ReportType.ComplaintType, List<ReportType.SubcomplaintType>> ReportReasons = new Dictionary<ReportType.ComplaintType, List<ReportType.SubcomplaintType>>
	{
		{
			ReportType.ComplaintType.INAPPROPRIATE_NAME,
			new List<ReportType.SubcomplaintType> { ReportType.SubcomplaintType.BATTLETAG }
		},
		{
			ReportType.ComplaintType.INAPPROPRIATE_COMMUNICATION,
			new List<ReportType.SubcomplaintType>
			{
				ReportType.SubcomplaintType.TEXT_CHAT,
				ReportType.SubcomplaintType.SPAM,
				ReportType.SubcomplaintType.CHAT_ADVERTISEMENT
			}
		},
		{
			ReportType.ComplaintType.CHEATING,
			new List<ReportType.SubcomplaintType>
			{
				ReportType.SubcomplaintType.HACKING,
				ReportType.SubcomplaintType.BOTTING,
				ReportType.SubcomplaintType.BOOSTING_DERANKING
			}
		}
	};

	private static Dictionary<ReportType.ComplaintType, string> ComplaintTypeLabels = new Dictionary<ReportType.ComplaintType, string>
	{
		{
			ReportType.ComplaintType.INAPPROPRIATE_NAME,
			"GLOBAL_REPORT_REASON_INAPPROPRIATE_NAME"
		},
		{
			ReportType.ComplaintType.INAPPROPRIATE_COMMUNICATION,
			"GLOBAL_REPORT_REASON_INAPPROPRIATE_CHAT"
		},
		{
			ReportType.ComplaintType.CHEATING,
			"GLOBAL_REPORT_REASON_CHEATING"
		}
	};

	private static Dictionary<ReportType.SubcomplaintType, string> SubcomplaintTypeLabels = new Dictionary<ReportType.SubcomplaintType, string>
	{
		{
			ReportType.SubcomplaintType.BATTLETAG,
			"GLOBAL_REPORT_DETAIL_BATTLETAG"
		},
		{
			ReportType.SubcomplaintType.TEXT_CHAT,
			"GLOBAL_REPORT_DETAIL_HARASSMENT"
		},
		{
			ReportType.SubcomplaintType.SPAM,
			"GLOBAL_REPORT_DETAIL_SPAM"
		},
		{
			ReportType.SubcomplaintType.CHAT_ADVERTISEMENT,
			"GLOBAL_REPORT_DETAIL_ADVERTISEMENT"
		},
		{
			ReportType.SubcomplaintType.HACKING,
			"GLOBAL_REPORT_DETAIL_HACKING"
		},
		{
			ReportType.SubcomplaintType.BOTTING,
			"GLOBAL_REPORT_DETAIL_BOTTING"
		},
		{
			ReportType.SubcomplaintType.BOOSTING_DERANKING,
			"GLOBAL_REPORT_DETAIL_INTENTIONALLY_LOSING_DERANKING"
		}
	};

	[SerializeField]
	private AsyncReference m_reportReasonHeaderReference;

	[SerializeField]
	private AsyncReference m_reportDetailsHeaderReference;

	[SerializeField]
	private AsyncReference m_reportReasonDropdownReference;

	[SerializeField]
	private AsyncReference m_reportReasonDetailReference;

	[SerializeField]
	private AsyncReference[] m_reportReasonDetailCheckboxReferences;

	[SerializeField]
	private AsyncReference m_reportReasonNextButtonReference;

	[SerializeField]
	private AsyncReference m_reportDetailsSubmitButtonReference;

	private Widget m_widget;

	private BnetPlayer m_player;

	private UberText m_reportReasonHeader;

	private UberText m_reportDetailHeader;

	private DropdownControl m_reportReasonDropdown;

	private List<CheckBox> m_reportDetailCheckboxes;

	private UIBButton m_reportReasonNextButton;

	private UIBButton m_reportDetailsSubmitButton;

	private ReportType.ComplaintType? m_selectedIssueType;

	private HashSet<ReportType.SubcomplaintType> m_selectedReportDetails = new HashSet<ReportType.SubcomplaintType>();

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.SetLayerOverride(GameLayer.HighPriorityUI);
		m_widget.RegisterEventListener(ReportingPopupEventListener);
		m_widget.RegisterReadyListener(delegate
		{
			m_widget.TriggerEvent("SHOW_REPORT_REASON");
		});
		m_reportReasonHeaderReference.RegisterReadyListener(delegate(UberText reportHeader)
		{
			m_reportReasonHeader = reportHeader;
		});
		m_reportDetailsHeaderReference.RegisterReadyListener(delegate(UberText reportHeader)
		{
			m_reportDetailHeader = reportHeader;
		});
		m_reportReasonDropdownReference.RegisterReadyListener(delegate(DropdownControl dropdownControl)
		{
			m_reportReasonDropdown = dropdownControl;
			SetReportReasons();
		});
		m_reportDetailCheckboxes = new List<CheckBox>();
		AsyncReference[] reportReasonDetailCheckboxReferences = m_reportReasonDetailCheckboxReferences;
		for (int i = 0; i < reportReasonDetailCheckboxReferences.Length; i++)
		{
			reportReasonDetailCheckboxReferences[i].RegisterReadyListener(delegate(CheckBox checkbox)
			{
				m_reportDetailCheckboxes.Add(checkbox);
				checkbox.SetChecked(isChecked: false);
			});
		}
		m_reportReasonNextButtonReference.RegisterReadyListener(delegate(UIBButton button)
		{
			m_reportReasonNextButton = button;
			SetButton(m_reportReasonNextButton, enableState: false, forceImmediate: true);
		});
		m_reportDetailsSubmitButtonReference.RegisterReadyListener(delegate(UIBButton button)
		{
			m_reportDetailsSubmitButton = button;
			SetButton(m_reportDetailsSubmitButton, enableState: false, forceImmediate: true);
		});
	}

	private void ReportingPopupEventListener(string eventName)
	{
		if (!(eventName == "SELECT_REASON_NEXT_BUTTON_CLICKED"))
		{
			if (eventName == "SELECT_DETAILS_SUBMIT_BUTTON_CLICKED")
			{
				OnReportDetailsSubmitReleased();
			}
		}
		else
		{
			OnReportReasonNextReleased();
		}
	}

	public void Init(BnetPlayer player)
	{
		m_player = player;
		string playerName = player.GetFullName() ?? player.GetBattleTag().ToString();
		if (BnetFriendMgr.Get().IsFriend(m_player))
		{
			m_reportReasonHeader.Text = GameStrings.Format("GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_FRIEND", playerName);
			m_reportDetailHeader.Text = GameStrings.Format("GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_FRIEND", playerName);
		}
		else
		{
			m_reportReasonHeader.Text = GameStrings.Format("GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_NONFRIEND", playerName);
			m_reportDetailHeader.Text = GameStrings.Format("GLOBAL_FRIENDLIST_REPORT_SELECT_HEADER_NONFRIEND", playerName);
		}
		m_selectedIssueType = null;
		m_selectedReportDetails.Clear();
		m_reportReasonDropdown.setSelection(null);
		SetButton(m_reportReasonNextButton, enableState: false, forceImmediate: true);
		SetButton(m_reportDetailsSubmitButton, enableState: false, forceImmediate: true);
		m_widget.TriggerEvent("SHOW_REPORT_REASON");
	}

	private void SetReportReasons()
	{
		m_reportReasonDropdown.clearItems();
		m_reportReasonDropdown.setItemChosenCallback(OnReportReasonSelect);
		m_reportReasonDropdown.setItemTextCallback(GetReportReasonString);
		m_reportReasonDropdown.setUnselectedItemText(GameStrings.Get("GLOBAL_FRIENDLIST_REPORT_SELECT_REASON"));
		foreach (KeyValuePair<ReportType.ComplaintType, List<ReportType.SubcomplaintType>> reportReason2 in ReportReasons)
		{
			ReportType.ComplaintType reportReason = reportReason2.Key;
			m_reportReasonDropdown.addItem(reportReason);
		}
		LayerUtils.SetLayer(m_reportReasonDropdown, GameLayer.HighPriorityUI);
		m_reportReasonDropdown.gameObject.SetActive(value: true);
	}

	private void SetDetailsForReason(ReportType.ComplaintType issue)
	{
		if (!ReportReasons.TryGetValue(issue, out var subcomplaints) || subcomplaints == null)
		{
			return;
		}
		for (int i = 0; i < subcomplaints.Count; i++)
		{
			if (i >= m_reportDetailCheckboxes.Count)
			{
				break;
			}
			CheckBox checkBox = m_reportDetailCheckboxes[i];
			checkBox.SetChecked(isChecked: false);
			ReportType.SubcomplaintType subcomplaintType = subcomplaints[i];
			checkBox.SetButtonText(SubcomplaintTypeLabels[subcomplaintType]);
			checkBox.ClearEventListeners();
			checkBox.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnReportDetailSelectionChanged(checkBox, subcomplaintType);
			});
			checkBox.gameObject.SetActive(value: true);
		}
		for (int i2 = m_reportDetailCheckboxes.Count - 1; i2 >= subcomplaints.Count; i2--)
		{
			CheckBox checkBox2 = m_reportDetailCheckboxes[i2];
			checkBox2.SetButtonText(string.Empty);
			checkBox2.ClearEventListeners();
			checkBox2.gameObject.SetActive(value: false);
		}
	}

	private string GetReportReasonString(object val)
	{
		string reasonString = "";
		if (val is ReportType.ComplaintType && ComplaintTypeLabels.TryGetValue((ReportType.ComplaintType)val, out var complaintLabel))
		{
			return complaintLabel;
		}
		return reasonString;
	}

	private void OnReportReasonSelect(object selection, object prevSelection)
	{
		if (selection is ReportType.ComplaintType complaintType)
		{
			m_selectedIssueType = complaintType;
			SetButton(m_reportReasonNextButton, enableState: true);
		}
		else
		{
			SetButton(m_reportReasonNextButton, enableState: false);
		}
	}

	private void OnReportDetailSelectionChanged(CheckBox checkbox, ReportType.SubcomplaintType subcomplaintType)
	{
		if (checkbox.IsChecked())
		{
			if (m_selectedReportDetails.Count == 0)
			{
				SetButton(m_reportDetailsSubmitButton, enableState: true);
			}
			m_selectedReportDetails.Add(subcomplaintType);
		}
		else
		{
			if (m_selectedReportDetails.Count == 1)
			{
				SetButton(m_reportDetailsSubmitButton, enableState: false);
			}
			m_selectedReportDetails.Remove(subcomplaintType);
		}
	}

	private void OnReportReasonNextReleased()
	{
		if (m_selectedIssueType.HasValue)
		{
			m_widget.TriggerEvent("SHOW_REPORT_REASON_CHECKBOXES");
			SetDetailsForReason(m_selectedIssueType.Value);
		}
	}

	private void OnReportDetailsSubmitReleased()
	{
		m_widget.TriggerEvent("SHOW_REPORT_COMPLETE");
		if (m_selectedIssueType.HasValue)
		{
			BattleNet.Get().SubmitReport(m_player.GetAccountId(), m_selectedIssueType.Value, new List<ReportType.SubcomplaintType>(m_selectedReportDetails));
		}
	}

	private void SetButton(UIBButton button, bool enableState, bool forceImmediate = false)
	{
		button.Flip(enableState, forceImmediate);
		button.SetEnabled(enableState);
	}
}
