using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] AudioMixer myMixed;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    private void Start()
    {
        if (AudioManager.HasInstance)
        {
            // Lấy giá trị đã lưu của SFX và Muisc dưới PlayerPrefs
            sfxSlider.value   = AudioManager.Instance.GetSFXVolume();
            musicSlider.value = AudioManager.Instance.GetMusicVolume();

            // Gán sự kiện cho slider của SFX và Music
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.UpdateSFXVolumeFromSlider);
            musicSlider.onValueChanged.AddListener(AudioManager.Instance.UpdateMusicVolumeFromSlider);
        }
    }
    public void OnClickClose()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject, UIManager.Instance.homePanel.gameObject);
        }
    }
}
