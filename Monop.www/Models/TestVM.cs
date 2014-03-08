using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monop.www.Models
{
	public class TestVM
	{
		public bool isDebug { get; set; }
		public List<TestPlayer> Players { get; set; }
	}

	public class TestPlayer
	{
		public string p0m { get; set; }
		public string p0p { get; set; }
		public string p0c { get; set; }
		public bool p0h { get; set; }
	}

	
}