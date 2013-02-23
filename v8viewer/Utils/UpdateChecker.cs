using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Net;

namespace V8Reader.Utils
{
    class UpdateChecker
    {

        public void CheckUpdates(UpdateCheckerResultHandler ResultHandler)
        {
            CheckUpdatesAsync(ResultHandler);
        }
        
        class RequestState
        {
            public WebRequest request;
            public WebResponse response;
            public UpdateCheckerResultHandler ResultHandler;
        }

        private static AsyncCallback GUIContextCallback(AsyncCallback initialCallback)
        {
            SynchronizationContext sc = SynchronizationContext.Current;
            if (sc == null)
            {
                return initialCallback;
            }

            return (asyncResult)=>
                {
                    sc.Post((result)=>
                        {
                            initialCallback((IAsyncResult)result);
                        }, asyncResult);
                };

        }

        private void CheckUpdatesAsync(UpdateCheckerResultHandler ResultHandler)
        {
            string url = @"http://sourceforge.net/projects/v8reader/files/update.xml/download";

            HttpWebRequest client = (HttpWebRequest)HttpWebRequest.Create(url);
            client.AllowAutoRedirect = true;
            client.Proxy = WebRequest.DefaultWebProxy;
            client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            RequestState state = new RequestState();
            state.request = client;
            state.ResultHandler = ResultHandler;

            IAsyncResult result = (IAsyncResult)client.BeginGetResponse(GUIContextCallback(DownloadFileCompleted), state);

        }

        void DownloadFileCompleted(IAsyncResult asynchronousResult)
        {
            RequestState state = (RequestState)asynchronousResult.AsyncState;

            UpdateCheckerResult CheckResult = new UpdateCheckerResult();

            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)state.request;
                state.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);

                CheckResult.Updates = LoadResult(state.response);
                CheckResult.Success = true;
            }
            catch (Exception exc)
            {
                CheckResult.Exception = exc;
                CheckResult.Success = false;
            }
            finally
            {
                if(state.response != null)
                    state.response.Close();
            }

            state.ResultHandler(this, CheckResult);

        }

        UpdateLog LoadResult(WebResponse response)
        {
            XDocument xmlDoc;

            using (var stream = response.GetResponseStream())
            {
                try
                {
                    xmlDoc = XDocument.Load(stream);
                }
                catch (Exception exc)
                {
                    throw new UpdaterException("Некорректные данные об обновлении", exc);
                }
            }

            var VersionList = xmlDoc.Root.Elements("version");
            var currentVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            UpdateLog log = new UpdateLog();

            foreach (var vDeclaration in VersionList)
            {
                Version inFile = Version.Parse(vDeclaration.Attribute("number").Value);
                if (currentVer < inFile)
                {
                    UpdateDefinition upd = new UpdateDefinition();
                    upd.Version = inFile.ToString();
                    upd.Url = vDeclaration.Element("url").Value;
                    log.Add(upd);
                }
            }

            return log;
        }

    }

    class UpdateCheckerResult
    {
        public bool Success { get; set; }
        public UpdateLog Updates { get; set; }
        public Exception Exception { get; set; }
    }

    internal delegate void UpdateCheckerResultHandler(UpdateChecker sender, UpdateCheckerResult result);

    class UpdateDefinition
    {
        public string Version { get; set; }
        public string News { get; set; }
        public string Url { get; set; }
    }

    class UpdateLog : IEnumerable<UpdateDefinition>
    {
        
        public void Add(UpdateDefinition item)
        {
            m_List.Add(item);
        }

        public int Count
        {
            get
            {
                return m_List.Count;
            }
        }

        List<UpdateDefinition> m_List = new List<UpdateDefinition>();

        #region IEnumerable<UpdateDefinition> Members

        public IEnumerator<UpdateDefinition> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        #endregion
    }

    class UpdaterException : Exception
    {
        public UpdaterException(string message) : this(message,null)
        {
            
        }

        public UpdaterException(Exception inner) : this(inner.Message, inner)
        {
            
        }

        public UpdaterException(string message, Exception inner):base(message,inner)
        {

        }
    }

}
