using ServiceConfigGenerator.src;
using Newtonsoft.Json;
namespace tests;

public class Tests
{
    public static readonly string _testDataPath = "/mnt/nero-priv-data/Workspace/Tech/Dev/Major/Homelab-Tools/service_config_generator/tests/TestData";

    [Test]
    public void CorrectDirectoriesCheck()
    {
        string dataDirPath = _testDataPath + "/TaskHandler/DirectoriesCheck/Pass";
        string configPath = dataDirPath + "/config.json";
        Config correctConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        string result = TaskHandler.DirectoriesCheck(correctConfig, dataDirPath, "y");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void IncorrectDirectoriesCheck()
    {
        string dataDirPath = _testDataPath + "/TaskHandler/DirectoriesCheck/Fail";
        string configPath = dataDirPath + "/config.json";
        Config incorrectConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        string result = TaskHandler.DirectoriesCheck(incorrectConfig, dataDirPath, "n");
        Assert.That(result, Is.Not.Empty);
    }

    // [Test]
    // public void UserGateEntryHostVarGenerateTest()
    // {
    //     Dictionary<string, List<string>> entryDetails = new Dictionary<string, List<string>>();
    //     entryDetails["day"] = ["111.11.111 # stardom", "22.222.22 # starfief"];
    //     entryDetails["night"] = ["111.11.111 # stardom", "33.333.33 # starfief"];
    //     string serviceName = "test";
    //     Dictionary<string, string> result = VarHandler.UserGateEntryHostVarGenerate(entryDetails, serviceName);

    //     string dayValue = """
    //     - name: test.day.wiresndreams.dev
    //         allowed_tailscale_client_ips:
    //         - 111.11.111 # stardom
    //         - 22.222.22 # starfief
    //     """;
    //     dayValue = dayValue.Replace("\r", "");
    //     string mediaValue = """
    //     - name: test.night.wiresndreams.dev
    //         allowed_tailscale_client_ips:
    //         - 111.11.111 # stardom
    //         - 33.333.33 # starfief
    //     """;
    //     mediaValue = mediaValue.Replace("\r", "");
    //     Dictionary<string, string> expectedResult = new Dictionary<string, string>
    //     {
    //         {"day", dayValue},
    //         {"night", mediaValue}
    //     };
    //     Assert.That(result, Is.EqualTo(expectedResult));
    // }

    [Test]
    public void NetworksHostVarGenerateTest()
    {
        Dictionary<string, List<string>> netSetup = new Dictionary<string, List<string>>();
        string networkType = "t3_only";
        string serviceName = "test";
        string result = VarHandler.NetworksHostVarGenerate(networkType, serviceName);

        string expectedResult = """
          # test
            - name: "{{test.network_backend.name}}"
              ipam_options: "{{test.network_backend.ipam_options}}"
              traefik_ip: "{{test.network_backend.traefik_ips.traefik}}"
        """;
        expectedResult = expectedResult.Replace("\r", "");

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    // [Test]
    // public void FileSysLogicTest()
    // {
    //     string dataDirPath = _testDataPath + "/VarHandler/FileSysLogicPassTest";
    //     string configPath = dataDirPath + "/config.json";
    //     Config correctConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
    //     VarHandler.FileSysLogic(correctConfig, dataDirPath);
    // }
}