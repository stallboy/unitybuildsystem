namespace BuildSystem
{
    public class ABMarkByRule : ABMark
    {
        public static void DoTheMarkAndSaveToCsv()
        {
            var marker = new ABMarkByRule();
            marker.MarkAllAndSave("markbyrule.csv");
            marker.SaveToConfig();
        }

        protected override void DoMark()
        {
            MarkDir("Assets/Standard Assets/Prototyping/Prefabs", "*.prefab");
            MarkDirName("Assets/Standard Assets/Prototyping/Shaders", "*.shader", "shader");
            MarkDir("Assets/Standard Assets/CrossPlatformInput/Sprites", "*.png");
        }
    }
}