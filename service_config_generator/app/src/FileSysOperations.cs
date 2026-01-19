namespace ServiceConfigGenerator.src
{
    public class FileSysOperations
    {
        public static void ClearAnsibleProject(string ansibleDirPath)
        {
            string hostVarsDirPath = ansibleDirPath + "/inventory/host_vars";
            string internalRolesDirPath = ansibleDirPath + "/roles";
            string copiedToolDataDirPath = ansibleDirPath + "/roles_data";
            Directory.Delete(hostVarsDirPath, true);
            Directory.CreateDirectory(hostVarsDirPath);
            string [] filesToMake = ["day.yaml", "dusk.yaml", "night.yaml"];
            foreach (string fileToMake in filesToMake)
            {
                File.WriteAllText(hostVarsDirPath + "/" + fileToMake, String.Empty);
            }
            Directory.Delete(internalRolesDirPath, true);
            Directory.CreateDirectory(internalRolesDirPath);
            Directory.Delete(copiedToolDataDirPath, true);
            Directory.CreateDirectory(copiedToolDataDirPath);
        }

        public static void RoleDir(Config config, string dataDirPath, string localAnsibleDirPath)
        {
            string projectRoleDirPath = localAnsibleDirPath + "/roles/" + config.ProjectName;
            // HANDLE GENERAL DIRECTORIES & COPYING TEMPLATES AND FILES
            Directory.CreateDirectory(projectRoleDirPath);
            List<string> roleSubDirNames = ["tasks"];
            if (config.FilesRemoteRelativePaths.Count > 0) roleSubDirNames.Add("files");
            if (config.TemplatesRemoteRelativePaths.Count > 0) roleSubDirNames.Add("templates");
            foreach (string dirName in roleSubDirNames)
            {
                Directory.CreateDirectory(projectRoleDirPath + "/" + dirName);
                if(string.Equals(dirName,"files"))
                {
                    foreach (string file in config.FilesRemoteRelativePaths.Keys)
                    {
                        string srcFilePath = dataDirPath + "/files/" + file;
                        string destFilePath = projectRoleDirPath + "/files/" + file;
                        File.Copy(srcFilePath,destFilePath);
                    }
                }
                if(string.Equals(dirName,"templates"))
                {
                    foreach (string template in config.TemplatesRemoteRelativePaths.Keys)
                    {
                        string srcFilePath = dataDirPath + "/templates/" + template;
                        string destFilePath = projectRoleDirPath + "/templates/" + template;
                        File.Copy(srcFilePath,destFilePath);
                    }
                }
            }

            // HANDLE COMPOSE FILE
            string composeFilePath = dataDirPath + "/compose.yml";
            if(File.Exists(composeFilePath))
            {
                if (!Directory.Exists(projectRoleDirPath + "/templates"))
                {
                    Directory.CreateDirectory(projectRoleDirPath + "/templates");
                }
                File.Copy(composeFilePath, projectRoleDirPath + "/templates/compose.yml");
            }
            // HANDLE LOCAL ANSIBLE DEFAULT VARS FILE
            string configVarsFilePath = dataDirPath + "/default.yml";
            if(File.Exists(configVarsFilePath))
            {
                Directory.CreateDirectory(projectRoleDirPath + "/defaults");
                string localRoleVarsFilePath = projectRoleDirPath + "/defaults/main.yaml";
                string localRoleVarsFileContents = File.ReadAllText(configVarsFilePath);
                File.WriteAllText(localRoleVarsFilePath, localRoleVarsFileContents);
            }
            // HANDLE TASK FILE
            string taskFileContents = TaskHandler.TasksFileGenerate(config, dataDirPath);
            File.WriteAllText(projectRoleDirPath + "/tasks/main.yaml", taskFileContents);
        }

        public static void HostVar(Config config, string dataDirPath, string localAnsibleDirPath)
        {
            string projectRoleDirPath = localAnsibleDirPath + "/roles/" + config.ProjectName;
            string hostVarsDirPath = localAnsibleDirPath + "/inventory/host_vars/";
            void fileInsertsWrite(string host, Dictionary<string,string> fileInserts)
            {
                string hostFilePath = hostVarsDirPath + host + ".yaml";
                List<string> fileLines = File.ReadAllLines(hostFilePath).ToList();
                foreach (KeyValuePair<string, string> fileInsert in fileInserts)
                {
                    int insertIndex = fileLines.FindIndex(line => line.Contains(fileInsert.Key));
                    fileLines.Insert(insertIndex + 1, fileInsert.Value);
                }
                string updatedFileContents = string.Join(Environment.NewLine, fileLines);
                File.WriteAllText(hostFilePath, updatedFileContents);
            };

            if(config.Hosts != Array.Empty<string>())
            {
                foreach(string host in config.Hosts)
                {
                    string hostVarFilePath = hostVarsDirPath + host + ".yaml";
                    string configHostVarFilePath = dataDirPath + "/host_vars/" + host;
                    string varContents = "";
                    if(File.Exists(configHostVarFilePath))
                    {
                        varContents = "\n" + File.ReadAllText(configHostVarFilePath) + "\n\n";
                    }
                    File.AppendAllText(hostVarFilePath, varContents);
                    Dictionary<string, string> networkSetupHostVarFileInserts = VarHandler.NetworkSetupHostVarFileInsertsGenerate(config, dataDirPath, host);
                    fileInsertsWrite(host, networkSetupHostVarFileInserts);
                }
            }

            Dictionary<string, string> userGateHostFileInserts = VarHandler.UserGateFileInsertsGenerate(config, dataDirPath);
            fileInsertsWrite(config.UserGateHost, userGateHostFileInserts);
        }

        public static void ToolData(string roleName, string dataDirPath, string localAnsibleDirPath)
        {
            string source = dataDirPath;
            string destination = localAnsibleDirPath + "/roles_data/" + roleName + "/";
            Directory.CreateDirectory(destination);
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(source, destination);
        }

        public static void PrepareTargetAnsibleDir(string targetAnsibleDirPath)
        {
            bool emptyTargetDirectory = Directory.GetDirectories(targetAnsibleDirPath).Length == 0 &&
                                        Directory.GetFiles(targetAnsibleDirPath).Length == 0;
            if (!emptyTargetDirectory)
            {
                Directory.Delete(targetAnsibleDirPath,true);
            }
            Directory.CreateDirectory(targetAnsibleDirPath);
            Directory.CreateDirectory(targetAnsibleDirPath + "/inventory");
            Directory.CreateDirectory(targetAnsibleDirPath + "/inventory/host_vars");
            Directory.CreateDirectory(targetAnsibleDirPath + "/roles");
        }
    }
}