using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("References")]
    public GameObject dialogueCanvas; // Assign your dialogue UI here
    public MonoBehaviour playerLookScript;   // Your camera look script (e.g., MouseLook)
    public KeyCode interactKey = KeyCode.Q;
    public string npcName = "NPC";

    private bool playerInRange = false;

    void Start()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // Hide cursor at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleDialogue();
        }
    }

    void ToggleDialogue()
    {
        if (dialogueCanvas == null) return;

        bool isActive = dialogueCanvas.activeSelf;
        dialogueCanvas.SetActive(!isActive);

        if (!isActive)
        {
            // Lock camera movement & show mouse
            if (playerLookScript != null)
                playerLookScript.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Unlock camera movement & hide mouse
            if (playerLookScript != null)
                playerLookScript.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"{npcName}: Player entered range.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(false);

            // restore camera & cursor when leaving NPC
            if (playerLookScript != null)
                playerLookScript.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log($"{npcName}: Player left range.");
        }
    }
}
