using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    private static T instance = null;
    private static readonly object padlock = new object();

    public static T Instance()
    {      
        if (instance == null)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new T();
                }
            }
        }

        return instance;      
    }

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        instance = null;
        Destroy(this.gameObject);
    }
}
