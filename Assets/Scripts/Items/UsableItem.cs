using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class UsableItem : MonoBehaviour
{
    //for testing
    public int StartLevel;
    [SerializeField] private int _maxLevel;

    protected int ActualLevel;

    protected void Awake()
    {
        ActualLevel = StartLevel > _maxLevel ? _maxLevel : StartLevel;
    }


    public void Upgrade()
    {
        if (ActualLevel < _maxLevel)
        {
            OnUpgrade(++ActualLevel);
        }
    }


    protected abstract void OnUpgrade(int level);
    
    protected abstract void OnUseItem();
}
