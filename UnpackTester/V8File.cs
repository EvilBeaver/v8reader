using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace V8Unpack
{

    public class V8File : IDisposable
    {
        public V8File(string FileName)
        {
            var mmFile = MemoryMappedFile.CreateFromFile(FileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            FileImage = new V8Image(mmFile);
        }

        public IImageLister GetLister()
        {
            return FileImage;
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ~V8File()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (FileImage != null)
            {
                FileImage.Dispose();
                FileImage = null;
            }
        }

        private V8Image FileImage;
    }

    internal class V8Image : IDisposable, IImageLister
    {
        public V8Image(MemoryMappedFile MemMap)
        {
            m_MemMap = MemMap;
        }

        protected MemoryMappedFile m_MemMap;

        private void CheckDisposal()
        {
            if (m_MemMap == null)
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            m_MemMap.Dispose();
        }

        #endregion

        #region IImageLister Members

        public IList<V8ItemHandle> Items
        {
            get 
            {
                CheckDisposal();

                var Reader = m_MemMap.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

                List<V8ItemHandle> items = new List<V8ItemHandle>();

                UInt32 startAddr = stFileHeader.Size;
                while (startAddr <= Reader.Capacity)
                {
                    stBlockHeader blockHdr;
                    Reader.Read<stBlockHeader>(startAddr, out blockHdr);

                    if (!blockHdr.Check())
                    {
                        throw new V8WrongFileException();
                    }

                    // read block data

                    unsafe
                    {
                        
                        UInt32 dataSize     = Helpers.FromHexStr((sbyte*)blockHdr.page_size_hex);
                        UInt32 NextPageAddr = Helpers.FromHexStr((sbyte*)blockHdr.next_page_addr_hex);
                        
                        stElemAddr tocItem;
                        UInt32 tocStart = startAddr + stBlockHeader.Size;
                        int bytesRead = 0;
                        do
                        {
                            Reader.Read<stElemAddr>(tocStart, out tocItem);

                            if (tocItem.fffffff != 0x7fffffff)
                            {
                                break; // это не данные оглавления
                            }

                            tocStart += stElemAddr.Size;
                            bytesRead += stElemAddr.Size;

                            stBlockHeader itemHdr;
                            Reader.Read<stBlockHeader>(tocItem.elem_header_addr, out itemHdr);

                            uint titleSize = Helpers.FromHexStr((sbyte*)itemHdr.data_size_hex);
                            uint titleDelta = stBlockHeader.Size + stElemHeaderPrefix.Size;
                            uint nameSize = titleSize - stElemHeaderPrefix.Size - 4;

                            sbyte[] arr = new sbyte[nameSize];
                            Reader.ReadArray<sbyte>(
                                tocItem.elem_header_addr + titleDelta,
                                arr, 
                                0, (int)nameSize);

                            string itemName;
                            fixed (sbyte* ptr = arr)
                            {
                                itemName = new string(ptr, 0, (int)nameSize, Encoding.Unicode);
                            }

                            // длина тела данных
                            Reader.Read<stBlockHeader>(tocItem.elem_header_addr+titleSize, out itemHdr);
                            
                            uint BodySize = Helpers.FromHexStr((sbyte*)itemHdr.data_size_hex);
                            
                            V8ItemHandle itemHandle = new V8ItemHandle();
                            itemHandle.Container = this;
                            itemHandle.Name = itemName;
                            itemHandle.Offset = tocItem.elem_data_addr;
                            itemHandle.Length = BodySize;

                            items.Add(itemHandle);

                        }
                        while(tocItem.fffffff == 0x7fffffff && bytesRead < dataSize);

                        if (NextPageAddr == 0x7fffffff)
                        {
                            break;
                        }

                        startAddr = NextPageAddr;

                    }
                }

                return items;
            }
        }

        #endregion

        internal virtual Stream GetDataStream(V8ItemHandle Handle)
        {
            CheckDisposal();

            MemoryStream resultStream = null;

            using (Stream ReadStream = m_MemMap.CreateViewStream(Handle.Offset, Handle.Length, MemoryMappedFileAccess.Read))
            {
                using (var DeflateStream = new System.IO.Compression.DeflateStream(ReadStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    resultStream = new MemoryStream();
                    DeflateStream.CopyTo(resultStream);
                }
            }

           return resultStream;
        }
    }

    internal class V8UncompressedImage : V8Image
    {
        public V8UncompressedImage(MemoryMappedFile MemMap) : base(MemMap)
        {

        }

        internal override Stream GetDataStream(V8ItemHandle Handle)
        {
            return base.GetDataStream(Handle);
        }
    }

    public class V8DataElement
    {

    }

    public interface IImageLister
    {
        IList<V8ItemHandle> Items { get; }
    }

    public struct V8ItemHandle
    {
        public string Name;
        
        internal V8Image Container;
        internal UInt32 Offset;
        internal UInt32 Length;

        public override string ToString()
        {
            return Name;
        }
    }

    public class V8WrongFileException : Exception
    {
        public V8WrongFileException()
            : base("Wrong file format")
        {

        }
    }

    internal static class Helpers
    {
        public static uint FromHexStr(string hexStr)
        {
            UInt32 value = UInt32.Parse(hexStr, System.Globalization.NumberStyles.AllowHexSpecifier);
            return value;
        }

        public static unsafe uint FromHexStr(sbyte* bArr)
        {
            return FromHexStr(new String(bArr, 0, 8));
        }

        public const int DecompressChunk = 16384;
    }

    ////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////
    // Unsafe section
    //
    #region unsafe structures

    unsafe struct stFileHeader
    {
        public UInt32 next_page_addr;
        public UInt32 page_size;
        public UInt32 storage_ver;
        public UInt32 reserved; // всегда 0x00000000 ?
        public const int Size = 16;
    };

    unsafe struct stElemAddr
    {
        public UInt32 elem_header_addr;
        public UInt32 elem_data_addr;
        public UInt32 fffffff; //всегда 0x7fffffff ?
        public const int Size = 12;
    };

    unsafe struct stBlockHeader
    {
        public byte EOL_0D;
        public byte EOL_0A;
        public fixed byte data_size_hex[8];
        public byte space1;
        public fixed byte page_size_hex[8];
        public byte space2;
        public fixed byte next_page_addr_hex[8];
        public byte space3;
        public byte EOL2_0D;
        public byte EOL2_0A;
        public const int Size = 31;

        public bool Check()
        {
            if (EOL_0D != 0x0d ||
                EOL_0A != 0x0a ||
                space1 != 0x20 ||
                space2 != 0x20 ||
                space3 != 0x20 ||
                EOL2_0D != 0x0d ||
                EOL2_0A != 0x0a)
            {
                return false;
            }

            return true;
        }

    };

    unsafe struct stElemHeaderPrefix
    {
        public UInt64 date_creation;
        public UInt64 date_modification;
        public UInt32 res; // всегда 0x000000?
        //изменяемая длина имени блока
        //после имени DWORD res; // всегда 0x000000?
        public const int Size = 20;
    };

    #endregion

}
