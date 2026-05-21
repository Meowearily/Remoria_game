using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Remoria.Core;
using Remoria.Player;

namespace Remoria.UI
{
    /// <summary>
    /// UI Controller for the Meta-Upgrade Shop in the Hub.
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private TextMeshProUGUI totalCurrencyText;

        [Header("Health Upgrade")]
        [SerializeField] private TextMeshProUGUI healthLevelText;
        [SerializeField] private TextMeshProUGUI healthCostText;
        [SerializeField] private Button healthUpgradeButton;

        [Header("Damage Upgrade")]
        [SerializeField] private TextMeshProUGUI damageLevelText;
        [SerializeField] private TextMeshProUGUI damageCostText;
        [SerializeField] private Button damageUpgradeButton;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (shopPanel != null) shopPanel.SetActive(false);

            // Bind button clicks
            if (healthUpgradeButton != null) healthUpgradeButton.onClick.AddListener(OnUpgradeHealthClicked);
            if (damageUpgradeButton != null) damageUpgradeButton.onClick.AddListener(OnUpgradeDamageClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        }

        private void OnEnable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTotalCurrencyChanged += UpdateCurrencyDisplay;
            }
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTotalCurrencyChanged -= UpdateCurrencyDisplay;
            }
        }

        public void OpenShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
            
            // Assuming we use Paused state for shop menus to free cursor
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameManager.GameState.Paused);
            }
            
            RefreshUI();
        }

        public void CloseShop()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
            
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            }
        }

        private void RefreshUI()
        {
            if (SaveManager.Instance == null || UpgradeManager.Instance == null) return;

            UpdateCurrencyDisplay(SaveManager.Instance.Data.totalCurrency);

            // Health Info
            if (healthLevelText != null) healthLevelText.text = $"Level: {SaveManager.Instance.Data.healthUpgradeLevel}";
            if (healthCostText != null) healthCostText.text = $"Cost: {UpgradeManager.Instance.GetHealthUpgradeCost()}";

            // Damage Info
            if (damageLevelText != null) damageLevelText.text = $"Level: {SaveManager.Instance.Data.damageUpgradeLevel}";
            if (damageCostText != null) damageCostText.text = $"Cost: {UpgradeManager.Instance.GetDamageUpgradeCost()}";
        }

        private void UpdateCurrencyDisplay(int amount)
        {
            if (totalCurrencyText != null)
            {
                totalCurrencyText.text = $"Shards: {amount}";
            }
        }

        private void OnUpgradeHealthClicked()
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeHealth())
            {
                RefreshUI();
            }
        }

        private void OnUpgradeDamageClicked()
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeDamage())
            {
                RefreshUI();
            }
        }
    }
}
