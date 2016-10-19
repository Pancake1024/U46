using System;

namespace com.miniclip
{
	public class AwardData
	{
		protected uint _id;
		protected string _title;
		protected string _description;

		public uint Id
		{
			get{ return _id; }
		}
		
		public string Title
		{
			get{ return _title; }
		}
		
		public string Description
		{
			get{ return _description; }
		}
	
		public AwardData(uint id, string title, string description)
		{
			_id = id;
			_title = title;
			_description = description;
		}	
	}
}

