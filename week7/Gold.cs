using Fusion;
using System.Collections;
using UnityEngine;

public class Gold : NetworkBehaviour
{
    public GameObject playerPrefab;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            RPC_PlayerWon(player.Object.InputAuthority);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayerWon(PlayerRef winner)
    {
        GameManager.Instance.ShowWinner(winner);
        if (HasStateAuthority)
            StartCoroutine(RespawnAllAfterDelay(5f));
    }

    private IEnumerator RespawnAllAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var obj in Runner.ActivePlayers)
        {
            // Despawn old player
            Player oldPlayer = Runner.GetPlayerObject(obj).GetComponent<Player>();
            Runner.Despawn(oldPlayer.Object);
            Runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, obj);
        }
    }
}
