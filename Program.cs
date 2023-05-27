namespace Windows.Files.Organizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var looseFilesdestinationPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\organized-files";
            var foldersDestinationPath = Path.Combine(looseFilesdestinationPath, "folders");

            var sourcePaths = PathsToAnalize();

            foreach(var sourcePath in sourcePaths!)
            {
                if (!Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Source directory {sourcePath} does not exist.");
                    return;
                }

                if (!Directory.Exists(foldersDestinationPath))
                {
                    Directory.CreateDirectory(foldersDestinationPath);
                }

                var files = Directory.GetFiles(sourcePath, "*", SearchOption.TopDirectoryOnly);
                var allFolders = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);

                var foldersToProcess = allFolders
                                    .Where(x => !x.Contains(looseFilesdestinationPath) && !x.Contains(foldersDestinationPath))
                                    .Select(x => x)
                                    .ToArray();

                CopyLooseFiles(looseFilesdestinationPath, files, PathOrganizationMode.ByExtension);

                CopyOthersDirectories(sourcePath, foldersDestinationPath, foldersToProcess);
            }

            Console.WriteLine("All files moved successfully.");
            Console.ReadLine();
        }

        private static string[] PathsToAnalize()
        {
            return new string[] 
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
            };
        }

        static void CopyOthersDirectories(string sourcePath, string foldersDestinationPath, string[] foldersToProcess)
        {
            foreach (string folder in foldersToProcess)
            {
                var relativePath = folder.Replace(sourcePath, "").TrimStart('\\');
                var fullFoldersDestinationPath = Path.Combine(foldersDestinationPath, relativePath);

                Directory.CreateDirectory(fullFoldersDestinationPath);

                var filesInFolder = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);

                CopyLooseFiles(fullFoldersDestinationPath, filesInFolder, PathOrganizationMode.KeepFolderStructure);
            }
        }

        static void CopyLooseFiles(string destinationPath, string[] files, PathOrganizationMode copyMode)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileExtension = Path.GetExtension(file);

                if (FileExtensionNotAllowed(fileExtension))
                    continue;

                if(copyMode == PathOrganizationMode.ByExtension)
                {
                    var fullPathByExtension = Path.Combine(destinationPath, fileExtension.TrimStart('.'));
                    Directory.CreateDirectory(fullPathByExtension);
                    string destinationFile = Path.Combine(fullPathByExtension, fileName);

                    destinationFile = FileExistsGuard(fileExtension, destinationFile);
                    
                    File.Move(file, destinationFile, false);

                    Console.WriteLine("[MODE EXTENSION] Moved file: " + fileName + " to: " + fullPathByExtension);

                }
                else
                {
                    Directory.CreateDirectory(destinationPath);
                    string destinationFile = Path.Combine(destinationPath, fileName);

                    destinationFile = FileExistsGuard(fileExtension, destinationFile);

                    File.Move(file, destinationFile, false);

                    Console.WriteLine("[MODE KEEP FOLDER STRUCTURE] Moved file: " + fileName + " to: " + destinationPath);
                }
            }
        }

        private static string FileExistsGuard(string fileExtension, string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                var temp = destinationFile.Replace(fileExtension, "_");
                destinationFile = temp + fileExtension;
            }
            
            return destinationFile;
        }

        static bool FileExtensionNotAllowed(string fileExtension)
        {
            if(fileExtension == ".lnk" || 
                fileExtension == ".ini" || 
                fileExtension == ".url")
                    return true;

            return false;
        }
    }

    enum PathOrganizationMode
    {
        ByExtension,
        KeepFolderStructure
    }
}
