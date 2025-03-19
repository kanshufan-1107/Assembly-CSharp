using System;
using UnityEngine;
using UnityEngine.UI;

public class StartupDialog : MonoBehaviour
{
	private const string kStartupDialogResourcePath = "Prefabs/StartupDialog";

	[SerializeField]
	private GameObject m_singleButtonRoot;

	[SerializeField]
	private GameObject m_doubleButtonRoot;

	[SerializeField]
	private Text m_headerText;

	[SerializeField]
	private Text m_bodyText;

	[SerializeField]
	private UGUIButton m_singleButton;

	[SerializeField]
	private UGUIButton m_doubleButton1;

	[SerializeField]
	private UGUIButton m_doubleButton2;

	private static StartupDialog s_instance;

	private PegUIElement m_inputBlocker;

	public static bool IsShown => s_instance != null;

	public static void ShowStartupDialog(string header, string body, string buttonText, Action buttonDelegate)
	{
		if (EnsureInstance())
		{
			s_instance.SetupSingleButtonDialog(header, body, buttonText, buttonDelegate);
		}
	}

	public static void ShowStartupDialog(string header, string body, string buttonText, Action buttonDelegate, bool closeAtClick)
	{
		if (EnsureInstance())
		{
			s_instance.SetupSingleButtonDialog(header, body, buttonText, buttonDelegate, closeAtClick);
		}
	}

	public static void ShowStartupDialog(string header, string body, string buttonText1, Action buttonDelegate1, string buttonText2, Action buttonDelegate2)
	{
		if (EnsureInstance())
		{
			s_instance.SetupDoubleButtonDialog(header, body, buttonText1, buttonDelegate1, buttonText2, buttonDelegate2);
		}
	}

	public static void ShowStartupDialog(string header, string body, string buttonText1, Action buttonDelegate1, bool closeAtClick1, string buttonText2, Action buttonDelegate2, bool closeAtClick2)
	{
		if (EnsureInstance())
		{
			s_instance.SetupDoubleButtonDialog(header, body, buttonText1, buttonDelegate1, closeAtClick1, buttonText2, buttonDelegate2, closeAtClick2);
		}
	}

	public static void Destroy()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(s_instance.gameObject);
			s_instance = null;
		}
	}

	private void CreateInputBlocker()
	{
		Transform root = base.transform.GetChild(0);
		Camera cam = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		GameObject inputBlocker = CameraUtils.CreateInputBlocker(cam, "StartupDialogInputBlocker", root, null, 10f);
		Canvas component = root.GetComponent<Canvas>();
		component.renderMode = RenderMode.ScreenSpaceCamera;
		component.worldCamera = cam;
		inputBlocker.transform.localPosition = new Vector3(0f, 10f, 1f);
		inputBlocker.transform.localRotation = Quaternion.identity;
		inputBlocker.GetComponent<BoxCollider>().size *= 10f;
		inputBlocker.layer = base.gameObject.layer;
		m_inputBlocker = inputBlocker.AddComponent<PegUIElement>();
	}

	private static bool EnsureInstance()
	{
		if (s_instance == null)
		{
			GameObject startupDialogPrefab = Resources.Load<GameObject>("Prefabs/StartupDialog");
			if (startupDialogPrefab == null)
			{
				Debug.LogErrorFormat("Couldn't load prefab at ({0}).", "Prefabs/StartupDialog");
				return false;
			}
			GameObject startupDialogObjectInstance = UnityEngine.Object.Instantiate(startupDialogPrefab);
			s_instance = startupDialogObjectInstance.GetComponent<StartupDialog>();
			if (s_instance == null)
			{
				UnityEngine.Object.Destroy(startupDialogObjectInstance);
				Debug.LogErrorFormat("Couldn't find StartupDialog component on prefab at ({0}).", "Prefabs/StartupDialog");
				return false;
			}
			s_instance.CreateInputBlocker();
			UnityEngine.Object.DontDestroyOnLoad(startupDialogObjectInstance);
		}
		return true;
	}

	private void SetupSingleButtonDialog(string header, string body, string buttonText, Action buttonDelegate)
	{
		m_singleButtonRoot.SetActive(value: true);
		m_doubleButtonRoot.SetActive(value: false);
		m_headerText.text = header;
		m_bodyText.text = body;
		m_singleButton.SetupButton(buttonText, buttonDelegate, Destroy);
	}

	private void SetupSingleButtonDialog(string header, string body, string buttonText, Action buttonDelegate, bool closeAtClick)
	{
		m_singleButtonRoot.SetActive(value: true);
		m_doubleButtonRoot.SetActive(value: false);
		m_headerText.text = header;
		m_bodyText.text = body;
		if (closeAtClick)
		{
			m_singleButton.SetupButton(buttonText, buttonDelegate, Destroy);
		}
		else
		{
			m_singleButton.SetupButton(buttonText, buttonDelegate, null);
		}
	}

	private void SetupDoubleButtonDialog(string header, string body, string buttonText1, Action buttonDelegate1, string buttonText2, Action buttonDelegate2)
	{
		m_singleButtonRoot.SetActive(value: false);
		m_doubleButtonRoot.SetActive(value: true);
		m_headerText.text = header;
		m_bodyText.text = body;
		m_doubleButton1.SetupButton(buttonText1, buttonDelegate1, Destroy);
		m_doubleButton2.SetupButton(buttonText2, buttonDelegate2, Destroy);
	}

	private void SetupDoubleButtonDialog(string header, string body, string buttonText1, Action buttonDelegate1, bool closeAtClick1, string buttonText2, Action buttonDelegate2, bool closeAtClick2)
	{
		m_singleButtonRoot.SetActive(value: false);
		m_doubleButtonRoot.SetActive(value: true);
		m_headerText.text = header;
		m_bodyText.text = body;
		if (closeAtClick1)
		{
			m_doubleButton1.SetupButton(buttonText1, buttonDelegate1, Destroy);
		}
		else
		{
			m_doubleButton1.SetupButton(buttonText1, buttonDelegate1, null);
		}
		if (closeAtClick2)
		{
			m_doubleButton2.SetupButton(buttonText2, buttonDelegate2, Destroy);
		}
		else
		{
			m_doubleButton2.SetupButton(buttonText2, buttonDelegate2, null);
		}
	}
}
