namespace Passtable.Components
{
    public static class Askers
    {
        public static bool AskPrimaryPassword(MainWindow window, bool saveMode, bool incorrectPassword,
            out string primaryPassword)
        {
            primaryPassword = "";
            var primaryPasswordWindow = new MasterPasswordWindow
            {
                Owner = window,
                Title = "Enter master password",
                SaveMode = saveMode,
                IncorrectPassword = incorrectPassword
            };
            if (primaryPasswordWindow.ShowDialog() != true) return false;
            primaryPassword = primaryPasswordWindow.pbPassword.Password;
            return true;
        }
    }
}