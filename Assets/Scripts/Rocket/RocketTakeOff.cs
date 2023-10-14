using System;
using UnityEngine;


public class RocketTakeOff : MonoBehaviour
{
    [SerializeField] private Animator _rocketAnimator;
    [SerializeField] private GameObject _rocketCamera;
    [SerializeField] private GameObject _rocketSprite;
    
    private static readonly int _takeOffTrigger = Animator.StringToHash("TakeOff");

    private void OnEnable()
    {
        GameManager.GameWon += OnGameWon;
    }

    private void OnDisable()
    {
        GameManager.GameWon -= OnGameWon;
    }

    private void OnGameWon()
    {
        TakeOff();
    }

    public void TakeOff()
    {
        if (!_rocketCamera || !_rocketAnimator)
            return;
        
        _rocketCamera.SetActive(true);
        Invoke(nameof(TakeOffAnimation), 2);
    }

    private void TakeOffAnimation()
    {
        _rocketAnimator.SetTrigger(_takeOffTrigger);
    }

    public void TakeOffDone()
    {
        GameManager.Instance.ToggleWinScreen();
    }
    
    public void TakeOffRemoveRocket()
    {
        _rocketSprite.SetActive(false);
    }
}
