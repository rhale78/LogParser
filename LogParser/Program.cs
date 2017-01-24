using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser
{
	class Program
	{
		static void Main(string[] args)
		{
			//Parser p = new Parser("c:\\tmp");
			Parser p = new Parser(@"\\10.30.4.60\PRD-Server-Logs-Archive\webservices");
			p.Parse();
		}
	}
}
