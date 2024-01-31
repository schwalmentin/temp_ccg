using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public abstract void Execute(IGameEngine gameEngine);
}
