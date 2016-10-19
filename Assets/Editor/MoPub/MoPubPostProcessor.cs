using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;
using System.Diagnostics;


public class MoPubPostProcessor : MonoBehaviour
{
	[PostProcessBuild( 200 )]
	private static void onPostProcessBuildPlayer( BuildTarget target, string pathToBuiltProject )
	{
		return;
		/*
		if( target == BuildTarget.iPhone )
		{
			// grab the path to the postProcessor.py file
			var scriptPath = Path.Combine( Application.dataPath, "Editor/MoPub/MoPubPostProcessor.py" );

			// sanity check
			if( !File.Exists( scriptPath ) )
			{
				UnityEngine.Debug.LogError( "MoPub post processor could not find the MoPubPostProcessor.py file. Did you accidentally delete it?" );
				return;
			}

			var pathToMoPubFolder = Path.Combine( Application.dataPath, "Editor/MoPub" );

			var args = string.Format( "\"{0}\" \"{1}\" \"{2}\"", scriptPath, pathToBuiltProject, pathToMoPubFolder );
			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "python2.6",
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
		}
		*/
	}
}
