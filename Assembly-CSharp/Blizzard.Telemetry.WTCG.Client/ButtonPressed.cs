using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ButtonPressed : IProtoBuf
{
	public bool HasButtonName;

	private string _ButtonName;

	public string ButtonName
	{
		get
		{
			return _ButtonName;
		}
		set
		{
			_ButtonName = value;
			HasButtonName = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasButtonName)
		{
			hash ^= ButtonName.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ButtonPressed other))
		{
			return false;
		}
		if (HasButtonName != other.HasButtonName || (HasButtonName && !ButtonName.Equals(other.ButtonName)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ButtonPressed Deserialize(Stream stream, ButtonPressed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ButtonPressed DeserializeLengthDelimited(Stream stream)
	{
		ButtonPressed instance = new ButtonPressed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ButtonPressed DeserializeLengthDelimited(Stream stream, ButtonPressed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ButtonPressed Deserialize(Stream stream, ButtonPressed instance, long limit)
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
				instance.ButtonName = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ButtonPressed instance)
	{
		if (instance.HasButtonName)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ButtonName));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasButtonName)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(ButtonName);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		return size;
	}
}
