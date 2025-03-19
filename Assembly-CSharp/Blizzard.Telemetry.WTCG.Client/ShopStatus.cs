using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopStatus : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasError;

	private string _Error;

	public bool HasTimeInHubSec;

	private double _TimeInHubSec;

	public bool HasTimeShownClosedSec;

	private float _TimeShownClosedSec;

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

	public string Error
	{
		get
		{
			return _Error;
		}
		set
		{
			_Error = value;
			HasError = value != null;
		}
	}

	public double TimeInHubSec
	{
		get
		{
			return _TimeInHubSec;
		}
		set
		{
			_TimeInHubSec = value;
			HasTimeInHubSec = true;
		}
	}

	public float TimeShownClosedSec
	{
		get
		{
			return _TimeShownClosedSec;
		}
		set
		{
			_TimeShownClosedSec = value;
			HasTimeShownClosedSec = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasError)
		{
			hash ^= Error.GetHashCode();
		}
		if (HasTimeInHubSec)
		{
			hash ^= TimeInHubSec.GetHashCode();
		}
		if (HasTimeShownClosedSec)
		{
			hash ^= TimeShownClosedSec.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopStatus other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasError != other.HasError || (HasError && !Error.Equals(other.Error)))
		{
			return false;
		}
		if (HasTimeInHubSec != other.HasTimeInHubSec || (HasTimeInHubSec && !TimeInHubSec.Equals(other.TimeInHubSec)))
		{
			return false;
		}
		if (HasTimeShownClosedSec != other.HasTimeShownClosedSec || (HasTimeShownClosedSec && !TimeShownClosedSec.Equals(other.TimeShownClosedSec)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ShopStatus Deserialize(Stream stream, ShopStatus instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopStatus DeserializeLengthDelimited(Stream stream)
	{
		ShopStatus instance = new ShopStatus();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopStatus DeserializeLengthDelimited(Stream stream, ShopStatus instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopStatus Deserialize(Stream stream, ShopStatus instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
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
				instance.Error = ProtocolParser.ReadString(stream);
				continue;
			case 25:
				instance.TimeInHubSec = br.ReadDouble();
				continue;
			case 37:
				instance.TimeShownClosedSec = br.ReadSingle();
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

	public static void Serialize(Stream stream, ShopStatus instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasError)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Error));
		}
		if (instance.HasTimeInHubSec)
		{
			stream.WriteByte(25);
			bw.Write(instance.TimeInHubSec);
		}
		if (instance.HasTimeShownClosedSec)
		{
			stream.WriteByte(37);
			bw.Write(instance.TimeShownClosedSec);
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
		if (HasError)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Error);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasTimeInHubSec)
		{
			size++;
			size += 8;
		}
		if (HasTimeShownClosedSec)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
