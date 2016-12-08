namespace BuildSystem
{
    public class ABMarkByRule_DupWindow : ABMarkDupWindow
    {
        public static void CheckDup()
        {
            ABMarkDupChecker.CheckDuplicate("dup_markbyrule.csv");
        }
        public static void ShowDupWindow()
        {
            GetWindow<ABMarkByRule_DupWindow>();
        }

        protected override string GetLoadCsvFile()
        {
            return "dup_markbyrule.csv";
        }

        protected override string GetTitle()
        {
            return "MarkByRuleDup";
        }
    }
}