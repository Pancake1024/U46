using UnityEngine;
using System.Collections;

public class CallArgs
{
	public string functionName;
	public string data;
	
	public CallArgs(string functionName)
	{
		this.functionName = functionName;
		this.data = null;
	}
	
	public CallArgs(string functionName, string data)
	{
		this.functionName = functionName;
		this.data = data;
	}
}
