using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Reader.Core
{

    class MDReader : IDisposable
    {

        public MDReader(String File, String OutputDir, DisposeMode DisposeMode = DisposeMode.DeleteWorkingDir)
        {
            WorkDir = OutputDir;
            CurrentDisposeMode = DisposeMode;

            Unpack(File);

            m_Disposed = false;
        }

        public MDReader(String File)
        {
            WorkDir = Path.GetTempPath() + "\\" + Guid.NewGuid().ToString();
            Directory.CreateDirectory(WorkDir);
            CurrentDisposeMode = DisposeMode.DeleteWorkingDir;

            Unpack(File);

        }

        public MDFileItem GetElement(String Name)
        {
            if (IsDisposed())
                throw new ObjectDisposedException(GetType().ToString());
            
            return new MDFileItem(WorkDir + "\\" + Name);
        }

        public bool IsDisposed()
        {
            return m_Disposed;
        }

        public enum DisposeMode
        {
            DeleteWorkingDir,
            DoNotDispose
        }

        public void Dispose()
        {
            DisposeImpl(true);
        }

        private void Unpack(String File)
        {
            int result = UnpackWrapper.UnpackToDir(File, WorkDir);
            if (result != 0)
                throw new UnpackException(result);
        }

        protected void DisposeImpl(bool ManualDisposal)
        {
            if (!IsDisposed())
            {
                if (ManualDisposal)
                    GC.SuppressFinalize(this);

                if (CurrentDisposeMode == DisposeMode.DeleteWorkingDir && Directory.Exists(WorkDir))
                {
                    try
                    {
                        Directory.Delete(WorkDir, true);
                    }
                    catch { } // Ошибку удаления временной папки отдельно обрабатывать не будем.

                }

                m_Disposed = true;
            }
        }

        ~MDReader()
        {
            DisposeImpl(false);
        }

        private String WorkDir;
        private DisposeMode CurrentDisposeMode;
        private bool m_Disposed;

    }
}
