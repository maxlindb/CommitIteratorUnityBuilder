namespace CommitIteratorUnityBuilder
{
    using System;
    using System.Diagnostics;
    using System.IO;

    class UnityBuildsFromCommits
    {
        const string kGitRepoPath = "/Users/flow/pc-cthulhu-keeper";
        const string kUnityProjectPath = kGitRepoPath;
        const string kBuildsRootPath = "/Users/flow/spambuilds";
        const string kRemoteName = "origin";
        const string kBranchName = "main";
        const bool kDoCleanBuilds = false;

        const string kStartCommitHashPrefix = "5acc904";

        static int step = 10;


        static void Main(string[] args)
        {
            // Navigate to Git repository
            Directory.SetCurrentDirectory(kGitRepoPath);

            // Ensure we're starting from the newest commit on the remote
            FetchLatestFromRemote(kRemoteName, kBranchName);

            // Get the commit hashes, starting from a specific commit
            var commitHashes = GetCommitsFromSpecificStart(kStartCommitHashPrefix);

            for (int i = 0; i < commitHashes.Length; i += step)
            {
                var hash = commitHashes[i];
                Console.WriteLine("Start handling commit " + hash);

                // Reset and clean the repository to ensure it's in a clean state
                ResetAndCleanRepository();

                // Checkout the commit
                RunCommand("git", $"checkout {hash}");

                var commitComment = RunCommand("git", "log -1 --pretty=format:%:s");
                var fileNameSafeCommitComment = UnSpace(commitComment);
                fileNameSafeCommitComment = fileNameSafeCommitComment.Substring(0, Math.Min(15, fileNameSafeCommitComment.Length));

                // Define a unique build folder for this commit
                var thisBatchFolder = "run_startFrom_" + hash.Substring(0, 9)+"_"+ fileNameSafeCommitComment+"_"+System.DateTime.UtcNow.ToFileTime();
                string buildFolderPath = Path.Combine(kBuildsRootPath, thisBatchFolder, hash+"_"+ fileNameSafeCommitComment+".app");
                //Directory.CreateDirectory(buildFolderPath);

                Console.WriteLine("BUILDING " + new DirectoryInfo(buildFolderPath).Name);
                // Build the Unity project
                BuildUnityProject(kUnityProjectPath, buildFolderPath);
                Console.WriteLine("Attempted to build "+hash);

                Console.WriteLine();
                Console.WriteLine();
            }

            // Optionally, checkout back to the main branch
            RunCommand("git", "checkout main");
        }

        static void FetchLatestFromRemote(string remoteName, string branchName)
        {
            // Fetch the latest changes from the remote without merging
            RunCommand("git", $"fetch {remoteName}");
            // Checkout the main branch or another branch you're targeting
            RunCommand("git", $"checkout {branchName}");
            // Reset the local branch to match the remote branch
            RunCommand("git", $"reset --hard {remoteName}/{branchName}");

            //CLEAN Library at start even if not doing every build clean, for internal consistency
            // Clean to remove untracked files and directories
            RunCommand("git", "clean -fdx");
        }

        static string[] GetCommitsFromSpecificStart(string startCommitHashPrefix)
        {
            string allCommits = RunCommand("git", "rev-list --all");
            var allCommitHashes = allCommits.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            // Find the index of the commit that starts with the specified prefix
            int startIndex = Array.FindIndex(allCommitHashes, hash => hash.StartsWith(startCommitHashPrefix));
            if (startIndex == -1)
            {
                Console.WriteLine("Start commit not found, using the last 50 commits as fallback.");
                // Fallback to the last 50 commits if the start commit is not found
                return GetLast50CommitHashes();
            }
            // Return the range of commits from the start index
            int length = Math.Min(50, allCommitHashes.Length - startIndex);
            return allCommitHashes.Skip(startIndex).Take(length).ToArray();
        }

        static string[] GetLast50CommitHashes()
        {
            string output = RunCommand("git", "log -n 50 --format=%H");
            return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        static void BuildUnityProject(string projectPath, string outputPath)
        {
            Console.WriteLine("FAKING A BUILD");
            Thread.Sleep(10000);
            return;

            // Adjust the path to your Unity editor version and the build options as per your needs
            string unityEditorPath = "/Applications/Unity/Hub/Editor/2022.3.2f1/Unity.app/Contents/MacOS/Unity";
            string unityArguments = $"-quit -batchmode -projectPath \"{projectPath}\" -buildOSXUniversalPlayer \"{outputPath}\"";
            RunCommand(unityEditorPath, unityArguments);
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

            
            if (kDoCleanBuilds)
            {
                // Clean to remove untracked files and directories
                RunCommand("git", "clean -fdx");
            }
        }



        public static string UnSpace(string str)
        {
            var charARr = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                if (i != str.Length - 1)
                {
                    if (charARr[i] == ' ')
                    {
                        charARr[i + 1] = charARr[i + 1].ToString().ToUpper()[0];
                    }
                }
            }
            str = new string(charARr);
            str = str.Replace(" ", "");
            str = str.Replace(".", "");
            str = RemoveBadCharsBesidesSpaces(str);

            return str;
        }

        public static string RemoveBadCharsBesidesSpaces(string inputString)
        {
            var allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

            var str = "";
            foreach (var item in inputString)
            {
                if (!allowedChars.Contains(item))
                    continue;

                str += item;
            }
            return str;
        }
    }

}
