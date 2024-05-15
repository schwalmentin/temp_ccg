using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    private static T instance = null;
    private static readonly object padlock = new object();

    public static T Instance
    {      
        get
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    /*
                    if (instance == null)
                    {
                        GameObject manager = new GameObject
                        {
                            name = typeof(T).Name
                        };
                        Instantiate(manager);
                        instance = manager.AddComponent<T>();
                    }
                    */
                }
            }

            return instance;
        }        
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
        Destroy(this.gameObject);
        instance = null;
    }
}
