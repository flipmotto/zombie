using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public GameObject panel;
    public Button quitButton;
    public Button menuButton;
    private PlayerMovement pm;

    private void Awake()
    {
        quitButton.onClick.AddListener(Quit);
        menuButton.onClick.AddListener(Menu);
    }

    private void OnEnable()
    {
        // If local player already exists, hook now
        if (PlayerMovement.Instance != null)
            Hook(PlayerMovement.Instance);

        // Also listen for future spawn/despawn
        PlayerMovement.LocalPlayerSpawned += Hook;
        PlayerMovement.LocalPlayerDespawned += UnhookOnDespawn;
    }

    private void OnDisable()
    {
        PlayerMovement.LocalPlayerSpawned -= Hook;
        PlayerMovement.LocalPlayerDespawned -= UnhookOnDespawn;
        Unhook();
    }

    private void Hook(PlayerMovement newPm)
    {
        Unhook();
        pm = newPm;
        pm.InMenuChanged += OnMenuChanged;

        // Emit current state once on hook
        OnMenuChanged(pm.InMenu);
    }

    private void UnhookOnDespawn()
    {
        Unhook();
    }

    private void Unhook()
    {
        if (pm != null)
        {
            pm.InMenuChanged -= OnMenuChanged;
            pm = null;
        }
    }

    private void OnMenuChanged(bool inMenu)
    {
        panel.SetActive(inMenu);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
