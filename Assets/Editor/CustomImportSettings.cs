using UnityEngine;
using UnityEditor;
using System;

public class CustomImportSettings : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;

        importer.importMaterials = true;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.RecursiveUp;

        //importer.generateAnimations = ModelImporterGenerateAnimations.None;
    }
}
