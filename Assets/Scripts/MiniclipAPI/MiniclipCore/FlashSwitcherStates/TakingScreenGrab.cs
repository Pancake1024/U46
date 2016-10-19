using System;
using System.IO;
using UnityEngine;
using System.Collections;

namespace com.miniclip
{
	public class TakingScreenGrab : FlashSwitcherState
	{
		private Texture2D 	_texture;
		//private Texture2D 	_singlePixel;
		
		//private int _textureWidth;
		//private int _textureHeight;
	
		public TakingScreenGrab(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> TakeScreenGrab - created!");
		}
		
		public override void Enter()
		{
			Debug.Log("-> TakeScreenGrab::Enter()");
			
			//initialize...
			//_textureWidth	= Screen.width; ///2;
			//_textureHeight	= Screen.height; ///2;
			
			//_texture = new Texture2D(1,1);
			//_texture = new Texture2D(Screen.width, Screen.height);
			
			//Note: Don't use a texture format with an alpha channel - texture with alpha channel
			//set to zero will appear as 'holes'
			_texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
			
			//_singlePixel = new Texture2D(1,1);
			//_singlePixel.SetPixel(0,0, Color.red );
		}
		
		public override void Update()
		{	
			Debug.Log("-> TakingScreenGrab::Update()");
			//do the work...
			
			string bitmapString = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWNIzqn4DwAExAJHbpI7hAAAAABJRU5ErkJggg==";
			//byte[] bitmapBytes;
			
			try
			{
				//_texture.ReadPixels(new Rect(0,0, 1, 1),0,0);
				_texture.ReadPixels(new Rect(0,0, Screen.width, Screen.height),0,0);
				_texture.Apply();
				
				//_texture.Resize( (int)Mathf.Ceil(Screen.width/2),  (int)Mathf.Ceil(Screen.height/2) );
				//_texture.Apply();
			
				QuarterTexture();	
				
				byte[] bitmapBytes = _texture.EncodeToPNG();
				bitmapString = Convert.ToBase64String(bitmapBytes);
			}
			catch(Exception exception)
			{
				Debug.Log("-> TakeScreenGrab::Update - exception: " + exception.Message);
				//if we can't ReadPixels, use the single red pixel as a bitmap background
				
				//bitmapBytes = _singlePixel.EncodeToPNG();
			}
			
			Debug.Log("PNG String Len: " + bitmapString.Length);	
			Debug.Log("PNG String: " + bitmapString);
			_flashSwitcher.bitmapString = bitmapString;		
			
			_stateMachine.ChangeState( StateId.CALLING_REQUESTED_FUNCTION );
		}
		
		private void QuarterTexture()
		{
			Texture2D temp = new Texture2D( _texture.width / 4, _texture.height / 4 );
			temp.SetPixels( _texture.GetPixels(2) ); //mipmap level 1 (i.e. half)
			temp.Apply();
			byte[] png = temp.EncodeToPNG();
			MonoBehaviour.Destroy(temp);
			_texture.LoadImage(png);
		}
				
		public override void Exit()
		{
			Debug.Log("-> TakeScreenGrab::Exit()");
			
			//clean up resources
			MonoBehaviour.Destroy( _texture );
			_texture = null;
		}
	
	}
}

