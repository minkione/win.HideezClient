using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HideezClient.Modules
{
    /// <summary>
    ///  Represents an object type that has a dictionary of classes ImageSource and keys for localization dictionary
    /// </summary>
    class TaskbarIconDataSource
    {
        private static TaskbarIconDataSource instance;

        private TaskbarIconDataSource()
        {
            LoadIconSources();
        }

        /// <summary>
        /// Singleton instance data source
        /// </summary>
        public static TaskbarIconDataSource Instance
        {
            get { return instance ?? (instance = new TaskbarIconDataSource()); }
        }

        /// <summary>
        /// Contains icons images for icon states
        /// </summary>
        public IDictionary<IconState, ImageSource[]> Icons { get; } = new Dictionary<IconState, ImageSource[]>();

        /// <summary>
        /// Contains keys for localisation dictionary for icon states
        /// </summary>
        public IDictionary<IconState, string> IconToolTipKeyLocalize { get; } = new Dictionary<IconState, string>
        {
            { IconState.Idle, "" },
            { IconState.IdleAlert, "Icon.NewNotificationsAvailable" },
            { IconState.Synchronizing, "Icon.SynchronizationInProgress" },
            { IconState.NoServiceConnection, "Icon.FailedConnectHideezService" },
            { IconState.NoKeyConnection, "Icon.DeviceNotConnected" },
            { IconState.NoKeyConnectionAlert, "Icon.NoKeyConnectionAlert" },
        };

        /// <summary>
        /// Load icons images from resources
        /// </summary>
        private void LoadIconSources()
        {
            // All icons are embedded as resources
            const string folder = "pack://application:,,,/Resources/Icon/";

            Icons[IconState.Idle] = LoadImagesArray($"{folder}Idle/", "Idle1.ico");
            Icons[IconState.IdleAlert] = LoadImagesArray($"{folder}Alert/", "Alert1.ico");
            Icons[IconState.Synchronizing] = LoadImagesArray($"{folder}Sync/", "Sync1.ico", "Sync2.ico", "Sync3.ico", "Sync4.ico", "Sync5.ico", "Sync6.ico", "Sync7.ico", "Sync8.ico");
            Icons[IconState.NoServiceConnection] = LoadImagesArray($"{folder}NoCon/", "NoCon1.ico");
            Icons[IconState.NoKeyConnection] = LoadImagesArray($"{folder}NoKey/", "NoKey1.ico");
            Icons[IconState.NoKeyConnectionAlert] = LoadImagesArray($"{folder}NoKeyAlert/", "NoKeyAlert1.ico");
        }

        /// <summary>
        /// Load array of images from list of name image.
        /// </summary>
        /// <param name="folder">Path to the folder with images.</param>
        /// <param name="fileNames">Array of name fo image.</param>
        /// <returns>Inited array ImageSource.</returns>
        private ImageSource[] LoadImagesArray(string folder, params string[] fileNames)
        {
            var count = fileNames.Length;
            ImageSource[] sources = new ImageSource[count];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    string filePath = $"{folder}{fileNames[i]}";
                    Uri uri = new Uri(filePath, UriKind.Absolute);
                    sources[i] = new BitmapImage(uri);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.Assert(false);
            }
            return sources;
        }
    }
}
