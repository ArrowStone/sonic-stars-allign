using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sonic_WinState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    public Sonic_WinState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    //Left blank for future changes
    public void EnterState()
    {
        IEnumerator ExitCoroutine()
        {
            yield return new WaitForSeconds(2f);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(0);
        }
        _ctx.StartCoroutine(ExitCoroutine());
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
    }
}