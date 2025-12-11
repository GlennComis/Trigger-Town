using UnityEngine;

public class PlayerController : CharacterController
{
    public int weaponDamage = 10;
    public System.Action OnShootRequested; // Player → BeatManager event

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnShootRequested?.Invoke();   // "I want to shoot now"
        }
    }

    // BeatManager will call this on successful timing
    public override void Shoot()
    {
        base.Shoot();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        Debug.Log("Player takes damage");
    }
}