using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingManager : MonoBehaviour
{
    private bool pressed;

    private void Start()
    {
        this.StartCoroutine("Timer");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            this.pressed = true;
        }
    }

    private IEnumerator Timer()
    {
        while (!pressed)
        {
            yield return new WaitForSeconds(3);
            print("Trying: " + this.pressed);
        }
    }
}
