using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace V8Reader.Comparison
{
    class EditablePropertyVisualizer : IValueVisualizer
    {
        protected Editors.IEditable _editableObject;
        public virtual string Text { get; protected set; }

        public EditablePropertyVisualizer(Editors.IEditable EditableObject, string Title)
        {
            _editableObject = EditableObject;
            Text = Title;
        }

        virtual public Block FlowContent
        {
            get 
            {
                Comparison.EditableObjectSection section = new Comparison.EditableObjectSection(_editableObject, new Run(Text));

                return section;
            }
        }

        virtual public string StringContent
        {
            get { return Text; }
        }


    }


    #region Module visualizer

    class V8ModulePropVisualizer : EditablePropertyVisualizer
    {

        public V8ModulePropVisualizer(Core.V8ModuleProcessor Module) : base(Module, Module.ModuleName)
        {    
        }

    }

    #endregion

    #region Help visualizer

    class HelpPropVisualizer : EditablePropertyVisualizer
    {
        
        public HelpPropVisualizer(Core.HTMLDocument Document, string Title) : base(null, Title)
        {
            _editableObject = new EditableHelp(Document);
        }

        class EditableHelp : Editors.IEditable, Editors.ICustomEditor
        {
            Core.HTMLDocument _doc;
            public EditableHelp(Core.HTMLDocument Document)
            {
                _doc = Document;
            }

            #region IEditable Members

            public Editors.ICustomEditor GetEditor()
            {
                return this;
            }

            #endregion

            #region ICustomEditor Members

            public void Edit()
            {
                Edit(null);
            }

            public void Edit(System.Windows.Window Owner)
            {
                if (!_doc.IsEmpty)
                {
                    String Path = _doc.Location;
                    System.Diagnostics.Process.Start(Path);

                }
            }

            public event Editors.EditorCompletionHandler EditComplete;

            #endregion
        }

    }

    #endregion

}
