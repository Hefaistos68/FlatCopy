using System.Xml.Linq;

namespace FlatCopy
{
	internal class Program
	{
		/// <summary>
		/// Mains the.
		/// </summary>
		/// <param name="args">The args.</param>
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

		/// <summary>
		/// Moves the or copy.
		/// </summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destinationFolder">The destination folder.</param>
		/// <param name="fileName">The file name.</param>
		/// <param name="bMove">If true, b move.</param>
		private static void MoveOrCopy(string sourceFile, string destinationFolder, string fileName, bool bMove)
		{
			string destinationFile = Path.Combine(destinationFolder, fileName);

			// check if destination file exists, rename if required
			while(File.Exists(destinationFile))
			{
				string existingFile = destinationFile;
				destinationFile = NewFilename(destinationFolder, fileName);

			}

			if(bMove)
			{
				File.Move(sourceFile, destinationFile);
			}
			else
			{
				File.Copy(sourceFile, destinationFile);
			}
		}

		/// <summary>
		/// News the filename.
		/// </summary>
		/// <param name="destinationFolder">The destination folder.</param>
		/// <param name="fileName">The file name.</param>
		/// <returns>A string.</returns>
		private static string NewFilename(string destinationFolder, string fileName)
		{
			
		}

		/// <summary>
		/// Usages the.
		/// </summary>
		private static void Usage()
		{
			Console.WriteLine("usage: flatcopy [path\\]*.ext destinationpath [/m]");
		}
	}
}