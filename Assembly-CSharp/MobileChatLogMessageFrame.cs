using UnityEngine;

public class MobileChatLogMessageFrame : MonoBehaviour, ITouchListItem
{
	public UberText text;

	public GameObject m_Background;

	public string Message
	{
		get
		{
			return text.Text;
		}
		set
		{
			text.Text = value;
			text.UpdateNow();
			UpdateLocalBounds();
		}
	}

	public bool IsHeader => false;

	public bool Visible
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public Color Color
	{
		get
		{
			return text.TextColor;
		}
		set
		{
			text.TextColor = value;
		}
	}

	public virtual float Width
	{
		get
		{
			return text.Width;
		}
		set
		{
			text.Width = value;
			if (m_Background != null)
			{
				float backgroundWidth = m_Background.GetComponent<MeshFilter>().mesh.bounds.size.x;
				m_Background.transform.localScale = new Vector3(value / backgroundWidth, m_Background.transform.localScale.y, 1f);
				m_Background.transform.localPosition = new Vector3((0f - value) / (0.5f * backgroundWidth), 0f, 0f);
			}
		}
	}

	public Bounds LocalBounds { get; protected set; }

	public new T GetComponent<T>() where T : Component
	{
		return ((Component)this).GetComponent<T>();
	}

	public virtual void RebuildUberText()
	{
		text.UpdateNow(updateIfInactive: true);
	}

	public void OnScrollOutOfView()
	{
	}

	public virtual void OnPositionUpdate()
	{
	}

	public virtual void UpdateLocalBounds()
	{
		RebuildUberText();
		Bounds textBounds = text.GetTextBounds();
		Vector3 textBoundsSize = textBounds.size;
		Bounds newBounds = default(Bounds);
		newBounds.center = base.transform.InverseTransformPoint(textBounds.center) + 10f * Vector3.up;
		Vector3 lossyScale = base.transform.lossyScale;
		newBounds.size = new Vector3(textBoundsSize.x / lossyScale.x, textBoundsSize.y / lossyScale.y, textBoundsSize.z / lossyScale.z);
		LocalBounds = newBounds;
	}

	GameObject ITouchListItem.get_gameObject()
	{
		return base.gameObject;
	}

	Transform ITouchListItem.get_transform()
	{
		return base.transform;
	}
}
