using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class AwardSlot
	{
		private Vector3 _position;
		private AwardNotification _usedBy = null;
		
		public bool IsAvailable
		{
			get
			{
				return (_usedBy == null);
			}
		}
			
		public Vector3 Position
		{
			get{ return _position; }
		}
		
		public AwardNotification UsedBy
		{
			get{ return _usedBy; }
			set
			{
				_usedBy = value;
			}
		}
			
		public AwardSlot(Vector3 position)
		{
			_position = position;
		}
	}
}

