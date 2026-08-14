using UnityEngine;
using UnityEngine.UI;

// Used to make the theatre button in extras activate after collecting all red rings
public class ButtonUnlock : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private Color disabledColour;
    void Awake()
    {
        Button button = GetComponent<Button>();
        Image image = GetComponent<Image>();

        bool collectedRedRings = true;

        foreach (bool ringCollected in gameStateManager.StageDataAssets[0].RedRings)
        {
            collectedRedRings = collectedRedRings && ringCollected;
        }

        button.enabled = collectedRedRings; // Would prefer to toggle interactable but trying to change that appears to actually do nothing because Unity is stupid
        image.color = collectedRedRings ? image.color : disabledColour;
        transform.GetChild(0).GetComponent<Image>().color = collectedRedRings ? image.color : disabledColour;
    }
}
