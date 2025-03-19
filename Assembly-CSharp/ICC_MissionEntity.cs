using System.Collections;
using UnityEngine;

public class ICC_MissionEntity : GenericDungeonMissionEntity
{
	public Vector3 ragLinePosition = new Vector3(95f, NotificationManager.DEPTH, 36.8f);

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ICCMulligan);
		}
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ICC);
	}

	protected Actor GetActorByCardId(string cardId)
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetControllerId() == player.GetPlayerId() && cardEntity.GetCardId() == cardId)
			{
				return cardEntity.GetCard().GetActor();
			}
		}
		return null;
	}

	protected Actor GetLichKingFriendlyMinion()
	{
		return GetActorByCardId("ICC_314");
	}

	protected IEnumerator IfPlayerPlaysDKHeroVO(Entity entity, Actor actor, string voString)
	{
		if (entity.GetCardType() == TAG_CARDTYPE.HERO && entity.GetCardSet() == TAG_CARD_SET.ICECROWN)
		{
			yield return new WaitForSeconds(0.3f);
			yield return PlayEasterEggLine(actor, voString);
		}
	}
}
