using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class MassDisenchant : MonoBehaviour
{
	public GameObject m_root;

	public GameObject m_disenchantContainer;

	public MassDisenchantFX m_FX;

	public MassDisenchantSound m_sound;

	public UberText m_headlineText;

	public UberText m_detailsHeadlineText;

	public UberText m_detailsText;

	public UberText m_totalAmountText;

	public NormalButton m_disenchantButton;

	public UberText m_singleSubHeadlineText;

	public UberText m_doubleSubHeadlineText;

	public GameObject m_singleRoot;

	public GameObject m_doubleRoot;

	public List<DisenchantBar> m_singleDisenchantBars;

	public List<DisenchantBar> m_doubleDisenchantBars;

	public UIBButton m_infoButton;

	public Material m_rarityBarNormalMaterial;

	public Material m_rarityBarGoldMaterial;

	public Mesh m_rarityBarNormalMesh;

	public Mesh m_rarityBarGoldMesh;

	private bool m_useSingle = true;

	private int m_totalAmount;

	private int m_totalCardsToDisenchant;

	private Vector3 m_origTotalScale;

	private Vector3 m_origDustScale;

	private int m_highestGlowBalls;

	private List<GameObject> m_cleanupObjects = new List<GameObject>();

	private long m_preMassDisenchantDustValue;

	private IGraphicsManager m_graphicsManager;

	private static MassDisenchant s_Instance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		s_Instance = this;
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_headlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_HEADLINE");
		m_detailsHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS_HEADLINE");
		m_disenchantButton.SetText(GameStrings.Get("GLUE_MASS_DISENCHANT_BUTTON_TEXT"));
		if (m_detailsText != null)
		{
			m_detailsText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS");
		}
		if (m_singleSubHeadlineText != null)
		{
			m_singleSubHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_SUB_HEADLINE_TEXT");
		}
		if (m_doubleSubHeadlineText != null)
		{
			m_doubleSubHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_SUB_HEADLINE_TEXT");
		}
		m_disenchantButton.SetUserOverYOffset(-0.04409015f);
		foreach (DisenchantBar singleDisenchantBar in m_singleDisenchantBars)
		{
			singleDisenchantBar.Init();
		}
		foreach (DisenchantBar doubleDisenchantBar in m_doubleDisenchantBars)
		{
			doubleDisenchantBar.Init();
		}
		CollectionManager.Get().RegisterMassDisenchantListener(OnMassDisenchant);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Start()
	{
		m_disenchantButton.AddEventListener(UIEventType.RELEASE, OnDisenchantButtonPressed);
		m_disenchantButton.AddEventListener(UIEventType.ROLLOVER, OnDisenchantButtonOver);
		m_disenchantButton.AddEventListener(UIEventType.ROLLOUT, OnDisenchantButtonOut);
		if (m_infoButton != null)
		{
			m_infoButton.AddEventListener(UIEventType.RELEASE, OnInfoButtonPressed);
		}
	}

	public static MassDisenchant Get()
	{
		return s_Instance;
	}

	public void Show()
	{
		m_root.SetActive(value: true);
	}

	public void Hide()
	{
		m_root.SetActive(value: false);
		BlockCurrencyFrame(block: false);
	}

	public bool IsShown()
	{
		return m_root.activeSelf;
	}

	private void OnDestroy()
	{
		foreach (GameObject cleanup in m_cleanupObjects)
		{
			if (cleanup != null)
			{
				UnityEngine.Object.Destroy(cleanup);
			}
		}
		CollectionManager.Get().RemoveMassDisenchantListener(OnMassDisenchant);
		BlockCurrencyFrame(block: false);
	}

	public int GetTotalAmount()
	{
		return m_totalAmount;
	}

	public void UpdateContents(List<CollectibleCard> disenchantCards)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_useSingle = true;
		}
		else
		{
			m_useSingle = true;
			foreach (CollectibleCard disenchantCard in disenchantCards)
			{
				if (disenchantCard.PremiumType == TAG_PREMIUM.GOLDEN)
				{
					m_useSingle = false;
					break;
				}
			}
		}
		List<DisenchantBar> disenchantBars = (m_useSingle ? m_singleDisenchantBars : m_doubleDisenchantBars);
		foreach (DisenchantBar item in disenchantBars)
		{
			item.Reset();
		}
		CraftingManager.Get();
		DefLoader defLoader = DefLoader.Get();
		m_totalAmount = 0;
		m_totalCardsToDisenchant = 0;
		foreach (CollectibleCard card in disenchantCards)
		{
			string cardId = card.CardId;
			TAG_PREMIUM premiumType = card.PremiumType;
			NetCache.CardValue cardValue = CraftingManager.GetCardValue(cardId, premiumType);
			if (cardValue != null)
			{
				EntityDef entityDef = defLoader.GetEntityDef(cardId);
				int disenchantCount = card.IsCraftableDisenchantCount;
				int stackSellValue = cardValue.GetSellValue() * disenchantCount;
				DisenchantBar bar = FindDisenchantBar(disenchantBars, premiumType, entityDef.GetRarity());
				if (bar == null)
				{
					Debug.LogWarning(string.Format("MassDisenchant.UpdateContents(): Could not find {0} bar to modify for card {1} (premium {2}, disenchant count {3})", m_useSingle ? "single" : "double", entityDef, premiumType, disenchantCount));
				}
				else
				{
					bar.AddCards(disenchantCount, stackSellValue, premiumType);
					m_totalCardsToDisenchant += disenchantCount;
					m_totalAmount += stackSellValue;
				}
			}
		}
		if (m_totalAmount > 0)
		{
			m_singleRoot.SetActive(m_useSingle);
			if (m_doubleRoot != null)
			{
				m_doubleRoot.SetActive(!m_useSingle);
			}
			m_disenchantButton.SetEnabled(enabled: true);
		}
		foreach (DisenchantBar item2 in disenchantBars)
		{
			item2.UpdateVisuals(m_totalCardsToDisenchant);
		}
		m_totalAmountText.Text = GameStrings.Format("GLUE_MASS_DISENCHANT_TOTAL_AMOUNT", m_totalAmount);
	}

	private DisenchantBar FindDisenchantBar(List<DisenchantBar> disenchantBars, TAG_PREMIUM premiumType, TAG_RARITY rarity)
	{
		int i = 0;
		for (int iMax = disenchantBars.Count; i < iMax; i++)
		{
			DisenchantBar disenchantBar = disenchantBars[i];
			if ((disenchantBar.m_premiumType == premiumType || (bool)UniversalInputManager.UsePhoneUI) && disenchantBar.m_rarity == rarity)
			{
				return disenchantBar;
			}
		}
		return null;
	}

	public IEnumerator StartHighlight()
	{
		yield return null;
		m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
	}

	public void OnMassDisenchant(int amount)
	{
		int maxGlowBalls = 10;
		maxGlowBalls = m_graphicsManager.RenderQualityLevel switch
		{
			GraphicsQuality.Low => 3, 
			GraphicsQuality.Medium => 6, 
			_ => 10, 
		};
		BlockUI();
		StartCoroutine(DoDisenchantAnims(maxGlowBalls, amount));
	}

	private void BlockCurrencyFrame(bool block)
	{
		BnetBar bnetBar = BnetBar.Get();
		if (!(bnetBar == null))
		{
			bnetBar.SetBlockCurrencyFrames(block);
		}
	}

	private void BlockUI(bool block = true)
	{
		BlockCurrencyFrame(block);
		m_FX.m_blockInteraction.SetActive(block);
	}

	private void OnDisenchantButtonOver(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			SoundManager.Get().LoadAndPlay("Hub_Mouseover.prefab:40130da7b734190479c527d6bca1a4a8");
		}
		else
		{
			m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	private void OnDisenchantButtonOut(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		else
		{
			m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	private void OnDisenchantButtonPressed(UIEvent e)
	{
		Options.Get().SetBool(Option.HAS_DISENCHANTED, val: true);
		m_disenchantButton.SetEnabled(enabled: false);
		m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		BlockCurrencyFrame(block: true);
		m_preMassDisenchantDustValue = NetCache.Get().GetArcaneDustBalance();
		Network.Get().MassDisenchant();
	}

	private void OnInfoButtonPressed(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_MASS_DISENCHANT_BUTTON_TEXT");
		info.m_text = string.Format("{0}\n\n{1}", GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS_HEADLINE"), GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS"));
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	private void Unbloomify(List<GameObject> glows, float newVal)
	{
		foreach (GameObject glow in glows)
		{
			glow.GetComponent<RenderToTexture>().m_BloomIntensity = newVal;
		}
	}

	private void UncolorTotal(float newVal)
	{
		m_totalAmountText.TextColor = Color.Lerp(Color.white, new Color(0.7f, 0.85f, 1f, 1f), newVal);
	}

	private void SetGemSaturation(List<DisenchantBar> disenchantBars, float saturation, bool onlyActive = false, bool onlyInactive = false)
	{
		foreach (DisenchantBar bar in disenchantBars)
		{
			int numCards = bar.GetNumCards();
			if ((onlyActive && numCards != 0) || (onlyInactive && numCards == 0) || (!onlyInactive && !onlyActive))
			{
				bar.m_rarityGem.GetComponent<Renderer>().GetMaterial().SetColor("_Fade", new Color(saturation, saturation, saturation, 1f));
			}
		}
	}

	private IEnumerator DoDisenchantAnims(int maxGlowBalls, int disenchantTotal)
	{
		if (disenchantTotal == 0)
		{
			yield return null;
		}
		m_origTotalScale = m_totalAmountText.transform.localScale;
		CurrencyFrame currencyFrame;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_origDustScale = ArcaneDustAmount.Get().m_dustJar.transform.localScale;
		}
		else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame))
		{
			m_origDustScale = currencyFrame.CurrencyIconContainer.transform.localScale;
		}
		List<DisenchantBar> disenchantBars = (m_useSingle ? m_singleDisenchantBars : m_doubleDisenchantBars);
		float vigTime = 0.2f;
		float time = vigTime;
		VignetteParameters? vignette = new VignetteParameters(0.8f);
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, time, iTween.EaseType.easeOutCirc, null, vignette, null, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		iTween.ValueTo(base.gameObject, iTween.Hash("from", 1f, "to", 0.3f, "time", vigTime, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGemSaturation(disenchantBars, (float)newVal);
		}));
		if (m_sound.m_intro != string.Empty)
		{
			SoundManager.Get().LoadAndPlay(m_sound.m_intro);
		}
		yield return new WaitForSeconds(vigTime);
		float duration = 0.5f;
		float rate = duration / 20f;
		iTween.ValueTo(base.gameObject, iTween.Hash("from", 0.3f, "to", 1.75f, "time", 1.5f * duration, "easeintype", iTween.EaseType.easeInCubic, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGemSaturation(disenchantBars, (float)newVal, onlyActive: true);
		}));
		List<GameObject> glows = new List<GameObject>();
		if (m_FX.m_glowTotal != null)
		{
			glows.Add(m_FX.m_glowTotal);
		}
		m_totalAmountText.transform.localScale = m_origTotalScale * 2.54f;
		iTween.ScaleTo(m_totalAmountText.gameObject, iTween.Hash("scale", m_origTotalScale, "time", 3f));
		if (m_FX.m_glowTotal != null)
		{
			m_FX.m_glowTotal.SetActive(value: true);
		}
		m_highestGlowBalls = 0;
		Color glowColor = new Color(0.7f, 0.85f, 1f, 1f);
		float origYSpeed = 0f;
		float origXSpeed = 0f;
		float origInten = 0f;
		foreach (DisenchantBar item in disenchantBars)
		{
			int numCards = item.GetNumCards();
			if (numCards > m_highestGlowBalls)
			{
				m_highestGlowBalls = numCards;
			}
		}
		m_highestGlowBalls = ((m_highestGlowBalls > maxGlowBalls) ? maxGlowBalls : m_highestGlowBalls);
		foreach (DisenchantBar bar in disenchantBars)
		{
			int numCards2 = bar.GetNumCards();
			if (numCards2 > 0)
			{
				RarityFX rareFX = GetRarityFX(bar);
				int maxBalls = ((numCards2 > maxGlowBalls) ? maxGlowBalls : numCards2);
				for (int i = 0; i < maxBalls; i++)
				{
					StartCoroutine(LaunchGlowball(bar, rareFX, i, maxBalls, m_highestGlowBalls));
				}
			}
		}
		int i2 = 0;
		while ((float)i2 < duration / rate)
		{
			float curDustTotal = 0f;
			foreach (DisenchantBar bar2 in disenchantBars)
			{
				RaritySound rareSound = GetRaritySound(bar2);
				int numCards3 = bar2.GetNumCards();
				if (i2 == 0 && numCards3 != 0)
				{
					if (rareSound.m_drainSound != string.Empty)
					{
						SoundManager.Get().LoadAndPlay(rareSound.m_drainSound);
					}
					if (bar2.m_numGoldText != null && bar2.m_numGoldText.gameObject.activeSelf)
					{
						bar2.m_numGoldText.gameObject.SetActive(value: false);
						TransformUtil.SetLocalPosX(bar2.m_numCardsText, 2.902672f);
					}
					Vector3 textScale = bar2.m_numCardsText.gameObject.transform.localScale;
					iTween.ScaleFrom(bar2.m_numCardsText.gameObject, iTween.Hash("x", textScale.x * 2.28f, "y", textScale.y * 2.28f, "z", textScale.z * 2.28f, "time", 3f));
					bar2.m_numCardsText.TextColor = glowColor;
					iTween.ColorTo(bar2.m_numCardsText.gameObject, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "time", 3f));
					if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.High && bar2.m_glow != null)
					{
						glows.Add(bar2.m_glow);
						bar2.m_glow.GetComponent<RenderToTexture>().m_BloomIntensity = 0.01f;
						bar2.m_glow.SetActive(value: true);
					}
					Material rarityGemMaterial = bar2.m_rarityGem.GetComponent<Renderer>().GetMaterial();
					origYSpeed = rarityGemMaterial.GetFloat("_YSpeed");
					origXSpeed = rarityGemMaterial.GetFloat("_XSpeed");
					origInten = bar2.m_amountBar.GetComponent<Renderer>().GetMaterial().GetFloat("_Intensity");
					rarityGemMaterial.SetFloat("_YSpeed", -10f);
					rarityGemMaterial.SetFloat("_XSpeed", 20f);
				}
			}
			if (i2 == 0)
			{
				if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.High)
				{
					iTween.ValueTo(base.gameObject, iTween.Hash("from", 1f, "to", 0.1f, "time", duration * 3f, "onupdate", (Action<object>)delegate(object newVal)
					{
						Unbloomify(glows, (float)newVal);
					}));
				}
				iTween.ValueTo(base.gameObject, iTween.Hash("from", 1f, "to", 0.1f, "time", duration * 3f, "onupdate", (Action<object>)delegate(object newVal)
				{
					UncolorTotal((float)newVal);
				}));
				float curTotal = m_preMassDisenchantDustValue;
				iTween.ValueTo(base.gameObject, iTween.Hash("from", curTotal, "to", curTotal + (float)disenchantTotal, "time", 3f * duration, "onupdate", (Action<object>)delegate(object newVal)
				{
					SetDustBalanceVisual((float)newVal);
				}, "oncomplete", (Action<object>)delegate
				{
					(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager).UpdateMassDisenchant();
					m_screenEffectsHandle.StopEffect(vigTime);
					BlockUI(block: false);
				}));
			}
			foreach (DisenchantBar bar3 in disenchantBars)
			{
				if (bar3.GetNumCards() != 0)
				{
					bar3.m_amountBar.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", 2f);
					curDustTotal += DrainBarAndDust(bar3, i2, duration, rate);
				}
			}
			m_totalAmountText.Text = Convert.ToInt32(curDustTotal).ToString();
			yield return new WaitForSeconds(rate / duration);
			int num = i2 + 1;
			i2 = num;
		}
		if (m_FX.m_glowTotal != null)
		{
			m_FX.m_glowTotal.SetActive(value: false);
		}
		m_totalAmountText.Text = "0";
		m_totalAmountText.TextColor = Color.white;
		iTween.ValueTo(base.gameObject, iTween.Hash("from", 0.3f, "to", 1f, "time", duration, "delay", vigTime, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGemSaturation(disenchantBars, (float)newVal, onlyActive: false, onlyInactive: true);
		}));
		iTween.ValueTo(base.gameObject, iTween.Hash("from", 1.75f, "to", 1f, "time", duration, "delay", vigTime, "onupdate", (Action<object>)delegate(object newVal)
		{
			SetGemSaturation(disenchantBars, (float)newVal, onlyActive: true);
		}));
		foreach (DisenchantBar bar4 in disenchantBars)
		{
			if (bar4.m_glow != null)
			{
				bar4.m_glow.SetActive(value: false);
			}
			bar4.m_numCardsText.TextColor = Color.white;
			Material material = bar4.m_rarityGem.GetComponent<Renderer>().GetMaterial();
			material.SetFloat("_YSpeed", origYSpeed);
			material.SetFloat("_XSpeed", origXSpeed);
			bar4.m_amountBar.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", origInten);
		}
	}

	private void SetDustBalanceVisual(float bal)
	{
		int iBal = (int)bal;
		CurrencyManager currencyManager;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ArcaneDustAmount.Get().m_dustCount.Text = iBal.ToString();
		}
		else if (ServiceManager.TryGet<CurrencyManager>(out currencyManager))
		{
			currencyManager.GetPriceDataModel(CurrencyType.DUST).DisplayText = iBal.ToString();
		}
	}

	private float DrainBarAndDust(DisenchantBar bar, int drainRun, float duration, float rate)
	{
		float numCards = bar.GetNumCards();
		numCards -= (float)(drainRun + 1) * numCards / (duration / rate);
		if (numCards < 0f)
		{
			numCards = 0f;
		}
		float amountDust = bar.GetAmountDust();
		amountDust -= (float)(drainRun + 1) * amountDust / (duration / rate);
		if (amountDust < 0f)
		{
			amountDust = 0f;
		}
		bar.m_numCardsText.Text = Convert.ToInt32(numCards).ToString();
		bar.m_amountText.Text = Convert.ToInt32(amountDust).ToString();
		float percentFill = 0f;
		if (m_totalCardsToDisenchant > 0)
		{
			percentFill = numCards / (float)m_totalCardsToDisenchant;
		}
		bar.SetPercent(percentFill);
		return amountDust;
	}

	private Vector3 GetRanBoxPt(GameObject box)
	{
		Vector3 boxScale = box.transform.localScale;
		Vector3 position = box.transform.position;
		Vector3 randPos = new Vector3(UnityEngine.Random.Range((0f - boxScale.x) / 2f, boxScale.x / 2f), UnityEngine.Random.Range((0f - boxScale.y) / 2f, boxScale.y / 2f), UnityEngine.Random.Range((0f - boxScale.z) / 2f, boxScale.z / 2f));
		return position + randPos;
	}

	private IEnumerator LaunchGlowball(DisenchantBar bar, RarityFX rareFX, int glowBallNum, int totalGlowBalls, int m_highestGlowBalls)
	{
		float pad = 0.02f;
		float gNum = glowBallNum;
		float timeRange = (1f - pad * (float)m_highestGlowBalls) / (float)totalGlowBalls;
		float delayStart = gNum * timeRange + gNum * pad;
		float delayEnd = (gNum + 1f) * timeRange + (gNum + 1f) * pad;
		GameObject glowBall = UnityEngine.Object.Instantiate(m_FX.m_glowBall);
		m_cleanupObjects.Add(glowBall);
		glowBall.GetComponent<Renderer>().SetSharedMaterial(rareFX.glowBallMat);
		glowBall.GetComponent<TrailRenderer>().SetMaterial(rareFX.glowTrailMat);
		glowBall.transform.position = bar.m_rarityGem.transform.position;
		glowBall.transform.position = new Vector3(glowBall.transform.position.x, glowBall.transform.position.y + 0.5f, glowBall.transform.position.z);
		List<Vector3> curvePoints = new List<Vector3> { glowBall.transform.position };
		if ((double)UnityEngine.Random.Range(0f, 1f) < 0.5)
		{
			curvePoints.Add(GetRanBoxPt(m_FX.m_gemBoxLeft1));
			curvePoints.Add(GetRanBoxPt(m_FX.m_gemBoxLeft2));
		}
		else
		{
			curvePoints.Add(GetRanBoxPt(m_FX.m_gemBoxRight1));
			curvePoints.Add(GetRanBoxPt(m_FX.m_gemBoxRight2));
		}
		GameObject dustJar = null;
		CurrencyFrame currencyFrame;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			dustJar = ArcaneDustAmount.Get().m_dustJar;
			curvePoints.Add(dustJar.transform.position);
		}
		else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame))
		{
			dustJar = currencyFrame.CurrencyIconContainer;
			curvePoints.Add(Camera.main.ViewportToWorldPoint(BaseUI.Get().m_BnetCamera.WorldToViewportPoint(dustJar.transform.position)));
		}
		yield return new WaitForSeconds(UnityEngine.Random.Range(delayStart, delayEnd));
		RaritySound rareSound = GetRaritySound(bar);
		if (rareSound.m_missileSound != string.Empty)
		{
			SoundManager.Get().LoadAndPlay(rareSound.m_missileSound);
		}
		if (glowBallNum == 0)
		{
			GameObject burst = UnityEngine.Object.Instantiate(rareFX.burstFX);
			m_cleanupObjects.Add(burst);
			burst.transform.position = bar.m_rarityGem.transform.position;
			burst.GetComponent<ParticleSystem>().Play();
			UnityEngine.Object.Destroy(burst, 3f);
		}
		float glowBallTime = 0.4f;
		glowBall.SetActive(value: true);
		iTween.MoveTo(glowBall, iTween.Hash("path", curvePoints.ToArray(), "time", glowBallTime, "easetype", iTween.EaseType.linear));
		UnityEngine.Object.Destroy(glowBall, glowBallTime);
		yield return new WaitForSeconds(glowBallTime);
		if (rareSound.m_jarSound != string.Empty)
		{
			SoundManager.Get().LoadAndPlay(rareSound.m_jarSound);
		}
		GameObject dustFX = null;
		CurrencyFrame currencyFrame2;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			dustFX = ArcaneDustAmount.Get().m_dustFX;
		}
		else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame2))
		{
			dustFX = currencyFrame2.m_dustFX;
		}
		if (dustFX != null && UnityEngine.Random.Range(0f, 1f) > 0.7f)
		{
			GameObject receiveFX = UnityEngine.Object.Instantiate(dustFX);
			m_cleanupObjects.Add(receiveFX);
			receiveFX.transform.parent = dustFX.transform.parent;
			receiveFX.transform.localPosition = dustFX.transform.localPosition;
			receiveFX.transform.localScale = dustFX.transform.localScale;
			receiveFX.transform.localRotation = dustFX.transform.localRotation;
			receiveFX.SetActive(value: true);
			receiveFX.GetComponent<ParticleSystem>().Play();
			UnityEngine.Object.Destroy(receiveFX, 4.9f);
		}
		if (rareFX.explodeFX != null)
		{
			GameObject receiveBurstFX = UnityEngine.Object.Instantiate(rareFX.explodeFX);
			m_cleanupObjects.Add(receiveBurstFX);
			receiveBurstFX.transform.parent = rareFX.explodeFX.transform.parent;
			receiveBurstFX.transform.localPosition = rareFX.explodeFX.transform.localPosition;
			receiveBurstFX.transform.localScale = rareFX.explodeFX.transform.localScale;
			receiveBurstFX.transform.localRotation = rareFX.explodeFX.transform.localRotation;
			receiveBurstFX.SetActive(value: true);
			receiveBurstFX.GetComponent<ParticleSystem>().Play();
			UnityEngine.Object.Destroy(receiveBurstFX, 3f);
		}
		if (dustJar != null)
		{
			Vector3 dustScale = Vector3.Min(dustJar.transform.localScale * 1.2f, m_origDustScale * 3f);
			iTween.ScaleTo(dustJar, iTween.Hash("scale", dustScale, "time", 0.15f));
			iTween.ScaleTo(dustJar, iTween.Hash("scale", m_origDustScale, "delay", 0.1f, "time", 1f));
		}
		yield return null;
	}

	private RarityFX GetRarityFX(DisenchantBar bar)
	{
		RarityFX rareFX = default(RarityFX);
		switch (bar.m_rarity)
		{
		case TAG_RARITY.RARE:
		{
			rareFX.burstFX = m_FX.m_burstFX_Rare;
			CurrencyFrame currencyFrame3;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				rareFX.explodeFX = ArcaneDustAmount.Get().m_explodeFX_Rare;
			}
			else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame3))
			{
				rareFX.explodeFX = currencyFrame3.m_explodeFX_Rare;
			}
			rareFX.glowBallMat = m_FX.m_glowBallMat_Rare;
			rareFX.glowTrailMat = m_FX.m_glowTrailMat_Rare;
			break;
		}
		case TAG_RARITY.EPIC:
		{
			rareFX.burstFX = m_FX.m_burstFX_Epic;
			CurrencyFrame currencyFrame4;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				rareFX.explodeFX = ArcaneDustAmount.Get().m_explodeFX_Epic;
			}
			else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame4))
			{
				rareFX.explodeFX = currencyFrame4.m_explodeFX_Epic;
			}
			rareFX.glowBallMat = m_FX.m_glowBallMat_Epic;
			rareFX.glowTrailMat = m_FX.m_glowTrailMat_Epic;
			break;
		}
		case TAG_RARITY.LEGENDARY:
		{
			rareFX.burstFX = m_FX.m_burstFX_Legendary;
			CurrencyFrame currencyFrame2;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				rareFX.explodeFX = ArcaneDustAmount.Get().m_explodeFX_Legendary;
			}
			else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame2))
			{
				rareFX.explodeFX = currencyFrame2.m_explodeFX_Legendary;
			}
			rareFX.glowBallMat = m_FX.m_glowBallMat_Legendary;
			rareFX.glowTrailMat = m_FX.m_glowTrailMat_Legendary;
			break;
		}
		default:
		{
			rareFX.burstFX = m_FX.m_burstFX_Common;
			CurrencyFrame currencyFrame;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				rareFX.explodeFX = ArcaneDustAmount.Get().m_explodeFX_Legendary;
			}
			else if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.DUST, out currencyFrame))
			{
				rareFX.explodeFX = currencyFrame.m_explodeFX_Legendary;
			}
			rareFX.glowBallMat = m_FX.m_glowBallMat_Common;
			rareFX.glowTrailMat = m_FX.m_glowTrailMat_Common;
			break;
		}
		}
		return rareFX;
	}

	private RaritySound GetRaritySound(DisenchantBar bar)
	{
		RaritySound rareSound = new RaritySound();
		switch (bar.m_rarity)
		{
		case TAG_RARITY.RARE:
			rareSound.m_drainSound = m_sound.m_rare.m_drainSound;
			rareSound.m_jarSound = m_sound.m_rare.m_jarSound;
			rareSound.m_missileSound = m_sound.m_rare.m_missileSound;
			break;
		case TAG_RARITY.EPIC:
			rareSound.m_drainSound = m_sound.m_epic.m_drainSound;
			rareSound.m_jarSound = m_sound.m_epic.m_jarSound;
			rareSound.m_missileSound = m_sound.m_epic.m_missileSound;
			break;
		case TAG_RARITY.LEGENDARY:
			rareSound.m_drainSound = m_sound.m_legendary.m_drainSound;
			rareSound.m_jarSound = m_sound.m_legendary.m_jarSound;
			rareSound.m_missileSound = m_sound.m_legendary.m_missileSound;
			break;
		default:
			rareSound.m_drainSound = m_sound.m_common.m_drainSound;
			rareSound.m_jarSound = m_sound.m_common.m_jarSound;
			rareSound.m_missileSound = m_sound.m_common.m_missileSound;
			break;
		}
		return rareSound;
	}
}
