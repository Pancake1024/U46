using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.miniclip.awards
{	
	public class AwardsConfig
	{
		private Vector3 		_start;
		private float 			_speed;
		private List<Vector3>	_slots;
		
		public Vector3 Start
		{
			get{ return _start; }
		}
		
		public float Speed
		{
			get{ return _speed; }
		}
		
		public List<Vector3> Slots
		{
			get{ return _slots; }
		}
		
		
		
		public AwardsConfig(Vector3 offscreen, float speed, List<Vector3> slots)
		{
			_start = offscreen;
			_speed = speed;
			_slots = slots;	
		}	
		
	}
}
