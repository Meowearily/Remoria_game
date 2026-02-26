using UnityEngine;

namespace Remoria.Dialogue
{
    /// <summary>
    /// ScriptableObject containing a dialogue conversation.
    /// 
    /// To create a new dialogue:
    ///   1. Right-click in the Project panel.
    ///   2. Create → RPG → Dialogue
    ///   3. Fill in the speaker name and lines.
    ///   4. Drag the asset onto an NPC's "Dialogue Data" field.
    /// 
    /// Each "line" is one text box shown to the player.
    /// The player presses E (or clicks) to advance to the next line.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "RPG/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [Header("Speaker")]
        [Tooltip("Name displayed above the dialogue text (e.g., 'Elder Mira')")]
        public string speakerName = "NPC";

        [Header("Dialogue Lines")]
        [Tooltip("Each entry is one line of dialogue shown to the player")]
        [TextArea(2, 5)] // Makes each field taller in the inspector (min 2 lines, max 5).
        public string[] lines;
    }
}
