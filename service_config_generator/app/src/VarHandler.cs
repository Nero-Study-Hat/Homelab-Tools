using System.Text;

namespace ServiceConfigGenerator.src
{
    public class VarHandler
    {
        public static string NetworkSetupCheck(string networkSetup, string hostVarFilePath)
        {
            List<string> errors = new List<string>();
            string[] validNetSetups = ["t3_only", "ts_only", "shared", "none"];
            bool isValidSetup = false;
            foreach (string validNetSetup in validNetSetups)
            {
                if (string.Equals(validNetSetup, networkSetup))
                {
                    isValidSetup = true;
                    break;
                }
            }
            if (isValidSetup == false)
            {
                errors.Add("Tool Data Error: Invalid network setup provided.");
            }

            string report = Helpers.CheckReport(errors, false);
            return report;
        }

        public static string NetworksHostVarGenerate(string networkType, string serviceName)
        {
            // types: ts_only, t3_only, shared, none
            if(string.Equals(networkType,"none"))
            {
                return "";
            }
            StringBuilder stringBuilder = new StringBuilder();
            string postCommentSpaces = "    ";
            string postNameSpaces = "      ";
            string networkBackend = "\"{{" + $"{serviceName}.network_backend.";
            stringBuilder.Append($"  # {serviceName}\n");
            stringBuilder.Append(postCommentSpaces + "- name: " + networkBackend + "name" + "}}\"\n");
            stringBuilder.Append(postNameSpaces + "ipam_options: " + networkBackend + "ipam_options" + "}}\"\n");
            if (string.Equals(networkType, "t3_only"))
            {
                stringBuilder.Append(postNameSpaces + "traefik_ip: " + networkBackend + "traefik_ips.traefik" + "}}\"");
            }
            if (string.Equals(networkType, "ts_only"))
            {
                stringBuilder.Append(postNameSpaces + "tailscale_ip: " + networkBackend + "tailscale_ips.tailscale" + "}}\"");
            }
            if (string.Equals(networkType, "shared"))
            {
                stringBuilder.Append(postNameSpaces + "traefik_ip: " + networkBackend + "ips.traefik" + "}}\"\n");
                stringBuilder.Append(postNameSpaces + "tailscale_ip: " + networkBackend + "ips.tailscale" + "}}\"");
            }
            string generatedContent = stringBuilder.ToString();
            return generatedContent;
        }

        // return key: host name - will be used to search file to place the entry contents
        // return value: file entry block - per container instance
        public static Dictionary<string,string> UserGateEntryHostVarGenerate(Dictionary<string, Dictionary<string,List<string>>> entryDetails)
        {
            Dictionary<string, string> userGateEntries = new Dictionary<string, string>();
            StringBuilder stringBuilder = new StringBuilder();
            for (int hostIndex = 0; hostIndex < entryDetails.Count; hostIndex++)
            {
                string currentHostName = entryDetails.ElementAt(hostIndex).Key;
                Dictionary<string,List<string>> currentHostEntryDetails = entryDetails.ElementAt(hostIndex).Value;
                for (int index = 0; index < currentHostEntryDetails.Count; index++)
                {
                    KeyValuePair<string,List<string>> currentPairContainerInstanceEntryDetails = currentHostEntryDetails.ElementAt(index);
                    string containerInstanceDomainPrefix = currentPairContainerInstanceEntryDetails.Key;
                    List<string> containerInstanceAllowedIps = currentPairContainerInstanceEntryDetails.Value;
                    string entryPrefix = $"""
                        - name: {containerInstanceDomainPrefix}.{currentHostName}.wiresndreams.dev
                          allowed_tailscale_client_ips:
                    """;
                    stringBuilder.AppendLine(entryPrefix);
                    foreach (string ipListing in containerInstanceAllowedIps)
                    {
                        stringBuilder.Append("        - " + ipListing);
                        stringBuilder.Append('\n');
                    }
                }
                string hostEntry = stringBuilder.ToString().TrimEnd('\n'); // entry is one container instance block
                userGateEntries.Add(currentHostName, hostEntry);
                stringBuilder.Clear(); 
            }
            return userGateEntries;
        }

        public static Dictionary<string, string> UserGateFileInsertsGenerate(Config config, string dataDirPath)
        {
            Dictionary<string,string> fileInserts = new Dictionary<string, string>{};
            Dictionary<string,string> userGateVar = UserGateEntryHostVarGenerate(config.UserGateDetails);
            foreach(KeyValuePair<string,string> UserGateEntry in userGateVar)
            {
                string search = $"    domains_list: # {UserGateEntry.Key}";
                string entry = UserGateEntry.Value;
                fileInserts.Add(search, entry);
            }
            return fileInserts;
        }

        //NOTE: directories for ansible not made here is required done beforehand
        // already existing file conflicts also not handled here
        public static Dictionary<string, string> NetworkSetupHostVarFileInsertsGenerate(Config config, string dataDirPath, string host)
        {
            Dictionary<string, string> fileInserts = new Dictionary<string, string>();
            string networkSetupVar = NetworksHostVarGenerate(config.NetworkSetup, config.ProjectName);
            switch (config.NetworkSetup)
            {
                case "shared":
                    string search = "  shared:";
                    fileInserts.Add(search, networkSetupVar);
                    break;
                case "t3_only":
                    search = "  traefik_specific: # these containers don't have Tailscale DNS and connection-ability";
                    fileInserts.Add(search, networkSetupVar);
                    break;
                case "tailscale_only":
                    search = "  tailscale_specific: # these containers are not reachable via Traefik";
                    fileInserts.Add(search, networkSetupVar);
                    break;
            }
            return fileInserts;
        }

    }
}