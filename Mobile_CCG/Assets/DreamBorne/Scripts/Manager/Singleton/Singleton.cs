using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    // private static GameObject manager;
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
                    if (instance == null)
                    {
                        // manager = Resources.Load("Manager") as GameObject;
                        // Instantiate(manager);
                        // instance = manager.AddComponent<T>();
                        instance = new T();
                    }
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

        // manager = Resources.Load("Manager") as GameObject;
        DontDestroyOnLoad(this.gameObject);
        instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        instance = null;
        Destroy(this.gameObject);
    }
}
