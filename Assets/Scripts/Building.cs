using UnityEngine;

public class Building : MonoBehaviour, IEnterable
{
    public void EnterBuilding()
    {
        Debug.Log($"Entering building: {gameObject.name}", gameObject);
        // Add your building logic here (scene switch, UI popup, etc.)
    }
}