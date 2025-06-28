using UnityEngine;

public abstract class WeaponEffectBase : ScriptableObject, IWeaponEffect
{
    public abstract void ApplyEffect(GameObject target);
}