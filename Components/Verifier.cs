using System.Linq;
using System.Text.RegularExpressions;

namespace Passtable.Components
{
    /// <summary>
    /// An object containing methods for verifying data entered by the user.
    /// </summary>
    public static class Verifier
    {
        /// <summary>
        /// Invalid characters of Unix & OS Windows for the file name.
        /// </summary>
        private const string FileNameInvalidChars = "\\ / : * ? \" < > |";

        /// <summary>
        /// Invalid words of OS Windows for the file name.
        /// </summary>
        private const string FileNameInvalidWinWords = "COM0..COM9 LPT0..LPT9 CON PRN AUX NUL CONIN$ CONOUT$";

        /// <summary>
        /// Get string containing allowed characters for use in the primary password.
        /// </summary>
        /// <param name="translationForSpace"></param>
        /// <returns>String containing allowed characters for use in the primary password.</returns>
        public static string GetPrimaryAllowedChars(string translationForSpace = "space")
        {
            return $"A..Z a..z 0..9 {translationForSpace}\n" +
                   "@ $ # % & ~ ! ? = + * - _ . , : ; ' \" ` ^ ( ) < > [ ] { } \\ / |";
        }

        /// <summary>
        /// Verify the primary password.
        /// </summary>
        /// <param name="primaryPass"></param>
        /// <returns>[0] - the primary password is correct, [1] - the primary password is empty,
        /// [2] - the primary password contains invalid character,
        /// [3] - the primary password starts with "/" (unacceptable), [4] - the primary password is too long.</returns>
        public static int VerifyPrimary(string primaryPass)
        {
            if (string.IsNullOrEmpty(primaryPass)) return 1;
            if (ContainsRegex(primaryPass, "[^ -~]")) return 2;
            if (primaryPass.StartsWith("/")) return 3;
            if (primaryPass.Length > 32) return 4;
            return 0;
        }

        private static bool ContainsRegex(string input, string pattern)
        {
            return input.Any(ch => Regex.IsMatch(ch.ToString(), pattern));
        }

        /// <summary>
        /// Verify the file name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>[0] - the file name is correct, [1] - the file name is blank,
        /// [2] - the file name contains invalid characters, [3] - the file name starts with whitespace character,
        /// [4] - the file name is invalid word for OS Windows, [5] - the file name is too long.</returns>
        public static int VerifyFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return 1;
            if (ContainsRegex(name, "[\\x00-\\x1F/\\\\:*?\"<>|]")) return 2;
            if (name.StartsWith(" ")) return 3;
            if (Regex.IsMatch(name, "(?i)(^(COM[0-9]|LPT[0-9]|CON|CONIN\\$|CONOUT\\$|PRN|AUX|NUL))")) return 4;
            if (name.Length > 200) return 5;
            return 0;
        }

        /// <summary>
        /// Verify the data for the suitability of adding to the collection.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Can the data be added to the collection?</returns>
        public static bool VerifyData(params string[] data)
        {
            return data.Where(d => !string.IsNullOrEmpty(d)).All(d => !ContainsRegex(d, "[\\x00-\\x1F]"));
        }

        /// <summary>
        /// Verify the data set against the item rule.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Can the item be added to the collection?</returns>
        public static bool VerifyItem(string note, string username, string password)
        {
            var noteIsBlank = string.IsNullOrWhiteSpace(note);
            var usernameIsBlank = string.IsNullOrWhiteSpace(username);
            var passwordIsEmpty = string.IsNullOrEmpty(password);
            return !noteIsBlank || !usernameIsBlank && !passwordIsEmpty;
        }
    }
}