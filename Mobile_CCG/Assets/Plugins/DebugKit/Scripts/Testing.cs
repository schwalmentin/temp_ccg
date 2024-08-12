using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private float timeBetweenTestLogs = 0.5f;
    private float resetTimeBetweenTestLogs;
    private int testLogCounter;


    void Start()
    {
        resetTimeBetweenTestLogs = timeBetweenTestLogs;
    }


    void Update()
    {
        timeBetweenTestLogs -= Time.deltaTime;
        
        if(timeBetweenTestLogs <= 0)
        {
            timeBetweenTestLogs = resetTimeBetweenTestLogs;

            print($"print Message {testLogCounter}");
            Debug.Log($"Debug.Log Message {testLogCounter}");
            Debug.LogWarning($"Debug.LogWarning Message {testLogCounter}");
            Debug.LogAssertion($"Debug.LogAssertion Message {testLogCounter}");
            Debug.LogError($"Debug.LogError Message {testLogCounter}\n");
            Debug.LogException(new DivideByZeroException());

            testLogCounter++;
        }
    }
}
