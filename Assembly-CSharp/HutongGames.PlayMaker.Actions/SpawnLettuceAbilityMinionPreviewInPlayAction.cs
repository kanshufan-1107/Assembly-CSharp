using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Create a fake minion in play that is purely local and does not exist on the server.")]
[ActionCategory("Pegasus")]
public class SpawnLettuceAbilityMinionPreviewInPlayAction : FsmStateAction
{
	public FsmGameObject m_FakeMinionGameObject;

	public FsmInt m_ZonePosition;

	public FsmInt m_FakeMinionDbId;

	public FsmInt m_FakeMinionAttack;

	public FsmInt m_FakeMinionHealth;

	public override void Reset()
	{
		m_FakeMinionGameObject = new FsmGameObject
		{
			UseVariable = true
		};
		m_FakeMinionDbId = new FsmInt
		{
			UseVariable = true
		};
		m_ZonePosition = new FsmInt
		{
			UseVariable = true
		};
		m_FakeMinionAttack = new FsmInt
		{
			UseVariable = true
		};
		m_FakeMinionHealth = new FsmInt
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_FakeMinionGameObject.Value != null && m_FakeMinionGameObject.Value.GetComponent<Actor>() != null)
		{
			Finish();
			return;
		}
		DefLoader.DisposableFullDef fakeMinionDef = DefLoader.Get().GetFullDef(GameUtils.TranslateDbIdToCardId(m_FakeMinionDbId.ToInt()));
		if (fakeMinionDef == null || fakeMinionDef.EntityDef == null)
		{
			Debug.LogErrorFormat("SpawnLettuceAbilityMinionPreviewInPlayAction - Unable to load fake actor with card id {0}", m_FakeMinionDbId);
			Finish();
		}
		else
		{
			GameObject minionActor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(fakeMinionDef.EntityDef, TAG_ZONE.PLAY), AssetLoadingOptions.IgnorePrefabPosition);
			OnFakeMinionActorLoaded(minionActor, fakeMinionDef);
			fakeMinionDef.Dispose();
		}
	}

	private void OnFakeMinionActorLoaded(GameObject go, DefLoader.DisposableFullDef fakeCardFullDef)
	{
		m_FakeMinionGameObject.Value = go;
		Actor fakeMinionActor = go.GetComponent<Actor>();
		if (fakeMinionActor == null)
		{
			Debug.LogError("SpawnLettuceAbilityMinionPreviewInPlayAction - No actor on loaded game object.");
			Finish();
			return;
		}
		fakeMinionActor.SetFullDef(fakeCardFullDef);
		EntityDef fakeEntityDef = fakeCardFullDef.EntityDef.Clone();
		fakeEntityDef.SetTag(GAME_TAG.ATK, m_FakeMinionAttack.ToInt());
		fakeEntityDef.SetTag(GAME_TAG.HEALTH, m_FakeMinionHealth.ToInt());
		fakeMinionActor.SetEntityDef(fakeEntityDef);
		fakeMinionActor.UpdateAllComponents();
		int playZoneIndex = m_ZonePosition.Value - 1;
		ZonePlay friendlyPlayZone = ZoneMgr.Get().FindZonesOfType<ZonePlay>(Player.Side.FRIENDLY).FirstOrDefault();
		friendlyPlayZone.SortWithSpotForLettuceAbilityCard(playZoneIndex);
		fakeMinionActor.transform.position = GetPositionOfFakeCard(friendlyPlayZone, playZoneIndex);
		fakeMinionActor.ActivateSpellBirthState(SpellType.ONE_SIDED_GHOSTLY);
		Finish();
	}

	private Vector3 GetPositionOfFakeCard(ZonePlay zone, int zoneIndex)
	{
		Vector3 fakeMinionPosition;
		if (zoneIndex == 0)
		{
			fakeMinionPosition = zone.GetCardPosition(zoneIndex + 1);
			return fakeMinionPosition - Vector3.right * zone.GetSlotWidth();
		}
		fakeMinionPosition = zone.GetCardPosition(zoneIndex - 1);
		return fakeMinionPosition + Vector3.right * zone.GetSlotWidth();
	}
}
