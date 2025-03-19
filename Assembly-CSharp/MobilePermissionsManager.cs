using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class MobilePermissionsManager : IService
{
	public delegate void PermissionResultCallback(MobilePermission permission, bool granted);

	private Map<MobilePermission, List<string>> m_androidPermissionMap = new Map<MobilePermission, List<string>>();

	private Map<MobilePermission, List<PermissionResultCallback>> m_pendingRequests = new Map<MobilePermission, List<PermissionResultCallback>>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InitAndroidPermissionStrings();
		_ = Application.isEditor;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(MobileCallbackManager) };
	}

	public void Shutdown()
	{
	}

	private void InitAndroidPermissionStrings()
	{
		m_androidPermissionMap[MobilePermission.BLUETOOTH] = new List<string> { "android.permission.BLUETOOTH", "android.permission.BLUETOOTH_ADMIN" };
		m_androidPermissionMap[MobilePermission.CAMERA] = new List<string> { "android.permission.CAMERA" };
		m_androidPermissionMap[MobilePermission.MICROPHONE] = new List<string> { "android.permission.RECORD_AUDIO" };
		m_androidPermissionMap[MobilePermission.GOOGLE_PUSH_NOTIFICATIONS] = new List<string> { "com.google.android.c2dm.permission.RECEIVE", "com.blizzard.wtcg.hearthstone.permission.C2D_MESSAGE" };
		m_androidPermissionMap[MobilePermission.AMAZON_PUSH_NOTIFICATIONS] = new List<string> { "com.blizzard.wtcg.hearthstone.permission.RECEIVE_ADM_MESSAGE", "com.amazon.device.messaging.permission.RECEIVE" };
	}
}
