using UnityEngine;
using System.Collections;

public class Question
{
	private string _text = "";
	private string _image = "";
	private string _movie = "";
	private uint _correctAnswerIndex = 0;
	private string[] _answers;
	
			
	public string Text 
	{
		get{ return _text; }
		set{ _text = value; }
	}
		
	public string Image
	{
		get{ return _image; }
		set{ _image = value; }
	}
	
	public string Movie
	{
		get{ return _movie; }
		set{ _movie = value; }
	}
	
	public uint CorrectAnswerIndex
	{
		get{ return _correctAnswerIndex; }
		set{ _correctAnswerIndex = value; }
	}
	
	public string[] Answers
	{
		get{ return _answers; }
		set{ _answers = value; }
	}
	
	
	
	
	public Question()
	{
		this._answers = new string[4];	
	}
}
