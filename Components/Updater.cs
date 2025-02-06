using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Passtable.Containers;

namespace Passtable.Components
{
    public static class Updater
    {
        private const string Source = "https://ladsers.com/wp-content/uploads/passtable-appversion.json";

        public static async Task<UpdaterCheckResult> Check()
        {
            string data;
            AppVersion appVersion;

            try
            {
                var task = new HttpClient().GetStringAsync(Source);
                data = await task;
                task.Dispose();
            }
            catch(Exception)
            {
                return UpdaterCheckResult.ConnectionError;
            }
            
            try
            {
                appVersion = JsonConvert.DeserializeObject<AppVersion>(data);
            }
            catch (Exception)
            {
                return UpdaterCheckResult.ParsingError;
            }

            var serverVersionTag = appVersion.windowsRelease;

            var currentVersionTag = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            currentVersionTag = currentVersionTag.Remove(currentVersionTag.Length - 2);

            return CompareVersions(currentVersionTag, serverVersionTag);
        }

        private static UpdaterCheckResult CompareVersions(string currentVersionTag, string serverVersionTag)
        {
            var currentVersion = new Version(currentVersionTag);
            var serverVersion = new Version(serverVersionTag);

            return serverVersion.CompareTo(currentVersion) > 0
                ? UpdaterCheckResult.NeedUpdate
                : UpdaterCheckResult.UpToDate;
        }
    }
}