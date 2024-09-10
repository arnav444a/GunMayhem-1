using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileHandler : Projectile
{
    Rigidbody2D rb;
    public float bulletVelocity = 10;
    public int damageAmount;
    public float pushAmount;
    public float despawnTime = 5f;
    public LayerMask targetLayers;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //Only server can destroy the bullet, client doesnt have the permission
        if (IsServer)
        {
            StartCoroutine(DespawnerCoroutine());
        }
    }
    private IEnumerator DespawnerCoroutine()
    {
        yield return new WaitForSeconds(despawnTime);
        NetworkManager.Destroy(this.gameObject);
    }

    public override void FireProjectile(Quaternion rotation)
    {
        rb.rotation = rotation.eulerAngles.z;
        rb.velocity = rb.transform.up * bulletVelocity;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if ((targetLayers & (1 << collision.gameObject.layer)) != 0)
        {
            Debug.Log("Damage player");
            Health healthScript = collision.gameObject.GetComponent<Health>();
            healthScript.TakeDamage(damageAmount);
            PushPlayerClientRpc(collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId, rb.velocity.normalized, pushAmount);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void PushPlayerClientRpc(ulong playerNetworkObjectId, Vector2 pushDirection, float pushForce)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (networkObject != null)
        {
            Rigidbody2D playerRb = networkObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Debug.Log("working");
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
            }
        }
    }
}
    public enum ShootDirection
{
    Right = 1, Left = -1
}

public abstract class Projectile : NetworkBehaviour{
    public abstract void FireProjectile(Quaternion rotation);
}
