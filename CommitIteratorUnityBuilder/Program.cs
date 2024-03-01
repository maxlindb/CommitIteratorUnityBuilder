namespace CommitIteratorUnityBuilder
{
    using System;
    using System.Diagnostics;
    using System.IO;

    class UnityBuildsFromCommits
    {
        static void Main(string[] args)
        {
            const string gitRepoPath = "/Users/flow/pc-cthulhu-keeper";
            const string unityProjectPath = gitRepoPath;
            const string buildsRootPath = "/Users/flow/spambuilds";

            // Navigate to Git repository
            Directory.SetCurrentDirectory(gitRepoPath);

            // Get the last 50 commit hashes
            var commitHashes = GetLast50CommitHashes();

            foreach (var hash in commitHashes)
            {
                // Reset and clean the repository to ensure it's in a clean state
                ResetAndCleanRepository();

                // Checkout the commit
                RunCommand("git", $"checkout {hash}");

                // Define a unique build folder for this commit
                string buildFolderPath = Path.Combine(buildsRootPath, hash);
                Directory.CreateDirectory(buildFolderPath);

                // Build the Unity project
                BuildUnityProject(unityProjectPath, buildFolderPath);
            }

            // Optionally, checkout back to the main branch
            RunCommand("git", "checkout main");
        }

        static string[] GetLast50CommitHashes()
        {
            string output = RunCommand("git", "log -n 50 --format=%H");
            return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        static void BuildUnityProject(string projectPath, string outputPath)
        {
            // Adjust the path to your Unity editor version and the build options as per your needs
            string unityEditorPath = "/Applications/Unity/Hub/Editor/2022.3.2f1/Unity.app/Contents/MacOS/Unity";
            string unityArguments = $"-quit -batchmode -projectPath \"{projectPath}\" -buildOSXUniversalPlayer \"{outputPath}\"";
            Console.WriteLine(RunCommand(unityEditorPath, unityArguments));
        }

        static string RunCommand(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command} {arguments}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
        }


        static void ResetAndCleanRepository()
        {
            // Hard reset to discard any changes in the index and working directory
            RunCommand("git", "reset --hard");
            // Clean to remove untracked files and directories
            RunCommand("git", "clean -fdx");
        }
    }

}
