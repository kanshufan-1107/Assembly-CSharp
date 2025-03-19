using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearthstone;
using Hearthstone.Http;
using MiniJSON;
using PegasusUtil;
using UnityEngine;

public class StoreChallengePrompt : UIBPopup
{
	public delegate void CancelListener(string challengeID);

	public delegate void CompleteListener(string challengeID, bool isSuccess, CancelPurchase.CancelReason? reason, string internalErrorInfo);

	public UIBButton m_submitButton;

	public UIBButton m_cancelButton;

	public UberText m_messageText;

	public UberText m_inputText;

	public GameObject m_infoButtonFrame;

	public UIBButton m_infoButton;

	private const int TASSADAR_CHALLENGE_TIMEOUT_SECONDS = 15;

	private string m_input = string.Empty;

	private string m_challengeID;

	private string m_challengeUrl;

	private JsonNode m_challengeJson;

	private JsonNode m_challengeInput;

	private string m_challengeType;

	public event CancelListener OnCancel;

	public event CompleteListener OnChallengeComplete;

	protected override void Awake()
	{
		base.Awake();
		m_inputText.RichText = false;
		m_submitButton.AddEventListener(UIEventType.RELEASE, OnSubmitPressed);
		m_cancelButton.AddEventListener(UIEventType.RELEASE, OnCancelPressed);
		m_infoButton.AddEventListener(UIEventType.RELEASE, OnInfoPressed);
	}

	public IEnumerator Show(string challengeUrl)
	{
		m_challengeJson = null;
		m_challengeUrl = challengeUrl;
		if (IsShown())
		{
			yield break;
		}
		m_shown = true;
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers["Accept"] = "application/json;charset=UTF-8";
		headers["Accept-Language"] = Localization.GetBnetLocaleName();
		IHttpRequest challenge = HttpRequestFactory.Get().CreateGetRequest(m_challengeUrl);
		challenge.SetRequestHeaders(headers);
		challenge.TimeoutSeconds = 15;
		yield return challenge.SendRequest();
		string error = null;
		if (challenge.IsNetworkError || challenge.IsHttpError)
		{
			error = challenge.ErrorString;
		}
		else if (string.IsNullOrEmpty(challenge.ResponseAsString))
		{
			error = "Empty Response";
		}
		else
		{
			if (HearthstoneApplication.IsInternal())
			{
				Log.BattleNet.PrintInfo("Challenge json received: {0}", challenge.ResponseAsString);
			}
			try
			{
				m_challengeJson = (JsonNode)Json.Deserialize(challenge.ResponseAsString);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				error = $"{ex.GetType().Name}: {ex.Message}";
			}
		}
		if (!string.IsNullOrEmpty(error))
		{
			Log.BattleNet.PrintError("Tassadar Challenge Retrieval Failed: " + error);
			Hide(animate: false);
			string header = GameStrings.Get("GLUE_STORE_GENERIC_BP_FAIL_HEADLINE");
			string message = GameStrings.Get("GLUE_STORE_FAIL_CHALLENGE_TIMEOUT");
			CancelPurchase.CancelReason? reason = null;
			if (challenge.DidTimeout)
			{
				reason = CancelPurchase.CancelReason.CHALLENGE_TIMEOUT;
			}
			DisplayError(header, message, allowInputAgain: false, reason, error);
			yield break;
		}
		JsonNode challengeNode = (JsonNode)m_challengeJson["challenge"];
		m_challengeID = (string)m_challengeJson["challenge_id"];
		string challengePrompt = (string)challengeNode["prompt"];
		m_challengeType = (string)challengeNode["type"];
		m_challengeInput = (JsonNode)((JsonList)challengeNode["inputs"])[0];
		JsonList errorList = (challengeNode.ContainsKey("errors") ? (challengeNode["errors"] as JsonList) : null);
		if (errorList != null && errorList.Count > 0)
		{
			string message2 = string.Join("\n", errorList.Select((object n) => (string)n).ToArray());
			DisplayError((string)m_challengeInput["label"], message2, allowInputAgain: false, CancelPurchase.CancelReason.CHALLENGE_OTHER_ERROR, message2);
			yield break;
		}
		bool showInfoButton = false;
		if (m_challengeType == "cvv")
		{
			showInfoButton = true;
		}
		m_messageText.Text = challengePrompt;
		if (string.IsNullOrEmpty(m_messageText.Text))
		{
			Log.BattleNet.PrintError("Challenge has no prompt text, json received: {0}", challenge.ResponseAsString);
		}
		m_infoButtonFrame.SetActive(showInfoButton);
		m_input = string.Empty;
		UpdateInputText();
		DoShowAnimation(OnShown);
	}

	public string HideChallenge()
	{
		string challengeID = m_challengeID;
		Hide(animate: false);
		return challengeID;
	}

	private void OnShown()
	{
		if (IsShown())
		{
			ShowInput();
		}
	}

	protected override void Hide(bool animate)
	{
		if (IsShown())
		{
			m_shown = false;
			HideInput();
			DoHideAnimation(!animate, OnHidden);
		}
	}

	protected override void OnHidden()
	{
		m_challengeID = null;
	}

	private void Cancel()
	{
		string activeChallengeID = m_challengeID;
		Hide(animate: true);
		if (this.OnCancel != null)
		{
			this.OnCancel(activeChallengeID);
		}
	}

	private void OnSubmitPressed(UIEvent e)
	{
		StartCoroutine(SubmitChallenge());
	}

	private IEnumerator SubmitChallenge()
	{
		HideInput();
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers["Accept"] = "application/json;charset=UTF-8";
		headers["Accept-Language"] = Localization.GetBnetLocaleName();
		headers["Content-Type"] = "application/json;charset=UTF-8";
		string inputId = ((m_challengeInput == null) ? null : ((string)m_challengeInput["input_id"]));
		if (inputId == null)
		{
			inputId = "";
		}
		string inputResponse = ((m_input == null) ? "" : m_input);
		string responseString = Json.Serialize(new JsonNode { 
		{
			"inputs",
			new JsonList
			{
				new JsonNode
				{
					{ "input_id", inputId },
					{ "value", inputResponse }
				}
			}
		} });
		IHttpRequest challengeResponse = HttpRequestFactory.Get().CreatePostRequest(m_challengeUrl, Encoding.UTF8.GetBytes(responseString));
		challengeResponse.SetRequestHeaders(headers);
		challengeResponse.TimeoutSeconds = 15;
		yield return challengeResponse.SendRequest();
		JsonNode responseResult = null;
		string error = null;
		if (challengeResponse.IsNetworkError || challengeResponse.IsHttpError)
		{
			error = challengeResponse.ErrorString;
		}
		else if (string.IsNullOrEmpty(challengeResponse.ResponseAsString))
		{
			error = "Empty Response";
		}
		else
		{
			if (HearthstoneApplication.IsInternal())
			{
				Log.BattleNet.PrintInfo("Submit challenge response json received: {0}", challengeResponse.ResponseAsString);
			}
			try
			{
				responseResult = (JsonNode)Json.Deserialize(challengeResponse.ResponseAsString);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				error = $"{ex.GetType().Name}: {ex.Message}";
			}
		}
		if (!string.IsNullOrEmpty(error))
		{
			Log.BattleNet.PrintError("Tassadar Challenge Submission Failed: " + error);
			Hide(animate: false);
			string header = GameStrings.Get("GLUE_STORE_GENERIC_BP_FAIL_HEADLINE");
			string message = GameStrings.Get("GLUE_STORE_FAIL_CHALLENGE_TIMEOUT");
			CancelPurchase.CancelReason? reason = null;
			if (challengeResponse.DidTimeout)
			{
				reason = CancelPurchase.CancelReason.CHALLENGE_TIMEOUT;
			}
			DisplayError(header, message, allowInputAgain: false, reason, error);
			yield break;
		}
		bool num = (bool)responseResult["done"];
		string activeChallengeID = m_challengeID;
		if (!num)
		{
			JsonNode challenge = responseResult["challenge"] as JsonNode;
			JsonList errorList = (challenge.ContainsKey("errors") ? (challenge["errors"] as JsonList) : new JsonList());
			string message2 = string.Join("\n", errorList.Select((object n) => (string)n).ToArray());
			DisplayError((string)m_challengeInput["label"], message2, allowInputAgain: true, null, null);
			yield break;
		}
		bool success = true;
		error = (responseResult.ContainsKey("error_code") ? (responseResult["error_code"] as string) : null);
		if (!string.IsNullOrEmpty(error))
		{
			success = false;
		}
		if (success)
		{
			Hide(animate: true);
			FireComplete(activeChallengeID, success, null, error);
			yield break;
		}
		string header2 = GameStrings.Get("GLUE_STORE_GENERIC_BP_FAIL_HEADLINE");
		string message3 = GameStrings.Get("GLUE_STORE_FAIL_THROTTLED");
		CancelPurchase.CancelReason reason2 = CancelPurchase.CancelReason.CHALLENGE_OTHER_ERROR;
		if (error == "DENIED")
		{
			reason2 = CancelPurchase.CancelReason.CHALLENGE_DENIED;
			error = null;
		}
		DisplayError(header2, message3, allowInputAgain: false, reason2, error);
	}

	private void DisplayError(string header, string message, bool allowInputAgain, CancelPurchase.CancelReason? reason, string internalErrorInfo)
	{
		ClearInput();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_showAlertIcon = false;
		info.m_headerText = header;
		info.m_text = message;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		if (allowInputAgain)
		{
			info.m_responseCallback = delegate
			{
				ShowInput();
			};
		}
		else
		{
			string activeChallengeID = HideChallenge();
			FireComplete(activeChallengeID, isSuccess: false, reason, internalErrorInfo);
		}
		DialogManager.Get().ShowPopup(info);
	}

	private void FireComplete(string challengeID, bool isSuccess, CancelPurchase.CancelReason? reason, string internalErrorInfo)
	{
		if (this.OnChallengeComplete != null)
		{
			this.OnChallengeComplete(challengeID, isSuccess, reason, internalErrorInfo);
		}
	}

	private void OnCancelPressed(UIEvent e)
	{
		Cancel();
	}

	private void OnInfoPressed(UIEvent e)
	{
		Application.OpenURL(ExternalUrlService.Get().GetCVVLink());
	}

	private void ShowInput()
	{
		m_inputText.gameObject.SetActive(value: false);
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		Bounds textBounds = m_inputText.GetBounds();
		Rect rect = CameraUtils.CreateGUIViewportRect(camera, textBounds.min, textBounds.max);
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = base.gameObject,
			m_password = true,
			m_rect = rect,
			m_updatedCallback = OnInputUpdated,
			m_completedCallback = OnInputComplete,
			m_canceledCallback = OnInputCanceled,
			m_font = m_inputText.TrueTypeFont,
			m_alignment = TextAnchor.MiddleCenter,
			m_maxCharacters = (int)((m_challengeInput != null) ? ((long)m_challengeInput["max_length"]) : 0)
		};
		UniversalInputManager.Get().UseTextInput(inputParams);
		m_submitButton.SetEnabled(enabled: true);
	}

	private void HideInput()
	{
		UniversalInputManager.Get().CancelTextInput(base.gameObject);
		m_inputText.gameObject.SetActive(value: true);
		m_submitButton.SetEnabled(enabled: false);
	}

	private void ClearInput()
	{
		UniversalInputManager.Get().SetInputText("");
	}

	private void OnInputUpdated(string input)
	{
		m_input = input;
		UpdateInputText();
	}

	private void OnInputComplete(string input)
	{
		m_input = input;
		UpdateInputText();
		StartCoroutine(SubmitChallenge());
	}

	private void OnInputCanceled(bool userRequested, GameObject requester)
	{
		m_input = string.Empty;
		UpdateInputText();
		Cancel();
	}

	private void UpdateInputText()
	{
		StringBuilder builder = new StringBuilder(m_input.Length);
		for (int i = 0; i < m_input.Length; i++)
		{
			builder.Append('*');
		}
		m_inputText.Text = builder.ToString();
	}
}
