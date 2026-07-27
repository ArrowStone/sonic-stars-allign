using UnityEngine;

public class AttackPass : MonoBehaviour
{
    public AttackPanel Panel;

    public void StartAttack(string _attack)
    {
        Panel.library.StartAttack(_attack);
    }

    public void StopAttack(string _attack)
    {
        Panel.library.StopAttack(_attack);
    }
}