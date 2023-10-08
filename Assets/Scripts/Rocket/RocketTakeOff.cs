using System;
using UnityEngine;


public class RocketTakeOff : MonoBehaviour
{
    [SerializeField] private Animator _rocketAnimator;
    [SerializeField] private GameObject _rocketCamera;
    
    private static readonly int _takeOffTrigger = Animator.StringToHash("TakeOff");

    private void Start()
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
        Debug.Log("takeoff anim");
        _rocketAnimator.SetTrigger(_takeOffTrigger);
    }
}
