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

            

            bool OperationAllowed = false;
            bool cancel = false;

            String fwPath = "";

            do
            {
                fwPath = Properties.Settings.Default.PathToFileWorkshop;
                OperationAllowed = !(fwPath == String.Empty || !System.IO.File.Exists(fwPath));

                if (!OperationAllowed)
                {

                    var mbr = System.Windows.MessageBox.Show(
                        "Путь к приложению \"1С:Работа с файлами\" не задан или задан неверно. Задать его сейчас?",
                        "V8 Reader",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (mbr == System.Windows.MessageBoxResult.Yes)
                    {
                        var mainWnd = new SettingsWindow();
                        mainWnd.Owner = Owner;
                        mainWnd.ShowDialog();
                    }
                    else if (mbr == System.Windows.MessageBoxResult.No)
                    {
                        cancel = true;
                    }

                    //throw new CustomEditorException("Путь к приложению \"1С:Работа с файлами\" не задан или задан неверно.");
                }
            }
            while(!OperationAllowed && !cancel);

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
