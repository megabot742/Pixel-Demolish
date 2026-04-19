using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [Header("Loading UI References")]
    [SerializeField] private Slider loadingSlider;           // Gán LoadingSlider trong Inspector
    [SerializeField] private float minFakeDuration = 1f;            // Delay tối thiểu sau khi scene ready
    [SerializeField] private float maxFakeDuration = 1.5f;          // Delay tối đa

    private AsyncOperation currentOperation;
    private bool isLoading = false;

    public void ShowLoading(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName) || isLoading) return;

        isLoading = true;
        gameObject.SetActive(true);
        
        // Pause time + audio ngay lập tức
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Reset slider
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        StartCoroutine(LoadSceneAsyncRoutine(targetSceneName));
    }
    private IEnumerator LoadSceneAsyncRoutine(string targetSceneName)
    {
        // Bắt đầu load async nhưng chưa activate
        currentOperation = SceneManager.LoadSceneAsync(targetSceneName);
        currentOperation.allowSceneActivation = false;

       // Bước 1: Tiến độ thực tế 0% → 90%
        while (currentOperation.progress < 0.9f)
        {
            float progress = currentOperation.progress / 0.9f;   // 0.0 → 1.0
            if (loadingSlider != null)
                loadingSlider.value = Mathf.Lerp(loadingSlider.value, progress, 0.15f); //làm mượt tiến độ slider

            yield return null;
        }

        // Đảm bảo slider chạm ít nhất 90%
        if (loadingSlider != null)
            loadingSlider.value = 0.9f;

        // Bước 2: Làm giả tiến độ từ 90% → 100% trong khoảng thời gian random
        float fakeDuration = Random.Range(minFakeDuration, maxFakeDuration);
        float elapsed = 0f;
        float startProgress = 0.9f;

        while (elapsed < fakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fakeDuration;
            // Dùng Ease Out để trông tự nhiên hơn (chậm dần về cuối)
            float fakeProgress = startProgress + (1f - startProgress) * Mathf.SmoothStep(0f, 1f, t);

            if (loadingSlider != null)
                loadingSlider.value = fakeProgress;

            yield return null;
        }

        // Đảm bảo full 100% trước khi activate
        if (loadingSlider != null)
            loadingSlider.value = 1f;

        // Unpause thời gian và âm thanh
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Activate scene
        currentOperation.allowSceneActivation = true;

        yield return new WaitUntil(() => currentOperation.isDone);

        // Ẩn đi loading
        gameObject.SetActive(false);
        isLoading = false;

        // Cập nhật UI cho scene mới
        if (UIManager.HasInstance)
        {
            UIManager.Instance.currentSceneName = targetSceneName;
            UIManager.Instance.UpdateUIForScene(targetSceneName);
        }
    }
}
