using System.Xml.Linq;

namespace FlatCopy
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string sourceDirectory = Environment.CurrentDirectory;
			string archiveDirectory = Environment.CurrentDirectory;

			if(args.Length < 1)
			{
				Usage();
				return;
			}

			string pattern = args[0];
			if(pattern.Contains('\\') || pattern.Contains('/'))
			{
				sourceDirectory = Path.GetDirectoryName(pattern);
				
				if (sourceDirectory is null)
				{
					throw new DirectoryNotFoundException(pattern);
				}

				pattern = Path.GetFileName(pattern);
			}

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
					string fileName = Path.GetFileName(currentFile);

					if(bMove)
					{
						File.Move(currentFile, Path.Combine(archiveDirectory, fileName));
					}
					else
					{
						File.Copy(currentFile, Path.Combine(archiveDirectory, fileName));
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