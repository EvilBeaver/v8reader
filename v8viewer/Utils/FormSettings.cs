﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.IO;

namespace V8Reader.Utils
{
    
    [DataContract(Name="FormSettings")]
    class FormSettings
    {
        public WindowGeometry Geometry
        {
            get { return m_Geometry; }
            set { m_Geometry = value; }
        }

        public void LoadFrom(Window srcWindow)
        {
            var wg = new WindowGeometry();
            wg.Top = srcWindow.Top;
            wg.Left = srcWindow.Left;
            wg.Width = srcWindow.Width;
            wg.Height = srcWindow.Height;

            Geometry = wg;
        }

        public void ApplyTo(Window destWindow)
        {
            var wg = Geometry;
            destWindow.Top = wg.Top;
            destWindow.Left = wg.Left;
            destWindow.Width = wg.Width;
            destWindow.Height = wg.Height;
        }

        [DataMember(Name="Geometry")]
        WindowGeometry m_Geometry;
    }

    [DataContract]
    struct WindowGeometry
    {
        [DataMember]
        public double Top;
        [DataMember]
        public double Left;
        [DataMember]
        public double Width;
        [DataMember]
        public double Height;
    }

    [CollectionDataContract
    (Name = "FormsSettingsList",
    ItemName = "FormSettings",
    KeyName = "WindowKey",
    ValueName = "Value")]
    class SettingsList : Dictionary<string, FormSettings>
    {

    }

    static class FormsSettingsManager
    {
        public static void Register(Window window, string WindowKey)
        {
            Init();

            FormSettings value;
            if (m_Storage.TryGetValue(WindowKey, out value))
            {
                value.ApplyTo(window);
            }
            else
            {
                value = new FormSettings();
                value.LoadFrom(window);
                m_Storage[WindowKey] = value;
            }

            window.Closed += window_Closed;
            m_RegisteredKeys[window] = WindowKey;

        }

        static void window_Closed(object sender, EventArgs e)
        {
            var window = (Window)sender;

            string winKey;
            if (m_RegisteredKeys.TryGetValue(window, out winKey))
            {
                var setting = new FormSettings();
                setting.LoadFrom(window);
                m_Storage[winKey] = setting;
                m_RegisteredKeys.Remove(window);
            }

        }
        
        public static void Store()
        {
            if (m_Storage == null)
                return;

            var storage = IsolatedStorageFile.GetUserStoreForAssembly();
            using (var writeMutex = new System.Threading.Mutex(true, ctWriteMtxName))
            {
                try
                {
                    using (var readMtx = System.Threading.Mutex.OpenExisting(ctReadMtxName, System.Security.AccessControl.MutexRights.Synchronize))
                    {
                        readMtx.WaitOne();
                    }
                }
                catch (System.Threading.WaitHandleCannotBeOpenedException)
                {
                }

                using (var fs = storage.OpenFile(ctFileName, FileMode.Truncate, FileAccess.Write, FileShare.None))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(SettingsList));
                    serializer.WriteObject(fs, m_Storage);
                }
            }

        }

        private static void Init()
        {
            
            if (m_Storage == null)
            {
                var storage = IsolatedStorageFile.GetUserStoreForAssembly();

                if (storage.FileExists(ctFileName))
                {

                    using (var readMutex = new System.Threading.Mutex(true, ctReadMtxName))
                    {
                        try
                        {
                            using (var writeMtx = System.Threading.Mutex.OpenExisting(ctWriteMtxName, System.Security.AccessControl.MutexRights.Synchronize))
                            {
                                writeMtx.WaitOne();
                            }
                        }
                        catch (System.Threading.WaitHandleCannotBeOpenedException)
                        {
                        }

                        using (var fs = storage.OpenFile(ctFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(SettingsList));

                            var rdr = new StreamReader(fs);
                            string str = rdr.ReadToEnd();
                            fs.Position = 0;
                            try
                            {
                                m_Storage = (SettingsList)serializer.ReadObject(fs);
                            }
                            catch (System.Runtime.Serialization.SerializationException)
                            {
                                m_Storage = new SettingsList();
                            }

                        }
                    }

                }
                else
                {
                    m_Storage = new SettingsList();
                }

            }
        }

        private static SettingsList m_Storage;
        private static Dictionary<Window, string> m_RegisteredKeys = new Dictionary<Window,string>();

        private const string ctFileName = "FormsSettings.xml";
        private const string ctWriteMtxName = "v8reader_settingswrite_mutex";
        private const string ctReadMtxName = "v8reader_settingsread_mutex";

    }

}
