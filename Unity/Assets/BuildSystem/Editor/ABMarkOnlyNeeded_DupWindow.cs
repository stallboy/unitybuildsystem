namespace BuildSystem
{
    public class ABMarkOnlyNeeded_DupWindow : ABMarkDupWindow
    {
        public static void CheckDup()
        {
            ABMarkDupChecker.CheckDuplicate("dup_markonlyneeded.csv");
        }

        public static void ShowDupWindow()
        {
            GetWindow<ABMarkOnlyNeeded_DupWindow>();
        }

        protected override string GetLoadCsvFile()
        {
            return "dup_markonlyneeded.csv";
        }

        protected override string GetTitle()
        {
            return "MarkOnlyNeededDup";
        }
    }
}