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
            // Load the saved value and assign it to the Slider
            sfxSlider.value   = AudioManager.Instance.GetSFXVolume();
            musicSlider.value = AudioManager.Instance.GetMusicVolume();

            // Assign event when dragging slider
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
