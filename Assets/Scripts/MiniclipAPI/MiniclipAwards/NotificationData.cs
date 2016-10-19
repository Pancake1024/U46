using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class NotificationData : AwardData
	{	
		private byte[]	_bitmapData;
		private int		_width;
		private int 	_height;
	
		public int Width
		{
			get{ return _width;}
		}
			
		public int Height
		{
			get{ return _height; }
		}
		
		public byte[] BitmapData
		{
			get{ return _bitmapData; }
			set
			{
				_bitmapData = value;
				ExtractDimensionsFromBitmapData();
			}
		}
		
		public NotificationData(uint id, string title, string description, byte[] bitmapData) : base(id, title, description)
		{
			this.BitmapData = bitmapData;
		}
		
		
		private void ExtractDimensionsFromBitmapData()
		{
			if(_bitmapData == null && _bitmapData.Length < 1)
				return;
						
			//The bitmap Width is defined at byte index [16] of the PNG bitmapData, and is 4 bytes long  
			this._width = ConvertBytesToDimension(16);
			
			//The bitmap Height is defined at byte index [20] of the PNG bitmapData, and is 4 bytes long 
			this._height = ConvertBytesToDimension(20);
			
			Debug.Log("-> NotificationData::ExtractDimensionsFromBitmapData - width: " + _width + ", height: " + _height);
		}
		
		private int ConvertBytesToDimension(uint startByteIndex)
		{
			byte[] fourBytes = new byte[4];
			
			for(int i=0; i < 4; i++)
			{ 
				fourBytes[i] = _bitmapData[(i+startByteIndex)];
			}
			
			if (BitConverter.IsLittleEndian)
				Array.Reverse( fourBytes );
			
			return BitConverter.ToInt32(fourBytes, 0);
		}
			
	}
}

