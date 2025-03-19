using System.IO;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using UnityEngine;

namespace Hearthstone.UI;

[DisallowMultipleComponent]
public class ManagedSprite : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	[HideInInspector]
	private bool m_usesSpriteAtlas = true;

	[HideInInspector]
	[SerializeField]
	private string m_spriteAtlasAssetString;

	private AssetReference m_spriteAtlasRef;

	private string m_atlasTag;

	private bool m_initialized;

	public SpriteRenderer SpriteRenderer => m_spriteRenderer;

	public bool UsesSpriteAtlas
	{
		get
		{
			return m_usesSpriteAtlas;
		}
		set
		{
			m_usesSpriteAtlas = value;
		}
	}

	public Sprite Sprite
	{
		get
		{
			if (m_spriteRenderer == null)
			{
				return null;
			}
			return m_spriteRenderer.sprite;
		}
		set
		{
			if (!(m_spriteRenderer == null) && !(m_spriteRenderer.sprite == Sprite))
			{
				m_spriteRenderer.sprite = value;
			}
		}
	}

	public AssetReference SpriteAtlas
	{
		get
		{
			if (m_spriteAtlasRef == null)
			{
				m_spriteAtlasRef = AssetReference.CreateFromAssetString(m_spriteAtlasAssetString);
			}
			return m_spriteAtlasRef;
		}
		set
		{
			if (value != null && !string.IsNullOrEmpty(value.guid) && !(m_spriteAtlasAssetString == value.ToString()))
			{
				m_spriteAtlasRef = value;
				m_spriteAtlasAssetString = value.ToString();
				RefreshAtlasTag();
			}
		}
	}

	public string SpriteAtlasTag
	{
		get
		{
			if (string.IsNullOrEmpty(m_atlasTag))
			{
				RefreshAtlasTag(shouldRegister: false);
			}
			return m_atlasTag;
		}
	}

	public static ManagedSprite Create(Sprite sprite, string spriteAtlasRef, GameObject targetObject)
	{
		ManagedSprite managedSprite = targetObject.AddComponent<ManagedSprite>();
		managedSprite.InitializeRenderer();
		AssetReference atlasRef = AssetReference.CreateFromAssetString(spriteAtlasRef);
		if (atlasRef != null)
		{
			managedSprite.SpriteAtlas = atlasRef;
			managedSprite.UsesSpriteAtlas = true;
		}
		if (sprite != null)
		{
			managedSprite.SpriteRenderer.sprite = sprite;
		}
		return managedSprite;
	}

	public void InitializeRenderer()
	{
		if (m_spriteRenderer == null && !base.gameObject.TryGetComponent<SpriteRenderer>(out m_spriteRenderer))
		{
			m_spriteRenderer = base.gameObject.AddComponent<SpriteRenderer>();
		}
		GameObjectUtils.SetHideFlags(m_spriteRenderer, HideFlags.HideInInspector);
	}

	public void RefreshAtlasTag(bool shouldRegister = true)
	{
		string newTag = ((SpriteAtlas != null && !string.IsNullOrEmpty(SpriteAtlas.FileName)) ? Path.GetFileNameWithoutExtension(SpriteAtlas.FileName) : string.Empty);
		if (!(m_atlasTag == newTag))
		{
			if (!string.IsNullOrEmpty(m_atlasTag) && m_initialized)
			{
				Log.UIFramework.PrintWarning("Overriding the an existing sprite atlas tag. Make sure that this new atlas ({0}) actually holds the sprite.", SpriteAtlas);
				SpriteAtlasProvider.Get()?.UnregisterSpriteComponent(m_atlasTag, this);
			}
			m_atlasTag = newTag;
			if (m_initialized && shouldRegister)
			{
				RegisterWithAtlasService();
			}
		}
	}

	private void Awake()
	{
		InitializeRenderer();
		ServiceManager.InitializeDynamicServicesIfNeeded(out var _, typeof(IAssetLoader), typeof(IAliasedAssetResolver), typeof(SpriteAtlasProvider));
	}

	private void Start()
	{
		RegisterWithAtlasService();
		m_initialized = true;
	}

	private void OnDestroy()
	{
		if (m_initialized && m_usesSpriteAtlas)
		{
			SpriteAtlasProvider.Get()?.UnregisterSpriteComponent(m_atlasTag, this);
		}
	}

	private void RegisterWithAtlasService()
	{
		if (!m_usesSpriteAtlas)
		{
			return;
		}
		if (string.IsNullOrEmpty(SpriteAtlasTag))
		{
			Log.Asset.PrintWarning($"[ManagedSprite ({GetInstanceID()})] Missing sprite atlas tag. Invalid or missing SpriteAtlas reference!");
			return;
		}
		SpriteAtlasProvider.RegisterReadyListener(delegate
		{
			SpriteAtlasProvider.Get()?.RegisterSpriteComponent(m_atlasTag, this);
		});
	}
}
