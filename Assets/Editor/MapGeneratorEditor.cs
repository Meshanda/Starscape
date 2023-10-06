using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StrateGeneration))]
public class MapGene1ratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StrateGeneration mapGen = (StrateGeneration)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.InitializeGrid();

            }
        }

        if (GUILayout.Button("Geneate"))
        {
            if (mapGen.autoUpdate)
            {
                mapGen.InitializeGrid();
            }
        }
        if (GUILayout.Button("Torche"))
        {
            mapGen.Fog.UpdateAllTorcheLight();
        }
    }
}

