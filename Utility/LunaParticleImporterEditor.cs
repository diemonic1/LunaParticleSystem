#if UNITY_EDITOR

using PlayablesPlugins;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayablesPlugins.LunaParticleSystem))]
public class LunaParticleImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        Color oldColor = GUI.backgroundColor;

        GUI.backgroundColor = Color.black;

        if (GUILayout.Button("Open Documentation"))
        {
            Application.OpenURL("https://github.com/diemonic1/LunaParticleSystem");
        }
        
        GUI.backgroundColor = new Color(0.02f, 0.57f, 0.08f);
        
        if (GUILayout.Button("Open compatibility table"))
        {
            Application.OpenURL("https://docs.google.com/spreadsheets/d/1yAGQokKCIa_cuVtBEkPlxS2xls-0DV8M09hwa01aYk0/edit?pli=1&gid=0#gid=0");
        }

        GUI.backgroundColor = oldColor;
        GUILayout.EndHorizontal();
        
        DrawDefaultInspector();

        PlayablesPlugins.LunaParticleSystem importer = (PlayablesPlugins.LunaParticleSystem)target;
        
        GUI.backgroundColor = new Color(0.2f, 0.88f, 0.27f);

        if (GUILayout.Button("Apply JSON", GUILayout.Height(40)))
        {
            importer.SetDataInEditor(importer.Json);

            importer.Json = "";

            EditorUtility.SetDirty(importer);

            if (importer.gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager
                    .MarkSceneDirty(importer.gameObject.scene);
            }
        }
        
        GUI.backgroundColor = oldColor;
    }
}

#endif