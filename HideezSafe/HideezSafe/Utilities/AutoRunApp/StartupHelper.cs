using System;
using System.Linq;
using Microsoft.Win32;
using System.Windows.Forms;

namespace HideezSafe.Utils
{
    public class StartupHelper : IStartupHelper
	{
		/// <summary>
		/// Add app to startap list
		/// </summary>
		/// <returns>if success add</returns>
		public bool AddToStartup()
		{
			return AddToStartup(AppName, ExecutablePath);
		}

		/// <summary>
		/// Add app to startap list
		/// </summary>
		/// <param name="appName">registry key</param>
		/// <returns>if success add</returns>
		public bool AddToStartup(string appName)
		{
			return AddToStartup(appName, ExecutablePath);
		}

		/// <summary>
		/// Add app to startap list
		/// </summary>
		/// <param name="appName">registry key</param>
		/// <param name="path">path to executable file</param>
		/// registry key
		/// <returns>if success add</returns>
		public bool AddToStartup(string appName, string path)
		{
			if (string.IsNullOrEmpty(appName))
				throw new ArgumentException("Value can not be empty", nameof(appName));

			try
			{
				if (IsWindows8orHigher)
				{
					RemoveFromStartup();
				}
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
				{
					registryKey.SetValue(appName, path);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Remove app from startap list
		/// </summary>
		/// <returns>if success add</returns>
		public bool RemoveFromStartup()
		{
			return RemoveFromStartup(AppName);
		}

		/// <summary>
		/// Remove app from startap list
		/// </summary>
		/// <param name="appName">registry key</param>
		/// <returns>if success add</returns>
		public bool RemoveFromStartup(string appName)
		{
			if (string.IsNullOrEmpty(appName))
				throw new ArgumentException("Value can not be empty", nameof(appName));

			bool res;
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
				{
					if (registryKey != null)
					{
						registryKey.DeleteValue(appName);
					}
				}
				res = true;
			}
			catch (Exception ex)
			{
				res = false;
			}

			try
			{
				if (IsWindows8orHigher)
				{
					// values for Task Manager
					using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run", true))
					{
						if (registryKey != null)
						{
							registryKey.DeleteValue(appName);
						}
					}
				}
				res = true;
			}
			catch (Exception ex)
			{
				if (!res)
				{
					res = false;
				}
			}
			return res;
		}

		/// <summary>
		/// Check if app in startap list
		/// </summary>
		public bool IsInStartup()
		{
			return IsInStartup(AppName);
		}

		/// <summary>
		/// Check is app in startap list
		/// </summary>
		/// <param name="appName">registry key</param>
		/// <returns></returns>
		public bool IsInStartup(string appName)
		{
			if (string.IsNullOrEmpty(appName))
				throw new ArgumentException("Value can not be empty", nameof(appName));

			bool isInStartup = false;

			// windows 8 or higher
			if (IsWindows8orHigher)
			{
				try
				{
					using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
					{
						object res = registryKey.GetValue(appName);
						if (res == null)
						{
							isInStartup = false;
						}
						else
						{
							// values for Task Manager
							using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run"))
							{
								byte[] data = (byte[])rk.GetValue(appName);
								if (data == null)
								{
									isInStartup = true;
								}
								else if (data[0] == 2 || data.Sum(v => v) == 0)
								{
									isInStartup = true;
								}
								else
								{
									isInStartup = false;
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
			else
			{
				try
				{
					using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
					{
						object value = registryKey.GetValue(appName);
						if (value == null)
						{
							isInStartup = false;
						}
						else
						{
							isInStartup = true;
						}
					}
				}
				catch (Exception)
				{
				}
			}

			return isInStartup;
		}

		/// <summary>
		/// Return name this aplication
		/// </summary>
		private string AppName
		{
			get
			{
				return Application.ProductName;
			}
		}

		/// <summary>
		/// Return path of the executable file
		/// </summary>
		private string ExecutablePath
		{
			get
			{
				return Application.ExecutablePath;
			}
		}

		/// <summary>
		/// Check windows version
		/// </summary>
		private bool IsWindows8orHigher
		{
			get
			{
				Version version = Environment.OSVersion.Version;

				// version of windows
				// 5.1 Windows Xp 32x
				// 5.2 Windows Xp 64x
				// 6.0 Windows Vista
				// 6.1 Windows 7
				// 6.2 Windows 8

				if (version.Major == 6 && version.Minor >= 2 || version.Major == 10)
					return true;
				else
					return false;
			}
		}
	}
}