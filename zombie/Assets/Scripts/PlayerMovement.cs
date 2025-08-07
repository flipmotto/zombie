using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (IsOwner)
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
            }

            Vector3 move = new Vector3(input.x, 0, input.y) * speed * Time.deltaTime;
            SubmitMovementRequestServerRpc(move);
        }

        // Only non-server clients should apply synced position updates
        if (!IsServer)
        {
            transform.position = syncedPosition.Value;
        }
    }

    [ServerRpc]
    private void SubmitMovementRequestServerRpc(Vector3 move)
    {
        transform.position += move;
        syncedPosition.Value = transform.position;
    }
}