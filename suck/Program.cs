using System;
using System.IO;
using System.Reflection;

namespace suck
{
	class MainClass
	{
		private static string _url = "";
		private static string _dest = "";
		public static void Main (string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("usage:");
				Console.WriteLine ("suck url [destination [buffersize]]");
				Console.WriteLine ("examples:");
				Console.WriteLine ("suck http://yourdomain.tdl/file.txt");
				Console.WriteLine ("suck http://yourdomain.tdl/file.txt /home/username/downloads/file.txt");
				Console.WriteLine ("suck http://yourdomain.tdl/file.txt /home/username/downloads/file.txt 4096");
			} else {
				_url = args [0];
				if (args.Length == 1) {
					string[] urlSplit = _url.Split (new string[] { "/" }, StringSplitOptions.None);
					_dest = Environment.CurrentDirectory + "/" + urlSplit [urlSplit.Length - 1];
				} else {
					_dest = args [1];
				}
				try
				{
					if(args.Length >= 3) {
						sucker.suck (_url, _dest, int.Parse(args[2]));
					}
					else {
						sucker.suck (_url, _dest);
					}
				}
				catch(Exception e) {
					ConsoleColor bc = Console.BackgroundColor;
					ConsoleColor fc = Console.ForegroundColor;
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine (e.Message);
					Console.BackgroundColor = bc;
					Console.ForegroundColor = fc;
					Console.ReadLine ();
				}
			}
		}
	}
}
