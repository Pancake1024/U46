using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBS.Racing
{
	public interface IOcean
	{
        float GetWaterHeightAtLocation(float x, float y);
	}
}
