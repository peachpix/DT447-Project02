using UnityEngine;

namespace DialogueSystem
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string name;
        [TextArea(3, 10)] public string text;
    }

    [CreateAssetMenu(fileName = "DialogueSO", menuName = "Scriptable Objects/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}