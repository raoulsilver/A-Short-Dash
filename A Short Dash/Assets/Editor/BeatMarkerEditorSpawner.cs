using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BeatMarkerEditorSpawner
{
    static BeatMarkerEditorSpawner()
    {
        EditorApplication.update += ListenForBeatKey;
    }

    private static void ListenForBeatKey()
    {
        if (!EditorApplication.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnMarkerAtPlayer();
        }
    }

    static void SpawnMarkerAtPlayer()
    {
        // find player
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Beat Marker";
        marker.transform.position = player.transform.position;

        // supposed to make it persistant but not working?
        Undo.RegisterCreatedObjectUndo(marker, "Create Beat Marker");
    }
}