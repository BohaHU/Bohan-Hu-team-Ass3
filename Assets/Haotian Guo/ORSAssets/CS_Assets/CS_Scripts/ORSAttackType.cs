using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines an attack type which can be a projectile shot, or a direct melee attack.
    /// </summary>
    public class ORSAttackType : MonoBehaviour
    {
        public string attackAnimation = "Attack";

        [Tooltip("The projectile object that will be shot from the muzzle of this enemy. If no projectile is assigned then this attack type is considered melee")]
        public ORSProjectile projectile;

        [Tooltip("The projectile or melee damage that this enemy causes")]
        public int attackDamage = 0;

        [Tooltip("The hit range of the projectile, or when attacking with melee, this is how close the enemy will try to move to the player before attacking")]
        public float attackRange = 0.5f;

        [Tooltip("The movement speed of the projectile, or when attacking with melee, this is how fast the enemy will move towards the player before attacking")]
        public float attackSpeed = 10;

        [Tooltip("The hurt effect that appears on the player screen when this attack hits us")]
        public Sprite hurtEffect;
    }
}