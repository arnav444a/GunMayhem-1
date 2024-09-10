using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
public class PlayerStateMachine : IStateMachine<PlayerState, PlayerStateMachine>
{
    [Header("MOVEMENT")]
    public float speed = 200;
    public float sprintSpeed;
    private bool sprinting;
    float horizontal;
    public float movementMultiplier = 1f;
    [Header("JUMPING")]
    public float gravityUpwardModifier;
    public float gravityDownwardModifier;
    public float jumpVelocity = 200;
    public float jumpMovementMultiplier = 1;
    public float jumpPeekMovementMultiplier = 1.5f;
    [Header("MISC")]
    public Transform groundCheck;
    public Rigidbody2D rb;
    public LayerMask groundLayer;
    [Header("Combat")]
    public Transform WeaponHolder;
    public IItem[] Weapon;
    public WeaponUsage[] CurrentItem { private set; get; } = new WeaponUsage[2];
    public WeaponUsage LeftHand => CurrentItem[0];
    public WeaponUsage RightHand => CurrentItem[1];

    [HideInInspector]
    private void Awake()
    {
        RegisterState(new PlayerMovementState(this));
        RegisterState(new PlayerJumpState(this));
        RegisterState(new PlayerFallState(this));
        ChangeState(PlayerState.Moving);
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
    }
    public void InitialisePlayer()
    {
        if (IsServer)
        {
            LoadWeaponClientRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //For new client to server synchronisation
            LoadWeaponClientRpc();
        }
    }
    private void OnClientConnected(ulong clientId)
    {
        //For server to new clent synchronisation
        LoadWeaponClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });
    }

    [ClientRpc(RequireOwnership = false)]
    public void LoadWeaponClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner)
        {
            Debug.Log("Weapon loaded for client");
        }
        for (int i = 0; i < Weapon.Length; i++)
        {
            var weapon = Weapon[i];
            ItemUsage usage = null;
            if (weapon && weapon.Set(this, out usage))
            {
                //nothing
            }
            CurrentItem[i] = usage as WeaponUsage;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        CurrentState.OnStateUpdate();
        if (Input.GetButtonDown("Jump"))
        {
            ChangeState(PlayerState.Jumping);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            UseItem();
        }
        sprinting = Input.GetButton("Sprint");
    }
    private void UseItem()
    {
        //filter hand based on enum with int assigned
        //CurrentItem[handInt].UseItem();
        LeftHand?.UseItem();
    }
    public void ChangeWeapon(ItemUsage item)
    {
        //LeftHand.InitializeItem(this);
    }
    public void PlayerMovement() 
    {
        horizontal = Input.GetAxis("Horizontal") * movementMultiplier;
        Vector3 currentLocalScale = transform.localScale;
        if (horizontal > 0)
        {
            currentLocalScale.x = Mathf.Abs(currentLocalScale.x);
        }
        else if (horizontal < 0)
        {
            currentLocalScale.x = -Mathf.Abs(currentLocalScale.x);
        }
        transform.localScale = currentLocalScale;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * (sprinting? sprintSpeed:speed) * Time.fixedDeltaTime, rb.velocity.y);
        CurrentState.OnStateFixedUpdate();
    }
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer);
    }
}

public enum PlayerState
{
    Moving,
    Jumping,
    Falling,
    Pause,
}
