using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Character : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner)
        {
            // Desabilita os controles se não for o jogador local
            GetComponent<CharacterMovementWithAnimator>().enabled = false;
        }
    }
}
