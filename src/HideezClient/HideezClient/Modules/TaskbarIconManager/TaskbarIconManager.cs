using Hardcodet.Wpf.TaskbarNotification;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace HideezClient.Modules
{
    /// <summary>
    /// A class manage a taskbar icon (NotifyIcon) that sits in the system's taskbar
    //  notification area ("system tray").
    /// </summary>
    class TaskbarIconManager : ITaskbarIconManager, IDisposable
    {
        private TaskbarIconDataSource dataSource;
        private TaskbarIconViewModel viewModel;
        private IconState iconState;
        private readonly object frameArrayLock = new object();
        private Timer animTimer;
        private int frameIndex;

        public TaskbarIconManager(TaskbarIcon taskbarIcon, TaskbarIconViewModel viewModel)
        {
            this.viewModel = viewModel;
            taskbarIcon.DataContext = viewModel;
            taskbarIcon.ContextMenu.Opened += ContextMenu_Opened;


            InitializeAnimator();
        }
        
        /// <summary>
        /// Array of image Sources for current icon state.
        /// </summary>
        private ImageSource[] FrameArray { get; set; }

        /// <summary>
        /// The method that determine position of TaskbarIcon's context menu 
        /// </summary>
        /// <param name="sender">The object where the method is attached</param>
        /// <param name="e">The event data</param>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var g = Graphics.FromHwnd(IntPtr.Zero);
            var scaleX = g.DpiX / 96.0;
            var scaleY = g.DpiY / 96.0;

            System.Windows.Controls.ContextMenu contextMenu = sender as System.Windows.Controls.ContextMenu;

            contextMenu.HorizontalOffset = System.Windows.Forms.Cursor.Position.X / scaleX;
            contextMenu.VerticalOffset = System.Windows.Forms.Cursor.Position.Y / scaleY;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
        }

        /// <summary>
        /// Prepare data for animation and start animation if need.
        /// </summary>
        private void InitializeAnimator()
        {
            animTimer = new Timer();
            animTimer.Elapsed += AnimTimer_Elapsed;

            dataSource = TaskbarIconDataSource.Instance;
            iconState = IconState.Idle;
            OnIconStateChanged();
        }

        private void AnimTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            NextFrame();
        }

        /// <summary>
        /// Start icon animation if icon state has more than one images.
        /// </summary>
        /// <param name="interval">Millisecond. Interval for change icon image.</param>
        private void StartIconAnimation(double interval = 70)
        {
            // No need to start timer when array has only 1 frame
            if (FrameArray.Length > 1)
            {
                animTimer.Interval = interval;
                frameIndex = 0;
                animTimer.Start();
            }
            else
            {
                frameIndex = 0;
                NextFrame();
            }
        }

        /// <summary>
        /// Stop icon animation
        /// </summary>
        private void StopIconAnimation()
        {
            animTimer.Stop();
        }

        /// <summary>
        /// Calculate and set next image to IconSource from Frame Array.
        /// </summary>
        private void NextFrame()
        {
            try
            {
                lock (frameArrayLock)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (frameIndex < FrameArray.Length)
                            viewModel.IconSource = FrameArray[frameIndex];

                        if (++frameIndex >= FrameArray.Length)
                            frameIndex = 0;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Key from localisation dictionary for ToolTip
        /// </summary>
        private string IconToolTip
        {
            set { viewModel.ToolTip = value; }
        }

        /// <summary>
        /// Current icon state
        /// </summary>
        public IconState IconState
        {
            get { return iconState; }
            set
            {
                if (iconState != value)
                {
                    iconState = value;
                    OnIconStateChanged();
                }

            }
        }

        private void OnIconStateChanged()
        {
            StopIconAnimation();
            FrameArray = dataSource.Icons[IconState];
            IconToolTip = dataSource.IconToolTipKeyLocalize[IconState];
            StartIconAnimation();
        }

        #region Dispose pattern

        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            animTimer?.Dispose();

            disposed = true;
        }

        ~TaskbarIconManager()
        {
            Dispose(false);
        }

        #endregion Dispose pattern
    }
}