namespace StaticCodeAnalysisSquared.src.Main
{
    /// <summary>
    /// Class that creates a workflow for each juliet test suite test.
    /// </summary>
    internal class Workflow
    {
        /// <summary>
        /// Makes the workflow by jumping recursively through all folders in a designated directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void MakeWorkflow(string directoryPath)
        {
            RecursiveDirectoryJumping(directoryPath);
        }
        /// <summary>
        /// Jumps recursively through all folders in a designated directory until it finds a folder with files in it.
        /// Then adds the data for the files in the folder.
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void RecursiveDirectoryJumping(string directoryPath)
        {
            if (Directory.GetFiles(directoryPath).Length == 0)
            {
                foreach (var folder in Directory.GetDirectories(directoryPath))
                {
                    RecursiveDirectoryJumping(folder);
                }
            }
            else
            {
                AddData(directoryPath);
            }
        }
        /// <summary>
        /// Gets all the filepaths in the directory which are of the .csproj kind if there are any.
        /// If there are .csproj files it then chops the file path to get the filename with and without extension, and the project folder path.
        /// It then puts the parts in a template and prints them.
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void AddData(string directoryPath)
        {
            var filePath = Directory.GetFiles(directoryPath)
                .Where(x => x.Contains(".csproj")).SingleOrDefault();
            if (filePath != null)
            {
                // Get the file name with extension
                string fileName = Path.GetFileName(filePath);
                string fileNameNoExtension = fileName.Split(".")[0];
                string projectFolderPath = string.Join(@"\", filePath.Split('\\').Skip(8).Take(2));

                string template = $"\n\n" +
                                  $"    - name: Build {fileNameNoExtension}\r\n" +
                                  $"      run: msbuild {projectFolderPath}/{fileName} /p:Configuration=Debug /p:Platform=AnyCPU /p:UseSharedCompilation=false";

                Console.WriteLine(template);
            }
        }
    }
}
