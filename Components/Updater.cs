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

            var lastVer = appVersion.windowsRelease;
            
            var currentVerTag = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            currentVerTag = currentVerTag.Remove(currentVerTag.Length - 2);

            return currentVerTag == lastVer ? UpdaterCheckResult.UpToDate : UpdaterCheckResult.NeedUpdate;
        }
    }
}