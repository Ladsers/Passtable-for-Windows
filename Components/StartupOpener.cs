namespace Passtable.Components
{
    public static class StartupOpener
    {
        public static void OpenFile(MainWindow window, string[] openCommands)
        {
            if (openCommands.Length > 2)
            {
                window.PrimaryPassword = openCommands[2];
            }

            if (openCommands.Length > 1)
            {
                window.OpenFile(openCommands[1]);
            }
        }
    }
}