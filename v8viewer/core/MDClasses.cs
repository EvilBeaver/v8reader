using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    abstract class MDClassBase : MDObjectBase
    {
        private IV8MetadataContainer m_Container;
        private MDObjectsCollection<MDForm> _Forms = new MDObjectsCollection<MDForm>();
        private MDObjectsCollection<MDTemplate> _Templates = new MDObjectsCollection<MDTemplate>();

        internal MDObjectsCollection<MDTemplate> Templates
        {
            get { return _Templates; }
        }

        internal MDObjectsCollection<MDForm> Forms
        {
            get { return _Forms; }
        }

        protected IV8MetadataContainer Container
        {
            get { return m_Container; }
            set { m_Container = value; }
        }
    }

    abstract class MDTypeDeclarator : MDClassBase
    {

    }

    abstract class MDObjectClass : MDTypeDeclarator, IHelpProvider
    {

        public MDObjectsCollection<MDAttribute> Attributes
        {
            get { return _Attributes; }
        }
        
        public MDObjectsCollection<MDTable> Tables
        {
            get { return _Tables; }
        }
        
        public String ObjectModule
        {
            get
            {
                MDFileItem DirElem;

                try
                {
                    DirElem = Container.GetElement(ObjectModuleFile());
                }
                catch (System.IO.FileNotFoundException)
                {
                    return String.Empty; // Модуля нет
                }

                if (DirElem.ElemType == MDFileItem.ElementType.Directory)
                {

                    try
                    {
                        var textElem = DirElem.GetElement("text");
                        return textElem.ReadAll();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        return String.Empty;
                    }

                }
                else
                {
                    return DirElem.ReadAll(); // если модуль зашифрован, то будет нечитаемый текст
                }
            }
        }
        
        public HTMLDocument Help
        {
            get 
            {
                if (_Help == null)
                {
                    _Help = new HelpProviderImpl(Container, HelpFile());
                }

                return _Help.Help;
            }
        }


        abstract protected string ObjectModuleFile();
        abstract protected string HelpFile();

        private HelpProviderImpl _Help = null;
        private MDObjectsCollection<MDAttribute> _Attributes = new MDObjectsCollection<MDAttribute>();
        private MDObjectsCollection<MDTable> _Tables = new MDObjectsCollection<MDTable>();

    }

    interface IHelpProvider
    {
        HTMLDocument Help
        {
            get;
        }
    }

    class HelpProviderImpl
    {
        public HelpProviderImpl(IV8MetadataContainer Container, string HelpFile)
        {
            _Container = Container;
            _HelpFile = HelpFile;
        }

        public HTMLDocument Help
        {
            get
            {
                if (_HelpDoc == null)
                {
                    try
                    {
                        var HelpItem = _Container.GetElement(_HelpFile);
                        var Stream = new SerializedList(HelpItem.ReadAll());

                        _HelpDoc = new HTMLDocument(Stream);

                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        _HelpDoc = new HTMLDocument();
                    }
                }

                return _HelpDoc;

            }
        }

        private string _HelpFile;
        private IV8MetadataContainer _Container;
        private HTMLDocument _HelpDoc;

    }

}
