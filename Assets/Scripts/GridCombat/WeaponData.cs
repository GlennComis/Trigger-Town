using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int range = 3;
    public float fireRate = 0.5f;
    public int maxPenetration = 1; // 1 for normal, 2 for sniper, 0 for AoE
    public WeaponTargetingPattern pattern;

    public WeaponEffectBase[] effects; // modular stackable effects
}

public enum WeaponTargetingPattern
{
    ForwardLine,
    ConeSpread,
    PiercingLine,
    ThrownExplosion,
    ForwardStun       
}