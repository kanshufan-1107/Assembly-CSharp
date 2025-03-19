using System;
using UnityEngine;

public interface ILegendaryHeroPortrait : IDisposable
{
	Texture PortraitTexture { get; }

	bool IsValidForPath(string assetPath, Player.Side playerSide);

	void AttachToActor(Actor actor);

	void RaiseAnimationEvent(LegendaryHeroAnimations animation);

	void RaiseEmoteAnimationEvent(EmoteType emote);

	void RaiseGenericEvent(string eventName, object eventData = null);

	void AddSlaveAnimator(Animator slaveAnimator, float transitionTimeMultiplier = 1f);

	void RemoveSlaveAnimator(Animator slaveAnimator);

	void ClearDynamicResolutionControllers();

	void ConnectDynamicResolutionController(LegendarySkinDynamicResController controller);

	void UpdateDynamicResolutionControllers();

	GameObject FindGameObjectInLegendaryPortraitPrefab(string objectName);
}
