using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines the attributes of a weapon, name, icon, fire rate, ammo, accuracy, as well as the UI/animations/sounds associated with a weapon
    /// </summary>
    public class ORSWeapon : MonoBehaviour
    {
        [Tooltip("(WARNING: This variable is deprectaed and will be removed in upcoming versions, use Attack Types variable instead).This is projectile that the enemy throws at the player. The damage and speed is decided by the projectile object")]
        public ORSProjectile projectile;

        // The name of the weapon
        internal string weaponName = "Blaster";

        [Tooltip("How far can a shot hit. Beyond this distance you can't hit anything.")]
        public float hitRange = 20;

        [Tooltip("How much damage a single pellet in a shot causes. For example if you use a shotgun then each pellet will cause x damage")]
        public float damage = 1;

        [Tooltip("How much damage a single pellet in a shot causes. For example if you use a shotgun then each pellet will cause x damage")]
        public Vector2 recoil = new Vector2(1,3);

        [Tooltip("The number of bulets we have in the weapon. When this number is 0 you must reload the weapon. If this is not the default weapon, it will be replaced by the default one")]
        public int ammo = 10;
        internal int ammoCount;

        [Tooltip("If this is true then the weapon fires automatically, otherwise it shoots single shots")]
        public bool autoFire = false;

        [Tooltip("How quickly this weapon shoots")]
        public float fireRate = 0;
        internal float fireRateCount = 0;

        [Tooltip("How many pellets each shot releases. For example the Blaster and Stinger releases one pellet each shot, while the shotgun releases 8 pellets at once. Each pellet causes damage and goes in a different direction based on shot spread")]
        public int pelletsPerShot = 1;

        [Range(0, 0.3f)]
        [Tooltip("How far off from the center pellets fly. 0 means that all pellets fly straight, while a higher number means that the pellets fly in different angles")]
        public float shotSpread = 0;
        
        [Tooltip("The icon that appears on screen as the ammo of the weapon")]
        public Sprite ammoIcon;

        // The width of the ammo icon is calculated automatically when loading a weapon
        internal float ammoIconWidth = 0;

        [Tooltip("The animation that appears when shooting this weapon ( ammo flying off )")]
        public AnimationClip ammoShootAnimation;

        [Tooltip("The animation that appears when reloading this weapon")]
        public AnimationClip ammoReloadAnimation;
        
        [Tooltip("Various sounds for shooting, reloading, and shooting when no ammo is left")]
        public AudioClip soundShoot;
        public AudioClip soundEmpty;
        public AudioClip soundReload;

        [Tooltip("The shape of the crosshair for this weapon")]
        public Sprite crosshair; 
    }
}