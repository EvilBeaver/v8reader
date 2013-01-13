using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using V8Reader.Core;
using V8Reader.Editors;
using V8Reader.Comparison;

namespace V8Reader
{

    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        
        }
        
    }

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                RunDefault();
            }
            else
            {
                RunParametrized(args);
            }

        }

        static void RunDefault()
        {

            SafeMessageLoop(() =>
                {
                    App WPFApp = new App();
                    var DefaultWindow = new StartupWindow();
                    WPFApp.Run(DefaultWindow);
                });

        }

        static void RunParametrized(string[] args)
        {
            if (args.Length == 1)
            {
                if (System.IO.File.Exists(args[0]))
                {
                    OpenFile(args[0]);
                }
                else
                {
                    // unknown args
                    RunDefault();
                }
            }
            else
            {
                if (args[0] == "-diff" && args.Length == 3)
                {
                    Diff(args[1], args[2]);
                }
                else
                {
                    RunDefault();
                }                
            }
        }

        private static void OpenFile(String FileName)
        {

            SafeMessageLoop(() =>
                {
                    MDDataProcessor Processor = null;
                    App WPFApp = new App();
                    using (Processor = MDDataProcessor.Create(FileName))
                    {
                        ICustomEditor editor = Processor.GetEditor();
                        editor.Edit();
                        WPFApp.Run();
                    }
                });
        }

        private static void Diff(String File1, String File2)
        {
            using (FileComparisonPerformer Comparator = new FileComparisonPerformer(File1, File2))
            {
                SafeMessageLoop(() =>
                {
                    App WPFApp = new App();
                    var CompareTree = Comparator.Perform();
                    var TreeWnd = new CompareTreeWnd();
                    TreeWnd.PrintResult(CompareTree);
                    WPFApp.Run(TreeWnd);
                });
            }
        }

        private static void SafeMessageLoop(Action DoMessageLoop)
        {
            try
            {
                DoMessageLoop();
            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }
        }

    }
}
