using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class MobileFailConnectGameServer : IProtoBuf
{
	public bool HasAddress;

	private string _Address;

	public bool HasMoreinfoPressed;

	private bool _MoreinfoPressed;

	public bool HasGotitPressed;

	private bool _GotitPressed;

	public string Address
	{
		get
		{
			return _Address;
		}
		set
		{
			_Address = value;
			HasAddress = value != null;
		}
	}

	public bool MoreinfoPressed
	{
		get
		{
			return _MoreinfoPressed;
		}
		set
		{
			_MoreinfoPressed = value;
			HasMoreinfoPressed = true;
		}
	}

	public bool GotitPressed
	{
		get
		{
			return _GotitPressed;
		}
		set
		{
			_GotitPressed = value;
			HasGotitPressed = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAddress)
		{
			hash ^= Address.GetHashCode();
		}
		if (HasMoreinfoPressed)
		{
			hash ^= MoreinfoPressed.GetHashCode();
		}
		if (HasGotitPressed)
		{
			hash ^= GotitPressed.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MobileFailConnectGameServer other))
		{
			return false;
		}
		if (HasAddress != other.HasAddress || (HasAddress && !Address.Equals(other.Address)))
		{
			return false;
		}
		if (HasMoreinfoPressed != other.HasMoreinfoPressed || (HasMoreinfoPressed && !MoreinfoPressed.Equals(other.MoreinfoPressed)))
		{
			return false;
		}
		if (HasGotitPressed != other.HasGotitPressed || (HasGotitPressed && !GotitPressed.Equals(other.GotitPressed)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static MobileFailConnectGameServer Deserialize(Stream stream, MobileFailConnectGameServer instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MobileFailConnectGameServer DeserializeLengthDelimited(Stream stream)
	{
		MobileFailConnectGameServer instance = new MobileFailConnectGameServer();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MobileFailConnectGameServer DeserializeLengthDelimited(Stream stream, MobileFailConnectGameServer instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MobileFailConnectGameServer Deserialize(Stream stream, MobileFailConnectGameServer instance, long limit)
	{
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
				instance.Address = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.MoreinfoPressed = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				instance.GotitPressed = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, MobileFailConnectGameServer instance)
	{
		if (instance.HasAddress)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Address));
		}
		if (instance.HasMoreinfoPressed)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.MoreinfoPressed);
		}
		if (instance.HasGotitPressed)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.GotitPressed);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAddress)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(Address);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasMoreinfoPressed)
		{
			size++;
			size++;
		}
		if (HasGotitPressed)
		{
			size++;
			size++;
		}
		return size;
	}
}
