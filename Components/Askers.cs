using Passtable.Resources;

namespace Passtable.Components
{
    public static class Askers
    {
        public enum Mode
        {
            Open,
            Save,
            SaveAs
        }

        public static bool AskPrimaryPassword(MainWindow window, Mode mode, bool incorrectPassword,
            ref string primaryPassword)
        {
            string title;
            if (mode == Mode.Open) title = Strings.title_openFile;
            else if (mode == Mode.Save) title = Strings.title_createNewFile;
            else title = Strings.title_saveAs; //Save as

            var primaryPasswordWindow = new MasterPasswordWindow
            {
                Owner = window,
                Title = title
            };

            if (mode == Mode.Open && incorrectPassword) primaryPasswordWindow.ShowMsgIncorrectPassword();
            if (mode != Mode.Open) primaryPasswordWindow.PrepareForSave(mode);

            if (primaryPasswordWindow.ShowDialog() != true) return false;
            primaryPassword = mode == Mode.SaveAs && primaryPasswordWindow.WithoutChange
                ? primaryPassword
                : primaryPasswordWindow.pbPassword.Password;
            return true;
        }
    }
}