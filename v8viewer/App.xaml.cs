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
                V8Reader.Properties.Settings.Default.Save();

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
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer = null;
                }
            }
            catch
            {
                #if DEBUG
                throw;
                #endif
            }
            
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
                if (args[0] == "-diff")
                {
                    string file1 = null;
                    string name1 = null;
                    string name2 = null;
                    string file2 = null;

                    short tokenLen = 6;

                    for (int i = 1; i < args.Length; i++)
                    {
                        if (args[i].StartsWith("-name1"))
                        {
                            name1 = args[i].Substring(tokenLen);
                        }
                        else if (args[i].StartsWith("-name2"))
                        {
                            name2 = args[i].Substring(tokenLen);
                        }
                        else
                        {
                            if (file1 == null)
                            {
                                file1 = args[i];
                            }

                            if (file2 == null)
                            {
                                file2 = args[i];
                            }
                        }
                    }

                    Diff(file1, file2, name1, name2);
                }
                else if (args.Length==2 && args[0] == "-browse" && System.IO.File.Exists(args[1]))
                {
                    BrowseFile(args[1]);
                }
                else
                {
                    RunDefault();
                }                
            }
        }

        private static void BrowseFile(string filename)
        {
            SafeMessageLoop(() =>
                {
                    App WPFApp = new App();
                    var frm = new Utils.Browser.BrowserWindow(filename);
                    WPFApp.MainWindow = frm;
                    WPFApp.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    WPFApp.Run(frm);

                });
        }

        private static void OpenFile(String FileName)
        {

            SafeMessageLoop(() =>
                {
                    using (V8MetadataContainer Container = new V8MetadataContainer(FileName))
                    {
                        IEditable editable = Container.RaiseObject() as IEditable;

                        if (editable == null)
                        {
                            MessageBox.Show("Редактирование данного объекта не поддерживается", "V8 Reader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        App WPFApp = new App();

                        ICustomEditor editor = editable.GetEditor();
                        editor.EditComplete += (s, e) =>
                        {
                            WPFApp.Shutdown();
                        };

                        editor.Edit();
                        WPFApp.Run();
                    }
                });
        }

        private static void Diff(string File1, string File2, string Name1, string Name2)
        {
            if (!(CheckExistence(File1) && CheckExistence(File2)))
            {
                return;
            }
            
            using (FileComparisonPerformer Comparator = new FileComparisonPerformer(File1, File2))
            {
                SafeMessageLoop(() =>
                {
                    App WPFApp = new App();
                    var TreeWnd = new CompareTreeWnd();
                    TreeWnd.LeftName = Name1;
                    TreeWnd.RightName = Name2;
                    TreeWnd.PrintResult(Comparator);
                    WPFApp.MainWindow = TreeWnd;
                    WPFApp.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    WPFApp.Run(TreeWnd);
                });
            }
        }

        private static bool CheckExistence(string Filename)
        {
            if (!System.IO.File.Exists(Filename))
            {
                Console.WriteLine("File not found {0}", Filename);
                return false;
            }

            return true;
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
