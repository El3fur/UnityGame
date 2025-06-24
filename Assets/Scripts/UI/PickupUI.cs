using UnityEngine;

public class PickupUI : MonoBehaviour
{
    public static PickupUI Instance;
    public GameObject prompt;

    private void Awake()
    {
        Instance = this;
        prompt.SetActive(false);
    }

    public void ShowPrompt(bool show)
    {
        prompt.SetActive(show);
    }
}
