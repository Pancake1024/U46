using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine;

class JenkinsBuild 
{
	static string[] SCENES = FindEnabledEditorScenes();
	
	[MenuItem ("Custom/CI/Build Mac OS X")]
	static void PerformMacOSXBuild ()
	{
		string buildPath = CommandLineReader.GetCustomArgument("path");  
		bool appendBuild = CommandLineReader.GetCustomArgument("append")=="true"; 
		if(appendBuild)
			GenericBuild(SCENES, buildPath, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);
		else
			GenericBuild(SCENES, buildPath, BuildTarget.iOS, BuildOptions.None);
	}
	
	private static string[] FindEnabledEditorScenes() {
		List<string> EditorScenes = new List<string>();
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}
	
	static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
		string res = BuildPipeline.BuildPlayer(scenes,target_dir,build_target,build_options);
		if (res.Length > 0) {
			throw new Exception("BuildPlayer failure: " + res);
		}
	}
}