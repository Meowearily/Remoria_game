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
        public static UpgradeUI GlobalInstance { get; private set; }

        [Header("UI References")]
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

        private void Awake()
        {
            if (GlobalInstance == null)
            {
                GlobalInstance = this;
                transform.SetParent(null); // Move to root to allow DontDestroyOnLoad
                DontDestroyOnLoad(gameObject);
                
                // Keep it hidden at start
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Bind button clicks
            if (healthUpgradeButton != null) healthUpgradeButton.onClick.AddListener(OnUpgradeHealthClicked);
            if (damageUpgradeButton != null) damageUpgradeButton.onClick.AddListener(OnUpgradeDamageClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        }

        private void OnEnable()
        {
            // Ensure all children (like UpgradeShopPannel) are active
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }

            // Ensure we are on a Canvas and correctly positioned
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            
            if (canvas != null)
            {
                // Make sure it's set to overlay or high sort order to be visible
                canvas.overrideSorting = true;
                canvas.sortingOrder = 999;
                
                // If it's the root object (our new ShopCanvas), ensure it's ScreenSpaceOverlay
                if (canvas.isRootCanvas)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            // Reset transform to be sure it's centered and visible
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                // If it's a panel inside a canvas, stretch it or center it
                // rect.anchoredPosition = Vector2.zero; 
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTotalCurrencyChanged += UpdateCurrencyDisplay;
            }
            
            // Force cursor to be visible whenever the shop is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Paused)
            {
                GameManager.Instance.SetState(GameManager.GameState.Paused);
            }
            
            RefreshUI();
        }

        private void Update()
        {
            // Extra safety: ensure cursor stays visible if we are in Paused state (shop open)
            if (gameObject.activeSelf && Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTotalCurrencyChanged -= UpdateCurrencyDisplay;
            }
        }

        public void CloseShop()
        {
            // Close the panel
            gameObject.SetActive(false);
            
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            }
        }

        private void RefreshUI()
        {
            if (SaveManager.Instance == null) return;

            UpdateCurrencyDisplay(SaveManager.Instance.Data.totalCurrency);

            // Health Info
            int healthLevel = SaveManager.Instance.Data.healthUpgradeLevel;
            int damageLevel = SaveManager.Instance.Data.damageUpgradeLevel;
            
            int hCost = Mathf.FloorToInt(100 * Mathf.Pow(1.5f, healthLevel));
            int dCost = Mathf.FloorToInt(100 * Mathf.Pow(1.5f, damageLevel));

            if (healthLevelText != null) healthLevelText.text = $"Level: {healthLevel}";
            if (healthCostText != null) healthCostText.text = $"Cost: {hCost}";

            // Damage Info
            if (damageLevelText != null) damageLevelText.text = $"Level: {damageLevel}";
            if (damageCostText != null) damageCostText.text = $"Cost: {dCost}";
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
            int level = SaveManager.Instance.Data.healthUpgradeLevel;
            int cost = Mathf.FloorToInt(100 * Mathf.Pow(1.5f, level));

            if (CurrencyManager.Instance.SpendCurrency(cost))
            {
                SaveManager.Instance.Data.healthUpgradeLevel++;
                SaveManager.Instance.Save();
                RefreshUI();
            }
        }

        private void OnUpgradeDamageClicked()
        {
            int level = SaveManager.Instance.Data.damageUpgradeLevel;
            int cost = Mathf.FloorToInt(100 * Mathf.Pow(1.5f, level));

            if (CurrencyManager.Instance.SpendCurrency(cost))
            {
                SaveManager.Instance.Data.damageUpgradeLevel++;
                SaveManager.Instance.Save();
                RefreshUI();
            }
        }
    }
}
