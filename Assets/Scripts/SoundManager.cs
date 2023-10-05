using System;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public static Action OnGameStartMusic;
    public static Action<Item> OnBreakBlock;
    public static Action OnPlayerFootStep;
    public static Action OnPlayerJump;
    public static Action OnRocketTakeoff;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip _music;
    [SerializeField] private AudioClip _menuMusic;

    [Header("Sfx Sounds")]
    [SerializeField] private AudioClip _breakBlock;
    [SerializeField] private AudioClip _rocketTakeoff;
    [SerializeField] private AudioClip _playerFootStep;
    [SerializeField] private AudioClip _playerJump;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip _screenChanged;
    [SerializeField] private AudioClip _buttonClicked;

    private void OnEnable()
    {
        OnGameStartMusic += Event_OnGameStartMusic;
        OnBreakBlock += Event_OnBreakBlock;
        OnPlayerFootStep += Event_OnPlayerFootStep;
        OnPlayerJump += Event_OnPlayerJump;
        OnRocketTakeoff += Event_RocketTakeoff;
    }

    private void OnDisable()
    {
        OnGameStartMusic -= Event_OnGameStartMusic;
        OnBreakBlock -= Event_OnBreakBlock;
        OnPlayerFootStep -= Event_OnPlayerFootStep;
        OnPlayerJump -= Event_OnPlayerJump;
        OnRocketTakeoff -= Event_RocketTakeoff;
    }

    private void Start()
    {
        SetVolumeMusic(Settings.volumeMusic);
        SetVolumeSfx(Settings.volumeFx);
    }
    
    private void Event_OnGameStartMusic()
    {
        _musicSource.clip = _music;
        _musicSource.Play();
    }

    private void Event_OnBreakBlock(Item item)
    {
        _sfxSource.PlayOneShot(_breakBlock);
    }

    private void Event_OnPlayerFootStep()
    {
        _sfxSource.PlayOneShot(_playerFootStep);
    }

    private void Event_OnPlayerJump()
    {
        _sfxSource.PlayOneShot(_playerJump);
    }

    private void Event_RocketTakeoff()
    {
        _sfxSource.PlayOneShot(_rocketTakeoff);
    }

    public void PlayClickSound()
    {
        _sfxSource.PlayOneShot(_buttonClicked);
    }
    public void PlayWhooshSound()
    {
        _sfxSource.PlayOneShot(_screenChanged);
    }

    public void PlayMenuMusic()
    {
        _musicSource.clip = _menuMusic;
        _musicSource.Play();
    }
    
    public void SetVolumeMusic(float value)
    {
        _musicSource.volume = value;
        Settings.volumeMusic = value;
    } 
    public void SetVolumeSfx(float value)
    {
        _sfxSource.volume = value;
        Settings.volumeFx = value;
    }
}
