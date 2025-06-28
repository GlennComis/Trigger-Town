using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Stun")]
public class StunEffect : WeaponEffectBase
{
    public float duration = 1.5f;

    public override void ApplyEffect(GameObject target)
    {
        var stun = target.GetComponent<IStunnable>();
        if (stun != null)
        {
            stun.Stun(duration);
        }
    }
}