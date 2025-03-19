using System;
using System.Collections.Generic;

public class DynamicTextTurnStartIndicator : TurnStartIndicator
{
	[Serializable]
	public class StringMapping
	{
		public UberText m_DynamicText;

		public int m_TagToPullStringIDFrom;
	}

	public List<StringMapping> m_stringMappings;

	public override void Show()
	{
		Player playerEntity = GameState.Get().GetFriendlySidePlayer();
		if (playerEntity == null)
		{
			Log.Gameplay.PrintError("DynamicTextTurnStartIndicator.Show(): playerEntity is somehow null, text will not be displayed! Contact a gameplay engineer.", this);
			return;
		}
		foreach (StringMapping mapping in m_stringMappings)
		{
			if (mapping.m_DynamicText == null)
			{
				Log.Gameplay.PrintError("DynamicTextTurnStartIndicator.Show(): m_DynamicText on {0} is null, please assign an UberText!", this);
				return;
			}
			if (mapping.m_TagToPullStringIDFrom == 0)
			{
				Log.Gameplay.PrintError("DynamicTextTurnStartIndicator.Show(): m_DynamicText on {0} is null, please assign an UberText!", this);
				return;
			}
			mapping.m_DynamicText.Text = GameDbf.GetIndex().GetClientString(playerEntity.GetTag(mapping.m_TagToPullStringIDFrom));
		}
		base.Show();
	}
}
