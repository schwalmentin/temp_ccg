using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingManager : MonoBehaviour
{
    private bool pressed;
    [SerializeField] private int cardId;

    private void Start()
    {
        // this.StartCoroutine("Timer");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // this.pressed = true;
            try
            {
                Card card = DatabaseManager.Instance.GetCardById(this.cardId);
            }
            catch(KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
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
