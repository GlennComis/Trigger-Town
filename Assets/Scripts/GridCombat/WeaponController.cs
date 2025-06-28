using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public WeaponData currentWeapon;
    private bool canShoot = true;

    public bool CanShoot => canShoot; // ← Add this

    public void TryShoot(Vector2Int userGridPos)
    {
        if (!canShoot || currentWeapon == null) return;

        StartCoroutine(ShootCooldown());

        List<GameObject> targets = TargetingSystem.GetTargets(currentWeapon, userGridPos, isPlayer: true);
        int hitsRemaining = currentWeapon.maxPenetration > 0 ? currentWeapon.maxPenetration : int.MaxValue;

        foreach (var target in targets)
        {
            foreach (var effect in currentWeapon.effects)
            {
                effect.ApplyEffect(target);
            }

            hitsRemaining--;
            if (hitsRemaining <= 0)
                break;
        }
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(currentWeapon.fireRate);
        canShoot = true;
    }
}
