/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.Plugins.DECRYPTTools.Util
{
    public class JsonDownloaderAndConverter
    {
        private const string LoginUrl = "https://cl.lingfil.uu.se/decode/database/api/login";
        private const string DownloadRecordsUrl = "https://cl.lingfil.uu.se/decode/database/api/records";
        private const string DownloadRecordUrl = "https://cl.lingfil.uu.se/decode/database/api/records/{0}";
        private static TimeSpan _timeout = new TimeSpan(0, 0, 0, 5);

        private const string UserAgent = "CrypTool 2/DECRYPT JsonDownloaderAndConverter";
        private static CookieContainer _cookieContainer = new CookieContainer();

        /// <summary>
        /// Login into DECRYPT database using username and password,
        /// also creates a new static CookieContainer, which it uses for storing and using the cookie
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Login(string username, string password)
        {
            try
            {
                //per default we set username and password to anonymous
                if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || username.ToLower().Equals("anonymous"))
                {
                    username = "anonymous";
                    password = "amomymous";
                }

                _cookieContainer = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (HttpClient client = new HttpClient(handler))
                {
                    StringContent usernamePasswordJson = new StringContent(string.Format("{{\"username\": \"{0}\", \"password\": \"{1}\"}}", username, password));
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = new TimeSpan(0, 0, 0, 5);

                    HttpResponseMessage response = client.PostAsync(LoginUrl, usernamePasswordJson).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return true;
                        case HttpStatusCode.Forbidden:
                            return false;
                        default:
                            throw new Exception(string.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while loggin into DECRYPT database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get the list of records of the DECRYPT database using the json protocol
        /// </summary>
        public static string GetRecords()
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }

                HttpClientHandler handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = _timeout;

                    HttpResponseMessage response = client.GetAsync(DownloadRecordsUrl).Result;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return response.Content.ReadAsStringAsync().Result;
                        default:
                            throw new Exception(string.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while downloading records from DECRYPT database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a single record as string from the DECRYPT database using the json protocol and http
        /// </summary>
        public static string GetRecord(int id)
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }

                HttpClientHandler handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = _timeout;

                    HttpResponseMessage response = client.GetAsync(string.Format(DownloadRecordUrl, id)).Result;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return response.Content.ReadAsStringAsync().Result;
                        default:
                            throw new Exception(string.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while downloading record {0} from DECRYPT database: {1}", id, ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a record object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        public static Record ConvertStringToRecord(string data)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Record));
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    stream.Position = 0;
                    Record record = (Record)serializer.ReadObject(stream);
                    return record;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not deserialize json data: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Downloads data from the specified URL and returns it as byte array
        /// </summary>
        public static byte[] GetData(string url, DownloadProgress downloadProgress = null)
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }
                HttpClientHandler handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    byte[] fileBytes = DownloadFile(client, url, downloadProgress).Result;
                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while downloading data from {0}: {1}", url, ex.Message), ex);
            }
        }

        /// <summary>
        /// Downloads the file and also triggers progress
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<byte[]> DownloadFile(HttpClient client, string url, DownloadProgress downloadProgress = null)
        {
            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long? totalBytes = response.Content.Headers.ContentLength;
                int totalBytesRead = 0;
                int lastTotalBytesRead = 0;
                byte[] fileBytes = new byte[(int)totalBytes];
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                {
                    bool isMoreToRead = true;
                    do
                    {
                        int bytesRead = await contentStream.ReadAsync(fileBytes, totalBytesRead, fileBytes.Length - totalBytesRead);
                        if (bytesRead == 0)
                        {
                            isMoreToRead = false;

                            continue;
                        }
                        totalBytesRead += bytesRead;
                        if (downloadProgress != null && totalBytesRead > lastTotalBytesRead + 1024)
                        {
                            lastTotalBytesRead = totalBytesRead;
                            downloadProgress.FireEvent(new DownloadProgressEventArgs((long)totalBytes, totalBytesRead));
                        }
                    } while (isMoreToRead);

                    if (downloadProgress != null)
                    {
                        downloadProgress.FireEvent(new DownloadProgressEventArgs((long)totalBytes, (long)totalBytes));
                    }

                }
                return fileBytes;
            }
        }


        /// <summary>
        /// Checks, if there is a valid cookie
        /// </summary>
        /// <returns></returns>
        public static bool IsLoggedIn()
        {
            try
            {
                return _cookieContainer.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the cookie to log out
        /// </summary>
        public static void LogOut()
        {
            _cookieContainer = new CookieContainer();
        }
    }

    /// <summary>
    /// "Callback" for download progress
    /// </summary>
    public class DownloadProgress
    {
        public event EventHandler<DownloadProgressEventArgs> NewDownloadProgress;
        public void FireEvent(DownloadProgressEventArgs args)
        {
            if (NewDownloadProgress != null)
            {
                NewDownloadProgress.Invoke(this, args);
            }
        }
    }

    /// <summary>
    /// EventArgs for download progress callback
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public long TotalBytes { get; set; }
        public long BytesDownloaded { get; set; }
        public DownloadProgressEventArgs(long totalBytes, long bytesDownloaded)
        {
            TotalBytes = totalBytes;
            BytesDownloaded = bytesDownloaded;
        }
    }
}
