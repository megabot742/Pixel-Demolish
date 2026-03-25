using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinManager : BaseManager<CoinManager>
{
    [SerializeField] private int maxCoins = 9999999; // Max coin
    [SerializeField] private string maxText = "Max Coin";
    public int currentCoins = 0;
    public int CurrentCoins => currentCoins;
    private const int DISPLAY_DIGITS = 7; // Always display 7 digits

    #region Singleton Manager
    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Chỉ reset coin khi vào Level (không reset ở Menu)
        if (scene.name == "Level 1" || scene.name == "Level 2" || 
            scene.name == "Level 3" || scene.name == "Level 4" || scene.name == "Level 5")
        {
            ResetCoins();
            Debug.Log($"[CoinManager] Scene {scene.name} loaded → Reset coin to 0");
        }
    }
    #endregion
    #region Handle Coin
    public void AddCoin(int amount = 1)
    {
        if (currentCoins >= maxCoins) return; //check max coin

        currentCoins += amount;
        if (currentCoins > maxCoins) //Cap coin for max
            currentCoins = maxCoins;
        UpdateUI();
    }
    public void SubtractCoin(int amount)
    {
        currentCoins -= amount;
        if (currentCoins < 0) currentCoins = 0;  //Cap for never negative

        UpdateUI();
    }
    private void UpdateUI()
    {
        if(UIManager.HasInstance)
        {
            if (UIManager.Instance.hUDPanel.coinTxt == null) return;

            if (currentCoins >= maxCoins)
            {
                UIManager.Instance.hUDPanel.coinTxt.text = maxText;
            }
            else
            {
                UIManager.Instance.hUDPanel.coinTxt.text = currentCoins.ToString($"D{DISPLAY_DIGITS}");
            }
        }
    }
    // Reset coin (dùng khi restart level)
    public void ResetCoins()
    {
        currentCoins = 0;
        UpdateUI();
    }
    // void OnValidate()
    // {
    //     coinText.text = currentCoins.ToString();
    // }
    #endregion
    #region ContextMenu
    [ContextMenu("Add 10 Coins (Testing)")]
    [ContextMenu("Add 1 Coin")]
    private void TestAdd100() => AddCoin(100);

    [ContextMenu("Add 1000000 Coins")]
    private void TestAddMillion() => AddCoin(1000000);
    [ContextMenu("Subtract 100000 Coins")]
    private void SubtractTenThousand() => SubtractCoin(10000);

    [ContextMenu("Reset to 0")]
    private void TestReset() 
    { 
        ResetCoins();
    }
    #endregion 
}
