using UnityEngine;

public class AttackPanel : MonoBehaviour
{
    public AttackLibrary library;

    private void OnEnable()
    {
        library.Initialize();
    }

    private void Update()
    {
        library.AttackUpdate(Time.deltaTime);
    }

    public void StartAttack(string _attack)
    {
        library.StartAttack(_attack);
    }

    public void StopAttack(string _attack)
    {
        library.StopAttack(_attack);
    }
}