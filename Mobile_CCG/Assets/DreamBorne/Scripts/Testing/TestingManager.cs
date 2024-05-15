using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestingManager : MonoBehaviour
{
    #region Timer

        private bool pressed;
        
        private IEnumerator Timer()
        {
            while (!pressed)
            {
                yield return new WaitForSeconds(3);
                print("Trying: " + this.pressed);
            }
        }

    #endregion
    
    #region ActionMapper

        private readonly Dictionary<string, Action<string>> staticActions = new Dictionary<string, Action<string>>();
        private readonly Dictionary<string, Delegate> dynamicActions = new Dictionary<string, Delegate>();
        
        private void DynamicTest1(int x, int y, int z)
        {
            
        }
        private void DynamicTest2(string x, int y)
        {
            
        }

        private void StaticTest1(string jsonBody)
        {
            
        }
        private void StaticTest2(string jsonBody)
        {
            
        }

    #endregion

    #region Json Converter

        private void TestJsonConverter()
        {
            DrawCardParams example1 = new DrawCardParams(1, 69);
            Debug.Log("Example1 Object: " + example1.id);

            string example1String = JsonUtility.ToJson(example1);
            Debug.Log("Example1 Json: " + example1String);

            DrawCardParams final1 = JsonUtility.FromJson<DrawCardParams>(example1String);
            Debug.Log("Final1 Object: " + final1.id);
        }

    #endregion

    private void Awake()
    {
        // this.StartCoroutine("Timer");
        
        this.dynamicActions.Add("test1", new Action<int, int, int>(this.DynamicTest1));
        this.dynamicActions.Add("test2", new Action<string, int>(this.DynamicTest2));

        this.staticActions.Add("test1", this.StaticTest1);
        this.staticActions.Add("test2", this.StaticTest2);

        // this.TestJsonConverter();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // this.pressed = true;
            /*
            try
            {
                Card card = DatabaseManager.Instance.GetCardById(1, 69);
                print(card.name);
            }
            catch(KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
            */
            
            print(LobbyManager.Instance.name);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            this.staticActions["test1"].Invoke("test");
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.dynamicActions["test1"].DynamicInvoke("test");
        }        
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            this.dynamicActions["test1"].DynamicInvoke(1, 2, 3);
        }
    }
    

    [SerializeField] private GameObject player;
    
    private void TestClass()
    {
        Vector3 velocity = new Vector3(5 * Time.deltaTime, 0, 0);
        this.player.transform.position = velocity;
    }
    
}
