using System;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSAngleAxis
#else
    public struct SBSAngleAxis
#endif
	{
#if UNITY_FLASH
		public float angle = 0.0f;
		public SBSVector3 axis = new SBSVector3(0.0f, 0.0f, 0.0f);
#else
        public float angle;
        public SBSVector3 axis;
#endif

#if UNITY_FLASH
        public SBSAngleAxis()
		{
		}
#endif

        public SBSAngleAxis(float _angle, SBSVector3 _axis)
		{
			angle  = _angle;
			axis.x = _axis.x;
			axis.y = _axis.y;
			axis.z = _axis.z;
        }
		
		public SBSAngleAxis(float _angle, float ax, float ay, float az)
		{
			angle  = _angle;
			axis.x = ax;
			axis.y = ay;
			axis.z = az;
        }

        override public string ToString()
        {
            return "(" + axis.x + ", " + axis.y + ", " + axis.z + ", " + angle + ")";
        }
    }
}

