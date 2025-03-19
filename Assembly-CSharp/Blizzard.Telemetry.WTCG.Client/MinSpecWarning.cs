using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class MinSpecWarning : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasNextVersion;

	private bool _NextVersion;

	private List<string> _Warnings = new List<string>();

	public DeviceInfo DeviceInfo
	{
		get
		{
			return _DeviceInfo;
		}
		set
		{
			_DeviceInfo = value;
			HasDeviceInfo = value != null;
		}
	}

	public bool NextVersion
	{
		get
		{
			return _NextVersion;
		}
		set
		{
			_NextVersion = value;
			HasNextVersion = true;
		}
	}

	public List<string> Warnings
	{
		get
		{
			return _Warnings;
		}
		set
		{
			_Warnings = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasNextVersion)
		{
			hash ^= NextVersion.GetHashCode();
		}
		foreach (string i in Warnings)
		{
			hash ^= i.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MinSpecWarning other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasNextVersion != other.HasNextVersion || (HasNextVersion && !NextVersion.Equals(other.NextVersion)))
		{
			return false;
		}
		if (Warnings.Count != other.Warnings.Count)
		{
			return false;
		}
		for (int i = 0; i < Warnings.Count; i++)
		{
			if (!Warnings[i].Equals(other.Warnings[i]))
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

	public static MinSpecWarning Deserialize(Stream stream, MinSpecWarning instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MinSpecWarning DeserializeLengthDelimited(Stream stream)
	{
		MinSpecWarning instance = new MinSpecWarning();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MinSpecWarning DeserializeLengthDelimited(Stream stream, MinSpecWarning instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MinSpecWarning Deserialize(Stream stream, MinSpecWarning instance, long limit)
	{
		if (instance.Warnings == null)
		{
			instance.Warnings = new List<string>();
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 24:
				instance.NextVersion = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				instance.Warnings.Add(ProtocolParser.ReadString(stream));
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

	public static void Serialize(Stream stream, MinSpecWarning instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasNextVersion)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.NextVersion);
		}
		if (instance.Warnings.Count <= 0)
		{
			return;
		}
		foreach (string i5 in instance.Warnings)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i5));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasNextVersion)
		{
			size++;
			size++;
		}
		if (Warnings.Count > 0)
		{
			foreach (string i5 in Warnings)
			{
				size++;
				uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(i5);
				size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
			}
		}
		return size;
	}
}
