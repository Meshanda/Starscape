using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> 
{
    public static T Instance { get; private set; }

    protected virtual bool DestroyOnLoad => true;

    protected virtual void SingletonAwake() { }

    private void Awake()
    { 
        //print("AWAKE");
        if (Instance == null)
        {
            Instance = (T)this;
            
            if (!DestroyOnLoad)
                DontDestroyOnLoad(this);
            
            SingletonAwake();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
