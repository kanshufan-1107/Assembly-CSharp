using System.Collections.Generic;
using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopBalanceAvailable : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	private List<Balance> _Balances = new List<Balance>();

	public Player Player
	{
		get
		{
			return _Player;
		}
		set
		{
			_Player = value;
			HasPlayer = value != null;
		}
	}

	public List<Balance> Balances
	{
		get
		{
			return _Balances;
		}
		set
		{
			_Balances = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		foreach (Balance i in Balances)
		{
			hash ^= i.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopBalanceAvailable other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (Balances.Count != other.Balances.Count)
		{
			return false;
		}
		for (int i = 0; i < Balances.Count; i++)
		{
			if (!Balances[i].Equals(other.Balances[i]))
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

	public static ShopBalanceAvailable Deserialize(Stream stream, ShopBalanceAvailable instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopBalanceAvailable DeserializeLengthDelimited(Stream stream)
	{
		ShopBalanceAvailable instance = new ShopBalanceAvailable();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopBalanceAvailable DeserializeLengthDelimited(Stream stream, ShopBalanceAvailable instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopBalanceAvailable Deserialize(Stream stream, ShopBalanceAvailable instance, long limit)
	{
		if (instance.Balances == null)
		{
			instance.Balances = new List<Balance>();
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
			case 10:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 18:
				instance.Balances.Add(Balance.DeserializeLengthDelimited(stream));
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

	public static void Serialize(Stream stream, ShopBalanceAvailable instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.Balances.Count <= 0)
		{
			return;
		}
		foreach (Balance i2 in instance.Balances)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, i2.GetSerializedSize());
			Balance.Serialize(stream, i2);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPlayer)
		{
			size++;
			uint size2 = Player.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (Balances.Count > 0)
		{
			foreach (Balance balance in Balances)
			{
				size++;
				uint size3 = balance.GetSerializedSize();
				size += size3 + ProtocolParser.SizeOfUInt32(size3);
			}
		}
		return size;
	}
}
