using System.Collections;
using System.Collections.Generic;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class BaconScene : BasicScene
{
	private bool m_ratingInfoReceived;

	private bool m_gameSaveDataReceived;

	protected override void Start()
	{
		Network.Get().RegisterNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, OnBaconRatingInfo);
		Network.Get().RequestBaconRatingInfo();
		GameSaveDataManager.Get().Request(new List<GameSaveKeyId>
		{
			GameSaveKeyId.BACON,
			GameSaveKeyId.BACON_DUOS
		}, OnGameSaveDataReceived);
		base.Start();
	}

	public override bool IsUnloading()
	{
		return false;
	}

	private void OnScreenPrefabLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"BaconScene.OnScreenLoaded() - failed to load screen {assetRef}");
		}
	}

	private void OnBaconRatingInfo()
	{
		Network.Get().RemoveNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, OnBaconRatingInfo);
		m_ratingInfoReceived = true;
	}

	private void OnGameSaveDataReceived(bool success)
	{
		m_gameSaveDataReceived = true;
	}

	protected override IEnumerator NotifySceneLoadedWhenReady()
	{
		yield return new WaitForSeconds(0.5f);
		while (!m_ratingInfoReceived)
		{
			yield return null;
		}
		while (!m_gameSaveDataReceived)
		{
			yield return null;
		}
		yield return base.NotifySceneLoadedWhenReady();
	}
}
