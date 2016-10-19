using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;
using System.Diagnostics;


public class MoPubPostProcessorPrivate : MonoBehaviour
{
	[MenuItem( "MoPub/Create unitypackages" )]
	static void createPackage()
	{
		var unitypackageExportFolder = Path.Combine( Directory.GetParent( Directory.GetParent( Application.dataPath ).FullName ).FullName, "UnitypackageExport" );
		var nativeCodeFolder = Path.Combine( Application.dataPath, "Editor/MoPub/NativeCode" );

		// find all subdirectories
		var allDirs = Directory.GetDirectories( nativeCodeFolder )
			.Where( d => !d.EndsWith( "MoPubSDK" ) );

		foreach( var dir in allDirs )
		{
			var allFiles = Directory.GetFiles( dir, "*", SearchOption.TopDirectoryOnly )
				.Where( file => !file.EndsWith( ".meta" ) )
				.Where( file => !file.EndsWith( ".DS_Store" ) )
				.Select( file => file.Replace( Application.dataPath, "Assets" ) ).ToList();

			var txtFile = Directory.GetFiles( dir, "*.txt", SearchOption.AllDirectories )
				.Select( file => file.Replace( Application.dataPath, "Assets" ) ).FirstOrDefault();
			if( txtFile != null )
				allFiles.Add( txtFile );

			var packageName = Path.GetFileName( dir ) + "Support.unitypackage";
			UnityEngine.Debug.Log( packageName + ": " + allFiles.Count );
			AssetDatabase.ExportPackage( allFiles.ToArray(), Path.Combine( unitypackageExportFolder, packageName ) );
		}

		createMainPackage( unitypackageExportFolder );
	}


	// debug method to select all files to ensure we are making the unitypackage with the files we want
	private static void select( List<string> files )
	{
		var allAssets = new List<UnityEngine.Object>();
		var allPaths = new List<string>();
		foreach( var path in files )
		{
			var actualAsset = AssetDatabase.LoadAssetAtPath( path, typeof( UnityEngine.Object ) );
			if( actualAsset == null )
			{
				UnityEngine.Debug.LogError( "Things went really, really wrong. Couldnt find asset at path: " + path );
			}
			else
			{
				allPaths.Add( path );
				allAssets.Add( actualAsset );
			}
		}

		// select our items and make the unitypackage
		Selection.objects = allAssets.ToArray();
	}


	private static bool isAllowedFile( string file )
	{
		var allowedJars = new string[] { "android-support-v4.jar", "MoPubPlugin.jar" };

		if( Path.GetExtension( file ).Equals( ".jar" ) && !allowedJars.Contains( Path.GetFileName( file ) ) )
			return false;

		return true;
	}


	static void createMainPackage( string unitypackageExportFolder )
	{
		// grab all the files and create a unitypackage
		var allFiles = Directory.GetFiles( Application.dataPath, "*", SearchOption.AllDirectories )
			.Where( file => isAllowedFile( file ) )
			.Where( file => Path.GetFileName( file ) != "mod_pbxproj.pyc" )
			.Where( file => !file.EndsWith( ".meta" ) )
			.Where( file => !file.EndsWith( ".DS_Store" ) )
			.Where( file => !file.Contains( "google-play-services_lib" ) )
			.Where( file => !file.Contains( "NativeCode" ) )
			.Where( file => !file.Contains( "Private" ) )
			.Where( file => !file.Contains( "private" ) )
			.Select( file => file.Replace( Application.dataPath, "Assets" ) ).ToList();

		// now we need to add the NativeCode/MoPubSDK folder
		var allMoPubFiles = Directory.GetFiles( Path.Combine( Application.dataPath, "Editor/MoPub/NativeCode/MoPubSDK" ), "*", SearchOption.AllDirectories )
			.Where( file => !file.EndsWith( ".meta" ) )
			.Where( file => !file.EndsWith( ".DS_Store" ) )
			.Select( file => file.Replace( Application.dataPath, "Assets" ) ).ToList();
		allFiles.AddRange( allMoPubFiles );

		select( allFiles );

		var packageName = "MoPubPlugin.unitypackage";
		UnityEngine.Debug.Log( packageName + ": " + allFiles.Count );
		AssetDatabase.ExportPackage( allFiles.ToArray(), Path.Combine( unitypackageExportFolder, packageName ) );
	}

}
