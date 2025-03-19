using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterDialogSequence : IEnumerable<CharacterDialog>, IEnumerable
{
	private List<CharacterDialog> m_dialogItems;

	public CharacterDialogDbfRecord m_characterDialogRecord;

	public int m_onCompleteBannerId;

	public bool m_ignorePopups = true;

	public bool m_deferOnComplete = true;

	public bool m_blockInput = true;

	public Action<CharacterDialogSequence> m_onPreShow;

	public int Count => m_dialogItems.Count;

	public CharacterDialogSequence(int dialogSequenceId, CharacterDialogEventType eventType = CharacterDialogEventType.UNSPECIFIED)
	{
		CharacterDialogDbfRecord characterDialogRecord = (m_characterDialogRecord = GameDbf.CharacterDialog.GetRecord(dialogSequenceId));
		m_onCompleteBannerId = characterDialogRecord.OnCompleteBannerId;
		m_ignorePopups = characterDialogRecord.IgnorePopups;
		m_deferOnComplete = characterDialogRecord.DeferOnComplete;
		m_blockInput = characterDialogRecord.BlockInput;
		m_dialogItems = new List<CharacterDialog>();
		List<CharacterDialogItemsDbfRecord> records = GameDbf.CharacterDialogItems.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			CharacterDialogItemsDbfRecord record = records[i];
			if (record.CharacterDialogId != dialogSequenceId)
			{
				continue;
			}
			if (eventType != 0)
			{
				CharacterDialogEventType objEventType = CharacterDialogEventType.UNSPECIFIED;
				if (record.AchieveEventType != null)
				{
					Enum.TryParse<CharacterDialogEventType>(record.AchieveEventType, ignoreCase: true, out objEventType);
				}
				if (objEventType != eventType)
				{
					continue;
				}
			}
			CharacterDialog dialog = new CharacterDialog
			{
				dbfRecordId = record.ID,
				playOrder = record.PlayOrder,
				useInnkeeperQuote = record.UseInnkeeperQuote,
				prefabName = record.PrefabName,
				bannerPrefabName = record.BannerPrefabName,
				audioName = record.AudioName,
				useAltSpeechBubble = record.AltBubblePosition,
				waitBefore = (float)record.WaitBefore,
				waitAfter = (float)record.WaitAfter,
				persistPrefab = record.PersistPrefab,
				useAltPosition = record.AltPosition,
				minimumDurationSeconds = (float)record.MinimumDurationSeconds,
				localeExtraSeconds = (float)record.LocaleExtraSeconds,
				bubbleText = record.BubbleText,
				useBannerStyle = record.UseBannerStyle,
				canvasAnchor = (CanvasAnchor)record.BannerAnchorPosition
			};
			m_dialogItems.Add(dialog);
		}
		m_dialogItems.Sort(delegate(CharacterDialog a, CharacterDialog b)
		{
			if (a.playOrder < b.playOrder)
			{
				return -1;
			}
			return (a.playOrder > b.playOrder) ? 1 : 0;
		});
	}

	public static List<string> GetAudioOfCharacterDialogSequence(int dialogSequenceId)
	{
		List<string> retval = new List<string>();
		CharacterDialogDbfRecord record = GameDbf.CharacterDialog.GetRecord(dialogSequenceId);
		foreach (CharacterDialogItemsDbfRecord onReceivedRecord in GameDbf.CharacterDialogItems.GetRecords().FindAll((CharacterDialogItemsDbfRecord obj) => obj.CharacterDialogId == record.ID))
		{
			retval.Add(onReceivedRecord.AudioName);
		}
		return retval;
	}

	public IEnumerator<CharacterDialog> GetEnumerator()
	{
		foreach (CharacterDialog dialogItem in m_dialogItems)
		{
			yield return dialogItem;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
