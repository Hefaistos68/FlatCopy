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

					MoveOrCopy(sourceFile: currentFile, destinationFolder: archiveDirectory, fileName: fileName, bMove: bMove);

				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private static void MoveOrCopy(string sourceFile, string destinationFolder, string fileName, bool bMove)
		{
			// check if destination file exists, rename if required


			if(bMove)
			{
				File.Move(sourceFile, Path.Combine(destinationFolder, fileName));
			}
			else
			{
				File.Copy(sourceFile, Path.Combine(destinationFolder, fileName));
			}
		}

		private static void Usage()
		{
			Console.WriteLine("usage: flatcopy [path\\]*.ext destinationpath [/m]");
		}
	}
}