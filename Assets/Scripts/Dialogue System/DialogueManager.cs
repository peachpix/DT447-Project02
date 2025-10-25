using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public bool IsOngoing { get; private set; } = false;

        [Header("UI")]
        [SerializeField] private RectTransform dialogeCanvas;
        [SerializeField] private TMPro.TextMeshProUGUI nameText;
        [SerializeField] private TMPro.TextMeshProUGUI dialogueText;

        [Header("Dialogue Setting")]
        [SerializeField] private float textSpeed = 10f;

        private bool isTyping = false;
        private Queue<DialogueLine> sentences;
        private string currentSentence;
        private Coroutine typingCoroutine;
        private UnityEvent OnDialogueEnds;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            OnDialogueEnds = new UnityEvent();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            sentences = new Queue<DialogueLine>();
            dialogeCanvas.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AdvanceDialogue();
            }
        }

        public void ShowDialogePanel()
        {
            Cursor.lockState = CursorLockMode.None;
            // Lock Look Movement
            dialogeCanvas.gameObject.SetActive(true);
        }

        public void HideDialoguePanel()
        {
            Cursor.lockState = CursorLockMode.Locked;
            // Unlock Look Movement
            dialogeCanvas.gameObject.SetActive(false);
        }

        public void StartDialogue(Dialogue dialogue)
        {
            sentences.Clear();
            IsOngoing = true;

            foreach (DialogueLine line in dialogue.lines)
            {
                sentences.Enqueue(line);
            }

            ShowDialogePanel();
            AdvanceDialogue();
        }

        public void AdvanceDialogue()
        {
            if (isTyping)
            {
                CompleteSentence(currentSentence);
                return;
            }

            if (sentences.Count <= 0)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = sentences.Dequeue();

            if (line.name != "" && nameText != null)
                nameText.SetText(line.name);

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            currentSentence = line.text;
            typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
        }

        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;

            dialogueText.SetText("");
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.SetText(dialogueText.text + letter);
                yield return new WaitForSeconds(1f / textSpeed);
            }

            isTyping = false;
        }

        private void CompleteSentence(string sentence)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.SetText(sentence);
            isTyping = false;
        }

        public void EndDialogue()
        {
            HideDialoguePanel();
            OnDialogueEnds?.Invoke();
            IsOngoing = false;
        }

        public void SubscribeDialogueEndsEvent(UnityEvent ext)
        {
            OnDialogueEnds?.AddListener(ext.Invoke);
        }

        public void UnsubscribeDialogueEnds()
        {
            OnDialogueEnds?.RemoveAllListeners();
        }
    }
}
