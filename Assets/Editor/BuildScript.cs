using UnityEditor;

public class BuildScript
{
    public static void BuildWebGL()
    {
        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            "build/WebGL",
            BuildTarget.WebGL,
            BuildOptions.None
        );
    }
}
