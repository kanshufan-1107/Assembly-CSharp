using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class HeroPickerButton : PegUIElement
{
	public GameObject m_heroClassIcon;

	public GameObject m_heroClassIconSepia;

	public UberText m_classLabel;

	public GameObject m_labelGradient;

	public GameObject m_button;

	public GameObject m_buttonFrame;

	public TAG_CLASS m_heroClass;

	public List<Material> CLASS_MATERIALS = new List<Material>();

	public HeroPickerButtonBones m_bones;

	public TooltipZone m_tooltipZone;

	public GameObject m_crown;

	public UberText m_lockReasonText;

	public GameObject m_raiseAndLowerRoot;

	public GameObject m_heroClassIconOffset;

	public Vector3 m_diamondHeroScale = Vector3.one;

	protected DefLoader.DisposableFullDef m_fullDef;

	protected TAG_PREMIUM m_premium;

	protected float? m_seed;

	private bool m_isSelected;

	private HighlightState m_highlightState;

	private bool m_locked;

	private long m_preconDeckID;

	private Renderer m_buttonRenderer;

	private readonly List<Material> m_cachedMaterials = new List<Material>();

	private ILegendaryHeroPortrait m_legendaryHeroPortrait;

	private CustomFrameController m_customFrameController;

	private static readonly Color BASIC_SET_COLOR_IN_PROGRESS = new Color(0.97f, 0.82f, 0.22f);

	public int HeroCardDbId => GameUtils.TranslateCardIdToDbId(m_fullDef?.EntityDef?.GetCardId());

	public bool HasCardDef => m_fullDef?.CardDef != null;

	public string HeroPickerSelectedPrefab
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_fullDef.CardDef.m_HeroPickerSelectedPrefab;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_buttonFrame != null)
		{
			m_buttonRenderer = m_buttonFrame.GetComponent<Renderer>();
			if (m_buttonRenderer != null)
			{
				m_buttonRenderer.GetSharedMaterials(m_cachedMaterials);
			}
		}
	}

	protected override void OnDestroy()
	{
		ReleaseFullDef();
		DestroyLegendaryHeroPortrait();
		DestroyCustomFrame();
		base.OnDestroy();
	}

	public void SetPreconDeckID(long preconDeckID)
	{
		m_preconDeckID = preconDeckID;
	}

	public long GetPreconDeckID()
	{
		return m_preconDeckID;
	}

	public virtual void UpdateDisplay(DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		SetFullDef(def);
		SetPremium(premium);
	}

	public void SetClassIcon(Material mat)
	{
		Renderer heroClassRenderer = m_heroClassIcon.GetComponent<Renderer>();
		heroClassRenderer.SetMaterial(mat);
		heroClassRenderer.GetMaterial().renderQueue = 3007;
		if (m_heroClassIconSepia != null)
		{
			Renderer component = m_heroClassIconSepia.GetComponent<Renderer>();
			component.GetMaterial().SetTextureOffset("_MainTex", heroClassRenderer.GetMaterial().GetTextureOffset("_MainTex"));
			component.GetMaterial().SetTextureScale("_MainTex", heroClassRenderer.GetMaterial().GetTextureScale("_MainTex"));
			component.GetMaterial().renderQueue = 3007;
		}
	}

	public void SetClassname(string s)
	{
		m_classLabel.Text = s;
	}

	public virtual GuestHeroDbfRecord GetGuestHero()
	{
		return null;
	}

	public void HideTextAndGradient()
	{
		m_classLabel.Hide();
		m_labelGradient.SetActive(value: false);
	}

	public void SetFullDef(DefLoader.DisposableFullDef def)
	{
		ReleaseFullDef();
		m_fullDef = def?.Share();
		UpdatePortrait();
	}

	public EntityDef GetEntityDef()
	{
		return m_fullDef?.EntityDef;
	}

	public DefLoader.DisposableCardDef ShareEntityDef()
	{
		return m_fullDef?.DisposableCardDef?.Share();
	}

	public DefLoader.DisposableFullDef ShareFullDef()
	{
		return m_fullDef?.Share();
	}

	public void SetSelected(bool isSelected)
	{
		m_isSelected = isSelected;
		if (isSelected)
		{
			Lower();
		}
		else
		{
			Raise();
		}
	}

	public bool IsSelected()
	{
		return m_isSelected;
	}

	public void SetLockReasonText(string text)
	{
		if (!(m_lockReasonText == null))
		{
			m_lockReasonText.Text = text;
		}
	}

	public void Lower()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			Activate(enable: false);
		}
		float offset = ((!m_locked) ? (-0.7f) : 0.7f);
		Vector3 position = new Vector3(GetOriginalLocalPosition().x, GetOriginalLocalPosition().y + offset, GetOriginalLocalPosition().z);
		if (m_customFrameController != null)
		{
			position.y = Mathf.Max(position.y, m_customFrameController.RaiseAndLowerLimit);
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", position);
		args.Add("time", 0.1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("islocal", true);
		iTween.MoveTo((m_raiseAndLowerRoot != null) ? m_raiseAndLowerRoot : base.gameObject, args);
	}

	public void Raise()
	{
		if (!m_isSelected)
		{
			Activate(enable: true);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", new Vector3(GetOriginalLocalPosition().x, GetOriginalLocalPosition().y, GetOriginalLocalPosition().z));
			args.Add("time", 0.1f);
			args.Add("easetype", iTween.EaseType.linear);
			args.Add("islocal", true);
			iTween.MoveTo(m_raiseAndLowerRoot, args);
		}
	}

	public void SetHighlightState(ActorStateType stateType)
	{
		if (m_highlightState == null)
		{
			m_highlightState = GetComponentInChildren<HighlightState>();
		}
		if (m_highlightState != null)
		{
			m_highlightState.ChangeState(stateType);
		}
	}

	public void Activate(bool enable)
	{
		SetEnabled(enable);
	}

	public virtual void Lock()
	{
		m_locked = true;
	}

	public virtual void Unlock()
	{
		m_locked = false;
	}

	public bool IsLocked()
	{
		return m_locked;
	}

	public TAG_PREMIUM GetPremium()
	{
		return m_premium;
	}

	public void SetPremium(TAG_PREMIUM premium)
	{
		m_premium = premium;
		UpdatePortrait();
	}

	public HeroPickerOptionDataModel GetDataModel()
	{
		WidgetTemplate widget = GetComponent<WidgetTemplate>();
		IDataModel dataModel = null;
		if (widget != null && !widget.GetDataModel(6, out dataModel))
		{
			dataModel = new HeroPickerOptionDataModel();
			widget.BindDataModel(dataModel);
		}
		return dataModel as HeroPickerOptionDataModel;
	}

	protected Material GetClassIconMaterial(TAG_CLASS classTag)
	{
		int index = 0;
		switch (classTag)
		{
		case TAG_CLASS.DRUID:
			index = 5;
			break;
		case TAG_CLASS.HUNTER:
			index = 4;
			break;
		case TAG_CLASS.MAGE:
			index = 7;
			break;
		case TAG_CLASS.PALADIN:
			index = 3;
			break;
		case TAG_CLASS.PRIEST:
			index = 8;
			break;
		case TAG_CLASS.ROGUE:
			index = 2;
			break;
		case TAG_CLASS.SHAMAN:
			index = 1;
			break;
		case TAG_CLASS.WARLOCK:
			index = 6;
			break;
		case TAG_CLASS.WARRIOR:
			index = 0;
			break;
		case TAG_CLASS.DEMONHUNTER:
			index = 9;
			break;
		case TAG_CLASS.DEATHKNIGHT:
			index = 10;
			break;
		case TAG_CLASS.INVALID:
		case TAG_CLASS.NEUTRAL:
			index = 11;
			break;
		}
		return CLASS_MATERIALS[index];
	}

	protected virtual void UpdatePortrait()
	{
		if (UpdateLegendaryHeroPortrait())
		{
			return;
		}
		CardDef cardDef = m_fullDef?.CardDef;
		if (cardDef == null)
		{
			return;
		}
		Material portraitMaterial = cardDef.GetDeckPickerPortrait();
		if (portraitMaterial == null)
		{
			return;
		}
		DeckPickerHero deckPickerHero = GetComponent<DeckPickerHero>();
		Renderer portraitMeshRenderer = deckPickerHero.m_PortraitMesh.GetComponent<Renderer>();
		List<Material> materials = portraitMeshRenderer.GetMaterials();
		Material goldenMaterial = cardDef.GetPremiumPortraitMaterial();
		if (m_premium == TAG_PREMIUM.GOLDEN && goldenMaterial != null)
		{
			materials[deckPickerHero.m_PortraitMaterialIndex] = UnityEngine.Object.Instantiate(goldenMaterial);
			materials[deckPickerHero.m_PortraitMaterialIndex].mainTextureOffset = portraitMaterial.mainTextureOffset;
			materials[deckPickerHero.m_PortraitMaterialIndex].mainTextureScale = portraitMaterial.mainTextureScale;
			materials[deckPickerHero.m_PortraitMaterialIndex].SetTexture("_ShadowTex", null);
			if (!m_seed.HasValue)
			{
				m_seed = UnityEngine.Random.value;
			}
			Material deckPickerMaterial = portraitMeshRenderer.GetMaterial();
			if (deckPickerMaterial.HasProperty("_Seed"))
			{
				deckPickerMaterial.SetFloat("_Seed", m_seed.Value);
			}
		}
		else
		{
			Material cachedMaterial = GetCachedMaterial(deckPickerHero.m_PortraitMaterialIndex);
			if (cachedMaterial != null)
			{
				materials[deckPickerHero.m_PortraitMaterialIndex] = UnityEngine.Object.Instantiate(cachedMaterial);
			}
			materials[deckPickerHero.m_PortraitMaterialIndex] = portraitMaterial;
		}
		portraitMeshRenderer.SetMaterials(materials);
		UberShaderAnimation portraitAnimation = cardDef.GetPortraitAnimation(m_premium);
		if (portraitAnimation != null)
		{
			UberShaderController uberController = deckPickerHero.m_PortraitMesh.GetComponent<UberShaderController>();
			if (uberController == null)
			{
				uberController = deckPickerHero.m_PortraitMesh.AddComponent<UberShaderController>();
			}
			uberController.UberShaderAnimation = UnityEngine.Object.Instantiate(portraitAnimation);
			uberController.m_MaterialIndex = 0;
		}
	}

	protected bool UpdateLegendaryHeroPortrait()
	{
		if (m_fullDef?.CardDef == null)
		{
			DestroyLegendaryHeroPortrait();
			UnloadCustomFrame();
			return false;
		}
		if (string.IsNullOrEmpty(m_fullDef.CardDef.m_CustomHeroFramePrefab))
		{
			DestroyLegendaryHeroPortrait();
			UnloadCustomFrame();
			return false;
		}
		UpdateLegendaryCardArt(m_fullDef.CardDef);
		LoadCustomFrame(m_fullDef.CardDef);
		OverrideTransformForCustomFrame(m_fullDef.EntityDef);
		if (m_legendaryHeroPortrait == null)
		{
			return false;
		}
		return true;
	}

	private void LoadCustomFrame(CardDef cardDef)
	{
		if (cardDef != null && !string.IsNullOrEmpty(cardDef.m_CustomHeroFramePrefab))
		{
			AssetReference assetPath = cardDef.m_CustomHeroFramePrefab;
			if (m_customFrameController == null || m_customFrameController.FrameAssetReference != assetPath)
			{
				UnloadCustomFrame();
				IAssetLoader assetLoader = AssetLoader.Get();
				if (assetLoader != null)
				{
					using (AssetHandle<GameObject> handle = assetLoader.GetOrInstantiateSharedPrefab(assetPath))
					{
						OnCustomFrameLoaded(assetPath, handle, null);
					}
				}
			}
			else if (m_customFrameController != null)
			{
				ApplyCustomFrame();
			}
		}
		else
		{
			UnloadCustomFrame();
		}
	}

	private void OverrideTransformForCustomFrame(EntityDef entityDef)
	{
		if (!(m_buttonFrame == null) && (m_customFrameController == null || !m_customFrameController.UseMetaCalibration))
		{
			m_buttonFrame.transform.localScale = (RewardUtils.IsShopPremiumHeroSkin(entityDef) ? m_diamondHeroScale : Vector3.one);
		}
	}

	private void UnloadCustomFrame()
	{
		if (m_customFrameController != null)
		{
			m_customFrameController.RestoreMeshAndMaterials(ref m_buttonFrame);
		}
		if (m_heroClassIconOffset != null)
		{
			m_heroClassIconOffset.transform.localPosition = Vector3.zero;
		}
	}

	private void DestroyCustomFrame()
	{
		UnloadCustomFrame();
		if (m_customFrameController != null)
		{
			((IDisposable)m_customFrameController).Dispose();
			m_customFrameController = null;
		}
	}

	private void ApplyCustomFrame()
	{
		if (m_customFrameController == null)
		{
			return;
		}
		m_customFrameController.ApplyCustomMeshAndMaterials(out m_buttonFrame);
		m_customFrameController.UpdateCustomDiamondMaterial();
		m_customFrameController.ApplyMetaFrameOffsets(m_buttonFrame.transform);
		if (m_heroClassIconOffset != null)
		{
			m_heroClassIconOffset.transform.localPosition = new Vector3(0f, 0f - m_customFrameController.HeroClassIconOffset, 0f);
		}
		Material portraitMaterial = m_buttonFrame.GetComponent<Renderer>().GetSharedMaterial(m_customFrameController.PortraitMatIdx);
		if (m_legendaryHeroPortrait != null)
		{
			portraitMaterial.mainTexture = m_legendaryHeroPortrait.PortraitTexture;
			ConnectLegendarySkinToDynamicResolutionController();
			return;
		}
		CardDef cardDef = m_fullDef?.CardDef;
		if (cardDef != null)
		{
			Material deckPickerPortraitMaterial = cardDef.GetDeckPickerPortrait();
			if (deckPickerPortraitMaterial != null)
			{
				portraitMaterial.mainTexture = deckPickerPortraitMaterial.mainTexture;
				portraitMaterial.mainTextureOffset = deckPickerPortraitMaterial.mainTextureOffset;
				portraitMaterial.mainTextureScale = deckPickerPortraitMaterial.mainTextureScale;
			}
		}
	}

	private void OnCustomFrameLoaded(AssetReference assetRef, AssetHandle<GameObject> go, object callbackData)
	{
		using (go)
		{
			if (go == null || go.Asset == null)
			{
				Debug.LogError($"{assetRef} - HeroPickerButton.OnCustomFrameLoaded() - failed to load legendary hero skin! GameObject = null!");
				return;
			}
			if (go.Asset.GetComponent<CustomFrameDef>() == null)
			{
				Debug.LogError($"{assetRef} - HeroPickerButton.OnCustomFrameLoaded() - failed to load legendary hero skin! CustomFrameDef = null!");
				return;
			}
			if (m_customFrameController == null)
			{
				m_customFrameController = new CustomFrameController(m_buttonFrame);
			}
			m_customFrameController.SetAssetHandle(assetRef, go);
			m_customFrameController.CacheHighlightState(GetComponentInChildren<HighlightState>());
			ApplyCustomFrame();
		}
	}

	protected override void OnRelease()
	{
		Lower();
	}

	protected void ReleaseFullDef()
	{
		m_fullDef?.Dispose();
		m_fullDef = null;
	}

	public void SetDivotTexture(Texture texture)
	{
		GetComponent<DeckPickerHero>().m_DivotMesh.GetMaterial().mainTexture = texture;
	}

	public void SetDivotVisible(bool visible)
	{
		GetComponent<DeckPickerHero>().m_DivotMesh.gameObject.SetActive(visible);
	}

	protected Material GetCachedMaterial(int materialIdx)
	{
		if (m_cachedMaterials != null && materialIdx < m_cachedMaterials.Count)
		{
			return m_cachedMaterials[materialIdx];
		}
		return null;
	}

	public void UpdateLegendaryCardArt(CardDef cardDef)
	{
		if (cardDef == null)
		{
			return;
		}
		IGraphicsManager graphicsManager = ServiceManager.Get<IGraphicsManager>();
		if (graphicsManager != null && graphicsManager.isVeryLowQualityDevice())
		{
			return;
		}
		string expectedLegendaryModel = cardDef.m_LegendaryModel;
		if (!string.IsNullOrEmpty(expectedLegendaryModel))
		{
			if (m_legendaryHeroPortrait != null && !m_legendaryHeroPortrait.IsValidForPath(expectedLegendaryModel, Player.Side.NEUTRAL))
			{
				DestroyLegendaryHeroPortrait();
			}
			if (m_legendaryHeroPortrait == null)
			{
				LegendaryHeroRenderToTextureService service = ServiceManager.Get<LegendaryHeroRenderToTextureService>();
				if (service != null)
				{
					m_legendaryHeroPortrait = service.CreatePortrait(expectedLegendaryModel, Player.Side.NEUTRAL);
				}
			}
		}
		else
		{
			DestroyLegendaryHeroPortrait();
		}
	}

	private void DestroyLegendaryHeroPortrait()
	{
		if (m_legendaryHeroPortrait != null)
		{
			m_legendaryHeroPortrait.Dispose();
			m_legendaryHeroPortrait = null;
		}
	}

	private void ConnectLegendarySkinToDynamicResolutionController()
	{
		if (m_customFrameController != null)
		{
			LegendarySkinDynamicResController controller = m_customFrameController.DynamicResolutionController;
			if (m_legendaryHeroPortrait != null)
			{
				m_legendaryHeroPortrait.ConnectDynamicResolutionController(controller);
			}
			else
			{
				controller.Skin = null;
			}
		}
	}
}
