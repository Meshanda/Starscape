using JetBrains.Annotations;
using UnityEngine;

public abstract class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
    
    /**
     * The difference between a normal Singleton and this one is that a LazySingleton
     * will only exist and add itself to the scene when someone tries to access its Instance.
     * It also means that contrary to the normal Singleton, the Instance will ALWAYS return something!
     */
    
    private static T _instance;

    [NotNull]
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject($"{typeof(T).Name} (created)");
                go.AddComponent<T>(); // This will call Awake() and initialize the _instance
            }

            return _instance;
        }
    }

    public static bool HasValidInstance()
    {
        return _instance != null;
    }

    protected virtual bool DestroyOnLoad => true;

    protected virtual void SingletonAwake() { }

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            
            if (!DestroyOnLoad)
                DontDestroyOnLoad(this);
            
            SingletonAwake();
        }
        else
        {
            Destroy(gameObject); // Safeguard, if we manually add an instance in the scene
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
