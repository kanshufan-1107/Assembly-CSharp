using UnityEngine;
using UnityEngine.UI;

public class KeyboardFinisherDriver : MonoBehaviour
{
	public KeyboardFinisherSettings Settings;

	[Tooltip("This should be set to the players hero.")]
	public GameObject m_Source;

	[Tooltip("This should be set (at least the location of) the enemy hero.")]
	public GameObject m_Target;

	[Tooltip("This should be set to true if attached to the enemy hero, false if attached to the player hero.")]
	public bool m_OpponentDriver;

	[Tooltip("The text box where warning messages should be displayed")]
	public Text WarningText;

	[Tooltip("The backing window for warning messages.")]
	public GameObject WarningWindow;

	public const float MAX_ALLOWED_FINISHER_TIME = 15f;
}
