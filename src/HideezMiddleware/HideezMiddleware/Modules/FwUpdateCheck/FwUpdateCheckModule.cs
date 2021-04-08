using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.FwUpdateCheck.Messages;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.FwUpdateCheck
{
    public class FwUpdateCheckModule : ModuleBase
    {
        private readonly RegistryKey _registryKey;
        private readonly string _updateConfigUrl = "https://hls.hideez.com/api/Firmwares/GetFirmwares/ST102";
        private readonly string _downloadFwUrl = "https://hls.hideez.com/api/Firmwares/GetFirmware/";
        private readonly string _fwUpdateCacheRegistryValueName = "fw_update_cache_path";

        public FwUpdateCheckModule(RegistryKey registryKey, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(FwUpdateCheckModule), log)
        {
            _registryKey = registryKey;

            Task.Run(RunUpdateCheck);
        }

        async Task RunUpdateCheck()
        {
            try
            {
                var availableUpdateInfo = GetAvailableFwUpdateInfo();
                if (availableUpdateInfo != null)
                {
                    DeleteCachedUpdate();
                    DownloadUpdate(availableUpdateInfo);
                }

                await CheckCachedUpdate();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        private string GetCachedFilePath()
        {
            try
            {
                string filepath = _registryKey.GetValue(_fwUpdateCacheRegistryValueName) as string;
                if (!string.IsNullOrWhiteSpace(filepath) && File.Exists(filepath))
                    return filepath;
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            return null;
        }

        private bool DownloadUpdate(FwUpdateInfo updateInfo)
        {
            try
            {
                string filename = $"{updateInfo.Version}.img";
                string folderPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string targetPath = Path.Combine(folderPath, filename);

                byte[] downloadedData;

                downloadedData = new byte[0];

                //open a data stream from the supplied URL
                WebRequest webReq = WebRequest.Create(_downloadFwUrl+updateInfo.Id);
                WebResponse webResponse = webReq.GetResponse();

                //Download the data in chuncks
                byte[] dataBuffer = new byte[1024];

                //Download the data
                using (Stream dataStream = webResponse.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        while (true)
                        {
                            //Let's try and read the data
                            int bytesFromStream = dataStream.Read(dataBuffer, 0, dataBuffer.Length);

                            if (bytesFromStream == 0)
                            {
                                //Download complete
                                break;
                            }
                            else
                            {
                                //Write the downloaded data
                                ms.Write(dataBuffer, 0, bytesFromStream);
                            }
                        }


                        //Convert the downloaded stream to a byte array
                        downloadedData = ms.ToArray();
                    }
                }
                //Convert the downloaded bytes to file data
                var base64Str = Encoding.UTF8.GetString(downloadedData);
                var fileData = Convert.FromBase64String(base64Str);

                // Ensure that target path is available
                Directory.CreateDirectory(folderPath);
                File.Delete(targetPath);

                //Write bytes to the specified file
                using (FileStream fs = new FileStream(targetPath, FileMode.Create))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }

                // Save path to downloaded file in registry
                _registryKey.SetValue(_fwUpdateCacheRegistryValueName, targetPath, RegistryValueKind.String);

                return true;
            }

            catch (Exception)
            {
                //We may not be connected to the internet
                //Or the URL may be incorrect
                return false;
            }
        }

        FwUpdateInfo GetAvailableFwUpdateInfo()
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(_updateConfigUrl);
                WebResponse webResponse = webRequest.GetResponse();

                JsonDocument jsonDocument = null;
                using (Stream dataStream = webResponse.GetResponseStream())
                {
                    jsonDocument = JsonDocument.Parse(dataStream);
                }

                string id = jsonDocument.RootElement[0].GetProperty("id").GetString();
                string version = jsonDocument.RootElement[0].GetProperty("version").GetString();
                string deviceModel = jsonDocument.RootElement[0].GetProperty("deviceModel").GetString();

                return new FwUpdateInfo
                {
                    Id = id,
                    Version = version,
                    DeviceModel = deviceModel
                };
            }
            catch (Exception)
            {
                // Any number of errors may prevent us from getting an update config
                return null;
            }
        }

        // Delete update file and registry reference to its path
        private void DeleteCachedUpdate()
        {
            string filepath = _registryKey.GetValue(_fwUpdateCacheRegistryValueName) as string;
            if (!string.IsNullOrWhiteSpace(filepath) && File.Exists(filepath))
            {
                File.Delete(filepath);

                var dir = Path.GetDirectoryName(filepath);
                if (dir != Path.GetTempPath())
                    Directory.Delete(dir);

                _registryKey.DeleteValue(_fwUpdateCacheRegistryValueName);
            }
        }

        // Check cache for possible update and publish a message if cached update is available
        private async Task CheckCachedUpdate()
        {
            var filePath = GetCachedFilePath();
            if (!string.IsNullOrEmpty(filePath))
                await SafePublish(new FwUpdateAvailableMessage(filePath, Path.GetFileNameWithoutExtension(filePath)));
        }
    }
}
