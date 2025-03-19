using System.Collections.Generic;
using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class BattlegroundsCollectionResult : IProtoBuf
{
	public enum TriggerEvent
	{
		Login,
		ExitBattlegroundsCollection
	}

	public bool HasTriggerEvent_;

	private TriggerEvent _TriggerEvent_;

	public bool HasNumberOfOwnedHeroSkins;

	private int _NumberOfOwnedHeroSkins;

	private List<int> _FavoriteBaseHeroCardIds = new List<int>();

	private List<int> _FavoriteHeroSkinIds = new List<int>();

	public bool HasNumberOfOwnedStrikes;

	private int _NumberOfOwnedStrikes;

	private List<int> _FavoriteStrikeIds = new List<int>();

	public bool HasNumberOfOwnedBartenders;

	private int _NumberOfOwnedBartenders;

	private List<int> _FavoriteBartenderSkinIds = new List<int>();

	public bool HasNumberOfOwnedBoardSkins;

	private int _NumberOfOwnedBoardSkins;

	private List<int> _FavoriteBoardSkinIds = new List<int>();

	public bool HasNumberOfOwnedEmotes;

	private int _NumberOfOwnedEmotes;

	private List<int> _EquippedEmoteIds = new List<int>();

	public TriggerEvent TriggerEvent_
	{
		get
		{
			return _TriggerEvent_;
		}
		set
		{
			_TriggerEvent_ = value;
			HasTriggerEvent_ = true;
		}
	}

	public int NumberOfOwnedHeroSkins
	{
		get
		{
			return _NumberOfOwnedHeroSkins;
		}
		set
		{
			_NumberOfOwnedHeroSkins = value;
			HasNumberOfOwnedHeroSkins = true;
		}
	}

	public List<int> FavoriteBaseHeroCardIds
	{
		get
		{
			return _FavoriteBaseHeroCardIds;
		}
		set
		{
			_FavoriteBaseHeroCardIds = value;
		}
	}

	public List<int> FavoriteHeroSkinIds
	{
		get
		{
			return _FavoriteHeroSkinIds;
		}
		set
		{
			_FavoriteHeroSkinIds = value;
		}
	}

	public int NumberOfOwnedStrikes
	{
		get
		{
			return _NumberOfOwnedStrikes;
		}
		set
		{
			_NumberOfOwnedStrikes = value;
			HasNumberOfOwnedStrikes = true;
		}
	}

	public List<int> FavoriteStrikeIds
	{
		get
		{
			return _FavoriteStrikeIds;
		}
		set
		{
			_FavoriteStrikeIds = value;
		}
	}

	public int NumberOfOwnedBartenders
	{
		get
		{
			return _NumberOfOwnedBartenders;
		}
		set
		{
			_NumberOfOwnedBartenders = value;
			HasNumberOfOwnedBartenders = true;
		}
	}

	public List<int> FavoriteBartenderSkinIds
	{
		get
		{
			return _FavoriteBartenderSkinIds;
		}
		set
		{
			_FavoriteBartenderSkinIds = value;
		}
	}

	public int NumberOfOwnedBoardSkins
	{
		get
		{
			return _NumberOfOwnedBoardSkins;
		}
		set
		{
			_NumberOfOwnedBoardSkins = value;
			HasNumberOfOwnedBoardSkins = true;
		}
	}

	public List<int> FavoriteBoardSkinIds
	{
		get
		{
			return _FavoriteBoardSkinIds;
		}
		set
		{
			_FavoriteBoardSkinIds = value;
		}
	}

	public int NumberOfOwnedEmotes
	{
		get
		{
			return _NumberOfOwnedEmotes;
		}
		set
		{
			_NumberOfOwnedEmotes = value;
			HasNumberOfOwnedEmotes = true;
		}
	}

	public List<int> EquippedEmoteIds
	{
		get
		{
			return _EquippedEmoteIds;
		}
		set
		{
			_EquippedEmoteIds = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasTriggerEvent_)
		{
			hash ^= TriggerEvent_.GetHashCode();
		}
		if (HasNumberOfOwnedHeroSkins)
		{
			hash ^= NumberOfOwnedHeroSkins.GetHashCode();
		}
		foreach (int favoriteBaseHeroCardId in FavoriteBaseHeroCardIds)
		{
			hash ^= favoriteBaseHeroCardId.GetHashCode();
		}
		foreach (int favoriteHeroSkinId in FavoriteHeroSkinIds)
		{
			hash ^= favoriteHeroSkinId.GetHashCode();
		}
		if (HasNumberOfOwnedStrikes)
		{
			hash ^= NumberOfOwnedStrikes.GetHashCode();
		}
		foreach (int favoriteStrikeId in FavoriteStrikeIds)
		{
			hash ^= favoriteStrikeId.GetHashCode();
		}
		if (HasNumberOfOwnedBartenders)
		{
			hash ^= NumberOfOwnedBartenders.GetHashCode();
		}
		foreach (int favoriteBartenderSkinId in FavoriteBartenderSkinIds)
		{
			hash ^= favoriteBartenderSkinId.GetHashCode();
		}
		if (HasNumberOfOwnedBoardSkins)
		{
			hash ^= NumberOfOwnedBoardSkins.GetHashCode();
		}
		foreach (int favoriteBoardSkinId in FavoriteBoardSkinIds)
		{
			hash ^= favoriteBoardSkinId.GetHashCode();
		}
		if (HasNumberOfOwnedEmotes)
		{
			hash ^= NumberOfOwnedEmotes.GetHashCode();
		}
		foreach (int equippedEmoteId in EquippedEmoteIds)
		{
			hash ^= equippedEmoteId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BattlegroundsCollectionResult other))
		{
			return false;
		}
		if (HasTriggerEvent_ != other.HasTriggerEvent_ || (HasTriggerEvent_ && !TriggerEvent_.Equals(other.TriggerEvent_)))
		{
			return false;
		}
		if (HasNumberOfOwnedHeroSkins != other.HasNumberOfOwnedHeroSkins || (HasNumberOfOwnedHeroSkins && !NumberOfOwnedHeroSkins.Equals(other.NumberOfOwnedHeroSkins)))
		{
			return false;
		}
		if (FavoriteBaseHeroCardIds.Count != other.FavoriteBaseHeroCardIds.Count)
		{
			return false;
		}
		for (int i = 0; i < FavoriteBaseHeroCardIds.Count; i++)
		{
			if (!FavoriteBaseHeroCardIds[i].Equals(other.FavoriteBaseHeroCardIds[i]))
			{
				return false;
			}
		}
		if (FavoriteHeroSkinIds.Count != other.FavoriteHeroSkinIds.Count)
		{
			return false;
		}
		for (int j = 0; j < FavoriteHeroSkinIds.Count; j++)
		{
			if (!FavoriteHeroSkinIds[j].Equals(other.FavoriteHeroSkinIds[j]))
			{
				return false;
			}
		}
		if (HasNumberOfOwnedStrikes != other.HasNumberOfOwnedStrikes || (HasNumberOfOwnedStrikes && !NumberOfOwnedStrikes.Equals(other.NumberOfOwnedStrikes)))
		{
			return false;
		}
		if (FavoriteStrikeIds.Count != other.FavoriteStrikeIds.Count)
		{
			return false;
		}
		for (int k = 0; k < FavoriteStrikeIds.Count; k++)
		{
			if (!FavoriteStrikeIds[k].Equals(other.FavoriteStrikeIds[k]))
			{
				return false;
			}
		}
		if (HasNumberOfOwnedBartenders != other.HasNumberOfOwnedBartenders || (HasNumberOfOwnedBartenders && !NumberOfOwnedBartenders.Equals(other.NumberOfOwnedBartenders)))
		{
			return false;
		}
		if (FavoriteBartenderSkinIds.Count != other.FavoriteBartenderSkinIds.Count)
		{
			return false;
		}
		for (int l = 0; l < FavoriteBartenderSkinIds.Count; l++)
		{
			if (!FavoriteBartenderSkinIds[l].Equals(other.FavoriteBartenderSkinIds[l]))
			{
				return false;
			}
		}
		if (HasNumberOfOwnedBoardSkins != other.HasNumberOfOwnedBoardSkins || (HasNumberOfOwnedBoardSkins && !NumberOfOwnedBoardSkins.Equals(other.NumberOfOwnedBoardSkins)))
		{
			return false;
		}
		if (FavoriteBoardSkinIds.Count != other.FavoriteBoardSkinIds.Count)
		{
			return false;
		}
		for (int m = 0; m < FavoriteBoardSkinIds.Count; m++)
		{
			if (!FavoriteBoardSkinIds[m].Equals(other.FavoriteBoardSkinIds[m]))
			{
				return false;
			}
		}
		if (HasNumberOfOwnedEmotes != other.HasNumberOfOwnedEmotes || (HasNumberOfOwnedEmotes && !NumberOfOwnedEmotes.Equals(other.NumberOfOwnedEmotes)))
		{
			return false;
		}
		if (EquippedEmoteIds.Count != other.EquippedEmoteIds.Count)
		{
			return false;
		}
		for (int n = 0; n < EquippedEmoteIds.Count; n++)
		{
			if (!EquippedEmoteIds[n].Equals(other.EquippedEmoteIds[n]))
			{
				return false;
			}
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BattlegroundsCollectionResult Deserialize(Stream stream, BattlegroundsCollectionResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BattlegroundsCollectionResult DeserializeLengthDelimited(Stream stream)
	{
		BattlegroundsCollectionResult instance = new BattlegroundsCollectionResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BattlegroundsCollectionResult DeserializeLengthDelimited(Stream stream, BattlegroundsCollectionResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BattlegroundsCollectionResult Deserialize(Stream stream, BattlegroundsCollectionResult instance, long limit)
	{
		instance.TriggerEvent_ = TriggerEvent.Login;
		if (instance.FavoriteBaseHeroCardIds == null)
		{
			instance.FavoriteBaseHeroCardIds = new List<int>();
		}
		if (instance.FavoriteHeroSkinIds == null)
		{
			instance.FavoriteHeroSkinIds = new List<int>();
		}
		if (instance.FavoriteStrikeIds == null)
		{
			instance.FavoriteStrikeIds = new List<int>();
		}
		if (instance.FavoriteBartenderSkinIds == null)
		{
			instance.FavoriteBartenderSkinIds = new List<int>();
		}
		if (instance.FavoriteBoardSkinIds == null)
		{
			instance.FavoriteBoardSkinIds = new List<int>();
		}
		if (instance.EquippedEmoteIds == null)
		{
			instance.EquippedEmoteIds = new List<int>();
		}
		while (true)
		{
			if (limit >= 0 && stream.Position >= limit)
			{
				if (stream.Position == limit)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int keyByte = stream.ReadByte();
			switch (keyByte)
			{
			case -1:
				break;
			case 8:
				instance.TriggerEvent_ = (TriggerEvent)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.NumberOfOwnedHeroSkins = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.FavoriteBaseHeroCardIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				instance.FavoriteHeroSkinIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				instance.NumberOfOwnedStrikes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.FavoriteStrikeIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 56:
				instance.NumberOfOwnedBartenders = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				instance.FavoriteBartenderSkinIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				instance.NumberOfOwnedBoardSkins = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				instance.FavoriteBoardSkinIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 88:
				instance.NumberOfOwnedEmotes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				instance.EquippedEmoteIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				if (key.Field == 0)
				{
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				}
				ProtocolParser.SkipKey(stream, key);
				continue;
			}
			}
			if (limit < 0)
			{
				break;
			}
			throw new EndOfStreamException();
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, BattlegroundsCollectionResult instance)
	{
		if (instance.HasTriggerEvent_)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TriggerEvent_);
		}
		if (instance.HasNumberOfOwnedHeroSkins)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NumberOfOwnedHeroSkins);
		}
		if (instance.FavoriteBaseHeroCardIds.Count > 0)
		{
			foreach (int i3 in instance.FavoriteBaseHeroCardIds)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)i3);
			}
		}
		if (instance.FavoriteHeroSkinIds.Count > 0)
		{
			foreach (int i4 in instance.FavoriteHeroSkinIds)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)i4);
			}
		}
		if (instance.HasNumberOfOwnedStrikes)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NumberOfOwnedStrikes);
		}
		if (instance.FavoriteStrikeIds.Count > 0)
		{
			foreach (int i6 in instance.FavoriteStrikeIds)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)i6);
			}
		}
		if (instance.HasNumberOfOwnedBartenders)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NumberOfOwnedBartenders);
		}
		if (instance.FavoriteBartenderSkinIds.Count > 0)
		{
			foreach (int i8 in instance.FavoriteBartenderSkinIds)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)i8);
			}
		}
		if (instance.HasNumberOfOwnedBoardSkins)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NumberOfOwnedBoardSkins);
		}
		if (instance.FavoriteBoardSkinIds.Count > 0)
		{
			foreach (int i10 in instance.FavoriteBoardSkinIds)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteUInt64(stream, (ulong)i10);
			}
		}
		if (instance.HasNumberOfOwnedEmotes)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NumberOfOwnedEmotes);
		}
		if (instance.EquippedEmoteIds.Count <= 0)
		{
			return;
		}
		foreach (int i12 in instance.EquippedEmoteIds)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, (ulong)i12);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasTriggerEvent_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TriggerEvent_);
		}
		if (HasNumberOfOwnedHeroSkins)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NumberOfOwnedHeroSkins);
		}
		if (FavoriteBaseHeroCardIds.Count > 0)
		{
			foreach (int i3 in FavoriteBaseHeroCardIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i3);
			}
		}
		if (FavoriteHeroSkinIds.Count > 0)
		{
			foreach (int i4 in FavoriteHeroSkinIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i4);
			}
		}
		if (HasNumberOfOwnedStrikes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NumberOfOwnedStrikes);
		}
		if (FavoriteStrikeIds.Count > 0)
		{
			foreach (int i6 in FavoriteStrikeIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i6);
			}
		}
		if (HasNumberOfOwnedBartenders)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NumberOfOwnedBartenders);
		}
		if (FavoriteBartenderSkinIds.Count > 0)
		{
			foreach (int i8 in FavoriteBartenderSkinIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i8);
			}
		}
		if (HasNumberOfOwnedBoardSkins)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NumberOfOwnedBoardSkins);
		}
		if (FavoriteBoardSkinIds.Count > 0)
		{
			foreach (int i10 in FavoriteBoardSkinIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i10);
			}
		}
		if (HasNumberOfOwnedEmotes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NumberOfOwnedEmotes);
		}
		if (EquippedEmoteIds.Count > 0)
		{
			foreach (int i12 in EquippedEmoteIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i12);
			}
		}
		return size;
	}
}
