using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        public Dialogue dialogue;

        [Header("Events")]
        [Space]
        public UnityEvent OnDialogueEnds;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        //void Start() { }

        // Update is called once per frame
        //void Update() { }

        [ContextMenu("Start Dialogue")]
        public void StartDialogue()
        {
            DialogueManager.Instance.UnsubscribeDialogueEnds();
            DialogueManager.Instance.SubscribeDialogueEndsEvent(OnDialogueEnds);
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}

