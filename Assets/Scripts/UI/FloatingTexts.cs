using System;
using UnityEngine;
using DG.Tweening;

public class FloatingTexts : MonoBehaviour
{
    [SerializeField] private float _yDistance = .1f;
    [SerializeField] private float _delay = .7f;
    [SerializeField] private Ease _easeCurve = Ease.InSine;
    private void Start()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(_yDistance, _delay).SetEase(_easeCurve));
        sequence.OnComplete(() => Destroy(gameObject));
    }
}
