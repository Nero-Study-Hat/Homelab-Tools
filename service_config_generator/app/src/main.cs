using Newtonsoft.Json;

namespace ServiceConfigGenerator.src
{
    public struct Config(
        string projectName,
        string networkSetup,
        Dictionary<string, Dictionary<string,List<string>>> userGateDetails,
        string[] directoriesToCreate,
        Dictionary<string, string> templatesRemoteRelativePaths,
        Dictionary<string, string> filesRemoteRelativePaths,
        string userGateHost,
        string[] hosts
    )
    {
        public string ProjectName = projectName;
        public string NetworkSetup = networkSetup;
        public Dictionary<string, Dictionary<string,List<string>>> UserGateDetails = userGateDetails;
        public string[] DirectoriesToCreate = directoriesToCreate;
        public Dictionary<string, string> TemplatesRemoteRelativePaths = templatesRemoteRelativePaths;
        public Dictionary<string, string> FilesRemoteRelativePaths = filesRemoteRelativePaths;
        public string UserGateHost = userGateHost;
        public string[] Hosts = hosts;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                throw new Exception("Too many input args.");
            }
            if (args.Length == 1 && !string.Equals(args[0], "headless"))
            {
                throw new Exception("Incorrect headless arguement.");
            }

            if (args.Length == 1 && string.Equals(args[0], "headless"))
            {
                // only meant to take a role_data dir
                // produced as input from this tool
                // for CI testing for drift
                HeadlessExportBuildMain(args);
            }
            else
            {
                // relies on external Ansible-Role-Data
                // repository directory on system

                // inputs are
                // <role data directory path>
                // <target dir path to build to>
                HeadfullMain();
            }
        }

        public static void HeadfullMain()
        {
            string ansibleRoleDataPath = "/home/nero/Workspace/Tech/IT/Ansible-Roles-Data";
            List<string> projects = new List<string>();
            foreach (DirectoryInfo dirInfoEntry in new DirectoryInfo(ansibleRoleDataPath).GetDirectories())
            {
                string dirName = dirInfoEntry.Name;
                if (String.Equals(dirName, "shared-roles") || String.Equals(dirName, ".git")) continue;
                projects.Add(dirName);
            }
            Console.WriteLine("Choose one of the below projects to generate for by inputing their number:");
            for (int index = 1; index <= projects.Count; index++)
            {
                Console.WriteLine($"{index}: {projects[index - 1]}");
            }
            string projectChosenInput = Console.ReadLine() ?? "2";
            if (string.Equals(projectChosenInput, string.Empty))
            {
                projectChosenInput = "2";
            }
            bool projectChosenBool = Int32.TryParse(projectChosenInput, out int projectChosenInt);
            if (!projectChosenBool)
            {
                throw new Exception("Input was not an interger number.");
            }
            string projectChosen = projects[projectChosenInt - 1];
            string projectPath = ansibleRoleDataPath + $"/{projectChosen}" + "/main";
            string[] projectFiles = Directory.GetFiles(projectPath);
            if (projectFiles.Length != 2 && !File.Exists(projectChosen + "/order.txt") && !File.Exists(projectChosen + "/target_directory.txt"))
            {
                throw new Exception("Invalid initial project dir provided");
            }
            string outputTargetAnsibleDirPath = File.ReadAllText(projectPath + "/target_directory.txt");

            FileSysOperations.ClearAnsibleProject(outputTargetAnsibleDirPath);
            string[] rolesOrdered = File.ReadAllLines(projectPath + "/order.txt");
            foreach (string role in rolesOrdered)
            {
                List<string> roleDataDirPaths = new List<string>
                {
                    $"{ansibleRoleDataPath}/shared-roles/main/" + role,
                    $"{ansibleRoleDataPath}/{projectChosen}/main/" + role
                };
                bool success = false;
                foreach (string roleDataDirPath in roleDataDirPaths)
                {
                    if (Directory.Exists(roleDataDirPath))
                    {
                        success = true;
                        string configPath = roleDataDirPath + "/config.json";
                        Config config = ToolDataCheck(configPath, roleDataDirPath, outputTargetAnsibleDirPath);
                        FileSysOperations.RoleDir(config, roleDataDirPath, outputTargetAnsibleDirPath);
                        FileSysOperations.HostVar(config, roleDataDirPath, outputTargetAnsibleDirPath);
                        FileSysOperations.ToolData(role, roleDataDirPath, outputTargetAnsibleDirPath);
                        break;
                    }
                }
                if (!success)
                {
                    string throwMsg = """
                    Invalid role dir listinging order.txt
                    Check if a role name was recently changed and not updated in order.txt
                    """;
                    throw new Exception(throwMsg);
                }
            }
            Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(projectPath + "/order.txt", outputTargetAnsibleDirPath + "/roles_data/order.txt");
        }

        public static void HeadlessExportBuildMain(string[] args)
        {
            string ansibleRoleDataDirPath = Console.ReadLine() ?? "";
            string outputTargetAnsibleDirPath = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(ansibleRoleDataDirPath) || string.IsNullOrEmpty(outputTargetAnsibleDirPath))
            {
                string throwMsg = """
                Headless: Did not fill required inputs
                Input 1: <ansible role data directory path (directory that was exported from this tool)>
                Input 2: <ansible directory (contains base directory structure and expected content)>
                """;
                throw new Exception(throwMsg);
            }

            FileSysOperations.PrepareTargetAnsibleDir(outputTargetAnsibleDirPath);
            string[] rolesOrdered = File.ReadAllLines(ansibleRoleDataDirPath + "/order.txt");
            foreach (string role in rolesOrdered)
            {
                string roleDataDirPath = ansibleRoleDataDirPath + "/" + role;
                if (!Directory.Exists(roleDataDirPath))
                {
                    string throwMsg = """
                    Headless:
                    Invalid role dir listinging order.txt
                    Check if a role name was recently changed and not updated in order.txt
                    """;
                    throw new Exception(throwMsg);
                }
                string configPath = roleDataDirPath + "/config.json";
                Config config = ToolDataCheck(configPath, roleDataDirPath, outputTargetAnsibleDirPath);
                FileSysOperations.RoleDir(config, roleDataDirPath, outputTargetAnsibleDirPath);
                FileSysOperations.HostVar(config, roleDataDirPath, outputTargetAnsibleDirPath);
            }
        }

        public static Config ToolDataCheck(string configPath, string dataDirPath, string localAnsibleDirPath)
        {
            // check here because config can't be accessed yet
            if (File.Exists(configPath) == false)
            {
                Console.WriteLine("Tool Data Error: '" + configPath + "' expected config.json file does not exist");
                Environment.Exit(1);
            }

            //TODO: add try catch here, also file exists check
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

            foreach (string host in config.Hosts)
            {
                string currentFile = dataDirPath + "/host_vars/" + host;
                if (File.Exists(currentFile) == false)
                {
                    Console.WriteLine("Tool Data Error: '" + currentFile + "' expected file does not exist");
                    Environment.Exit(1);
                }
            }

            List<string> errors = new List<string>();
            string hostVarsDirPath = localAnsibleDirPath + "/inventory/host_vars";
            string projectRoleDirPath = localAnsibleDirPath + "/roles/" + config.ProjectName;

            errors.Add(TaskHandler.DirectoriesCheck(config, dataDirPath));
            errors.Add(TaskHandler.FilesCheck(config, dataDirPath));
            errors.Add(VarHandler.NetworkSetupCheck(config.NetworkSetup, hostVarsDirPath + config.UserGateHost));

            string[] requiredAnsibleDirPaths = [localAnsibleDirPath, hostVarsDirPath];
            foreach (string dir in requiredAnsibleDirPaths)
            {
                if (!Directory.Exists(dir))
                {
                    errors.Add("Invalid ansible dev directory path provided.");
                    break;
                }
            }
            errors = errors.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            try
            {
                Helpers.CheckReport(errors: errors, shouldThrow: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
            return config;
        }
    }
}