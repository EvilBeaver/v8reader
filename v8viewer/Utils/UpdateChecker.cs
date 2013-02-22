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

        public bool HasUpdates()
        {
            m_UpdLog = CheckUpdates();
            return m_UpdLog.Count > 0;
        }

        public UpdateLog GetLog()
        {
            return m_UpdLog;
        }

        UpdateLog m_UpdLog;

        class RequestState
        {
            public WebRequest request;
            public WebResponse response;
            public AutoResetEvent wait;
            public Exception exception;
        }

        private UpdateLog CheckUpdates()
        {
            string url = @"http://sourceforge.net/projects/v8reader/files/update.xml/download";

            HttpWebRequest client = (HttpWebRequest)HttpWebRequest.Create(url);
            client.AllowAutoRedirect = true;
            client.Proxy = WebRequest.DefaultWebProxy;
            client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            AutoResetEvent awaitEvent = new AutoResetEvent(false);

            RequestState state = new RequestState();
            state.request = client;
            state.wait = awaitEvent;

            IAsyncResult result = (IAsyncResult)client.BeginGetResponse(new AsyncCallback(DownloadFileCompleted), state);

            if (awaitEvent.WaitOne(30 * 1000))
            {
                var resState = (RequestState)result.AsyncState;
                if (resState.exception != null)
                {
                    throw resState.exception;
                }
                
                return LoadResult(resState.response);
            }
            else
            {
                client.Abort();
                throw new UpdaterException("Превышено время ожидания");
            }

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

        void DownloadFileCompleted(IAsyncResult asynchronousResult)
        {
            RequestState state = (RequestState)asynchronousResult.AsyncState;
            
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)state.request;
                state.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);
            }
            catch (Exception exc)
            {
                state.exception = exc;
            }

            state.wait.Set();

        }

    }

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
