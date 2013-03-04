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

            m_FileImage = new V8File(filename);

        }

        protected override void OnClosed(EventArgs e)
        {
            if(m_FileImage != null)
                m_FileImage.Dispose();

            base.OnClosed(e);
        }

        V8File m_FileImage;

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            IImageLister lister = m_FileImage.GetLister();
            foreach (var item in lister.Items)
            {
                FileTree.Items.Add(new FileTreeItem(item.GetElement()));
            }

            if (FileTree.HasItems)
            {
                var lst = this.Resources["DocListKey"] as DocList;
                FileTreeItem elem = FileTree.Items[0] as FileTreeItem;
                lst.Add(new OpenedDocument(elem));
            }

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
                _children = new List<V8DataElement>();
                foreach (var item in (elem as IImageLister).Items)
                {
                    _children.Add(item.GetElement());
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

        public List<V8DataElement> Items
        {
            get
            {
                return _children;
            }
        }

        private V8DataElement _elem;
        private bool _isFolder;
        List<V8DataElement> _children;

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
