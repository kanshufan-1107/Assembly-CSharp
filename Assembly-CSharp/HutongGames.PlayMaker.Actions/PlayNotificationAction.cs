using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Plays a notification.")]
[ActionCategory("Pegasus Audio")]
public class PlayNotificationAction : FsmStateAction
{
	[Tooltip("Notification quote prefab to use.")]
	public FsmString m_NotificationPrefab;

	[Tooltip("The VO line to play (+loc string).")]
	public FsmString m_NotificationVO;

	[Tooltip("Notification popup position")]
	public FsmVector3 m_NotificationPosition;

	public override void Reset()
	{
		m_NotificationPrefab = string.Empty;
		m_NotificationVO = string.Empty;
	}

	public override void OnEnter()
	{
		Vector3 position = NotificationManager.DEFAULT_CHARACTER_POS;
		if (!m_NotificationPosition.IsNone)
		{
			position = m_NotificationPosition.Value;
		}
		string gameString = new AssetReference(m_NotificationVO.Value).GetLegacyAssetName();
		NotificationManager.Get().CreateCharacterQuote(m_NotificationPrefab.Value, position, GameStrings.Get(gameString), m_NotificationVO.Value);
	}
}
