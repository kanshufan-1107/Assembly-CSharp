using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.APIGateway;
using Hearthstone.Core;
using UnityEngine;

public class PrivacyGate : IService
{
	private Map<PrivacyFeatures, bool> featuresData;

	public static bool IsServiceReady { get; private set; }

	public event Action OnPrivacySettingsUpdated;

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(RAFManager),
			typeof(LoginManager)
		};
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		featuresData = InitFeaturesMap();
		Processor.RunCoroutine(GetOptInData(OnOptInDataReceived));
		yield return null;
	}

	public void Shutdown()
	{
	}

	public static PrivacyGate Get()
	{
		return ServiceManager.Get<PrivacyGate>();
	}

	public bool FeatureEnabled(PrivacyFeatures privacyFeature)
	{
		if (featuresData.ContainsKey(privacyFeature))
		{
			return featuresData[privacyFeature];
		}
		Log.Privacy.PrintError("Checking Privacy Feature: Privacy feature does not exist in PrivacyGate: " + privacyFeature);
		return false;
	}

	public void SetFeature(PrivacyFeatures privacyFeature, bool isEnabled)
	{
		if (featuresData.ContainsKey(privacyFeature))
		{
			featuresData[privacyFeature] = isEnabled;
			SetOptInData(privacyFeature, !isEnabled);
		}
		else
		{
			Log.Privacy.PrintError("Set Privacy feature: Privacy feature does not exist in PrivacyGate: " + privacyFeature.ToString() + ".");
		}
	}

	public void RefreshPrivacySettings()
	{
		Processor.RunCoroutine(GetOptInData(OnOptInDataReceived));
	}

	private Map<PrivacyFeatures, bool> InitFeaturesMap()
	{
		Map<PrivacyFeatures, bool> features = new Map<PrivacyFeatures, bool>();
		foreach (PrivacyFeatures feature in Enum.GetValues(typeof(PrivacyFeatures)))
		{
			if (feature != PrivacyFeatures.INVALID)
			{
				features.Add(feature, value: false);
			}
		}
		return features;
	}

	private IEnumerator GetOptInData(Action dataReceivedCallback)
	{
		yield return new WaitUntil(() => LoginManager.Get().OptInsReceivedDependency.IsReady());
		featuresData[PrivacyFeatures.CHAT] = !LoginManager.Get().OptInApi.GetAccountOptIn(OptInApi.OptInType.DISABLE_CHAT);
		featuresData[PrivacyFeatures.NEARBY_FRIENDS] = !LoginManager.Get().OptInApi.GetAccountOptIn(OptInApi.OptInType.DISABLE_NEARBY_FRIENDS);
		featuresData[PrivacyFeatures.PUSH_NOTIFICATIONS] = !LoginManager.Get().OptInApi.GetAccountOptIn(OptInApi.OptInType.DISABLE_PUSH_NOTIFICATIONS);
		featuresData[PrivacyFeatures.PERSONALIZED_STORE_ITEMS] = !LoginManager.Get().OptInApi.GetAccountOptIn(OptInApi.OptInType.DISABLE_PERSONALIZED_PRODUCTS);
		dataReceivedCallback?.Invoke();
	}

	private void SetOptInData(PrivacyFeatures privacyFeature, bool isDisabled)
	{
		OptInApi.OptInType optInType = OptInApi.OptInType.INVALID;
		switch (privacyFeature)
		{
		case PrivacyFeatures.CHAT:
			optInType = OptInApi.OptInType.DISABLE_CHAT;
			break;
		case PrivacyFeatures.NEARBY_FRIENDS:
			optInType = OptInApi.OptInType.DISABLE_NEARBY_FRIENDS;
			break;
		case PrivacyFeatures.PUSH_NOTIFICATIONS:
			optInType = OptInApi.OptInType.DISABLE_PUSH_NOTIFICATIONS;
			break;
		case PrivacyFeatures.PERSONALIZED_STORE_ITEMS:
			optInType = OptInApi.OptInType.DISABLE_PERSONALIZED_PRODUCTS;
			break;
		}
		LoginManager.Get().OptInApi.SetAccountOptIn(optInType, isDisabled);
		Processor.RunCoroutine(SetFeaturesStatus());
	}

	private void OnOptInDataReceived()
	{
		Processor.RunCoroutine(SetFeaturesStatus());
	}

	private IEnumerator SetFeaturesStatus()
	{
		yield return new WaitUntil(() => ChatMgr.Get() != null);
		ChatMgr.Get().SetChatFeatureStatus(FeatureEnabled(PrivacyFeatures.CHAT));
		yield return new WaitUntil(() => BnetFriendMgr.Get() != null);
		BnetFriendMgr.Get().SetFriendInviteFeatureStatus(FeatureEnabled(PrivacyFeatures.CHAT));
		yield return new WaitUntil(() => PushNotificationManager.Get() != null);
		PushNotificationManager.Get().SetPushNotificationFeatureStatus(FeatureEnabled(PrivacyFeatures.PUSH_NOTIFICATIONS));
		BnetNearbyPlayerMgr.Get().SetEnabled(FeatureEnabled(PrivacyFeatures.NEARBY_FRIENDS));
		Options.Get().SetBool(Option.NEARBY_PLAYERS, FeatureEnabled(PrivacyFeatures.NEARBY_FRIENDS));
		TelemetryManager.SetTelemetryFeatureStatus(isEnabled: true);
		IsServiceReady = true;
		this.OnPrivacySettingsUpdated?.Invoke();
	}
}
