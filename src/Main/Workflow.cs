namespace StaticCodeAnalysisSquared.src.Main
{
    internal class Workflow
    {
        public static void MakeWorkflow(string directoryPath)
        {
            RecursiveDirectoryJumping(directoryPath);
        }
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
