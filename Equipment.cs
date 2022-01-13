using System;
using System.Collections.Generic;

namespace EllyGGEpochFilter
{
	public class Equipment
	{
		public string type;
		public xmlNode xml;

		public Equipment(string inputType, xmlNode inputXml)
		{
			type = inputType;
			xml = inputXml;
		}
	}
}