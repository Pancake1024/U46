using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

[CustomEditor(typeof(TrackMap))]
public class TrackMapEditor : Editor
{
    protected string imagePath = "Assets/Textures/map.png";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("padding"), new GUIContent("Padding"), true);
        serializedObject.ApplyModifiedProperties();

        if (!serializedObject.isEditingMultipleObjects)
        {
            TrackMap map = (TrackMap)target;

            EditorGUILayout.LabelField("Bounds: " + map.bounds);
            map.image = (Texture2D)EditorGUILayout.ObjectField("Image", map.image, typeof(Texture2D), false);

            map.imageWidth = EditorGUILayout.IntField("Width", map.imageWidth);
            map.imageHeight = EditorGUILayout.IntField("Height", map.imageHeight);
            if (null == map.image)
                imagePath = EditorGUILayout.TextField("Image Path", imagePath);
            else
            {
                imagePath = AssetDatabase.GetAssetPath(map.image);
                EditorGUILayout.LabelField(new GUIContent("Image Path"), new GUIContent(imagePath));
            }

            if (GUILayout.Button("Render Map"))
            {
                byte[] pngData = map.CreateTexture(map.imageWidth, map.imageHeight, imagePath);

                System.IO.File.WriteAllBytes(imagePath, pngData);
                AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);

                TextureImporter imageImp = TextureImporter.GetAtPath(imagePath) as TextureImporter;
                imageImp.textureType = TextureImporterType.Advanced;
                imageImp.npotScale = TextureImporterNPOTScale.None;
                imageImp.mipmapEnabled = false;
                imageImp.isReadable = false;
                imageImp.linearTexture = true;
                imageImp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                imageImp.wrapMode = TextureWrapMode.Clamp;
                imageImp.filterMode = FilterMode.Point;
                AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);

                map.image = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath, typeof(Texture2D));
            }
        }
    }
}