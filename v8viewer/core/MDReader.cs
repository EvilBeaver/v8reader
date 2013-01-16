using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Reader.Core
{

    class MDReader : IDisposable
    {
        public MDReader(String File)
        {
            m_FileReader = new CFReader.V8File(File);
        }

        public MDFileItem GetElement(String Name)
        {
            if (IsDisposed())
                throw new ObjectDisposedException(GetType().ToString());

            return new MDFileItem(m_FileReader.GetLister(), Name);

        }

        public bool IsDisposed()
        {
            return m_Disposed;
        }

        public void Dispose()
        {
            DisposeImpl(true);
        }

        private void DisposeImpl(bool ManualDisposal)
        {
            if (!IsDisposed())
            {
                if (ManualDisposal)
                    GC.SuppressFinalize(this);

                m_FileReader.Dispose();

                m_Disposed = true;
            }
        }

        ~MDReader()
        {
            DisposeImpl(false);
        }

        private bool m_Disposed;
        private CFReader.V8File m_FileReader;

    }
}
