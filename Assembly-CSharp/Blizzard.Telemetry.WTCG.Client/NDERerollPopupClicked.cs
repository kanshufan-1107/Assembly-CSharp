using System.Collections.Generic;
using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class NDERerollPopupClicked : IProtoBuf
{
	public bool HasAcceptedReroll;

	private bool _AcceptedReroll;

	private List<long> _NoticeIds = new List<long>();

	public bool HasCardAsset;

	private int _CardAsset;

	private List<int> _CardPremium = new List<int>();

	public bool HasCardQuantity;

	private int _CardQuantity;

	public bool HasNdePremium;

	private int _NdePremium;

	public bool HasIsForcedPremium;

	private bool _IsForcedPremium;

	public bool AcceptedReroll
	{
		get
		{
			return _AcceptedReroll;
		}
		set
		{
			_AcceptedReroll = value;
			HasAcceptedReroll = true;
		}
	}

	public List<long> NoticeIds
	{
		get
		{
			return _NoticeIds;
		}
		set
		{
			_NoticeIds = value;
		}
	}

	public int CardAsset
	{
		get
		{
			return _CardAsset;
		}
		set
		{
			_CardAsset = value;
			HasCardAsset = true;
		}
	}

	public List<int> CardPremium
	{
		get
		{
			return _CardPremium;
		}
		set
		{
			_CardPremium = value;
		}
	}

	public int CardQuantity
	{
		get
		{
			return _CardQuantity;
		}
		set
		{
			_CardQuantity = value;
			HasCardQuantity = true;
		}
	}

	public int NdePremium
	{
		get
		{
			return _NdePremium;
		}
		set
		{
			_NdePremium = value;
			HasNdePremium = true;
		}
	}

	public bool IsForcedPremium
	{
		get
		{
			return _IsForcedPremium;
		}
		set
		{
			_IsForcedPremium = value;
			HasIsForcedPremium = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAcceptedReroll)
		{
			hash ^= AcceptedReroll.GetHashCode();
		}
		foreach (long noticeId in NoticeIds)
		{
			hash ^= noticeId.GetHashCode();
		}
		if (HasCardAsset)
		{
			hash ^= CardAsset.GetHashCode();
		}
		foreach (int item in CardPremium)
		{
			hash ^= item.GetHashCode();
		}
		if (HasCardQuantity)
		{
			hash ^= CardQuantity.GetHashCode();
		}
		if (HasNdePremium)
		{
			hash ^= NdePremium.GetHashCode();
		}
		if (HasIsForcedPremium)
		{
			hash ^= IsForcedPremium.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NDERerollPopupClicked other))
		{
			return false;
		}
		if (HasAcceptedReroll != other.HasAcceptedReroll || (HasAcceptedReroll && !AcceptedReroll.Equals(other.AcceptedReroll)))
		{
			return false;
		}
		if (NoticeIds.Count != other.NoticeIds.Count)
		{
			return false;
		}
		for (int i = 0; i < NoticeIds.Count; i++)
		{
			if (!NoticeIds[i].Equals(other.NoticeIds[i]))
			{
				return false;
			}
		}
		if (HasCardAsset != other.HasCardAsset || (HasCardAsset && !CardAsset.Equals(other.CardAsset)))
		{
			return false;
		}
		if (CardPremium.Count != other.CardPremium.Count)
		{
			return false;
		}
		for (int j = 0; j < CardPremium.Count; j++)
		{
			if (!CardPremium[j].Equals(other.CardPremium[j]))
			{
				return false;
			}
		}
		if (HasCardQuantity != other.HasCardQuantity || (HasCardQuantity && !CardQuantity.Equals(other.CardQuantity)))
		{
			return false;
		}
		if (HasNdePremium != other.HasNdePremium || (HasNdePremium && !NdePremium.Equals(other.NdePremium)))
		{
			return false;
		}
		if (HasIsForcedPremium != other.HasIsForcedPremium || (HasIsForcedPremium && !IsForcedPremium.Equals(other.IsForcedPremium)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static NDERerollPopupClicked Deserialize(Stream stream, NDERerollPopupClicked instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static NDERerollPopupClicked DeserializeLengthDelimited(Stream stream)
	{
		NDERerollPopupClicked instance = new NDERerollPopupClicked();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static NDERerollPopupClicked DeserializeLengthDelimited(Stream stream, NDERerollPopupClicked instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static NDERerollPopupClicked Deserialize(Stream stream, NDERerollPopupClicked instance, long limit)
	{
		if (instance.NoticeIds == null)
		{
			instance.NoticeIds = new List<long>();
		}
		if (instance.CardPremium == null)
		{
			instance.CardPremium = new List<int>();
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
				instance.AcceptedReroll = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				instance.NoticeIds.Add((long)ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				instance.CardAsset = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.CardPremium.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				instance.CardQuantity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.NdePremium = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.IsForcedPremium = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, NDERerollPopupClicked instance)
	{
		if (instance.HasAcceptedReroll)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.AcceptedReroll);
		}
		if (instance.NoticeIds.Count > 0)
		{
			foreach (long i2 in instance.NoticeIds)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)i2);
			}
		}
		if (instance.HasCardAsset)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CardAsset);
		}
		if (instance.CardPremium.Count > 0)
		{
			foreach (int i4 in instance.CardPremium)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)i4);
			}
		}
		if (instance.HasCardQuantity)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CardQuantity);
		}
		if (instance.HasNdePremium)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NdePremium);
		}
		if (instance.HasIsForcedPremium)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.IsForcedPremium);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAcceptedReroll)
		{
			size++;
			size++;
		}
		if (NoticeIds.Count > 0)
		{
			foreach (long i2 in NoticeIds)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i2);
			}
		}
		if (HasCardAsset)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CardAsset);
		}
		if (CardPremium.Count > 0)
		{
			foreach (int i4 in CardPremium)
			{
				size++;
				size += ProtocolParser.SizeOfUInt64((ulong)i4);
			}
		}
		if (HasCardQuantity)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CardQuantity);
		}
		if (HasNdePremium)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NdePremium);
		}
		if (HasIsForcedPremium)
		{
			size++;
			size++;
		}
		return size;
	}
}
