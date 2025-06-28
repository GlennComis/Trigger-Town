using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Damage")]
public class DamageEffect : WeaponEffectBase
{
    public int damageAmount = 10;

    public override void ApplyEffect(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }
}