using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    interface IMDTreeItem
    {

        String Key { get; }
        String Text { get; }
        AbstractImage Icon { get; }
        
        bool HasChildren();
        IEnumerable<IMDTreeItem> ChildItems { get; }

        IEnumerable<UICommand> Commands { get; }

    }

    class UICommand
    {
        public UICommand(object Source, CommandCallback CallbackMethod, object CallbackParam)
        {
            m_Object = Source;
            m_CallbackMethod = CallbackMethod;
            m_CallbackParam = CallbackParam;
        }

        public void Execute()
        {
            m_CallbackMethod(m_Object, m_CallbackParam);
        }

        private object m_Object;
        private object m_CallbackParam;
        private CommandCallback m_CallbackMethod;

        public delegate void CommandCallback(object sender, object parameter);

    }

}
