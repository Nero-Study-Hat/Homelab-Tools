using ServiceConfigGenerator.src;
using Newtonsoft.Json;
namespace tests;

public class Tests
{
    [Test]
    public void CorrectDirectoriesCheck()
    {
        string dataDirPath = "/mnt/nero-priv-data/Workspace/Tech/IT/Homelab/tools/service_config_generator/tests/TestData/TaskHandler/DirectoriesCheck";
        string configPath = dataDirPath + "/config.json";
        Config correctConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        Assert.DoesNotThrow(() => TaskHandler.DirectoriesCheck(correctConfig, dataDirPath, "y"));
    }

    [Test]
    public void IncorrectDirectoriesCheck()
    {
        Assert.Throws<Exception>(() => TaskHandler.DirectoriesCheck(_incorrectConfig, _dataDirPath, "y"));
    }
}
