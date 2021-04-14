﻿using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.FwUpdateCheck.Messages;
using HideezMiddleware.Modules.UpdateCheck;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.FwUpdateCheck
{
    public class FwUpdateCheckModule : ModuleBase
    {
        private readonly RegistryKey _registryKey;
        private readonly string _updateConfigUrl = "https://hls.hideez.com/api/Firmwares/GetFirmwares/";
        private readonly string _downloadFwUrl = "https://hls.hideez.com/api/Firmwares/GetFirmware/";
        private readonly string _fwUpdateCacheRegistryValueName = "fw_update_cache_path";

        public FwUpdateCheckModule(RegistryKey registryKey, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(FwUpdateCheckModule), log)
        {
            _registryKey = registryKey;

            Task.Run(DeleteOldCachedUpdates);
            _messenger.Subscribe<GetFwUpdateByModelMessage>(OnGetFwUpdateFilePath);
            _messenger.Subscribe<GetFwUpdatesCollectionMessage>(OnGetFwUpdatesCollection);
        }

        //Delete the oldest used files if there are more than 10 ones
        private void DeleteOldCachedUpdates()
        {
            try
            {
                string folderPath = _registryKey.GetValue(_fwUpdateCacheRegistryValueName) as string;
                if(!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
                {
                    var filePaths = Directory.GetFiles(folderPath);
                    if (filePaths.Length > 10)
                    {
                        var filesDictionary = new Dictionary<string, DateTime>();
                        foreach (var filePath in filePaths)
                        {
                            var lastAccessTime = File.GetLastAccessTime(filePath);
                            filesDictionary.Add(filePath, lastAccessTime);
                        }

                        var sortedCollection = filesDictionary.OrderBy(pair => pair.Value).ToArray();
                        for (int i = 10; i < sortedCollection.Length; i++)
                        {
                            File.Delete(sortedCollection[i].Key);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLine(ex);
            }
        }

        private Task OnGetFwUpdatesCollection(GetFwUpdatesCollectionMessage arg)
        {
            return Task.CompletedTask;
        }

        private async Task OnGetFwUpdateFilePath(GetFwUpdateByModelMessage arg)
        {
            string filePath = string.Empty;
            try
            {
                var availableUpdateInfo = GetAvailableFwUpdateInfo(arg.DeviceModel);
                if (availableUpdateInfo.ReleaseStage == ReleaseStage.Release)
                {
                    var cachedFilePath = GetCachedFilePath(availableUpdateInfo.Version, arg.DeviceModel);
                    if (availableUpdateInfo != null && string.IsNullOrWhiteSpace(cachedFilePath))
                        DownloadUpdate(availableUpdateInfo);

                    filePath = GetCachedFilePath(availableUpdateInfo.Version, arg.DeviceModel);
                }
            }
            catch(Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                await _messenger.Publish(new GetFwUpdateByModelMessageResponse(filePath));
            }
        }

        /// <summary>
        /// Returns <see cref="CachedUpdateInfo"/> populated by data from local cache
        /// </summary>
        private string GetCachedFilePath(string version, string deviceModel)
        {
            try
            {
                string folderPath = _registryKey.GetValue(_fwUpdateCacheRegistryValueName) as string;
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    string filepath = Path.Combine(folderPath, $"fw{deviceModel}_{version}.img");
                    if (File.Exists(filepath))
                        return filepath;
                }
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
                string filename = $"fw{updateInfo.DeviceModel}_{updateInfo.Version}.img";
                string folderPath = _registryKey.GetValue(_fwUpdateCacheRegistryValueName) as string;
                if (string.IsNullOrWhiteSpace(folderPath))
                    folderPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string targetPath = Path.Combine(folderPath, filename);

                byte[] downloadedData;

                downloadedData = new byte[0];

                //open a data stream from the supplied URL
                WebRequest webReq = WebRequest.Create(_downloadFwUrl + updateInfo.Id);
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
                _registryKey.SetValue(_fwUpdateCacheRegistryValueName, folderPath, RegistryValueKind.String);

                return true;
            }
            catch (Exception)
            {
                //We may not be connected to the internet
                //Or the URL may be incorrect
                return false;
            }
        }

        FwUpdateInfo GetAvailableFwUpdateInfo(string deviceModel)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(_updateConfigUrl + deviceModel);
                WebResponse webResponse = webRequest.GetResponse();

                JsonDocument jsonDocument = null;
                using (Stream dataStream = webResponse.GetResponseStream())
                {
                    jsonDocument = JsonDocument.Parse(dataStream);
                }

                string id = jsonDocument.RootElement[0].GetProperty("id").GetString();
                string version = jsonDocument.RootElement[0].GetProperty("version").GetString();
                string releaseStageStr = jsonDocument.RootElement[0].GetProperty("releaseStage").GetString();
                Enum.TryParse(releaseStageStr, out ReleaseStage releaseStage);

                return new FwUpdateInfo
                {
                    Id = id,
                    Version = version,
                    DeviceModel = deviceModel,
                    ReleaseStage = releaseStage
                };
            }
            catch (Exception)
            {
                // Any number of errors may prevent us from getting an update config
                return null;
            }
        }
    }
}
