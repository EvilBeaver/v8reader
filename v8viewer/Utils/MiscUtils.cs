using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace V8Reader.Utils
{    
    static class UIHelper
    {
        public static void DefaultErrHandling(Exception exc)
        {
            MessageBox.Show(exc.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        public static T FindLogicalParent<T>(DependencyObject childElement) where T : DependencyObject
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(childElement);
            T parentAsT = parent as T;
            if (parent == null)
            {
                return null;
            }
            else if (parentAsT != null)
            {
                return parentAsT;
            }
            return FindLogicalParent<T>(parent);
        }

        public static T FindVisualParent<T>(DependencyObject childElement) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(childElement);
            T parentAsT = parent as T;
            if (parent == null)
            {
                return null;
            }
            else if (parentAsT != null)
            {
                return parentAsT;
            }
            return FindVisualParent<T>(parent);
        }

        public static bool AskForFileWorkshop(out string result, Window OwnerWindow)
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
                        mainWnd.Owner = OwnerWindow;
                        mainWnd.ShowDialog();
                    }
                    else if (mbr == System.Windows.MessageBoxResult.No)
                    {
                        cancel = true;
                    }

                }
            }
            while (!OperationAllowed && !cancel);

            result = OperationAllowed ? fwPath : null;
            
            return OperationAllowed;

        }

    }

    internal static class TempFileCleanup
    {
        public static void RegisterTempFile(string tempfile)
        {
            RegisteredFiles.Add(tempfile);
        }

        public static void Unregister(string tempfile)
        {
            if (RegisteredFiles.Contains(tempfile))
            {
                RegisteredFiles.Remove(tempfile);
            }
        }

        public static void PerformCleanup()
        {
            foreach (var path in RegisteredFiles)
            {
                DestroyTempFile(path);
            }
            
            RegisteredFiles.Clear();
        }

        public static void DestroyTempFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                }
                catch
                {
                }
            }
        }

        private static HashSet<string> RegisteredFiles = new HashSet<string>();

    }

}