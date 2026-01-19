using System.Text;
using System.Text.RegularExpressions;

namespace ServiceConfigGenerator.src
{
    public class TaskHandler
    {

        public static string LocalDevPathsCheck(string localRoleDirPath, string localHostVarsDirPath)
        {
            return "";
        }

        public static string FilesCheck(Config config, string dataDirPath)
        {
            List<string> errors = new List<string>();

            // --- remote paths validate, key is local file name, value is remote relative path in project
            string[] currentFilePathsCheck = ["templates", "files"];
            Dictionary<string, string>[] currentFilePathsDictionary = [config.TemplatesRemoteRelativePaths, config.FilesRemoteRelativePaths];
            for (int index = 0; index < 2; index++)
            {
                foreach (KeyValuePair<string, string> currentFilePaths in currentFilePathsDictionary[index])
                {
                    string expectedFileToExist = dataDirPath + $"/{currentFilePathsCheck[index]}/" + currentFilePaths.Key;
                    if (string.IsNullOrEmpty(currentFilePaths.Key) || File.Exists(expectedFileToExist) == false)
                    {
                        errors.Add("Tool Data Error: Remote paths key empty or template does not exist at the expected path.");
                        errors.Add($"Problem Dir: {currentFilePathsCheck[index]}");
                        errors.Add($"Problem File (Expected to exist): {currentFilePaths.Key}");
                    }
                    string baseRemotePath = "/home/ansible/docker/" + config.ProjectName;
                    if (string.IsNullOrEmpty(currentFilePaths.Value) || currentFilePaths.Value.StartsWith(baseRemotePath))
                    {
                        errors.Add("Tool Data Error: Template remote paths value invalid, null or empty value or remote path is not relative to the project.");
                    }
                }
            }

            string report = Helpers.CheckReport(errors, false);
            return report;
        }

        public static string DirectoriesCheck(Config config, string dataDirPath, string userChoice = "")
        {
            List<string> errors = new List<string>();

            List<string> expectedDirectories = new List<string>();
            if (config.TemplatesRemoteRelativePaths.Count > 0)
            {
                expectedDirectories.Add("templates");
            }
            if (config.FilesRemoteRelativePaths.Count > 0)
            {
                expectedDirectories.Add("files");
            }
            if (expectedDirectories.Count > 0)
            {
                foreach (string dir in expectedDirectories)
                {
                    string currentDir = dataDirPath + "/" + dir;
                    if (Directory.Exists(currentDir) == false)
                    {
                        errors.Add("Tool Data Error: '" + currentDir + "' directory does not exist");
                    }
                }
            }

            foreach (string dir in config.DirectoriesToCreate)
            {
                string shadyPathCharactersPattern = "[^a-zA-Z0-9/_-]"; // matches chars not here
                if (Regex.IsMatch(dir, shadyPathCharactersPattern))
                {
                    string promptP1 = "This directory to create has suspicious characters: " + dir;
                    string promptP2 = "Do you wish to preceed (y) or go back and edit the path (N): ";
                    string prompt = promptP1 + "\n" + promptP2;
                    bool userInputProcess = true;
                    int loopHardLimit = 100;
                    int count = 0;
                    while (userInputProcess == true && count < loopHardLimit)
                    {
                        userChoice = Helpers.ReadConsoleInput(userChoice, prompt);
                        if (userChoice == "y" || userChoice == "Y")
                        {
                            userInputProcess = false;
                        }
                        else if (userChoice == "n" || userChoice == "N")
                        {
                            errors.Add("Tool Data Error: invalid directory to create");
                            userInputProcess = false;
                        }
                        else Console.WriteLine("Invalid input: please input y or n");
                        count++;
                    }
                    if (count == 100)
                    {
                        Console.WriteLine("TaskHandler: Looped to many times trying to read console input.");
                        Environment.Exit(1);
                    }
                }
            }

            foreach (string dir in config.DirectoriesToCreate)
            {
                string[] parentDirectories = ["/", "home/", "ansible/", "docker/", $"{config.ProjectName}/"];
                foreach (string parentDir in parentDirectories)
                {
                    if (dir.StartsWith(parentDir))
                    {
                        errors.Add("Tool Data Error: directories to create should relative in the project directory");
                    }
                }
            }

            string report = Helpers.CheckReport(errors, false);
            return report;

        }

        public static string FileOpTaskGenerate(string specificTask, string srcFileName, string relativeRemoteDestination)
        {
            string remotePath = "{{stack_dir}}/" + relativeRemoteDestination;
            string fileOpTask = $$"""
            - name: Copy a file to a remote loc (and render if template)
              ansible.builtin.{{specificTask}}:
                src: "{{srcFileName}}"
                dest: "{{remotePath}}"
            """;
            return fileOpTask;
        }

        public static string DirectoriesCreateTaskGenerate(string directoryRelativePath)
        {
            string remotePath = "'{{stack_dir}}/" + directoryRelativePath + "'";
            string createDirectoryTask = $$"""
            - name: Creates remote project directory
              ansible.builtin.file:
                path: {{remotePath}}
                state: directory
            """;
            return createDirectoryTask;
        }

        public static string TasksFileGenerate(Config config, string dataDirPath)
        {
            if(File.Exists(dataDirPath + "/tasks.yaml"))
            {
                string taskFileContents = File.ReadAllText(dataDirPath + "/tasks.yaml");
                return taskFileContents;
            }

            string defaultServiceTasksBeginning = """
            ### BOILER PLATE ###
            - name: Creates remote docker projects directory
              ansible.builtin.file:
                path: '{{docker_dir}}'
                state: directory

            - name: Update local docker directory name variable
              ansible.builtin.set_fact:
                stack_dir: "{{ [docker_dir, project_name] | join('/') }}"

            - name: Creates remote project directory
              ansible.builtin.file:
                path: '{{stack_dir}}'
                state: directory

            - name: Transfer and convert the docker compose jinja2 template
              ansible.builtin.template:
                src: "compose.yml"
                dest: "{{stack_dir}}/compose.yml"

            ### MAIN ###
            """;

            string defaultServiceTasksEnd = """
            ### DEPLOY ###
            - name: Deploy Docker Compose Stack
              community.docker.docker_compose_v2:
                project_name: '{{project_name}}'
                project_src: '{{stack_dir}}'
                files:
                - compose.yml
            """;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(defaultServiceTasksBeginning);
            string currentTask = "";
            foreach (string dir in config.DirectoriesToCreate)
            {
                currentTask = DirectoriesCreateTaskGenerate(dir);
                stringBuilder.Append(currentTask);
                stringBuilder.AppendLine("\n");
            }
            foreach (KeyValuePair<string, string> templateTask in config.TemplatesRemoteRelativePaths)
            {
                currentTask = FileOpTaskGenerate("template", templateTask.Key, templateTask.Value);
                stringBuilder.Append(currentTask);
                stringBuilder.AppendLine("\n");
            }
            foreach (KeyValuePair<string, string> copyTask in config.FilesRemoteRelativePaths)
            {
                currentTask = FileOpTaskGenerate("copy", copyTask.Key, copyTask.Value);
                stringBuilder.Append(currentTask);
                stringBuilder.AppendLine("\n");
            }
            string configTasksDirPath = dataDirPath + "/tasks";
            if(File.Exists(configTasksDirPath + "/main.yaml"))
            {
                string configMainTasks = File.ReadAllText(configTasksDirPath + "/main.yaml");
                stringBuilder.Append(configMainTasks);
                stringBuilder.AppendLine("\n");
            }
            stringBuilder.AppendLine(defaultServiceTasksEnd);
            stringBuilder.AppendLine("\n");
            if(File.Exists(configTasksDirPath + "/post_deploy.yaml"))
            {
                string configPostDeployTasks = File.ReadAllText(configTasksDirPath + "/post_deploy.yaml");
                stringBuilder.Append(configPostDeployTasks);
                stringBuilder.AppendLine("\n");
            }
            string ansibleTasks = stringBuilder.ToString();
            return ansibleTasks;
        }
    }
}