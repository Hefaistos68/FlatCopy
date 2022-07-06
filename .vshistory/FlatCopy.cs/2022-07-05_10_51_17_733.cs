using System.Xml.Linq;

namespace FlatCopy
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string sourceDirectory = Environment.CurrentDirectory;
			string archiveDirectory = string.Empty;

			if(args.Length < 2)
			{
				Usage();
				return;
			}

			string pattern = args[0];
			bool bMove = false;

			// when having commandline switch /c it will copy instead of moving the files
			if (args.Contains("/m"))
			{
				bMove = true;
			}

			try
			{
				var allFiles = Directory.EnumerateFiles(sourceDirectory, pattern, SearchOption.AllDirectories);

				foreach (string currentFile in allFiles)
				{
					string fileName = currentFile.Substring(sourceDirectory.Length + 1);
					if(bMove)
					{
						Directory.Move(currentFile, Path.Combine(archiveDirectory, fileName));
					}
					else
					{
						Directory.Move(currentFile, Path.Combine(archiveDirectory, fileName));
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private static void Usage()
		{
			Console.WriteLine("usage: flatcopy [path\\]*.ext destinationpath [/m]");
		}
	}
}