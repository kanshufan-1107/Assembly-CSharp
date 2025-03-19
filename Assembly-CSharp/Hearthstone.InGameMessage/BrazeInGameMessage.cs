using System;
using System.Collections.Generic;

namespace Hearthstone.InGameMessage;

[Serializable]
public class BrazeInGameMessage
{
	public string Header { get; set; }

	public string Message { get; set; }

	public string Campaign_Id { get; set; }

	public string Uri { get; set; }

	public string Image_Url { get; set; }

	public Dictionary<string, string> Extras { get; set; }
}
