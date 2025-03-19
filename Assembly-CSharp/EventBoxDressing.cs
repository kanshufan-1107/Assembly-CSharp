using Hearthstone.UI;
using UnityEngine;

public class EventBoxDressing : MonoBehaviour
{
	public enum State
	{
		UNKNOWN = -1,
		DISABLED,
		ENABLED
	}

	public class BoxDressingMaterials
	{
		private Material m_boxMaterial;

		private Material m_tableMaterial;

		private Material m_bottomSpinnerMaterial;

		private Material m_spotLightMaterial;

		private Material m_setRotationButtonMaterial;

		public Material BoxMaterial => m_boxMaterial;

		public Material TableMaterial => m_tableMaterial;

		public Material BottomSpinnerMaterial => m_bottomSpinnerMaterial;

		public Material SpotLightMaterial => m_spotLightMaterial;

		public Material SetRotationButtonMaterial => m_setRotationButtonMaterial;

		public BoxDressingMaterials(Material box, Material table, Material bottomSpinner, Material spotLight, Material setRotationButton)
		{
			m_boxMaterial = box;
			m_tableMaterial = table;
			m_bottomSpinnerMaterial = bottomSpinner;
			m_spotLightMaterial = spotLight;
			m_setRotationButtonMaterial = setRotationButton;
		}
	}

	[SerializeField]
	private Material m_boxMaterial;

	[SerializeField]
	private Material m_boxMaterialMobile;

	[SerializeField]
	private Material m_tableMaterial;

	[SerializeField]
	private Material m_bottomSpinnerMaterial;

	[SerializeField]
	private Material m_spotLightMaterial;

	[SerializeField]
	private Material m_setRotationButtonMaterial;

	[SerializeField]
	private MusicPlaylistType m_playlistToPlay;

	[SerializeField]
	private WeakAssetReference m_innkeeperGreetings;

	private BoxDressingMaterials m_materials;

	public void Start()
	{
		m_materials = new BoxDressingMaterials(UniversalInputManager.UsePhoneUI ? m_boxMaterialMobile : m_boxMaterial, m_tableMaterial, m_bottomSpinnerMaterial, m_spotLightMaterial, m_setRotationButtonMaterial);
	}

	public BoxDressingMaterials GetBoxDressingMaterials()
	{
		return m_materials;
	}

	public MusicPlaylistType GetPlaylistType()
	{
		return m_playlistToPlay;
	}

	public WeakAssetReference GetInnkeeperGreetings()
	{
		return m_innkeeperGreetings;
	}
}
