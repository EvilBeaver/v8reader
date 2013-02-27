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

            string storedValue = V8Reader.Properties.Settings.Default.SettingsVersion;
            string currentValue = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (storedValue != currentValue)
            {
                V8Reader.Properties.Settings.Default.Upgrade();
                V8Reader.Properties.Settings.Default.SettingsVersion = currentValue;
                V8Reader.Properties.Settings.Default.Save();
            }

            DateTime lastCheck = V8Reader.Properties.Settings.Default.LastUpdateCheck;
            if (lastCheck.Date != DateTime.Now.Date)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
				#if DEBUG
                dispatcherTimer.Interval = TimeSpan.FromMinutes(0.2);
				#else
                dispatcherTimer.Interval = TimeSpan.FromMinutes(1);
				#endif
                dispatcherTimer.Start();
            }

        }

        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var Timer = sender as System.Windows.Threading.DispatcherTimer;

            Timer.Stop();

            Utils.UpdateChecker chk = new Utils.UpdateChecker();
            try
            {
                chk.CheckUpdates(UpdateCheckerCallback);
            }
            catch
            {
                #if DEBUG
				throw;
				#endif
            }

        }

        void UpdateCheckerCallback(Utils.UpdateChecker uc, Utils.UpdateCheckerResult result)
        {            
            try
            {

                if (!result.Success) return;

                V8Reader.Properties.Settings.Default.LastUpdateCheck = DateTime.Now.Date;

                if (result.Updates.Count > 0)
                {
                    var answer = MessageBox.Show("Обнаружены новые версии. Обновить программу?", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (answer == MessageBoxResult.Yes)
                    {
                        var UpdWnd = new Utils.UpdatesWnd();
                        UpdWnd.Updates = result.Updates;
                        UpdWnd.Show();
                    }
                }

            }
            catch
            {
                #if DEBUG
				throw;
				#endif
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

            try
            {
                Utils.FormsSettingsManager.Store();
            }
            catch
            {
				#if DEBUG
				throw;
				#endif
            }

            try
            {
                Utils.TempFileCleanup.PerformCleanup();
            }
            catch
            {
				#if DEBUG
				throw;
				#endif
            }

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
                    WPFApp.MainWindow = DefaultWindow;
                    WPFApp.ShutdownMode = ShutdownMode.OnMainWindowClose;
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
                        editor.EditComplete += (s, e) =>
                        {
                            WPFApp.Shutdown();
                        };

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
                    var TreeWnd = new CompareTreeWnd();
                    TreeWnd.PrintResult(Comparator);
                    WPFApp.MainWindow = TreeWnd;
                    WPFApp.ShutdownMode = ShutdownMode.OnMainWindowClose;
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
