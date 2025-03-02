using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
	public class Xxx : IFormattable
	{
		public string txt;

		public Xxx(string txt)
		{
			this.txt = txt;
		}
	
	
		public override string ToString()
		{
			return txt;
		}

		public string ToString(string format)
		{
		   return txt;
		}

		public string ToString(string? format, IFormatProvider? provider)
		{
		   return txt;
		}

	}
}
