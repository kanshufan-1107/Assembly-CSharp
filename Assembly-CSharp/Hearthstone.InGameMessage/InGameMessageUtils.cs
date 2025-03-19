using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using MiniJSON;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public static class InGameMessageUtils
{
	public readonly struct InGameMessageDef
	{
		public readonly string m_key;

		public readonly object m_defValue;

		public readonly Func<object, object> m_converter;

		public InGameMessageDef(string key, object defValue, Func<object, object> converter = null)
		{
			m_key = key;
			m_defValue = defValue;
			m_converter = converter;
		}
	}

	private static IReadOnlyDictionary<string, BnetRegion> s_regionMap = new Dictionary<string, BnetRegion>
	{
		{
			"US",
			BnetRegion.REGION_US
		},
		{
			"EU",
			BnetRegion.REGION_EU
		},
		{
			"KR",
			BnetRegion.REGION_KR
		},
		{
			"CN",
			BnetRegion.REGION_CN
		},
		{
			"TW",
			BnetRegion.REGION_TW
		},
		{
			"All",
			BnetRegion.REGION_UNKNOWN
		}
	};

	public static IReadOnlyDictionary<InGameMessageAttributes, InGameMessageDef> Attributes { get; private set; } = new Dictionary<InGameMessageAttributes, InGameMessageDef>
	{
		{
			InGameMessageAttributes.UID,
			new InGameMessageDef("uid", string.Empty)
		},
		{
			InGameMessageAttributes.TAGS,
			new InGameMessageDef("tags", Array.Empty<string>(), TagsNodeConverter)
		},
		{
			InGameMessageAttributes.EVENT_ID,
			new InGameMessageDef("eventid", string.Empty)
		},
		{
			InGameMessageAttributes.VIEWCOUNT_ID,
			new InGameMessageDef("viewcountid", string.Empty)
		},
		{
			InGameMessageAttributes.TEXT,
			new InGameMessageDef("text", string.Empty)
		},
		{
			InGameMessageAttributes.TEXT_OFFSET_X,
			new InGameMessageDef("textoffsetx", string.Empty)
		},
		{
			InGameMessageAttributes.TEXT_OFFSET_Y,
			new InGameMessageDef("textoffsety", string.Empty)
		},
		{
			InGameMessageAttributes.TEXT_WIDTH,
			new InGameMessageDef("textwidth", string.Empty)
		},
		{
			InGameMessageAttributes.TEXT_HEIGHT,
			new InGameMessageDef("textheight", string.Empty)
		},
		{
			InGameMessageAttributes.ENTRYTITLE,
			new InGameMessageDef("title", string.Empty)
		},
		{
			InGameMessageAttributes.TITLE,
			new InGameMessageDef("displaytitle", string.Empty)
		},
		{
			InGameMessageAttributes.TEXTURE_ASSET_URL,
			new InGameMessageDef("textureasset.url", string.Empty)
		},
		{
			InGameMessageAttributes.TEXTURE_OFFSET_X,
			new InGameMessageDef("texture_offset_x", 0f)
		},
		{
			InGameMessageAttributes.TEXTURE_OFFSET_Y,
			new InGameMessageDef("texture_offset_y", 0f)
		},
		{
			InGameMessageAttributes.LINK,
			new InGameMessageDef("link", string.Empty)
		},
		{
			InGameMessageAttributes.EFFECT,
			new InGameMessageDef("effect", string.Empty)
		},
		{
			InGameMessageAttributes.MAX_VIEW_COUNT,
			new InGameMessageDef("maxviewcount", 0)
		},
		{
			InGameMessageAttributes.PUBLISH_DATE,
			new InGameMessageDef("publish_details.time", default(DateTime))
		},
		{
			InGameMessageAttributes.BEGINNING_DATE,
			new InGameMessageDef("beginningdate", default(DateTime))
		},
		{
			InGameMessageAttributes.EXPIRY_DATE,
			new InGameMessageDef("expirydate", default(DateTime))
		},
		{
			InGameMessageAttributes.PRIORITY_LEVEL,
			new InGameMessageDef("prioritylevel", 0)
		},
		{
			InGameMessageAttributes.GAME_VERSION,
			new InGameMessageDef("gameversion", 0)
		},
		{
			InGameMessageAttributes.MIN_GAME_VERSION,
			new InGameMessageDef("mingameversion", 0)
		},
		{
			InGameMessageAttributes.MAX_GAME_VERSION,
			new InGameMessageDef("maxgameversion", 0)
		},
		{
			InGameMessageAttributes.PLATFORM,
			new InGameMessageDef("platform", null)
		},
		{
			InGameMessageAttributes.ANDROID_STORE,
			new InGameMessageDef("androidstore", null)
		},
		{
			InGameMessageAttributes.GAME_ATTRS,
			new InGameMessageDef("gameattrs", string.Empty)
		},
		{
			InGameMessageAttributes.DISMISS_COND,
			new InGameMessageDef("dismiss", string.Empty)
		},
		{
			InGameMessageAttributes.LAYOUT_TYPE,
			new InGameMessageDef("layouttype", string.Empty)
		},
		{
			InGameMessageAttributes.DISPLAY_IMAGE_TYPE,
			new InGameMessageDef("displayimage", string.Empty)
		},
		{
			InGameMessageAttributes.PRODUCT_ID,
			new InGameMessageDef("productid", 0L)
		},
		{
			InGameMessageAttributes.TEXT_BODY,
			new InGameMessageDef("textbody", string.Empty)
		},
		{
			InGameMessageAttributes.OPEN_FULL_SHOP,
			new InGameMessageDef("openfullshop", false)
		},
		{
			InGameMessageAttributes.LAUNCH_EFFECT_ID,
			new InGameMessageDef("effectid", string.Empty)
		},
		{
			InGameMessageAttributes.LAUNCH_EFFECT_COLOR,
			new InGameMessageDef("effectcolor", string.Empty)
		},
		{
			InGameMessageAttributes.LAUNCH_EFFECT_SOUND_ID,
			new InGameMessageDef("effectsoundid", string.Empty)
		},
		{
			InGameMessageAttributes.CHANGE_ITEM_TYPE,
			new InGameMessageDef("itemtype", string.Empty)
		},
		{
			InGameMessageAttributes.CHANGE_ITEM_ID,
			new InGameMessageDef("itemid", string.Empty)
		},
		{
			InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE,
			new InGameMessageDef("itempremiumtype", string.Empty)
		},
		{
			InGameMessageAttributes.MERC_ITEM_ART_VARIATION_ID,
			new InGameMessageDef("mercartvariationid", 0)
		},
		{
			InGameMessageAttributes.IMAGE_MATERIAL,
			new InGameMessageDef("display_material", string.Empty)
		},
		{
			InGameMessageAttributes.TEXTURE_REFERENCE,
			new InGameMessageDef("texture_reference", string.Empty)
		},
		{
			InGameMessageAttributes.LAUNCH_SUB_LAYOUT,
			new InGameMessageDef("launch_sub_layout", "mode")
		},
		{
			InGameMessageAttributes.SHOP_DEEP_LINK,
			new InGameMessageDef("shopdeeplink", string.Empty)
		},
		{
			InGameMessageAttributes.SLOT_COMPATIBILITY,
			new InGameMessageDef("slot_compatibility", string.Empty)
		},
		{
			InGameMessageAttributes.AUTO_FRAME,
			new InGameMessageDef("auto_frame", false)
		},
		{
			InGameMessageAttributes.REGION,
			new InGameMessageDef("region", string.Empty)
		}
	};

	public static int ClientVersion { get; private set; } = Convert.ToInt32($"{32}{0}{0:D2}");

	public static GameMessage ReadInGameMessage(JsonNode mainNode)
	{
		if (mainNode == null)
		{
			return null;
		}
		GameMessage inGameMessage = new GameMessage
		{
			UID = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.UID]),
			EventId = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.EVENT_ID]),
			ViewCountId = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.VIEWCOUNT_ID]),
			EntryTitle = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.ENTRYTITLE]),
			Title = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.TITLE]),
			TextureAssetUrl = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.TEXTURE_ASSET_URL]),
			ImageMaterial = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.IMAGE_MATERIAL]),
			TextureReference = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.TEXTURE_REFERENCE]),
			LaunchSubLayoutType = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.LAUNCH_SUB_LAYOUT]),
			Link = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.LINK]),
			Effect = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.EFFECT]),
			MaxViewCount = GetAttribute<int>(mainNode, Attributes[InGameMessageAttributes.MAX_VIEW_COUNT]),
			PublishDate = GetAttribute<DateTime>(mainNode, Attributes[InGameMessageAttributes.PUBLISH_DATE]),
			BeginningDate = GetAttribute<DateTime>(mainNode, Attributes[InGameMessageAttributes.BEGINNING_DATE]),
			ExpiryDate = GetAttribute<DateTime>(mainNode, Attributes[InGameMessageAttributes.EXPIRY_DATE]),
			PriorityLevel = GetAttribute<int>(mainNode, Attributes[InGameMessageAttributes.PRIORITY_LEVEL]),
			GameVersion = GetAttribute<int>(mainNode, Attributes[InGameMessageAttributes.GAME_VERSION]),
			MinGameVersion = GetAttribute<int>(mainNode, Attributes[InGameMessageAttributes.MIN_GAME_VERSION]),
			MaxGameVersion = GetAttribute<int>(mainNode, Attributes[InGameMessageAttributes.MAX_GAME_VERSION]),
			Platform = GetAttributeList<string>(mainNode, Attributes[InGameMessageAttributes.PLATFORM]),
			AndroidStore = GetAttributeList<string>(mainNode, Attributes[InGameMessageAttributes.ANDROID_STORE]),
			GameAttrs = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.GAME_ATTRS]),
			DismissCond = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.DISMISS_COND]),
			LayoutType = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.LAYOUT_TYPE]),
			DisplayImageType = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.DISPLAY_IMAGE_TYPE]),
			ProductID = GetAttribute<long>(mainNode, Attributes[InGameMessageAttributes.PRODUCT_ID]),
			TextBody = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.TEXT_BODY]),
			OpenFullShop = GetAttribute<bool>(mainNode, Attributes[InGameMessageAttributes.OPEN_FULL_SHOP]),
			ShopDeepLink = GetAttribute<string>(mainNode, Attributes[InGameMessageAttributes.SHOP_DEEP_LINK]),
			Region = GetAttributeList<string>(mainNode, Attributes[InGameMessageAttributes.REGION])
		};
		if (mainNode.ContainsKey("texts"))
		{
			foreach (object item in mainNode["texts"] as JsonList)
			{
				JsonNode textNode = item as JsonNode;
				inGameMessage.m_textGroups.Add(new TextGroup
				{
					Text = GetAttribute<string>(textNode, Attributes[InGameMessageAttributes.TEXT]),
					TextOffsetX = GetAttribute<string>(textNode, Attributes[InGameMessageAttributes.TEXT_OFFSET_X]),
					TextOffsetY = GetAttribute<string>(textNode, Attributes[InGameMessageAttributes.TEXT_OFFSET_Y]),
					TextWidth = GetAttribute<string>(textNode, Attributes[InGameMessageAttributes.TEXT_WIDTH]),
					TextHeight = GetAttribute<string>(textNode, Attributes[InGameMessageAttributes.TEXT_HEIGHT])
				});
			}
		}
		if (mainNode.ContainsKey("launcheffect"))
		{
			JsonNode launchEffectNode = mainNode["launcheffect"] as JsonNode;
			inGameMessage.LaunchEffect = new LaunchMessageEffectContent();
			inGameMessage.LaunchEffect.EffectId = GetAttribute<string>(launchEffectNode, Attributes[InGameMessageAttributes.LAUNCH_EFFECT_ID]);
			inGameMessage.LaunchEffect.EffectColor = GetAttribute<string>(launchEffectNode, Attributes[InGameMessageAttributes.LAUNCH_EFFECT_COLOR]);
			inGameMessage.LaunchEffect.EffectSoundId = GetAttribute<string>(launchEffectNode, Attributes[InGameMessageAttributes.LAUNCH_EFFECT_SOUND_ID]);
		}
		if (mainNode.ContainsKey("itemdisplay"))
		{
			JsonList itemList = mainNode["itemdisplay"] as JsonList;
			inGameMessage.ItemDisplay = new List<InGameMessageItemDisplayContent>(itemList.Count);
			foreach (JsonNode curItemNode in itemList)
			{
				InGameMessageItemDisplayContent itemDisplay = new InGameMessageItemDisplayContent
				{
					itemChangeId = GetAttribute<string>(curItemNode, Attributes[InGameMessageAttributes.CHANGE_ITEM_ID])
				};
				string changeType = GetAttribute<string>(curItemNode, Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE]);
				switch (changeType.ToLower())
				{
				case "card":
					itemDisplay.ChangeType = InGameMessageItemDisplayContent.ItemType.Card;
					break;
				case "hero":
					itemDisplay.ChangeType = InGameMessageItemDisplayContent.ItemType.Hero;
					break;
				case "merc":
					itemDisplay.ChangeType = InGameMessageItemDisplayContent.ItemType.Merc;
					break;
				case "bounty":
					itemDisplay.ChangeType = InGameMessageItemDisplayContent.ItemType.Bounty;
					break;
				case "battlegrounds card":
					itemDisplay.ChangeType = InGameMessageItemDisplayContent.ItemType.BattlegroundsCard;
					break;
				default:
					Log.InGameMessage.PrintError("Received an unknown item display type with the in game message response! (Type: {0})", changeType);
					break;
				}
				switch (GetAttribute<string>(curItemNode, Attributes[InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE]).ToLower())
				{
				default:
					itemDisplay.itemPremiumType = TAG_PREMIUM.NORMAL;
					break;
				case "golden":
					itemDisplay.itemPremiumType = TAG_PREMIUM.GOLDEN;
					break;
				case "signature":
					itemDisplay.itemPremiumType = TAG_PREMIUM.SIGNATURE;
					break;
				case "diamond":
					itemDisplay.itemPremiumType = TAG_PREMIUM.DIAMOND;
					break;
				}
				itemDisplay.mercArtVariationId = GetAttribute<int>(curItemNode, Attributes[InGameMessageAttributes.MERC_ITEM_ART_VARIATION_ID]);
				inGameMessage.ItemDisplay.Add(itemDisplay);
			}
		}
		return inGameMessage;
	}

	public static JsonNode ConvertGameMessageToJsonNode(this GameMessage message)
	{
		JsonNode node = new JsonNode();
		node.Add(Attributes[InGameMessageAttributes.UID].m_key, message.UID);
		node.Add(Attributes[InGameMessageAttributes.EVENT_ID].m_key, message.EventId);
		node.Add(Attributes[InGameMessageAttributes.ENTRYTITLE].m_key, message.EntryTitle);
		node.Add(Attributes[InGameMessageAttributes.TITLE].m_key, message.Title);
		node.Add(Attributes[InGameMessageAttributes.TEXTURE_ASSET_URL].m_key, message.TextureAssetUrl);
		node.Add(Attributes[InGameMessageAttributes.IMAGE_MATERIAL].m_key, message.ImageMaterial);
		node.Add(Attributes[InGameMessageAttributes.TEXTURE_REFERENCE].m_key, message.TextureReference);
		node.Add(Attributes[InGameMessageAttributes.LAUNCH_SUB_LAYOUT].m_key, message.LaunchSubLayoutType);
		node.Add(Attributes[InGameMessageAttributes.LINK].m_key, message.Link);
		node.Add(Attributes[InGameMessageAttributes.EFFECT].m_key, message.Effect);
		node.Add(Attributes[InGameMessageAttributes.MAX_VIEW_COUNT].m_key, message.MaxViewCount);
		node.Add(Attributes[InGameMessageAttributes.PUBLISH_DATE].m_key, message.PublishDate);
		node.Add(Attributes[InGameMessageAttributes.BEGINNING_DATE].m_key, message.BeginningDate);
		node.Add(Attributes[InGameMessageAttributes.EXPIRY_DATE].m_key, message.ExpiryDate);
		node.Add(Attributes[InGameMessageAttributes.PRIORITY_LEVEL].m_key, message.PriorityLevel);
		node.Add(Attributes[InGameMessageAttributes.GAME_VERSION].m_key, message.GameVersion);
		node.Add(Attributes[InGameMessageAttributes.MIN_GAME_VERSION].m_key, message.MinGameVersion);
		node.Add(Attributes[InGameMessageAttributes.MAX_GAME_VERSION].m_key, message.MaxGameVersion);
		node.Add(Attributes[InGameMessageAttributes.PLATFORM].m_key, message.Platform);
		node.Add(Attributes[InGameMessageAttributes.ANDROID_STORE].m_key, message.AndroidStore);
		node.Add(Attributes[InGameMessageAttributes.GAME_ATTRS].m_key, message.GameAttrs);
		node.Add(Attributes[InGameMessageAttributes.DISMISS_COND].m_key, message.DismissCond);
		node.Add(Attributes[InGameMessageAttributes.LAYOUT_TYPE].m_key, message.LayoutType);
		node.Add(Attributes[InGameMessageAttributes.DISPLAY_IMAGE_TYPE].m_key, message.DisplayImageType);
		node.Add(Attributes[InGameMessageAttributes.PRODUCT_ID].m_key, message.ProductID);
		node.Add(Attributes[InGameMessageAttributes.TEXT_BODY].m_key, message.TextBody);
		node.Add(Attributes[InGameMessageAttributes.OPEN_FULL_SHOP].m_key, message.OpenFullShop);
		node.Add(Attributes[InGameMessageAttributes.SHOP_DEEP_LINK].m_key, message.ShopDeepLink);
		node.Add(Attributes[InGameMessageAttributes.REGION].m_key, message.Region);
		if (message.m_textGroups != null)
		{
			JsonList textJsonList = new JsonList();
			foreach (TextGroup group in message.m_textGroups)
			{
				JsonNode textNode = new JsonNode();
				textNode.Add(Attributes[InGameMessageAttributes.TEXT].m_key, group.Text);
				textNode.Add(Attributes[InGameMessageAttributes.TEXT_OFFSET_X].m_key, group.TextOffsetX);
				textNode.Add(Attributes[InGameMessageAttributes.TEXT_OFFSET_Y].m_key, group.TextOffsetY);
				textNode.Add(Attributes[InGameMessageAttributes.TEXT_WIDTH].m_key, group.TextWidth);
				textNode.Add(Attributes[InGameMessageAttributes.TEXT_HEIGHT].m_key, group.TextHeight);
				textJsonList.Add(textNode);
			}
			node.Add("texts", textJsonList);
		}
		if (message.LaunchEffect != null)
		{
			JsonNode launchEffectNode = new JsonNode();
			launchEffectNode.Add(Attributes[InGameMessageAttributes.LAUNCH_EFFECT_ID].m_key, message.LaunchEffect.EffectId);
			launchEffectNode.Add(Attributes[InGameMessageAttributes.LAUNCH_EFFECT_COLOR].m_key, message.LaunchEffect.EffectColor);
			launchEffectNode.Add(Attributes[InGameMessageAttributes.LAUNCH_EFFECT_SOUND_ID].m_key, message.LaunchEffect.EffectSoundId);
			node.Add("launcheffect", launchEffectNode);
		}
		if (message.ItemDisplay != null)
		{
			JsonList itemList = new JsonList();
			foreach (InGameMessageItemDisplayContent item in message.ItemDisplay)
			{
				JsonNode itemNode = new JsonNode();
				itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_ID].m_key, item.itemChangeId);
				switch (item.ChangeType)
				{
				case InGameMessageItemDisplayContent.ItemType.Card:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE].m_key, "card");
					break;
				case InGameMessageItemDisplayContent.ItemType.Hero:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE].m_key, "hero");
					break;
				case InGameMessageItemDisplayContent.ItemType.Merc:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE].m_key, "merc");
					break;
				case InGameMessageItemDisplayContent.ItemType.Bounty:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE].m_key, "bounty");
					break;
				case InGameMessageItemDisplayContent.ItemType.BattlegroundsCard:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_TYPE].m_key, "battlegrounds card");
					break;
				default:
					Log.InGameMessage.PrintError("Tried to serialize an item with an unknown item type! (Type: {0})", item.ChangeType);
					break;
				}
				switch (item.itemPremiumType)
				{
				case TAG_PREMIUM.NORMAL:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE].m_key, "normal");
					break;
				case TAG_PREMIUM.GOLDEN:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE].m_key, "golden");
					break;
				case TAG_PREMIUM.SIGNATURE:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE].m_key, "signature");
					break;
				case TAG_PREMIUM.DIAMOND:
					itemNode.Add(Attributes[InGameMessageAttributes.CHANGE_ITEM_PREMIUM_TYPE].m_key, "diamond");
					break;
				default:
					Log.InGameMessage.PrintError("Tried to serialize an item with an unknown premium type! (Type: {0})", item.ChangeType);
					break;
				}
				itemNode.Add(Attributes[InGameMessageAttributes.MERC_ITEM_ART_VARIATION_ID].m_key, item.mercArtVariationId);
				itemList.Add(itemNode);
			}
		}
		return node;
	}

	public static List<GameMessage> GetAllMessagesFromJsonResponse(JsonNode response, ViewCountController viewCountController)
	{
		if (response == null)
		{
			return new List<GameMessage>();
		}
		List<GameMessage> inGameMessages = new List<GameMessage>();
		try
		{
			JsonList responseMessages = GetRootListNode(response);
			if (responseMessages == null)
			{
				return new List<GameMessage>();
			}
			foreach (object item in responseMessages)
			{
				GameMessage message = ReadInGameMessage(item as JsonNode);
				if (message.MaxViewCount <= 0 || viewCountController == null || viewCountController.GetViewCount(message) < message.MaxViewCount)
				{
					DateTime curTime = DateTime.Now;
					if ((!(message.BeginningDate != default(DateTime)) || !(curTime < message.BeginningDate)) && (!(message.ExpiryDate != default(DateTime)) || !(curTime > message.ExpiryDate)))
					{
						inGameMessages.Add(message);
					}
				}
			}
			inGameMessages.Sort(GameMessage.CompareByOrder);
			return inGameMessages;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to get correct message: " + ex);
			return new List<GameMessage>();
		}
	}

	public static string GetUIDOfAttr(InGameMessageAttributes attr)
	{
		return Attributes[attr].m_key;
	}

	public static void SaveGameMessagesToDisk(List<GameMessage> messages, string path)
	{
		if (messages == null)
		{
			Log.InGameMessage.PrintWarning("Tried to serialize a null value. Aborting save.");
			return;
		}
		if (string.IsNullOrEmpty(path))
		{
			Log.InGameMessage.PrintWarning("Tried to serialize game messages to an invalid path: " + path + ". Aborting save.");
			return;
		}
		try
		{
			JsonNode rootNode = new JsonNode();
			JsonList entriesList = new JsonList();
			foreach (GameMessage message in messages)
			{
				JsonNode messageNode = message.ConvertGameMessageToJsonNode();
				if (messageNode != null)
				{
					entriesList.Add(messageNode);
				}
			}
			rootNode.Add("entries", entriesList);
			string json = Json.Serialize(rootNode);
			if (!string.IsNullOrEmpty(json))
			{
				File.WriteAllText(path, json);
			}
		}
		catch (Exception ex)
		{
			Log.InGameMessage.PrintException(ex, "Failed to write json message data to '" + path + "': " + ex.Message);
		}
	}

	public static string QueryAnd(string query1, string query2)
	{
		return $"{{\"$and\":[{query1},{query2}]}}";
	}

	public static string QueryOr(string query1, string query2)
	{
		return $"{{\"$or\":[{query1},{query2}]}}";
	}

	public static string QueryConvertVal(object val)
	{
		if (val == null)
		{
			return "null";
		}
		if (val is string)
		{
			return "\"" + val?.ToString() + "\"";
		}
		if (val is bool)
		{
			return val.ToString().ToLower();
		}
		if (val is IList<string> list)
		{
			if (list.Count == 0)
			{
				return "null";
			}
			if (list.Count == 1)
			{
				return QueryConvertVal(list[0]);
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(QueryConvertVal(list[0]));
			for (int i = 1; i < list.Count; i++)
			{
				sb.Append(",");
				sb.Append(QueryConvertVal(list[i]));
			}
			return sb.ToString();
		}
		return val.ToString();
	}

	public static string QueryEqual(InGameMessageAttributes attr, object val)
	{
		return $"{{\"{GetUIDOfAttr(attr)}\":{QueryConvertVal(val)}}}";
	}

	public static string QueryOp(InGameMessageAttributes attr, object val, string op)
	{
		return $"{{\"{GetUIDOfAttr(attr)}\":{{\"{op}\":{QueryConvertVal(val)}}}}}";
	}

	public static string QueryOpIn(InGameMessageAttributes attr, object val, string op)
	{
		return $"{{\"{GetUIDOfAttr(attr)}\":{{\"{op}\":[{QueryConvertVal(val)}]}}}}";
	}

	public static string QueryOpInArray(InGameMessageAttributes attr, List<string> val)
	{
		return string.Format("{{\"{0}\":{{\"{1}\":[{2}]}}}}", GetUIDOfAttr(attr), "$in", QueryConvertVal(val));
	}

	public static string QueryLess(InGameMessageAttributes attr, object val)
	{
		return QueryOp(attr, val, "$lt");
	}

	public static string QueryGreater(InGameMessageAttributes attr, object val)
	{
		return QueryOp(attr, val, "$gt");
	}

	public static string QueryNullOr(InGameMessageAttributes attr, object val, string op)
	{
		return QueryOr(QueryEqual(attr, null), QueryOp(attr, val, op));
	}

	public static string QueryNullOrEqual(InGameMessageAttributes attr, object val)
	{
		return QueryOr(QueryEqual(attr, null), QueryEqual(attr, val));
	}

	public static string QueryNullOrLess(InGameMessageAttributes attr, object val)
	{
		return QueryNullOr(attr, val, "$lt");
	}

	public static string QueryNullOrLessEqual(InGameMessageAttributes attr, object val)
	{
		return QueryNullOr(attr, val, "$lte");
	}

	public static string QueryNullOrGreater(InGameMessageAttributes attr, object val)
	{
		return QueryNullOr(attr, val, "$gt");
	}

	public static string QueryNullOrGreaterEqual(InGameMessageAttributes attr, object val)
	{
		return QueryNullOr(attr, val, "$gte");
	}

	public static string QueryEmptyOrIncludedIn(InGameMessageAttributes attr, object val)
	{
		return QueryOr(QueryOpIn(attr, string.Empty, "$nin"), QueryOpIn(attr, val, "$in"));
	}

	public static string QueryEmptyOrRegEx(InGameMessageAttributes attr, object val)
	{
		return QueryOr(QueryEqual(attr, string.Empty), QueryOp(attr, val, "$regex"));
	}

	public static string DefaultQuery()
	{
		string query = QueryNullOrEqual(InGameMessageAttributes.GAME_VERSION, ClientVersion);
		string cMaxQuery = QueryNullOrGreaterEqual(InGameMessageAttributes.MAX_GAME_VERSION, ClientVersion);
		query = QueryAnd(query, cMaxQuery);
		string cMinQuery = QueryNullOrLessEqual(InGameMessageAttributes.MIN_GAME_VERSION, ClientVersion);
		query = QueryAnd(query, cMinQuery);
		string pQuery = QueryEmptyOrIncludedIn(InGameMessageAttributes.PLATFORM, PlatformSettings.OS.ToString());
		query = QueryAnd(query, pQuery);
		if (PlatformSettings.OS == OSCategory.Android)
		{
			string squery = QueryEmptyOrIncludedIn(InGameMessageAttributes.ANDROID_STORE, AndroidDeviceSettings.Get().GetAndroidStore().ToString());
			query = QueryAnd(query, squery);
		}
		string reg = "";
		foreach (string attr in GameAttributes.Get().GetAttributions(activeOnly: true))
		{
			if (!string.IsNullOrEmpty(reg))
			{
				reg += "|";
			}
			reg = reg + "\\\\b" + attr + "\\\\b";
		}
		if (!string.IsNullOrEmpty(reg))
		{
			string rQuery = QueryEmptyOrRegEx(InGameMessageAttributes.GAME_ATTRS, "[" + reg + "]");
			query = QueryAnd(query, rQuery);
		}
		return query;
	}

	public static string MakeStandardQueryString()
	{
		return DefaultQuery();
	}

	public static string MakePersonalizedQueryString(List<string> personalizedIDs)
	{
		return QueryAnd(DefaultQuery(), QueryOpInArray(InGameMessageAttributes.UID, personalizedIDs));
	}

	private static object TagsNodeConverter(object jsonTags)
	{
		string[] tags = Array.Empty<string>();
		if (jsonTags is JsonList tagsList)
		{
			int numTags = 0;
			tags = new string[tagsList.Count];
			foreach (object item in tagsList)
			{
				if (item is string tag && !string.IsNullOrEmpty(tag))
				{
					tags[numTags++] = tag;
				}
			}
			if (numTags != tagsList.Count)
			{
				Array.Resize(ref tags, numTags);
			}
		}
		return tags;
	}

	public static JsonList GetRootListNode(JsonNode response)
	{
		if (response.ContainsKey("entries"))
		{
			return response["entries"] as JsonList;
		}
		if (response.ContainsKey("entry"))
		{
			return new JsonList { response["entry"] };
		}
		return null;
	}

	public static T GetAttribute<T>(JsonNode aNode, InGameMessageDef attrDef)
	{
		T returnVal = default(T);
		returnVal = GetValueFromNode<T>(attrDef.m_key, attrDef.m_converter, aNode);
		if (EqualityComparer<T>.Default.Equals(returnVal, default(T)))
		{
			return (T)attrDef.m_defValue;
		}
		return returnVal;
	}

	public static bool IncludesRegion(List<string> regionIds, Region region)
	{
		if (regionIds == null || regionIds.Count == 0)
		{
			return true;
		}
		foreach (string regionId in regionIds)
		{
			if (regionId.Equals(region.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	private static T GetValueFromNode<T>(string keyName, Func<object, object> converter, JsonNode node)
	{
		node = GetNodeAt(ref keyName, node);
		if (node == null)
		{
			return default(T);
		}
		if (node.ContainsKey(keyName))
		{
			try
			{
				if (converter != null)
				{
					return (T)converter(node[keyName]);
				}
				return (T)Convert.ChangeType(node[keyName], typeof(T));
			}
			catch
			{
			}
		}
		return default(T);
	}

	private static List<T> GetAttributeList<T>(JsonNode aNode, InGameMessageDef attrDef)
	{
		List<T> returnVal = GetValueFromNodeList<T>(attrDef.m_key, aNode);
		if (EqualityComparer<List<T>>.Default.Equals(returnVal, null))
		{
			return null;
		}
		return returnVal;
	}

	private static List<T> GetValueFromNodeList<T>(string keyName, JsonNode node)
	{
		node = GetNodeAt(ref keyName, node);
		if (node == null)
		{
			return null;
		}
		List<T> returnList = null;
		if (node.ContainsKey(keyName) && node[keyName] is JsonList)
		{
			returnList = new List<T>();
			foreach (object obj in node[keyName] as JsonList)
			{
				try
				{
					returnList.Add((T)Convert.ChangeType(obj, typeof(T)));
				}
				catch
				{
				}
			}
			if (returnList.Count == 0)
			{
				returnList = null;
			}
		}
		return returnList;
	}

	private static JsonNode GetNodeAt(ref string keyName, JsonNode node)
	{
		if (keyName.Contains('.'))
		{
			string[] values = keyName.Split('.');
			for (int i = 0; i < values.Length; i++)
			{
				string value = values[i];
				if (i == values.Length - 1)
				{
					keyName = value;
					break;
				}
				if (node.ContainsKey(value) && node[value] is JsonNode)
				{
					node = node[value] as JsonNode;
					continue;
				}
				return null;
			}
		}
		return node;
	}
}
