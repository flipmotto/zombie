using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    public static event Action<PlayerMovement> LocalPlayerSpawned;
    public static event Action LocalPlayerDespawned;

    private float speed = 5f;
    private float jumpForce = 5f;
    private float mouseSensitivity = 0.1f;
    public Transform cameraRoot;

    private float yaw;
    private float pitch;
    private Rigidbody rb;

    // Local input state
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool inMenu;
    public event Action<bool> InMenuChanged;
    public bool InMenu
    {
        get => inMenu;
        private set
        {
            if (inMenu == value) return;
            inMenu = value;
            InMenuChanged?.Invoke(inMenu);

            // cursor handling
            Cursor.lockState = inMenu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inMenu;
        }
    }

    // Sequence tracking
    private uint inputSequence;
    private struct InputState
    {
        public Vector2 move;
        public bool jump;
        public float yaw;
    }
    private List<(uint, InputState)> pendingInputs = new();

    // Server state
    private struct ServerState : INetworkSerializable
    {
        public Vector3 position;
        public Vector3 velocity;
        public float yaw;
        public uint lastProcessedInput;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref yaw);
            serializer.SerializeValue(ref lastProcessedInput);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Instance = this;
            LocalPlayerSpawned?.Invoke(this);

            // Lock and hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Initialize yaw from body rotation
            yaw = transform.eulerAngles.y;

            // Initialize pitch from camera's starting local rotation (normalize -180..180)
            if (cameraRoot != null)
            {
                float rawPitch = cameraRoot.localEulerAngles.x;
                if (rawPitch > 180f) rawPitch -= 360f;
                pitch = rawPitch;

                // Apply immediately so camera starts in correct position
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

                // Enable camera for local player
                cameraRoot.gameObject.SetActive(true);
            }
        }
        else
        {
            if (cameraRoot != null)
                cameraRoot.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            LocalPlayerDespawned?.Invoke();
            if (Instance == this) Instance = null;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            InMenu = !InMenu;

        if (InMenu) return;

        Vector2 i = Vector2.zero;
        if (kb != null)
        {
            if (kb.wKey.isPressed) i.y += 1;
            if (kb.sKey.isPressed) i.y -= 1;
            if (kb.aKey.isPressed) i.x -= 1;
            if (kb.dKey.isPressed) i.x += 1;
        }

        if (i.sqrMagnitude > 1f) i.Normalize();

        if (mouse != null)
        {
            yaw += mouse.delta.x.ReadValue() * mouseSensitivity;
            pitch -= mouse.delta.y.ReadValue() * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }

        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        bool jump = kb != null && kb.spaceKey.wasPressedThisFrame;

        moveInput = i;
        jumpPressed = jump;

        // Create an input packet
        var inputState = new InputState { move = i, jump = jump, yaw = yaw };
        pendingInputs.Add((inputSequence, inputState));

        // Predict locally
        SimulateMovement(inputState.move, inputState.jump, inputState.yaw);

        // Send to server
        SubmitInputServerRpc(inputState.move, inputState.jump, inputState.yaw, inputSequence);

        inputSequence++;
    }

    [ServerRpc(RequireOwnership = true, Delivery = RpcDelivery.Unreliable)]
    private void SubmitInputServerRpc(Vector2 move, bool jump, float targetYaw, uint sequence)
    {
        // Apply authoritative movement
        SimulateMovement(move, jump, targetYaw);

        // Send back authoritative state
        var state = new ServerState
        {
            position = transform.position,
            velocity = rb.linearVelocity,
            yaw = targetYaw,
            lastProcessedInput = sequence
        };
        SendServerStateClientRpc(state);
    }

    [ClientRpc]
    private void SendServerStateClientRpc(ServerState state)
    {
        if (!IsOwner) return;

        // Correct position if server differs
        float dist = Vector3.Distance(transform.position, state.position);
        if (dist > 0.01f)
        {
            rb.position = state.position;
            rb.linearVelocity = state.velocity;
            yaw = state.yaw;
        }

        // Remove acknowledged inputs
        pendingInputs.RemoveAll(p => p.Item1 <= state.lastProcessedInput);

        // Reapply pending unacknowledged inputs
        foreach (var (_, input) in pendingInputs)
        {
            SimulateMovement(input.move, input.jump, input.yaw);
        }
    }

    private void SimulateMovement(Vector2 move, bool jump, float targetYaw)
    {
        rb.MoveRotation(Quaternion.Euler(0f, targetYaw, 0f));
        Vector3 moveDir = (transform.forward * move.y) + (transform.right * move.x);
        Vector3 targetVel = moveDir * speed;
        rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

        if (jump && IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    private bool IsGrounded()
    {
        float checkRadius = 0.2f;
        Vector3 checkPos = transform.position + Vector3.down * (GetComponent<Collider>().bounds.extents.y - checkRadius);
        return Physics.CheckSphere(checkPos, checkRadius, LayerMask.GetMask("Ground"));
    }
}
