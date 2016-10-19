using System;  
using System.Collections.Generic;  
using System.Linq;  
using UnityEngine;  

public class CommandLineReader  
{  
	public static string GetCustomArgument(string argumentName)  
	{  
		var customArgsDict = GetCustomArguments();  
		string argumentValue;  
		
		if (customArgsDict.ContainsKey(argumentName))  
		{  
			argumentValue = customArgsDict[argumentName];  
		}  
		else  
		{  
			Debug.Log("GetCustomArgument() Can't retrieve any custom argument named [" + argumentName + "] in command line [" + GetCommandLine() + "].");  
			argumentValue = String.Empty;  
		}  
		
		Debug.Log("CommandLineReader " + argumentName + "=\"" + argumentValue + "\"");  
		return argumentValue;  
	}  
	
	//Config  
	private const string CUSTOM_ARGS_PREFIX = "-CustomArgs:";  
	private const char CUSTOM_ARGS_SEPARATOR = ';';  
	
	private static string[] GetCommandLineArgs()  
	{  
		return Environment.GetCommandLineArgs();  
	}  
	
	private static string GetCommandLine()  
	{  
		string[] args = GetCommandLineArgs();  
		if (args.Length > 0)  
		{  
			return string.Join(" ", args);  
		}  
		else  
		{  
			Debug.LogError("GetCommandLine() - Can't find any command line arguments!");  
			return String.Empty;  
		}  
	}  
	
	private static Dictionary<string, string> GetCustomArguments()  
	{  
		var customArgsDict = new Dictionary<string, string>();  
		string[] commandLineArgs = GetCommandLineArgs();  
		
		string customArgsStr;  
		try  
		{  
			customArgsStr = commandLineArgs.SingleOrDefault(row => row.Contains(CUSTOM_ARGS_PREFIX));  
			if (String.IsNullOrEmpty(customArgsStr))  
			{  
				return customArgsDict;  
			}  
		}  
		catch (Exception e)  
		{  
			Debug.LogError("GetCustomArguments() - Can't retrieve any custom arguments in command line [" + commandLineArgs + "]. Exception: " + e);  
			return customArgsDict;  
		}  
		
		customArgsStr = customArgsStr.Replace(CUSTOM_ARGS_PREFIX, String.Empty);  
		string[] customArgs = customArgsStr.Split(CUSTOM_ARGS_SEPARATOR);  
		
		foreach (string customArg in customArgs)  
		{  
			string[] customArgBuffer = customArg.Split('=');  
			if (customArgBuffer.Length == 2)  
			{  
				customArgsDict.Add(customArgBuffer[0], customArgBuffer[1]);  
			}  
			else  
			{  
				Debug.LogWarning("GetCustomArguments() - The custom argument [" + customArg + "] seems to be malformed.");  
			}  
		}  
		return customArgsDict;  
	}  
}  