using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject Player;
    void Awake()
    {
        Sonic_PlayerStateMachine ctx = Player.GetComponent<Sonic_PlayerStateMachine>();
        ctx.Chs.SpawnData = new PosRot() { Position = transform.position, Rotation = transform.rotation };
        ctx.Physics_Snap(transform.position);
    }
}
