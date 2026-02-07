using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;

public static class ProjectBuilders
{
    const string WindowsSignalRClientProfile = "Win_SignalR_Client";

    public static BuildProfile GetBuildProfile(string profileName)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(BuildProfile)}");
        
        foreach(var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);

            if(profile.name == profileName)
                return profile;
        }

        return null;
    }

    public static void BuildCommand()
    {
        string pathArg = "C:\\Users\\rahma\\Desktop\\Builds\\FunBoardGames.exe";

        var path = Path.GetDirectoryName(pathArg);

        if(Directory.Exists(path) == false)
            Directory.CreateDirectory(path);

        BuildWindowsSignalRClient(pathArg);
    }

    public static void BuildWindowsSignalRClient(string outPath)
    {
        BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
        {
            buildProfile = GetBuildProfile(WindowsSignalRClientProfile),
            options = BuildOptions.None,
            locationPathName = outPath,
        });
    }
}
