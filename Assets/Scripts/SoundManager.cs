using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _winMusic;
    [SerializeField] private AudioClip _loseMusic;

    [Header("Sfx Sounds")]
    [SerializeField] private AudioClip _breakBlock;
    [SerializeField] private AudioClip _rocketTakeoff;
    [SerializeField] private AudioClip _playerFootStep;
    [SerializeField] private AudioClip _playerJump;
    [SerializeField] private AudioClip _teleport;
    [SerializeField] private AudioClip _inventoryClick;
    [SerializeField] private AudioClip _breaking;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip _screenChanged;
    [SerializeField] private AudioClip _buttonClicked;

    private bool _breakingCooldown;
    
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
        _musicSource.clip = _gameMusic;
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
    public void PlayTeleportSound()
    {
        _sfxSource.PlayOneShot(_teleport);
    }
    public void PlayInventoryClickSound()
    {
        _sfxSource.PlayOneShot(_inventoryClick);
    }
    
    public void PlayBreakingSound()
    {
        if (!_breakingCooldown)
        {
            _breakingCooldown = true;
            StartCoroutine(PlayClipWithCooldown(_breaking, .06f, () => _breakingCooldown = false));
        }

    }

    public void PlayMenuMusic()
    {
        _musicSource.clip = _menuMusic;
        _musicSource.Play();
    }
    public void PlayWinMusic()
    {
        _musicSource.clip = _winMusic;
        _musicSource.Play();
    }
    public void PlayLoseMusic()
    {
        _musicSource.clip = _loseMusic;
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

    private IEnumerator PlayClipWithCooldown(AudioClip clip, float delay, Action callback = null)
    {
        _sfxSource.PlayOneShot(clip);
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
