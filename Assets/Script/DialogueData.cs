using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ConvoData", menuName = "Dialogue/ConvoData")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct conversationData {
        public int speaker;
        [TextArea(10, 10)] public string text;
    }

    [System.Serializable]
    public struct characterData {
        public string name;
        public Sprite sprite;
    }
    public characterData[] speaker;
    public conversationData[] data;
}
