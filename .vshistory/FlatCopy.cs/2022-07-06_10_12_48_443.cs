using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FlatCopy
{
	internal class FlatCopy
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

			// get the file pattern and if it contains a source directory, use it
			string pattern = args[0];
			if(pattern.Contains(Path.DirectorySeparatorChar) || pattern.Contains(Path.AltDirectorySeparatorChar))
			{
				sourceDirectory = Path.GetDirectoryName(pattern) ?? throw new DirectoryNotFoundException(pattern);

				pattern = Path.GetFileName(pattern);
			}

			// if we have more than one parameters, check the second one if its a path
			if(args.Length > 1)
			{
				if(!args[1].StartsWith('/'))
				{
					archiveDirectory = Path.GetFullPath(args[1]) ?? throw new DirectoryNotFoundException(args[1]);

					if(!Directory.Exists(archiveDirectory))
					{
						Directory.CreateDirectory(archiveDirectory);
					}
				}
			}

			// when having commandline switch /m it will move instead of copying the files
			bool bMove = args.Contains("/m") | args.Contains("/M");

			// commandline switch /r removes empty folders after moving files
			bool bRemoveEmpty = args.Contains("/r") | args.Contains("/R");

			// commandline switch /u copies only unique files, duplicate names are not copied
			bool bOnlyUnique = args.Contains("/u") | args.Contains("/U");

			try
			{
				var allFiles = Directory.EnumerateFiles(sourceDirectory, pattern, SearchOption.AllDirectories);

				int nFiles = 0;
				foreach (string currentFile in allFiles)
				{
					string fileName = Path.GetFileName(currentFile);

					if(MoveOrCopy(currentFile, archiveDirectory, fileName, bMove, bRemoveEmpty, bOnlyUnique))
					{
						nFiles++;
					}

#if DEBUG
					if (Debugger.IsAttached)
					{
						if (nFiles > 10)
						{
							return;
						}
					}
#endif
				}

				Console.WriteLine($"{nFiles} file(s) {(bMove ? "moved" : "copied")}");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// Moves or copies a file
		/// </summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destinationFolder">The destination folder.</param>
		/// <param name="fileName">The file name.</param>
		/// <param name="bMove">If true, move.</param>
		/// <param name="bRemoveEmpty">If true, removes the containing folder if its empty</param>
		/// <param name="bUnique">If true, processes the file only if its unique, that is, existing files with the same name must differ in content</param>
		private static bool MoveOrCopy(string sourceFile, string destinationFolder, string fileName, bool bMove, bool bRemoveEmpty, bool bUnique)
		{
			string sourceFolder = Path.GetDirectoryName(sourceFile);
			if (sourceFolder == destinationFolder)
			{
				return false;
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

			// check if a file with the same name already exists and if so,
			// rename the existing with index and increment index for current file
			while (File.Exists(destinationFile))
			{
				// if only unique files, ignore this one since its a duplicate filename
				if (bUnique)
				{
					if (!IsUnique(sourceFile, destinationFile))
					{
						Console.WriteLine($"{sourceFile} !! duplicate");
						return false;
					}
				}

				string existingFile = destinationFile;
				fileName = NewFilename(fileName);
				
				destinationFile = Path.Combine(destinationFolder, fileName);

				// if the existing file has no index, rename it to be the first in the list
				if(!HasIndex(existingFile))
				{
					File.Move(existingFile, Path.Combine(destinationFolder, possibleFilename));
					Console.WriteLine($"{existingFile} == {possibleFilename}");
				}
			}

			try
			{
				// finally move or copy the file 
				if(bMove)
				{
					File.Move(sourceFile, destinationFile);
					Console.WriteLine($"{sourceFile} => {fileName}");

					if (bRemoveEmpty)
					{
						// if the folder is empty, remove it
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
					Console.WriteLine($"{sourceFile} +=> {fileName}");
				}

				return true;
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return false;
		}

		private static bool IsUnique(string sourceFile, string destinationFile)
		{
			FileInfo sourceInfo = new FileInfo(sourceFile);
			FileInfo destinationInfo = new FileInfo(destinationFile);
			
			if (sourceInfo.Length == destinationInfo.Length)
			{
				if(sourceInfo.Length < 1024*1000)
				{
					byte[] sourceBytes = File.ReadAllBytes(sourceFile);
					byte[] destinationBytes = File.ReadAllBytes(destinationFile);
					if (sourceBytes.SequenceEqual(destinationBytes))
					{
						return false;
					}
				}
				else
				{

					return !FilesAreEqual(sourceInfo, destinationInfo);
				}
			}

			return true;
		}

		const int BYTES_TO_READ = sizeof(Int64);

		static bool FilesAreEqual(FileInfo first, FileInfo second)
		{
			int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

			using (FileStream fs1 = first.OpenRead())
			using (FileStream fs2 = second.OpenRead())
			{
				byte[] one = new byte[BYTES_TO_READ];
				byte[] two = new byte[BYTES_TO_READ];

				for (int i = 0; i < iterations; i++)
				{
					fs1.Read(one, 0, BYTES_TO_READ);
					fs2.Read(two, 0, BYTES_TO_READ);

					if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if the given file already contains an index
		/// </summary>
		/// <param name="existingFile">The existing file.</param>
		/// <returns>A bool.</returns>
		private static bool HasIndex(string existingFile)
		{
			string nameOnly = Path.GetFileNameWithoutExtension(existingFile);

			// see if filename already contains a index number
			if (nameOnly.Contains('(') && nameOnly.Contains(')'))
			{
				var match = regex.Match(nameOnly);
				if (match.Success)
				{
					return int.TryParse(match.Groups[1].Value, out int index);
				}
			}

			return false;
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
		private static string NewFilename(string fileName)
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

			return $"{nameOnly} ({index}){extension}";
		}

		/// <summary>
		/// Usages the.
		/// </summary>
		private static void Usage()
		{
			Console.WriteLine("usage: flatcopy [path\\]*.ext [destinationpath] [/m] [/r] [/u]");
			Console.WriteLine("destinationpath\tif not specified, current directory is used" );
			Console.WriteLine("/m\tmove files instead of copying" );
			Console.WriteLine("/r\tremove empty folders after moving files" );
			Console.WriteLine("/u\tcopy only unique files, duplicate files are ignored");
		}
	}
}