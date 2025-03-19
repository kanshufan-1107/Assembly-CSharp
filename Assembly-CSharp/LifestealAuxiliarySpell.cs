using PegasusGame;

public class LifestealAuxiliarySpell : Spell
{
	public override bool AttachPowerTaskList(PowerTaskList taskList)
	{
		if (!base.AttachPowerTaskList(taskList))
		{
			return false;
		}
		if (taskList == null)
		{
			Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): Tasklist is NULL. Can't check for healing and damage metadata.", this);
			return false;
		}
		Card sourceCard = GetSourceCard();
		if (sourceCard == null)
		{
			Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): No source card found.", this);
			return false;
		}
		Entity sourceEntity = sourceCard.GetEntity();
		if (sourceEntity == null)
		{
			Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): Current tasklist has no source entity.", this);
			return false;
		}
		Player sourceController = sourceEntity.GetController();
		if (sourceController == null)
		{
			Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): Source entity has no controller.", this);
			return false;
		}
		Entity targetHero = null;
		if (sourceController.HasTag(GAME_TAG.LIFESTEAL_DAMAGES_OPPOSING_HERO))
		{
			Player opposingController = GameState.Get().GetFirstOpponentPlayer(sourceController);
			if (opposingController != null)
			{
				targetHero = opposingController.GetHero();
			}
			if (targetHero == null)
			{
				Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): Opposing entity's controller has no hero.", this);
				return false;
			}
		}
		else
		{
			targetHero = sourceController.GetHero();
			if (targetHero == null)
			{
				Log.Gameplay.PrintError("{0}.AttachPowerTaskList(): Source entity's controller has no hero.", this);
				return false;
			}
		}
		foreach (PowerTask task in taskList.GetTaskList())
		{
			if (task.GetPower() is Network.HistMetaData metadata && (metadata.MetaType == HistoryMeta.Type.HEALING || metadata.MetaType == HistoryMeta.Type.DAMAGE))
			{
				Entity targetEntity = GameState.Get().GetEntity(metadata.Info[0]);
				if (targetEntity != null && targetEntity == targetHero && !(targetEntity.GetCard() == null))
				{
					SetSource(targetEntity.GetCard().gameObject);
					return true;
				}
			}
		}
		return false;
	}
}
