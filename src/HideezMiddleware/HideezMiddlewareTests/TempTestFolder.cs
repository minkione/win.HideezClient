using System;
using System.IO;

namespace HideezMiddleware.Tests
{
    /// <summary>
    /// Provides a temporary directory for tests of classes that cannot be separated from file system. 
    /// Directory is automatically removed along with all data when <see cref="TempTestFolder"/> object is disposed.
    /// If directory already exists, it is deleted during object construction.
    /// </summary>
    class TempTestFolder : IDisposable
    {
        const string TEST_BASE_DIRECTORY = "middleware tests";

        public string FolderPath { get; }

        public TempTestFolder(string testClassName)
        {
            FolderPath = Path.Combine(Path.GetTempPath(), TEST_BASE_DIRECTORY, testClassName);

            // Make sure data is deleted before starting a test
            DeleteFolder();
        }

        void DeleteFolder()
        {
            if (Directory.Exists(FolderPath))
                Directory.Delete(FolderPath, true);
        }

        #region IDisposable Support
        bool disposed = false; 
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // ...
                }

                DeleteFolder();

                disposed = true;
            }
        }

        ~TempTestFolder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
