using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip _music;

    [SerializeField] private AudioClip _breakBlock;

    private void Start()
    {
        _musicSource.clip = _music;
        _musicSource.Play();
    }

    private void OnEnable()
    {
        Mining.OnBreakBlock += OnBreakBlock;
    }

    private void OnDisable()
    {
        Mining.OnBreakBlock -= OnBreakBlock;
    }

    private void OnBreakBlock()
    {
        _sfxSource.PlayOneShot(_breakBlock);
    }
}
