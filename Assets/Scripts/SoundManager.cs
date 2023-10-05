using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static Action OnStartMusic;
    public static Action<Item> OnBreakBlock;
    public static Action OnPlayerFootStep;
    public static Action OnPlayerJump;
    public static Action OnRocketTakeoff;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip _music;

    [Header("Sfx Sounds")]
    [SerializeField] private AudioClip _breakBlock;
    [SerializeField] private AudioClip _rocketTakeoff;
    [SerializeField] private AudioClip _playerFootStep;
    [SerializeField] private AudioClip _playerJump;

    private void OnEnable()
    {
        OnStartMusic += Event_OnStartMusic;
        OnBreakBlock += Event_OnBreakBlock;
        OnPlayerFootStep += Event_OnPlayerFootStep;
        OnPlayerJump += Event_OnPlayerJump;
        OnRocketTakeoff += Event_RocketTakeoff;
    }

    private void OnDisable()
    {
        OnStartMusic -= Event_OnStartMusic;
        OnBreakBlock -= Event_OnBreakBlock;
        OnPlayerFootStep -= Event_OnPlayerFootStep;
        OnPlayerJump -= Event_OnPlayerJump;
        OnRocketTakeoff -= Event_RocketTakeoff;
    }


    private void Event_OnStartMusic()
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
}
