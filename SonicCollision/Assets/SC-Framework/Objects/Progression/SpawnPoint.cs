using UnityEngine;

// Fairly straightforward
public class SpawnPoint : MonoBehaviour
{
    public GameObject Player;
    void Awake()
    {
        SetPos();
    }
    public void SetPos()
    {
        Sonic_PlayerStateMachine ctx = Player.GetComponent<Sonic_PlayerStateMachine>();
        ctx.Chs.SpawnData = new PosRot() { Position = transform.position, Rotation = transform.rotation };
        ctx.Physics_Snap(transform.position);
    }
}
