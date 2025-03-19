using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI;

public class TransformDynamicPropertyResolverProxy : IDynamicPropertyResolverProxy, IDynamicPropertyResolver
{
	private List<DynamicPropertyInfo> m_dynamicProperties = new List<DynamicPropertyInfo>
	{
		new DynamicPropertyInfo
		{
			Id = "position",
			Name = "Position",
			Type = typeof(Vector3),
			Value = Vector3.zero
		},
		new DynamicPropertyInfo
		{
			Id = "rotation",
			Name = "Rotation",
			Type = typeof(Vector3),
			Value = Vector3.zero
		},
		new DynamicPropertyInfo
		{
			Id = "scale",
			Name = "Scale",
			Type = typeof(Vector3),
			Value = Vector3.zero
		},
		new DynamicPropertyInfo
		{
			Id = "position_x",
			Name = "Position_X",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "position_y",
			Name = "Position_Y",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "position_z",
			Name = "Position_Z",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "rotation_x",
			Name = "Rotation_X",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "rotation_y",
			Name = "Rotation_Y",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "rotation_z",
			Name = "Rotation_Z",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "scale_x",
			Name = "Scale_X",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "scale_y",
			Name = "Scale_Y",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "scale_z",
			Name = "Scale_Z",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_position",
			Name = "World_Position",
			Type = typeof(Vector3),
			Value = Vector3.zero
		},
		new DynamicPropertyInfo
		{
			Id = "world_position_x",
			Name = "World_Position_X",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_position_y",
			Name = "World_Position_Y",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_position_z",
			Name = "World_Position_Z",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_rotation",
			Name = "World_Rotation",
			Type = typeof(Vector3),
			Value = Vector3.zero
		},
		new DynamicPropertyInfo
		{
			Id = "world_rotation_x",
			Name = "World_Rotation_X",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_rotation_y",
			Name = "World_Rotation_Y",
			Type = typeof(float),
			Value = 0f
		},
		new DynamicPropertyInfo
		{
			Id = "world_rotation_z",
			Name = "World_Rotation_Z",
			Type = typeof(float),
			Value = 0f
		}
	};

	private Transform m_transform;

	public ICollection<DynamicPropertyInfo> DynamicProperties => m_dynamicProperties;

	public void SetTarget(object target)
	{
		m_transform = (Transform)target;
		m_dynamicProperties[0].Value = m_transform.localPosition;
		m_dynamicProperties[1].Value = m_transform.localRotation.eulerAngles;
		m_dynamicProperties[2].Value = m_transform.localScale;
		m_dynamicProperties[3].Value = m_transform.localPosition.x;
		m_dynamicProperties[4].Value = m_transform.localPosition.y;
		m_dynamicProperties[5].Value = m_transform.localPosition.z;
		m_dynamicProperties[6].Value = m_transform.localRotation.eulerAngles.x;
		m_dynamicProperties[7].Value = m_transform.localRotation.eulerAngles.y;
		m_dynamicProperties[8].Value = m_transform.localRotation.eulerAngles.z;
		m_dynamicProperties[9].Value = m_transform.localScale.x;
		m_dynamicProperties[10].Value = m_transform.localScale.y;
		m_dynamicProperties[11].Value = m_transform.localScale.z;
		m_dynamicProperties[12].Value = m_transform.position;
		m_dynamicProperties[13].Value = m_transform.position.x;
		m_dynamicProperties[14].Value = m_transform.position.y;
		m_dynamicProperties[15].Value = m_transform.position.z;
		m_dynamicProperties[16].Value = m_transform.rotation.eulerAngles;
		m_dynamicProperties[17].Value = m_transform.rotation.eulerAngles.x;
		m_dynamicProperties[18].Value = m_transform.rotation.eulerAngles.y;
		m_dynamicProperties[19].Value = m_transform.rotation.eulerAngles.z;
	}

	public bool GetDynamicPropertyValue(string id, out object value)
	{
		value = null;
		switch (id)
		{
		case "position":
			value = m_transform.localPosition;
			return true;
		case "scale":
			value = m_transform.localScale;
			return true;
		case "rotation":
			value = m_transform.localRotation.eulerAngles;
			return true;
		case "position_x":
			value = m_transform.localPosition.x;
			return true;
		case "position_y":
			value = m_transform.localPosition.y;
			return true;
		case "position_z":
			value = m_transform.localPosition.z;
			return true;
		case "rotation_x":
			value = m_transform.localRotation.x;
			return true;
		case "rotation_y":
			value = m_transform.localRotation.y;
			return true;
		case "rotation_z":
			value = m_transform.localRotation.z;
			return true;
		case "scale_x":
			value = m_transform.localScale.x;
			return true;
		case "scale_y":
			value = m_transform.localScale.y;
			return true;
		case "scale_z":
			value = m_transform.localScale.z;
			return true;
		case "world_position":
			value = m_transform.position;
			return true;
		case "world_position_x":
			value = m_transform.position.x;
			return true;
		case "world_position_y":
			value = m_transform.position.y;
			return true;
		case "world_position_z":
			value = m_transform.position.z;
			return true;
		case "world_rotation":
			value = m_transform.rotation.eulerAngles;
			return true;
		case "world_rotation_x":
			value = m_transform.rotation.eulerAngles.x;
			return true;
		case "world_rotation_y":
			value = m_transform.rotation.eulerAngles.y;
			return true;
		case "world_rotation_z":
			value = m_transform.rotation.eulerAngles.z;
			return true;
		default:
			return false;
		}
	}

	public bool SetDynamicPropertyValue(string id, object value)
	{
		switch (id)
		{
		case "position":
			m_transform.localPosition = new Vector3(((Vector4)value).x, ((Vector4)value).y, ((Vector4)value).z);
			return true;
		case "rotation":
		{
			Vector3 v3Value = new Vector3(((Vector4)value).x, ((Vector4)value).y, ((Vector4)value).z);
			m_transform.localRotation = Quaternion.Euler(v3Value);
			return true;
		}
		case "scale":
			m_transform.localScale = new Vector3(((Vector4)value).x, ((Vector4)value).y, ((Vector4)value).z);
			return true;
		case "position_x":
		{
			Vector3 temp15 = m_transform.localPosition;
			temp15.x = (float)value;
			m_transform.localPosition = temp15;
			return true;
		}
		case "position_y":
		{
			Vector3 temp14 = m_transform.localPosition;
			temp14.y = (float)value;
			m_transform.localPosition = temp14;
			return true;
		}
		case "position_z":
		{
			Vector3 temp13 = m_transform.localPosition;
			temp13.z = (float)value;
			m_transform.localPosition = temp13;
			return true;
		}
		case "rotation_x":
		{
			Quaternion temp12 = m_transform.localRotation;
			Vector3 angles6 = temp12.eulerAngles;
			angles6.x = (float)value;
			temp12.eulerAngles = angles6;
			m_transform.localRotation = temp12;
			return true;
		}
		case "rotation_y":
		{
			Quaternion temp11 = m_transform.localRotation;
			Vector3 angles5 = temp11.eulerAngles;
			angles5.y = (float)value;
			temp11.eulerAngles = angles5;
			m_transform.localRotation = temp11;
			return true;
		}
		case "rotation_z":
		{
			Quaternion temp10 = m_transform.localRotation;
			Vector3 angles4 = temp10.eulerAngles;
			angles4.z = (float)value;
			temp10.eulerAngles = angles4;
			m_transform.localRotation = temp10;
			return true;
		}
		case "scale_x":
		{
			Vector3 temp9 = m_transform.localScale;
			temp9.x = (float)value;
			m_transform.localScale = temp9;
			return true;
		}
		case "scale_y":
		{
			Vector3 temp8 = m_transform.localScale;
			temp8.y = (float)value;
			m_transform.localScale = temp8;
			return true;
		}
		case "scale_z":
		{
			Vector3 temp7 = m_transform.localScale;
			temp7.z = (float)value;
			m_transform.localScale = temp7;
			return true;
		}
		case "world_position":
			m_transform.position = new Vector3(((Vector4)value).x, ((Vector4)value).y, ((Vector4)value).z);
			return true;
		case "world_position_x":
		{
			Vector3 temp6 = m_transform.position;
			temp6.x = (float)value;
			m_transform.position = temp6;
			return true;
		}
		case "world_position_y":
		{
			Vector3 temp5 = m_transform.position;
			temp5.y = (float)value;
			m_transform.position = temp5;
			return true;
		}
		case "world_position_z":
		{
			Vector3 temp4 = m_transform.position;
			temp4.z = (float)value;
			m_transform.position = temp4;
			return true;
		}
		case "world_rotation":
			m_transform.rotation = Quaternion.Euler(new Vector3(((Vector4)value).x, ((Vector4)value).y, ((Vector4)value).z));
			return true;
		case "world_rotation_x":
		{
			Quaternion temp3 = m_transform.rotation;
			Vector3 angles3 = temp3.eulerAngles;
			angles3.x = (float)value;
			temp3.eulerAngles = angles3;
			m_transform.rotation = temp3;
			return true;
		}
		case "world_rotation_y":
		{
			Quaternion temp2 = m_transform.rotation;
			Vector3 angles2 = temp2.eulerAngles;
			angles2.y = (float)value;
			temp2.eulerAngles = angles2;
			m_transform.rotation = temp2;
			return true;
		}
		case "world_rotation_z":
		{
			Quaternion temp = m_transform.rotation;
			Vector3 angles = temp.eulerAngles;
			angles.z = (float)value;
			temp.eulerAngles = angles;
			m_transform.rotation = temp;
			return true;
		}
		default:
			return false;
		}
	}
}
