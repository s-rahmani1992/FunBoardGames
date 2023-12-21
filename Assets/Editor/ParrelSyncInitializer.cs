using ParrelSync;
using UnityEditor;

public class ParrelSyncInitializer
{
    [InitializeOnLoadMethod]
    static void InitializeClone()
    {
        if (!ClonesManager.IsClone())
            return;

        string cloneArgument = ClonesManager.GetArgument();

        if (!PlayerSettings.productName.Contains(cloneArgument))
            PlayerSettings.productName = $"{PlayerSettings.productName}_{cloneArgument}";
    }
}
