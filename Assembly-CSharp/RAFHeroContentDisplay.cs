using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class RAFHeroContentDisplay : MonoBehaviour
{
	private const string c_keyArtFadeAnim = "HeroSkinArt_WipeAway";

	private const string c_keyArtAppearAnim = "HeroSkinArtGlowIn";

	[CustomEditField(Sections = "Display Frame")]
	public GameObject m_renderArtQuad;

	[CustomEditField(Sections = "Display Frame")]
	public UIBButton m_previewToggle;

	[CustomEditField(Sections = "Display Frame")]
	public int m_backgroundMaterialIndex;

	[CustomEditField(Sections = "Display Frame")]
	private Texture m_defaultBackgroundTexture;

	[CustomEditField(Sections = "Display Frame")]
	public GameObject m_heroContainer;

	[CustomEditField(Sections = "Display Frame")]
	public GameObject m_heroPowerContainer;

	[CustomEditField(Sections = "Display Frame")]
	public GameObject m_cardBackContainer;

	[CustomEditField(Sections = "Display Frame")]
	public GameObject m_previewButtonFX;

	[CustomEditField(Sections = "Hero Display")]
	public UberText m_heroName;

	[CustomEditField(Sections = "Hero Display")]
	public UberText m_className;

	[CustomEditField(Sections = "Hero Display")]
	public MeshRenderer m_classIcon;

	[CustomEditField(Sections = "Hero Display")]
	public MeshRenderer m_fauxPlateTexture;

	[CustomEditField(Sections = "Hero Display")]
	public MeshRenderer m_backgroundFrame;

	[CustomEditField(Sections = "Hero Display")]
	public Animator m_keyArtAnimation;

	[CustomEditField(Sections = "Hero Display")]
	public MeshRenderer m_renderQuad;

	[CustomEditField(Sections = "Hero Display")]
	public RenderToTexture m_renderToTexture;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Sound")]
	public string m_keyArtFadeSound;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Sound")]
	public string m_keyArtAppearSound;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Sound")]
	public string m_previewButtonClickSound;

	private GameObject m_cardBack;

	private Actor m_heroActor;

	private Actor m_heroPowerActor;

	private bool m_keyArtShowing = true;

	private CardSoundSpell m_previewEmote;

	private CardSoundSpell m_purchaseEmote;

	private MeshRenderer m_keyArt;

	private void Awake()
	{
		if (m_defaultBackgroundTexture == null && m_backgroundFrame != null && m_backgroundMaterialIndex >= 0 && m_backgroundMaterialIndex < m_backgroundFrame.GetMaterials().Count)
		{
			m_defaultBackgroundTexture = m_backgroundFrame.GetMaterial(m_backgroundMaterialIndex).GetTexture("_MainTex");
		}
		m_previewToggle.AddEventListener(UIEventType.RELEASE, delegate
		{
			TogglePreview();
		});
	}

	public void SetKeyArtRenderer(MeshRenderer keyArtRenderer)
	{
		m_keyArt = keyArtRenderer;
	}

	public void PlayPreviewEmote()
	{
		if (!(m_previewEmote == null) && !(Box.Get() == null) && !(Box.Get().GetCamera() == null))
		{
			m_previewEmote.SetPosition(Box.Get().GetCamera().transform.position);
			m_previewEmote.Reactivate();
		}
	}

	public void Init()
	{
		if (m_heroActor == null)
		{
			GameObject obj = AssetLoader.Get().InstantiatePrefab("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", AssetLoadingOptions.IgnorePrefabPosition);
			m_heroActor = obj.GetComponent<Actor>();
			m_heroActor.SetUnlit();
			m_heroActor.Show();
			m_heroActor.GetHealthObject().Hide();
			m_heroActor.GetAttackObject().Hide();
			GameUtils.SetParent(m_heroActor, m_heroContainer, withRotation: true);
			LayerUtils.SetLayer(m_heroActor, m_heroContainer.layer);
		}
		if (m_heroPowerActor == null)
		{
			GameObject obj2 = AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", AssetLoadingOptions.IgnorePrefabPosition);
			m_heroPowerActor = obj2.GetComponent<Actor>();
			m_heroPowerActor.SetUnlit();
			m_heroPowerActor.Show();
			GameUtils.SetParent(m_heroPowerActor, m_heroPowerContainer, withRotation: true);
			LayerUtils.SetLayer(m_heroPowerActor, m_heroPowerContainer.layer);
		}
	}

	public void UpdateFrame(CardHeroDbfRecord cardHeroDbfRecord, int cardBackIdx, CollectionHeroDef heroDef)
	{
		Init();
		if (heroDef.m_fauxPlateTexture != null)
		{
			m_fauxPlateTexture.GetMaterial().SetTexture("_MainTex", heroDef.m_fauxPlateTexture);
		}
		m_keyArt.SetMaterial(heroDef.m_previewMaterial.GetMaterial());
		string animationAssetPath = heroDef.GetHeroUberShaderAnimationPath();
		if (!string.IsNullOrEmpty(animationAssetPath))
		{
			UberShaderAnimation materialAnim = (AssetLoader.Get() as AssetLoader).LoadUberAnimation(animationAssetPath, usePrefabPosition: false);
			if (materialAnim == null)
			{
				Error.AddDevFatal("Failed to load animation {0} for {1}", animationAssetPath, heroDef);
			}
			else
			{
				UberShaderController uberShaderController = m_keyArt.GetComponent<UberShaderController>();
				if (uberShaderController == null)
				{
					uberShaderController = m_keyArt.gameObject.AddComponent<UberShaderController>();
				}
				uberShaderController.UberShaderAnimation = materialAnim;
				uberShaderController.m_MaterialIndex = 0;
			}
		}
		DefLoader.Get().LoadFullDef(GameUtils.TranslateDbIdToCardId(cardHeroDbfRecord.CardId), delegate(string heroCardId, DefLoader.DisposableFullDef heroFullDef, object data1)
		{
			using (heroFullDef)
			{
				m_heroActor.SetPremium(TAG_PREMIUM.NORMAL);
				m_heroActor.SetFullDef(heroFullDef);
				m_heroActor.UpdateAllComponents();
				m_heroActor.Hide();
				m_heroName.Text = heroFullDef.EntityDef.GetName();
				m_className.Text = GameStrings.GetClassName(heroFullDef.EntityDef.GetClass());
				string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(heroCardId);
				DefLoader.Get().LoadFullDef(heroPowerCardIdFromHero, delegate(string powerCardId, DefLoader.DisposableFullDef powerDef, object data2)
				{
					using (powerDef)
					{
						m_heroPowerActor.SetPremium(TAG_PREMIUM.GOLDEN);
						m_heroPowerActor.SetFullDef(powerDef);
						m_heroPowerActor.UpdateAllComponents();
						m_heroPowerActor.Hide();
					}
				});
				if (CollectionPageManager.s_classTextureOffsets.TryGetValue(heroFullDef.EntityDef.GetClass(), out var value))
				{
					m_classIcon.GetMaterial().SetTextureOffset("_MainTex", value);
				}
				ClearEmotes();
				if (heroDef.m_storePreviewEmote != 0)
				{
					GameUtils.LoadCardDefEmoteSound(heroFullDef.CardDef.m_EmoteDefs, heroDef.m_storePreviewEmote, delegate(CardSoundSpell spell)
					{
						if (!(spell == null))
						{
							m_previewEmote = spell;
							GameUtils.SetParent(m_previewEmote, this);
						}
					});
				}
				if (heroDef.m_storePurchaseEmote != 0)
				{
					GameUtils.LoadCardDefEmoteSound(heroFullDef.CardDef.m_EmoteDefs, heroDef.m_storePurchaseEmote, delegate(CardSoundSpell spell)
					{
						if (!(spell == null))
						{
							m_purchaseEmote = spell;
							GameUtils.SetParent(m_purchaseEmote, this);
						}
					});
				}
			}
		});
		if (m_cardBack != null)
		{
			Object.Destroy(m_cardBack);
			m_cardBack = null;
		}
		if (cardBackIdx != 0)
		{
			CardBackManager.Get().LoadCardBackByIndex(cardBackIdx, delegate(CardBackManager.LoadCardBackData cardBackData)
			{
				GameObject gameObject = cardBackData.m_GameObject;
				gameObject.name = "CARD_BACK_" + cardBackIdx;
				m_cardBack = gameObject;
				LayerUtils.SetLayer(gameObject, m_cardBackContainer.gameObject.layer, null);
				GameUtils.SetParent(gameObject, m_cardBackContainer);
				m_cardBack.transform.localPosition = Vector3.zero;
				m_cardBack.transform.localScale = Vector3.one;
				m_cardBack.transform.localRotation = Quaternion.identity;
				AnimationUtil.FloatyPosition(m_cardBack, 0.05f, 10f);
			});
		}
		if (!(m_backgroundFrame != null) || m_backgroundMaterialIndex < 0 || m_backgroundMaterialIndex > m_backgroundFrame.GetMaterials().Count)
		{
			return;
		}
		Texture bgTexture = m_defaultBackgroundTexture;
		if (!string.IsNullOrEmpty(cardHeroDbfRecord.StoreBackgroundTexture))
		{
			Texture texture = AssetLoader.Get().LoadTexture(cardHeroDbfRecord.StoreBackgroundTexture);
			if (texture != null)
			{
				bgTexture = texture;
			}
		}
		if (bgTexture != null)
		{
			m_backgroundFrame.GetMaterial(m_backgroundMaterialIndex).SetTexture("_MainTex", bgTexture);
		}
	}

	public void TogglePreview()
	{
		if (!string.IsNullOrEmpty(m_previewButtonClickSound))
		{
			SoundManager.Get().LoadAndPlay(m_previewButtonClickSound);
		}
		PlayKeyArtAnimation(m_keyArtShowing);
		m_keyArtShowing = !m_keyArtShowing;
		if (!m_keyArtShowing)
		{
			m_heroActor.Show();
			m_heroPowerActor.Show();
			PlayPreviewEmote();
		}
		else
		{
			m_heroActor.Hide();
			m_heroPowerActor.Hide();
		}
	}

	public void ResetPreview()
	{
		m_keyArtShowing = true;
		m_keyArtAnimation.enabled = true;
		m_keyArtAnimation.StopPlayback();
		m_keyArtAnimation.Play("HeroSkinArtGlowIn", -1, 1f);
		m_previewButtonFX.SetActive(value: false);
	}

	private void PlayKeyArtAnimation(bool showPreview)
	{
		string animName = (showPreview ? "HeroSkinArt_WipeAway" : "HeroSkinArtGlowIn");
		string soundFile = (showPreview ? m_keyArtFadeSound : m_keyArtAppearSound);
		m_previewButtonFX.SetActive(showPreview);
		if (!string.IsNullOrEmpty(soundFile))
		{
			SoundManager.Get().LoadAndPlay(soundFile);
		}
		m_keyArtAnimation.enabled = true;
		m_keyArtAnimation.StopPlayback();
		m_keyArtAnimation.Play(animName, -1, 0f);
	}

	private void ClearEmotes()
	{
		if (m_previewEmote != null)
		{
			Object.Destroy(m_previewEmote.gameObject);
		}
		if (m_purchaseEmote != null)
		{
			Object.Destroy(m_purchaseEmote.gameObject);
		}
	}
}
