using System.Text;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace ServiceConfigGenerator.src
{
    public struct Config(
        string projectName,
        string networkSetup,
        string[] directoriesToCreate,
        Dictionary<string, string> templatesRemoteRelativePaths,
        Dictionary<string, string> filesRemoteRelativePaths
    )
    {
        public string ProjectName = projectName;
        public string NetworkSetup = networkSetup;
        public string[] DirectoriesToCreate = directoriesToCreate;
        public Dictionary<string, string> TemplatesRemoteRelativePaths = templatesRemoteRelativePaths;
        public Dictionary<string, string> FilesRemoteRelativePaths = filesRemoteRelativePaths;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string remoteDockerDirPath = "/home/ansible/docker/";
            Console.WriteLine("Enter data directory path: ");
            string dataDirPath = Console.ReadLine() ?? "/mnt/nero-priv-data/Workspace/Tech/IT/Homelab/tools/service_config_generator/data";
            if (Directory.Exists(dataDirPath) != true)
            {
                throw new Exception("Invalid data directory path provided.");
            }

            string[] files = Directory.GetFiles(dataDirPath);
            // user input data directory is for a single service
            // apply logic using given directory
            //TODO: error msg here to remind user no files should be in parent dir
            // if trying multiple service sub directories
            if (files.Length == 0)
            {
                string[] subDirectories = Directory.GetDirectories(dataDirPath);
                foreach (string dir in subDirectories)
                {
                    string configPath = dir + "/config.json";
                    ServiceLogic(configPath, dir, remoteDockerDirPath);
                }
            }
            // user input data directory is for a multiple services
            // apply logic using each sub directory of given directory
            else
            {
                string configPath = dataDirPath + "/config.json";
                ServiceLogic(configPath, dataDirPath, remoteDockerDirPath);
            }
        }

        public static string ReadConsoleInput(string input = "", string prompt = "")
        {
            if (String.IsNullOrEmpty(input))
            {
                    Console.WriteLine(prompt);
                    input = Console.ReadLine() ?? "";
                    bool userInputProcess = true;
                    while (userInputProcess == true)
                    {
                        if (String.IsNullOrEmpty(input))
                        {
                            Console.WriteLine("Please input a string here.");
                        }
                        else
                        {
                            userInputProcess = false;
                        }
                    }
            }
            return input;
        }

        public static void ServiceLogic(string configPath, string dataDirPath, string remoteDockerDirPath)
        {
            Config config = ToolDataCheck(configPath, dataDirPath); // checks from TaskHandler & VarHandler here
            VarGenerator();
            TaskGenerator(config, remoteDockerDirPath);
        }

        public static Config ToolDataCheck(string configPath, string dataDirPath)
        {
            // unneeded directories and files will not raise exceptions as they are not a problem
            Config config = new Config();
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)); //TODO: add try catch here, also file exists check
                TaskHandler.DirectoriesCheck(config, dataDirPath);
                Console.WriteLine("Dir Check Success");
                TaskHandler.JinjaTemplatesCheck(config.TemplatesRemoteRelativePaths);
                TaskHandler.FilesCheck();
                VarHandler.AnsibleVarsCheck();
                VarHandler.ComposeCheck();
                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return config;
        }

        public static void VarGenerator()
        {
            //
        }

        public static void TaskGenerator(Config config, string remoteDockerDirPath)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(TaskHandler.defaultServiceTasksBeginning);
            
            //TODO: loop over all tasks for each task type and append

            string baseDirectoriesPath = remoteDockerDirPath + config.ProjectName;
            foreach (string dir in config.DirectoriesToCreate)
            {
                
            }

            string templateTasks = "";
            string regularFileTasks = "";
            string directoryCreateTasks = "";
            stringBuilder.AppendLine(templateTasks);
            stringBuilder.AppendLine(regularFileTasks);
            stringBuilder.AppendLine(directoryCreateTasks);
            string AnsibleTasks = stringBuilder.ToString();
            Console.WriteLine(AnsibleTasks);
        }


    }
}