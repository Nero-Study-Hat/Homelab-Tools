using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ServiceConfigGenerator.src
{
    public class TaskHandler
    {
        public readonly static string defaultServiceTasksBeginning = """
            ### BOILER PLATE ###
            - name: Creates remote docker projects directory
            ansible.builtin.file:
                path: '{{docker_dir}}'
                state: directory

            - name: Update local docker directory name variable
            ansible.builtin.set_fact:
                stack_dir: "{{ [docker_dir, project_name] | join('/') }}"

            - name: Creates remote project directory
            file:
                path: '{{stack_dir}}'
                state: directory

            - name: Transfer and convert the docker compose jinja2 template
            ansible.builtin.template:
                src: "compose.yml"
                dest: "{{stack_dir}}/compose.yml"

            ### MAIN ###

        """;

        public readonly static string defaultServiceTasksEnd = """
        
            ### DEPLOY ###
            - name: Deploy Docker Compose Stack
            community.docker.docker_compose_v2:
                project_name: '{{project_name}}'
                project_src: '{{stack_dir}}'
                files:
                - compose.yml
        """;

        public static void JinjaTemplatesCheck(Dictionary<string,string> templatesRemoteRelativePaths)
        {
            // --- template remote paths validate
            // foreach (KeyValuePair<string, string> templateTarget in config.TemplatesRemoteRelativePaths)
            // {
            //     if (templateTarget.Key == "" || File.Exists(dataDirPath + "/templates/" + templateTarget.Key) == false)
            //     {
            //         throw new Exception("Config Validator Error: Template remote paths key invalid.");
            //     }
            //     if (templateTarget.Value == "" || templateTarget.Value.StartsWith(baseDirectoriesPath) == false)
            //     {
            //         throw new Exception("Config Validator Error: Template remote paths value invalid.");
            //     }
            // }
        }

        public static void JinjaTemplatesCopy(string templateNameToCopy, string remoteTemplateDestination)
        {
            string copyTemplateFileTask = """
                - name: Transfer and convert the docker compose jinja2 template
                ansible.builtin.template:
                    src: "compose.yml"
                    dest: "{{stack_dir}}/compose.yml"
            """;
            Console.WriteLine(copyTemplateFileTask);
        }

        public static void FilesCheck()
        {
            // string[] expectedFiles = ["compose.yml", "config.json"];
            // foreach (string file in expectedFiles)
            // {
            //     string currentFile = dataDirPath + "/" + file;
            //     if (File.Exists(currentFile) == false)
            //     {
            //         throw new Exception("Tool Data Error: '" + currentFile + "' file does not exist");
            //     }
            // }
            // foreach (KeyValuePair<string, string> templateTarget in config.FilesRemoteRelativePaths)
            // {
            //     if (templateTarget.Key == "" || File.Exists(dataDirPath + "/files/" + templateTarget.Key) == false)
            //     {
            //         throw new Exception("Config Validator Error: File remote paths key invalid.");
            //     }
            //     if (templateTarget.Value == "" || templateTarget.Value.StartsWith(baseDirectoriesPath) == false)
            //     {
            //         throw new Exception("Config Validator Error: File remote paths value invalid.");
            //     }
            // }
        }

        public static void FilesCopy(string fileNameToCopy, string remoteFileDestination)
        {
            string copyRegularFileTask = """
                - name: Copy file
                ansible.builtin.copy:
                    src: "compose.yml"
                    dest: "{{stack_dir}}/compose.yml"
            """;
            Console.WriteLine(copyRegularFileTask);

        }

        public static void DirectoriesCheck(Config config, string dataDirPath, string userChoice = "")
        {
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
                        throw new Exception("Tool Data Error: '" + currentDir + "' directory does not exist");
                    }
                }
            }

            foreach (string dir in config.DirectoriesToCreate)
            {
                //FIXME: _ is not shady
                string shadyPathCharactersPattern = "[^a-zA-Z0-9/]"; // matches chars not here
                if (Regex.IsMatch(dir, shadyPathCharactersPattern))
                {
                    string promptP1 = "This directory to create has suspicious characters: " + dir;
                    string promptP2 = "Do you wish to preceed (yes) or go back and edit the path (No): ";
                    string prompt = promptP1 + "\n" + promptP2;
                    userChoice = Program.ReadConsoleInput(userChoice, prompt);
                    bool userInputProcess = true;
                    while (userInputProcess == true)
                    {
                        if (userChoice == "y" || userChoice == "Y")
                        {
                            userInputProcess = false;
                        }
                        else if (userChoice == "n" || userChoice == "N")
                        {
                            throw new Exception("Tool Data Error: invalid directory to create");
                        }
                        else Console.WriteLine("Invalid input: please input y or n");
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
                        throw new Exception("Tool Data Error: directories to create should relative in the project directory");
                    }
                }
            }
        }

        public static void DirectoriesCreate(string directoryRelativePath)
        {
            string remotePath = "'{{stack_dir}}" + directoryRelativePath + "'";
            string createDirectoryTask = $$"""
                - name: Creates remote project directory
                file:
                    path: {{remotePath}}
                    state: directory
            """;
            Console.WriteLine(createDirectoryTask);
        }
    }
}