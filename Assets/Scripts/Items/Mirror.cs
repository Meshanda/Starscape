using System.Collections;
using UnityEngine;

public class Mirror : UsableItem
{
    [Tooltip("Curve to represent the distance per level (time = level; value = distance)")]
    [SerializeField] private AnimationCurve _teleportDistanceCurve;
    
    [Tooltip("Time to execute the teleportation")]
    [SerializeField] private float _teleportTime;
    
    [SerializeField] private Transform _rocket;
    [SerializeField] private Transform _player;

    public Transform Rocket
    {
        set => _rocket = value;
    }

    public Transform Player
    {
        set => _player = value;
    }

    private float _teleportDistance;

    private bool _teleporting;

    private void Start()
    {
        _teleportDistance = _teleportDistanceCurve.Evaluate(ActualLevel);
    }


    protected override void OnUpgrade(int level)
    {
        _teleportDistance = _teleportDistanceCurve.Evaluate(level);
    }

    protected override void OnUseItem()
    {
        if (!_rocket || !_player)
            return;
        
        var distFromRocket = _rocket.position.y - _player.position.y;

        if (distFromRocket > _teleportDistance) return;
        // can add code after to tell the player he is to deep
        
        if (_teleporting) return;
            
        _teleporting = true;
        StartCoroutine(TimedTeleport());
    }

    private IEnumerator TimedTeleport()
    {
        yield return new WaitForSeconds(_teleportTime);
        
        
        _teleporting = false;
        _player.position = _rocket.position;
    }

}
