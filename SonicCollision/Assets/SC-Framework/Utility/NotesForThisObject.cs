using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class NotesForThisObject : MonoBehaviour
{
    public string notes;

    public GUIStyle NoteStyle;

    private void OnValidate()
    {
        if (NoteStyle == null) { NoteStyle = new GUIStyle(EditorStyles.textArea); }

        NoteStyle.wordWrap = true;
    }
}

[CustomEditor(typeof(NotesForThisObject))]
public class NotesEditor : Editor
{

    NotesForThisObject notesTarget;

    public override void OnInspectorGUI()
    {
        // Get reference to the target object
        notesTarget = (NotesForThisObject)target;

        DrawNotes();

        DrawEditNotesStyle();

    }

    private void DrawNotes()
    {

        // Always start with change check if you want undo/redo support
        EditorGUI.BeginChangeCheck();

        // Draw an editable multi-line text area
        notesTarget.notes = EditorGUILayout.TextArea(
            notesTarget.notes,
            notesTarget.NoteStyle
        );

        // If something changed, mark object dirty so Unity saves it + supports Undo
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(notesTarget, "Edit Notes");
            EditorUtility.SetDirty(notesTarget);
        }
    }

    private void DrawEditNotesStyle()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("NoteStyle"));
        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(notesTarget, "Edit Notes Style");
            EditorUtility.SetDirty(notesTarget);
        }

    }
}

#endif
