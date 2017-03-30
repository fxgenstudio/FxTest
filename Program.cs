using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxTest
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			using (var game = new Game1())
				game.Run();
		}
	}
}
