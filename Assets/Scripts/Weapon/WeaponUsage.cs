using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity;
[System.Serializable]
public class WeaponUsage : ItemUsage
{
    public float damage;
    public float fireRate;
    public float nextFire;
    [Header("Ammo")]
    public int mags;
    public int ammo;
    public int magAmmo;
    bool canShoot;
    public GameObject gunObject;
    public Transform gunBarrel;
    [HideInInspector]
    public PlayerStateMachine player;
    public override void UseItem()
    {
        //fire gun
        if (ammo > 0)
        {
            ammo--;
            //MonoBehaviour.StartCoroutine
            Fire();
        }
        if (ammo==0)
        {
            Reload();
        }
    }
    public override void InitializeItem(IItem item)
    {

    }

    public override bool SetItem(PlayerStateMachine entity, out ItemUsage usage)
    {
        ItemUsage newUsage = null;
        GameObject weaponObject = GameObject.Instantiate(gunObject, entity.WeaponHolder);
        var item = weaponObject.GetComponent<IItem>();
        WeaponUsage playerUsage = (WeaponUsage)item.usage;
        playerUsage.player = entity;
        newUsage = item.usage = item.usage.CreateInstance(item);
        usage = newUsage;
        return true;
    }
    private void Fire()
    {
        SpawnBullet();
    }
    public void SpawnBullet()
    {
        if (gunBarrel.lossyScale.x < 0)
        {
            Vector2 direction = Vector2.left;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion rotation = Quaternion.Euler(0,0,angle);
            SpawnManager.Instance.RequestSpawn(SpawnManager.ObjectType.AkBullet, gunBarrel.transform.position, rotation);
        }
        else
        {
            Vector2 direction = Vector2.right;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion rotation = Quaternion.Euler(0,0,angle);
            SpawnManager.Instance.RequestSpawn(SpawnManager.ObjectType.AkBullet, gunBarrel.transform.position, rotation);
        }
    }
    public void Reload()
    {
        if (mags > 0)
        {
            mags--;
            ammo = magAmmo;
        }
    }
    public void Initialise(PlayerStateMachine entity)
    {
        player = entity;
        MonoBehaviour.Instantiate(gunObject, entity.WeaponHolder);
    }
    public void UpdateUI()
    {
        //UI update
    }
    IEnumerator WaitGunReset(float nextFireTime)
    {
        canShoot = false;
        yield return new WaitForSeconds(nextFireTime);
        canShoot = true;

    }
}
