using System;
using System.Collections.Generic;
using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public class MessageContentFeed
{
	public string ContentType { get; }

	public IDataTranslator DataTranslator { get; }

	public List<MessageUIData> Messages { get; set; }

	public Func<string> GetQuery { get; }

	public MessageSourceType SourceType { get; }

	public MessageContentFeed(string contentType, IDataTranslator dataTranslator, Func<string> getQuery, MessageSourceType sourceType)
	{
		ContentType = contentType;
		DataTranslator = dataTranslator;
		GetQuery = getQuery;
		SourceType = sourceType;
	}
}
