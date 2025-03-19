using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementToast : MonoBehaviour
{
	[SerializeField]
	private ThreeSliceElement m_threeSliceElement;

	[SerializeField]
	private UberText m_text;

	[SerializeField]
	private Renderer m_fxShine;

	private WidgetTemplate m_toast;

	private const string CODE_HIDE = "CODE_HIDE";

	private const int X_MORE_TIER = -1;

	private void Awake()
	{
		m_toast = GetComponent<WidgetTemplate>();
		m_toast.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE")
			{
				Hide();
			}
		});
	}

	public void Initialize(AchievementDataModel dataModel)
	{
		if (dataModel.Tier == 1 && dataModel.NextTierID == 0)
		{
			m_text.Text = GameStrings.Format("GLUE_PROGRESSION_ACHIEVEMENT_TOAST", dataModel.Name);
		}
		else if (dataModel.Tier == -1)
		{
			m_text.Text = GameStrings.Format("GLUE_PROGRESSION_ACHIEVEMENT_TOAST_AND_X_MORE", dataModel.Name);
		}
		else
		{
			m_text.Text = GameStrings.Format("GLUE_PROGRESSION_ACHIEVEMENT_TOAST_TIER", dataModel.Name, dataModel.Tier);
		}
		m_toast.BindDataModel(dataModel);
		m_toast.RegisterDoneChangingStatesListener(delegate
		{
			float x = m_text.GetTextWorldSpaceBounds().size.x;
			m_threeSliceElement.SetMiddleWidth(x);
			float num = m_threeSliceElement.GetSize().x / m_fxShine.bounds.size.x;
			Vector3 lossyScale = m_fxShine.transform.lossyScale;
			lossyScale.x *= num;
			TransformUtil.SetWorldScale(m_fxShine, lossyScale);
			TransformUtil.SetPoint(m_fxShine, Anchor.LEFT_XZ, m_threeSliceElement, Anchor.LEFT_XZ);
		});
	}

	public void Show()
	{
		if (!(m_toast == null))
		{
			Transform toastParentTransform = BnetBar.Get().m_socialToastBone.transform;
			TransformUtil.AttachAndPreserveLocalTransform(base.gameObject.transform.parent, toastParentTransform);
			BoxCollider collider = base.gameObject.GetComponent<BoxCollider>();
			if ((bool)collider)
			{
				BoxCollider boxCollider = BnetBar.Get().m_socialToastBone.AddComponent<BoxCollider>();
				Matrix4x4 matrix = BnetBar.Get().m_socialToastBone.transform.worldToLocalMatrix * base.transform.localToWorldMatrix;
				boxCollider.center = matrix.MultiplyPoint(collider.center);
				boxCollider.size = matrix.MultiplyPoint(collider.size);
			}
			else
			{
				Debug.LogWarning("AchievementToast requires a box collider to maintain correct anchoring on the SocialToastBone when BnetBar.UpdateLayout() is called");
			}
			m_toast.Show();
		}
	}

	public void Hide()
	{
		if (!(m_toast == null))
		{
			m_toast.Hide();
			BoxCollider collider = BnetBar.Get().m_socialToastBone.GetComponent<BoxCollider>();
			if ((bool)collider)
			{
				Object.Destroy(collider);
			}
			Object.Destroy(base.transform.parent.gameObject);
		}
	}

	public static void ShowAndXMoreToast(int x)
	{
		ShowFake(new AchievementDataModel
		{
			Name = x.ToString(),
			Tier = -1
		});
	}

	public static void ShowFake(AchievementDataModel dataModel)
	{
		Widget fakeToast = WidgetInstance.Create(AchievementManager.ACHIEVEMENT_TOAST_PREFAB);
		fakeToast.RegisterReadyListener(delegate
		{
			AchievementToast componentInChildren = fakeToast.GetComponentInChildren<AchievementToast>();
			componentInChildren.Initialize(dataModel);
			componentInChildren.Show();
		});
	}
}
