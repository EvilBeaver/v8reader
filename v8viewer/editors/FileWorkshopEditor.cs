using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{
    class FileWorkshopEditor : CustomEditor, ICustomEditor
    {
        public FileWorkshopEditor(FWOpenableDocument TemplateObject)
        {
            m_Document = TemplateObject;
        }

        private FWOpenableDocument m_Document;
        private String m_TempFile;

        public void Edit()
        {
            Edit(null);
        }

        public void Edit(System.Windows.Window Owner)
        {

            String fwPath = null;
            bool OperationAllowed = Utils.UIHelper.AskForFileWorkshop(out fwPath, Owner);
            
            if (!OperationAllowed)
                return;

            m_TempFile = m_Document.Extract();
            
            try
            {

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.EnableRaisingEvents = true;
            
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName  = fwPath;
                startInfo.Arguments = String.Format("\"{0}\"", m_TempFile);

                process.Exited += new EventHandler(process_Exited);
                process.StartInfo = startInfo;
                process.Start();

            }
            catch(Exception e)
            {
                DestroyTempFile();

                CustomEditorException WrapperExc = new CustomEditorException(e.Message,e);

                throw WrapperExc;
            }            

        }

        private void DestroyTempFile()
        {
            if (System.IO.File.Exists(m_TempFile))
            {
                try
                {
                    System.IO.File.Delete(m_TempFile);
                }
                catch
                {
                }
            }
        }

        void process_Exited(object sender, EventArgs e)
        {
            DestroyTempFile();

            OnEditComplete(true, m_Document);
        }

    }
}
