using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CFReader;

namespace V8Reader.Utils.Browser
{
    /// <summary>
    /// Interaction logic for BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        internal BrowserWindow(string filename)
        {
            InitializeComponent();

            Utils.FormsSettingsManager.Register(this, "FileBrowser");

            m_FileImage = new V8File(filename);
            m_OpenedDocs = new Dictionary<FileTreeItem, OpenedDocument>();

        }

        protected override void OnClosed(EventArgs e)
        {
            if (m_FileImage != null)
            {
                FileTree.Items.Clear();
                m_OpenedDocs.Clear();
                m_FileImage.Dispose();
            }
            base.OnClosed(e);
        }

        V8File m_FileImage;
        Dictionary<FileTreeItem, OpenedDocument> m_OpenedDocs;

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            IImageLister lister = m_FileImage.GetLister();
            foreach (var item in lister.Items)
            {
                FileTree.Items.Add(new FileTreeItem(item.GetElement()));
            }

        }

        private void btnTabClose_Click(object sender, RoutedEventArgs e)
        {
            var tabItm = Utils.UIHelper.FindVisualParent<TabItem>((DependencyObject)sender);
            if (tabItm != null)
            {

                var ToRemove = from Opened in m_OpenedDocs where Opened.Value == tabItm.Content select Opened.Key;

                foreach (var kv in ToRemove.ToArray<FileTreeItem>())
                {
                    m_OpenedDocs.Remove(kv);
                }

                int idx = FilePanel.Items.IndexOf(tabItm.Content);
                FilePanel.Items.Remove(tabItm.Content);
                tabItm = null;
                FilePanel.SelectedIndex = idx;
            }
        }

        private void FileTree_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as TreeViewItem).Header is FileTreeItem)
            {
                var fti = (sender as TreeViewItem).Header as FileTreeItem;
                if (!fti.IsFolder)
                {
                    e.Handled = true;

                    OpenedDocument od;
                    m_OpenedDocs.TryGetValue(fti, out od);
                    if (od != null)
                    {
                        int tabIdx = FilePanel.Items.IndexOf(od);
                        if (tabIdx >= 0)
                        {
                            FilePanel.SelectedIndex = tabIdx;
                        }
                        else
                        {
                            m_OpenedDocs.Remove(fti);
                        }
                    }
                    else
                    {
                        var NewDoc = new OpenedDocument(fti);
                        m_OpenedDocs.Add(fti, NewDoc);
                        int tabIdx = FilePanel.Items.Add(NewDoc);
                        FilePanel.SelectedIndex = tabIdx;
                    }

                }
            }
        }

        private void tbSaveAs_Click(object sender, RoutedEventArgs e)
        {
            int tabIdx = FilePanel.SelectedIndex;
            if (tabIdx < 0)
                return;

            var dlg = new Microsoft.Win32.SaveFileDialog();
            if ((bool)dlg.ShowDialog())
            {
                string fileToSave = dlg.FileName;
                var od = FilePanel.Items[tabIdx];
                var findReslt = from Opened in m_OpenedDocs where Opened.Value == od select Opened.Key;
                foreach (var fti in findReslt)
                {
                    SaveToFileAsync(fileToSave, fti);
                    break;
                }

            }
        }

        private void SaveToFileAsync(string fileToSave, FileTreeItem fti)
        {
            
            try
            {
                var fs = new System.IO.FileStream(fileToSave, System.IO.FileMode.Create);
                var src = fti.GetDataStream();

                const int CHUNK_SIZE = 1024*10; //kilobytes
                byte[] buffer = new byte[CHUNK_SIZE];

                Action<Exception> Done = (exc) =>
                    {
                        if (exc != null)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() => Utils.UIHelper.DefaultErrHandling(exc)));
                        }

                        fs.Close();
                        src.Close();

                    };

                AsyncCallback CallbackExpr = null;
                CallbackExpr = (IAsyncResult readResult)=>
                {
                    try 
                    { 
                        int read = src.EndRead(readResult);
                        if (read > 0)
                        {
                            fs.BeginWrite(buffer, 0, read, writeResult =>
                                {
                                    try
                                    {
                                        fs.EndWrite(writeResult);
                                        src.BeginRead(buffer, 0, buffer.Length, CallbackExpr, null);
                                    }
                                    catch (Exception exc)
                                    {
                                        Done(exc);
                                    }
                                },
                             null);
                        }
                        else
                        {
                            Done(null);
                        }
                    } 
                    catch (Exception exc) 
                    {
                        Done(exc);
                    }
                };

                // Here we go!
                src.BeginRead(buffer, 0, buffer.Length, CallbackExpr, null);

            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }
        }

        private void tbGC_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect(2);
        }

    }

    class FileTreeItem
    {
        public FileTreeItem(V8DataElement elem)
        {
            _elem = elem;
            _isFolder = elem is IImageLister;

            if (_isFolder)
            {
                _children = new List<FileTreeItem>();
                foreach (var item in (elem as IImageLister).Items)
                {
                    _children.Add(new FileTreeItem(item.GetElement()));
                }

            }
        }

        public string Name
        {
            get
            {
                return _elem.Name;
            }
        }

        public System.IO.Stream GetDataStream()
        {
            return _elem.GetDataStream();
        }

        public string Text
        {
            get
            {
                if (_isFolder)
                {
                    return null;
                }
                else
                {
                    string txt = null;
                    using (var sr = new System.IO.StreamReader(_elem.GetDataStream()))
                    {
                        txt = sr.ReadToEnd();
                    }

                    return txt;
                }
            }
        }

        public List<FileTreeItem> Items
        {
            get
            {
                return _children;
            }
        }

        private V8DataElement _elem;
        private bool _isFolder;

        public bool IsFolder
        {
            get { return _isFolder; }
            set { _isFolder = value; }
        }

        List<FileTreeItem> _children;

    }

    class OpenedDocument
    {
        public OpenedDocument(FileTreeItem item)
        {
            Text = item.Text;
            Name = item.Name;
        }

        public string Text { get; private set; }
        public string Name { get; private set; }

    }

    class DocList : IList<OpenedDocument>
    {
        public DocList()
        {
            _list = new List<OpenedDocument>();
        }

        private List<OpenedDocument> _list;

        #region IList<OpenedDocument> Members

        public int IndexOf(OpenedDocument item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, OpenedDocument item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public OpenedDocument this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        #endregion

        #region ICollection<OpenedDocument> Members

        public void Add(OpenedDocument item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(OpenedDocument item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(OpenedDocument[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(OpenedDocument item)
        {
            return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<OpenedDocument> Members

        public IEnumerator<OpenedDocument> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }

}
