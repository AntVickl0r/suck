using System;
using System.IO;
using System.Net;
using System.Threading;

namespace suck
{
	public static class sucker
	{
		private static int _consoleWidth = 80;
		private static UInt64 _contentLength = 0;

		private static byte[] _content;

		private static bool _unknownFileSize = false;

		public static void suck(string url, string dest, int bufferSize = 4096)
		{
			_contentLength = getContentLength (url);
			_content = loadFile (url, bufferSize);
			safeFile (dest, _content);
		}

		private static UInt64 getContentLength(string url)
		{
			WebRequest req = HttpWebRequest.Create(url);
			req.Method = "HEAD";
			using (WebResponse response = req.GetResponse())
			{
				UInt64 ContentLength;
				if (UInt64.TryParse (response.Headers.Get ("Content-Length"), out ContentLength)) { 
					_unknownFileSize = false;
					return ContentLength;
				} else {
					_unknownFileSize = true;
					return 0;
				}
			}
		}

		private static byte[] loadFile(string url, int bufferSize)
		{
			byte[] result;
			//byte[] buffer = new byte[bufferSize % 524288]; maximum buffersize?
			byte[] buffer = new byte[bufferSize];

			WebRequest wr = WebRequest.Create(url);

			using(WebResponse response = wr.GetResponse())
			{
				using(Stream responseStream = response.GetResponseStream())
				{
					using(MemoryStream memoryStream = new MemoryStream())
					{
						int count = 0;
						UInt64 bytesLoaded = 0;
						DateTime lastTick = DateTime.Now;
						drawStatus (url, _contentLength, 0);
						do
						{
							count = responseStream.Read(buffer, 0, buffer.Length);
							memoryStream.Write(buffer, 0, (int)count);
							bytesLoaded += (uint)count;
							if(DateTime.Now.Subtract(lastTick).TotalMilliseconds >= 500)
							{
								lastTick = DateTime.Now;
								drawStatus (url, _contentLength, bytesLoaded);
							}
							// SIMULATE SLOW CONNECTION 4 DEBUGGER
							//Thread.Sleep(10);
							//
						} while(count != 0);
						drawStatus (url, _contentLength, bytesLoaded);
						Console.WriteLine ();
						result = memoryStream.ToArray();
						memoryStream.Close ();
					}
					responseStream.Close ();
				}
				response.Close ();
			}

			return result;
		}

		private static void safeFile(string dest, byte[] data)
		{
			using (FileStream fileStream = new FileStream (dest, FileMode.Create))
			{
				fileStream.Write (data, 0, data.Length);
				fileStream.Close ();
			}
		}

		private static void drawStatus(string path, UInt64 bytesTotal, UInt64 bytesLoaded)
		{

			if (_unknownFileSize) {
				// TODO draw animtation for unknown filesize
			}
			int allowedPathLength = _consoleWidth - 41;
			string printablePath = "";
			if (path.Length > allowedPathLength) {
				printablePath = "..." + path.Substring (path.Length - allowedPathLength + 3);
			} else {
				printablePath += path;
				for (int i = 0; i < allowedPathLength - path.Length; i++) {
					printablePath += " ";
				}
			}
			Console.CursorLeft = 0;
			ConsoleColor bc = Console.BackgroundColor;
			ConsoleColor fc = Console.ForegroundColor;
			// TODO ConsoleColor 4 Unix
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			Console.Write(" {0} ]", new string[] { printablePath });
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				Console.ForegroundColor = ConsoleColor.Green;
			}
			Console.Write(barBuilder(bytesTotal, bytesLoaded));
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			Console.Write("[ {0}", new string[] { fileSizeString(bytesLoaded) + "|" + fileSizeString(bytesTotal) });

			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				Console.BackgroundColor = bc;
				Console.ForegroundColor = fc;
			}
		}

		private static string barBuilder(UInt64 bytesTotal, UInt64 bytesLoaded)
		{
			int steps = (int)(bytesLoaded / (bytesTotal / 20));
			string output = "";
			if (steps == 0) {
//				output = "";
			}
			else if (steps == 1) {
				output = "]";
			}
			else if (steps == 2) {
				output = "[]";
			}
			else {
				output = "[";
				for (int i = 0; i < steps - 2; i++) {
					output += ">";
				}
				output += "]";
			}
			int spacesNeeded = 20 - output.Length;
			for (int i = 0; i < spacesNeeded; i++) {
				output += " ";
			}
			return output;
		}

		private static string fileSizeString(UInt64 fileSize)
		{
			string[] suffixes = { "B ", "kB", "MB", "GB", "TB", "PB", "EB", "ZB" };
			int suffix = 0;
			double dFileSize = (double)fileSize;
			while ((dFileSize >= 1000.0) && (suffix < suffixes.Length - 1))
			{
				dFileSize /= 1024.0;
				suffix++;
			}
			string fsString = "";
			if (((int)dFileSize).ToString ().Length == 1) {
				fsString = dFileSize.ToString ("0.000");
			}
			else if (((int)dFileSize).ToString ().Length == 2) {
				fsString = dFileSize.ToString ("0.00");
			}
			else {
				fsString = dFileSize.ToString ("0.0");
			}
			return String.Format("{0}{1}", fsString, suffixes[suffix]);
		}
	}
}

