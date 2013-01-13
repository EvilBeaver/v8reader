using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{

    public interface ICustomEditor
    {
        void Edit();
        void Edit(System.Windows.Window Owner);

        event EditorCompletionHandler EditComplete;
    }
    
    public interface IEditable
	{
        ICustomEditor GetEditor();
	}

    abstract public class CustomEditor
    {
        public event EditorCompletionHandler EditComplete;
        
        // objectless event
        public virtual void OnEditComplete(bool Success, IEditable EditedObject)
        {
            if (EditComplete != null)
            {
                EditComplete(this, new EditorEventArgs(Success, EditedObject));
            }
        }

    }

    public delegate void EditorCompletionHandler(Object Sender, EditorEventArgs e);

    public class EditorEventArgs
    {

        public EditorEventArgs(bool SuccessFlag) : this(SuccessFlag, null) { }

        public EditorEventArgs(bool SuccessFlag, IEditable EditedObject)
        {
            m_success = SuccessFlag;
            m_Object = EditedObject;
        }

        public bool Success 
        { 
            get { return m_success; } 
        }
        
        public IEditable EditedObject
        {
            get { return m_Object; }
        }

        private IEditable m_Object;
        private bool m_success;
    }

    class CustomEditorException : Exception
    {
        public CustomEditorException(String msg) : base(msg)
        {

        }

        public CustomEditorException(String msg, Exception inner)
            : base(msg,inner)
        {

        }

    }

}
