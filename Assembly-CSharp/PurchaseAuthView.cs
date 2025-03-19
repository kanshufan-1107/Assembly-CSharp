using System;
using UnityEngine;

public class PurchaseAuthView : ShopView.IComponent
{
	private StorePurchaseAuth m_purchaseAuth;

	public bool IsLoaded => m_purchaseAuth != null;

	public bool IsShown
	{
		get
		{
			if (IsLoaded)
			{
				return m_purchaseAuth.IsShown();
			}
			return false;
		}
	}

	public event Action OnComponentReady = delegate
	{
	};

	public event Action<bool, MoneyOrGTAPPTransaction> OnPurchaseResultAcknowledged = delegate
	{
	};

	public event Action OnCancelButtonPressed = delegate
	{
	};

	public event Action OnAuthExit = delegate
	{
	};

	public void Load(IAssetLoader assetLoader)
	{
		if (!IsLoaded)
		{
			assetLoader.InstantiatePrefab((string)ShopPrefabs.ShopPurchaseAuthPrefab, OnLoaded);
		}
	}

	public void Unload()
	{
		if (IsLoaded)
		{
			UnityEngine.Object.Destroy(m_purchaseAuth.gameObject);
			m_purchaseAuth = null;
		}
	}

	public void Show(MoneyOrGTAPPTransaction transaction, bool isZeroCostLicense, StorePurchaseAuth.ButtonStyle waitButtonStyle = StorePurchaseAuth.ButtonStyle.NoButton)
	{
		if (IsLoaded)
		{
			m_purchaseAuth.Show(transaction, isZeroCostLicense, waitButtonStyle);
		}
	}

	public void Hide()
	{
		if (IsShown)
		{
			m_purchaseAuth.Hide();
		}
	}

	public void StartNewTransaction(MoneyOrGTAPPTransaction transaction, bool isZeroCostLicense, StorePurchaseAuth.ButtonStyle waitButtonStyle = StorePurchaseAuth.ButtonStyle.NoButton)
	{
		if (IsLoaded)
		{
			m_purchaseAuth.StartNewTransaction(transaction, isZeroCostLicense, waitButtonStyle);
		}
	}

	public void ShowPreviousPurchaseSuccess(MoneyOrGTAPPTransaction transaction, StorePurchaseAuth.ButtonStyle waitButtonStyle = StorePurchaseAuth.ButtonStyle.NoButton)
	{
		if (IsLoaded)
		{
			m_purchaseAuth.ShowPreviousPurchaseSuccess(transaction, waitButtonStyle);
		}
	}

	public void ShowPreviousPurchaseFailure(MoneyOrGTAPPTransaction transaction, string details, StorePurchaseAuth.ButtonStyle waitButtonStyle, Network.PurchaseErrorInfo.ErrorType error)
	{
		if (IsLoaded)
		{
			m_purchaseAuth.ShowPurchaseMethodFailure(transaction, details, waitButtonStyle, error);
		}
	}

	public bool CompletePurchaseSuccess(MoneyOrGTAPPTransaction transaction)
	{
		if (!IsLoaded)
		{
			return false;
		}
		return m_purchaseAuth.CompletePurchaseSuccess(transaction);
	}

	public bool CompletePurchaseFailure(MoneyOrGTAPPTransaction transaction, string details, Network.PurchaseErrorInfo.ErrorType error)
	{
		if (IsLoaded)
		{
			return m_purchaseAuth.CompletePurchaseFailure(transaction, details, error);
		}
		return false;
	}

	private void OnLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError("PurchaseAuthView.OnLoaded(): go is null!");
			return;
		}
		m_purchaseAuth = go.GetComponent<StorePurchaseAuth>();
		if (m_purchaseAuth == null)
		{
			Debug.LogError("PurchaseAuthView.OnLoaded(): go has no StorePurchaseAuth component");
			return;
		}
		m_purchaseAuth.Hide();
		m_purchaseAuth.RegisterAckPurchaseResultListener(delegate(bool success, MoneyOrGTAPPTransaction transaction)
		{
			this.OnPurchaseResultAcknowledged(success, transaction);
		});
		m_purchaseAuth.RegisterCancelButtonListener(delegate
		{
			this.OnCancelButtonPressed();
		});
		m_purchaseAuth.RegisterExitListener(delegate
		{
			this.OnAuthExit();
		});
		this.OnComponentReady();
	}

	public bool HideCancelButton()
	{
		if (!IsLoaded || m_purchaseAuth == null)
		{
			return false;
		}
		return m_purchaseAuth.HideCancelButton();
	}
}
