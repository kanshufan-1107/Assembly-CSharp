using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class StageTransistion : MonoBehaviour
{
	public GameObject hlBase;

	public GameObject hlEdge;

	public GameObject entireObj;

	public GameObject inplayObj;

	public GameObject rays;

	public GameObject flash;

	public GameObject fxEmitterA;

	public GameObject fxEmitterB;

	public float FxEmitterAKillTime = 1f;

	private Shader shaderBucket;

	private bool colorchange;

	private bool powerchange;

	private bool amountchange;

	private bool turnon;

	private bool rayschange;

	private bool flashchange;

	public Color endColor;

	public Color flashendColor;

	private int stage;

	public float RayTime = 10f;

	public float fxATime = 1f;

	public float FxEmitterAWaitTime = 1f;

	public float FxEmitterATimer = 2f;

	private bool FxStartAnim;

	private bool FxStartStop;

	private bool fxEmitterAScale;

	private bool raysdone;

	private Renderer m_hlBaseRenderer;

	private Renderer hlEdgeRenderer;

	private void Start()
	{
		rays.SetActive(value: false);
		flash.SetActive(value: false);
		entireObj.SetActive(value: true);
		inplayObj.SetActive(value: false);
		m_hlBaseRenderer = hlBase.GetComponent<Renderer>();
		hlEdgeRenderer = hlEdge.GetComponent<Renderer>();
		m_hlBaseRenderer.GetMaterial().SetFloat("_Amount", 0f);
		hlEdgeRenderer.GetMaterial().SetFloat("_Amount", 0f);
	}

	private void OnGUI()
	{
		if (Event.current.isKey)
		{
			amountchange = true;
		}
	}

	private void OnMouseEnter()
	{
		if (!FxStartAnim)
		{
			FxStartStop = false;
			FxStartAnim = true;
			powerchange = true;
			fxEmitterAScale = true;
		}
	}

	private void OnMouseExit()
	{
		if (!FxStartStop)
		{
			FxStartAnim = false;
			FxStartStop = true;
		}
	}

	private void OnMouseDown()
	{
		switch (stage)
		{
		case 0:
			ManaUse();
			break;
		case 1:
			RaysOn();
			break;
		}
		stage++;
	}

	private void RaysOn()
	{
		rays.SetActive(value: true);
		flash.SetActive(value: true);
		rayschange = true;
	}

	private void ManaUse()
	{
		colorchange = true;
	}

	private void Update()
	{
		Material hlEdgeMaterial = hlEdgeRenderer.GetMaterial();
		Material hlBaseMaterial = m_hlBaseRenderer.GetMaterial();
		if (amountchange)
		{
			float num = Time.deltaTime / 0.5f;
			float amountChangeAmtPerFrame = num * 0.6954f;
			float amountEdgeChangeAmtPerFrame = num * 0.6954f;
			Debug.Log("amount edge " + (hlEdgeMaterial.GetFloat("_Amount") + amountEdgeChangeAmtPerFrame));
			hlBaseMaterial.SetFloat("_Amount", hlBaseMaterial.GetFloat("_Amount") + amountChangeAmtPerFrame);
			if (hlBaseMaterial.GetFloat("_Amount") >= 0.6954f)
			{
				amountchange = false;
			}
			hlEdgeMaterial.SetFloat("_Amount", hlEdgeMaterial.GetFloat("_Amount") + amountEdgeChangeAmtPerFrame);
		}
		if (colorchange)
		{
			float speed = Time.deltaTime / 0.5f;
			Color startColor = hlBaseMaterial.color;
			hlBaseMaterial.color = Color.Lerp(startColor, endColor, speed);
		}
		if (powerchange)
		{
			float num2 = Time.deltaTime / 0.5f;
			float powerChangeAmtPerFrame = num2 * 18f;
			float amountChangeAmtPerFrame2 = num2 * 0.6954f;
			hlBaseMaterial.SetFloat("_power", hlBaseMaterial.GetFloat("_power") + powerChangeAmtPerFrame);
			if (hlBaseMaterial.GetFloat("_power") >= 29f)
			{
				powerchange = false;
			}
			hlBaseMaterial.SetFloat("_Amount", hlBaseMaterial.GetFloat("_Amount") + amountChangeAmtPerFrame2);
			if (hlBaseMaterial.GetFloat("_Amount") >= 1.12f)
			{
				amountchange = false;
			}
		}
		if (rayschange)
		{
			float scaleAmtPerFrame = Time.deltaTime / 0.5f * RayTime;
			rays.transform.localScale += new Vector3(0f, scaleAmtPerFrame, 0f);
			if (!raysdone && rays.transform.localScale.y >= 20f)
			{
				rays.SetActive(value: false);
				GetComponent<Renderer>().enabled = false;
				inplayObj.SetActive(value: true);
				inplayObj.GetComponent<Animation>().Play();
				fxEmitterA.SetActive(value: false);
				raysdone = true;
			}
		}
		if (raysdone)
		{
			Material material = flash.GetComponent<Renderer>().GetMaterial();
			float test = material.GetFloat("_InvFade") - Time.deltaTime;
			material.SetFloat("_InvFade", test);
			Debug.Log("InvFade " + test);
			if (test <= 0.01f)
			{
				entireObj.SetActive(value: false);
			}
		}
		if (fxEmitterAScale)
		{
			float fxscaleAmtPerFrame = Time.deltaTime / 0.5f * fxATime;
			fxEmitterA.transform.localScale += new Vector3(fxscaleAmtPerFrame, fxscaleAmtPerFrame, fxscaleAmtPerFrame);
		}
	}
}
