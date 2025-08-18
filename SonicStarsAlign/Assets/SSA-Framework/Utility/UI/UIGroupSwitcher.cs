using UnityEngine;

// Use for switching tabs and UIs on a single scene.
public class UIGroupSwitcher : MonoBehaviour
{
    public GameObject[] groups;
    public byte activeGroup = 0;

    void OnEnable()
    {
        SwitchGroup(activeGroup);
    }

    public void SwitchGroup(int gr_id)
    {
        for (int i=0; i<groups.Length; i++) // i know foreach exists
        {
            if (i == gr_id)
            {
                groups[i].SetActive(true);
            }
            else
            {
                groups[i].SetActive(false);
            }
        }
    }
}