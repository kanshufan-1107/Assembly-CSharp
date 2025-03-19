using System;
using System.Collections.Generic;
using Blizzard.Commerce;
using Hearthstone.InGameMessage.UI;
using MiniJSON;

namespace Hearthstone.InGameMessage;

public static class BrazeInGameMessageExtensions
{
	private const string c_headerJsonKey = "header";

	private const string c_messageJsonKey = "message";

	private const string c_campaignIdJsonKey = "campaign_id";

	private const string c_imageUrlJsonKey = "image_url";

	private const string c_deeplinkUriJsonKey = "uri";

	private const string c_extrasJsonKey = "extras";

	public static JsonNode ConvertBrazeIgmToJsonNode(this BrazeInGameMessage brazeIgm)
	{
		if (brazeIgm == null)
		{
			return null;
		}
		JsonNode node = new JsonNode();
		if (!string.IsNullOrEmpty(brazeIgm.Header))
		{
			node["header"] = brazeIgm.Header;
		}
		if (!string.IsNullOrEmpty(brazeIgm.Message))
		{
			node["message"] = brazeIgm.Message;
		}
		if (!string.IsNullOrEmpty(brazeIgm.Campaign_Id))
		{
			node["campaign_id"] = brazeIgm.Campaign_Id;
		}
		if (!string.IsNullOrEmpty(brazeIgm.Image_Url))
		{
			node["image_url"] = brazeIgm.Image_Url;
		}
		if (!string.IsNullOrEmpty(brazeIgm.Uri))
		{
			node["uri"] = brazeIgm.Uri;
		}
		if (brazeIgm.Extras != null)
		{
			JsonNode dictToNode = new JsonNode();
			foreach (KeyValuePair<string, string> kvp in brazeIgm.Extras)
			{
				dictToNode[kvp.Key] = kvp.Value;
			}
			node["extras"] = dictToNode;
		}
		return node;
	}

	public static BrazeInGameMessage GetBrazeIgmFromJsonNode(JsonNode node)
	{
		if (node == null)
		{
			return null;
		}
		BrazeInGameMessage brazeIgm = new BrazeInGameMessage();
		if (node.TryGetValueAs<string>("header", out var header))
		{
			brazeIgm.Header = header;
		}
		if (node.TryGetValueAs<string>("message", out var message))
		{
			brazeIgm.Message = message;
		}
		if (node.TryGetValueAs<string>("campaign_id", out var campaignId))
		{
			brazeIgm.Campaign_Id = campaignId;
		}
		if (node.TryGetValueAs<string>("image_url", out var imgUrl))
		{
			brazeIgm.Image_Url = imgUrl;
		}
		if (node.TryGetValueAs<string>("uri", out var uri))
		{
			brazeIgm.Uri = uri;
		}
		if (node.TryGetValueAs<JsonNode>("extras", out var extras) && extras != null)
		{
			brazeIgm.Extras = new Dictionary<string, string>();
			foreach (KeyValuePair<string, object> kvp in extras)
			{
				brazeIgm.Extras[kvp.Key] = kvp.Value as string;
			}
		}
		return brazeIgm;
	}

	public static GameMessage ConvertBrazeIgmToGameMessage(this BrazeInGameMessage brazeIgm)
	{
		if (brazeIgm == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(brazeIgm.Header) || string.IsNullOrEmpty(brazeIgm.Message))
		{
			return null;
		}
		GameMessage gm = new GameMessage
		{
			UID = brazeIgm.Campaign_Id,
			LayoutType = "Simple Text",
			ContentType = "in_game_message_braze",
			Title = brazeIgm.Header,
			TextBody = brazeIgm.Message,
			TextureAssetUrl = brazeIgm.Image_Url,
			EventId = PopupEvent.OnHubScene.ToString(),
			DisplayImageType = "logo_generic",
			PriorityLevel = 1,
			MaxViewCount = 1
		};
		AppendBrazeMessageExtrasToGameMessage(gm, brazeIgm);
		if (!string.IsNullOrEmpty(brazeIgm.Uri) && !string.IsNullOrEmpty(gm.LayoutType))
		{
			if (gm.LayoutType.Equals("Shop", StringComparison.OrdinalIgnoreCase))
			{
				if (brazeIgm.Uri.Contains("store", StringComparison.OrdinalIgnoreCase))
				{
					gm.OpenFullShop = true;
				}
				else
				{
					Log.InGameMessage.PrintError("Ignoring Braze IGM deeplink as layout was shop but deeplink didn't target shop. Link: " + brazeIgm.Uri + " - Expected hearthstone://store/[pmt id]. Defaulting to ...://store");
					gm.OpenFullShop = true;
				}
			}
			else
			{
				gm.Link = brazeIgm.Uri;
			}
		}
		return gm;
	}

	private static void AppendBrazeMessageExtrasToGameMessage(GameMessage gmToUpdate, BrazeInGameMessage brazeIgm)
	{
		if (gmToUpdate == null || brazeIgm?.Extras == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> igmParameter in brazeIgm.Extras)
		{
			string key = igmParameter.Key;
			string data = igmParameter.Value;
			if (string.IsNullOrEmpty(key))
			{
				continue;
			}
			if (key.Contains("Layout", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				gmToUpdate.LayoutType = data;
			}
			else if (key.Contains("EventId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				gmToUpdate.EventId = data;
			}
			else if (key.Contains("MaxViewCount", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				if (int.TryParse(data, out var viewCount))
				{
					gmToUpdate.MaxViewCount = Math.Min(viewCount, 1);
				}
			}
			else if (key.Contains("ImageType", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				gmToUpdate.DisplayImageType = data;
			}
			else if (key.Contains("ProductId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				ProductId productId = ProductId.CreateFrom(data);
				if (productId.Value != -1)
				{
					gmToUpdate.ProductID = productId.Value;
				}
			}
			else if (key.Contains("PriorityLevel", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
			{
				if (int.TryParse(data, out var priorityLevel))
				{
					gmToUpdate.PriorityLevel = priorityLevel;
				}
			}
			else
			{
				Log.InGameMessage.PrintWarning("Unhandled Braze IGM parameter while converting to GameMessage: " + key + " (Data: " + data + ")");
			}
		}
	}
}
