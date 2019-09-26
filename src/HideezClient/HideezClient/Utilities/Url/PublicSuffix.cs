using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Utilities
{
    internal class PublicSuffix : IPublicSuffix
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly string urlSuffixList = @"https://publicsuffix.org/list/public_suffix_list.dat";
        private readonly int validDays = 7;
        private readonly string fileWithSuffixName = "public_suffix_list.dat";
        private readonly string fileFromResource;
        private readonly string fileFromLocal;
        private readonly Trie prefixTree = new Trie();

        public PublicSuffix()
        {
            fileFromResource = AppDomain.CurrentDomain.BaseDirectory + @"\Resources\Data\" + fileWithSuffixName;
            fileFromLocal = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Hideez\Data\" + fileWithSuffixName;
            ThreadPool.QueueUserWorkItem(obj =>
            {
                try
                {
                    ParseSuffixListIntoTree();
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
            });
        }

        /// <summary>
        /// TLD: top-level domain
        /// </summary>
        /// <param name="hostname">Example: mail.example.com.ua</param>
        /// <returns>com.ua</returns>
        public string GetTLD(string hostname)
        {
            string tld = hostname;

            while (true)
            {
                int index = tld.IndexOf('.');
                if (index > 0)
                {
                    tld = tld.Substring(index + 1);
                    if (prefixTree.HasPrefix(tld))
                    {
                        return tld;
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        private void Error(Exception ex)
        {
            Debug.WriteLine($"DEBUG EXCEPTION in {Assembly.GetCallingAssembly().GetName().Name}, Exception type: {ex.GetType()}, Message: {ex.Message}");
        }

        /// <summary>
        /// Read suffixes from file and parse them into prefix tree
        /// </summary>
        private void ParseSuffixListIntoTree()
        {
            try
            {
                string fileContext = GetDataWithSuffix();
                var lines = fileContext?.Split('\n')?.Where(s => !(s.Equals("") || s.StartsWith("//", StringComparison.OrdinalIgnoreCase)));
                foreach (var line in lines)
                {
                    prefixTree.AddWord(line);
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        private void LoadSuffixListFromWeb()
        {
            string strContent;
            var webRequest = WebRequest.Create(urlSuffixList);
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
            }

            File.WriteAllText(fileFromLocal, strContent, encoding);
        }

        private string GetDataWithSuffix()
        {
            string fileContext = "";
            bool updateSuffixList = !File.Exists(fileFromLocal);
            if (!updateSuffixList)
            {
                try
                {
                    fileContext = File.ReadAllText(fileFromLocal, encoding);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    // try to reload data
                    fileContext = File.ReadAllText(fileFromResource, encoding);
                }

                updateSuffixList = DateTime.Now.Date - File.GetLastWriteTime(fileFromLocal).Date > TimeSpan.FromDays(validDays);
            }
            else
            {
                fileContext = File.ReadAllText(fileFromResource, encoding);
            }

            if (updateSuffixList)
            {
                ThreadPool.QueueUserWorkItem(obj => LoadSuffixListFromWeb());
            }

            return fileContext;
        }
    }
}
