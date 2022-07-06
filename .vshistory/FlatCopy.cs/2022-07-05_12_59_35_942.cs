using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FlatCopy
{
	internal class Program
	{
		static String s_pattern = ".*.\\((\\d*)\\)\\..*";
		static RegexOptions s_options = RegexOptions.IgnoreCase|RegexOptions.Singleline;
		static Regex regex = new Regex(s_pattern, s_options);

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

			// when having commandline switch /m it will move instead of copying the files
			bool bMove = args.Contains("/m") | args.Contains("/M");

			// commandline swith /r removes empty folders after moving files
			bool bRemoveEmpty = args.Contains("/r") | args.Contains("/R");

			try
			{
				var allFiles = Directory.EnumerateFiles(sourceDirectory, pattern, SearchOption.AllDirectories);

				foreach (string currentFile in allFiles)
				{
					string fileName = Path.GetFileName(currentFile);

					MoveOrCopy(currentFile, archiveDirectory, fileName, bMove, bRemoveEmpty);
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
		private static void MoveOrCopy(string sourceFile, string destinationFolder, string fileName, bool bMove, bool bRemoveEmpty)
		{
			string sourceFolder = Path.GetDirectoryName(sourceFile);
			if (sourceFolder == destinationFolder)
			{
				return;
			}

			string destinationFile = Path.Combine(destinationFolder, fileName);

			// check if destination file exists, rename if required
			string possibleFilename = BuildFilenameWithIndex(fileName, 0);
			
			if(File.Exists(Path.Combine(destinationFolder, possibleFilename)))
			{
				int index = 1;
				while (File.Exists(Path.Combine(destinationFolder, possibleFilename)))
				{
					possibleFilename = BuildFilenameWithIndex(fileName, index);
					index++;
				}
				destinationFile = Path.Combine(destinationFolder, possibleFilename);
			}


			while (File.Exists(destinationFile))
			{
				string existingFile = destinationFile;
				fileName = NewFilename(destinationFolder, fileName);
				
				destinationFile = Path.Combine(destinationFolder, fileName);
			}

			try
			{
				if(bMove)
				{
					File.Move(sourceFile, destinationFile);
	
					if (bRemoveEmpty)
					{
						if (Directory.GetFiles(sourceFolder).Length == 0 && Directory.GetDirectories(sourceFolder).Length == 0)
						{
							try
							{
								Directory.Delete(sourceFolder);
								Console.WriteLine($"Removed empty folder '{sourceFolder}'");
							}
							catch (System.Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}
					}
				}
				else
				{
					File.Copy(sourceFile, destinationFile);
				}
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Builds a filename with index.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="index">The index.</param>
		/// <returns>A string.</returns>
		private static string BuildFilenameWithIndex(string fileName, int index)
		{
			string nameOnly = Path.GetFileNameWithoutExtension(fileName);
			string extension = Path.GetExtension(fileName);
			
			return $"{nameOnly} ({index}){extension}";
		}

		/// <summary>
		/// creates a new filename based on the previous one, incrementing the index counter if needed
		/// </summary>
		/// <param name="destinationFolder">The destination folder.</param>
		/// <param name="fileName">The file name.</param>
		/// <returns>A string.</returns>
		private static string NewFilename(string destinationFolder, string fileName)
		{
			int index = 0;

			string nameOnly = Path.GetFileNameWithoutExtension(fileName);
			string extension = Path.GetExtension(fileName);
			
			// see if filename already contains a index number
			if (nameOnly.Contains('('))
			{
				var match = regex.Match(nameOnly);
				if (match.Success)
				{
					int.TryParse(match.Groups[1].Value, out index);
				}		
			}

			index++;

			return $"{nameOnly} ({index}).{extension}";
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