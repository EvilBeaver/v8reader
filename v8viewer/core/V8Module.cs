using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    static class V8ModuleProcessor
    {
        public static IList<V8ModulePart> ParseParts(string Text)
        {

            using (var Reader = new System.IO.StringReader(Text))
            {

                bool blockIsOpen = false;
                ParserBlock currentBlock = new ParserBlock();

                string currentLine = Reader.ReadLine();

                while (currentLine != null)
                {
                    string TrimmedLine = currentLine.Trim();

                    if (blockIsOpen)
                    {

                    }
                    else
                    {
                        blockIsOpen = SwitchReaderContext(currentLine, ref currentBlock);
                    }

                }

            }

        }

        static private Nullable<ParserBlock> OpenBlockForLine(string DocumentLine)
        {
            var TrimmedLine = DocumentLine.TrimStart();

            bool blockIsOpen = false;
            var newBlock = new ParserBlock();

            if(TrimmedLine.StartsWith("//", StringComparison.Ordinal))
            {
                blockIsOpen = true;
                newBlock.type = BlockType.Comment;
                newBlock.title = TrimmedLine;
                newBlock.Append(DocumentLine);
            }
            else if(TrimmedLine.StartsWith("ПРОЦЕДУРА ", StringComparison.OrdinalIgnoreCase))
	        {
                blockIsOpen = true;
                newBlock.type = BlockType.Procedure;
                newBlock.title = TrimmedLine.Substring(10);
                newBlock.Append(DocumentLine);
	        }
		    else if(TrimmedLine.StartsWith("ФУНКЦИЯ ", StringComparison.OrdinalIgnoreCase))
	        {
                blockIsOpen = true;
                newBlock.type = BlockType.Procedure;
                newBlock.title = TrimmedLine.Substring(9);
                newBlock.Append(DocumentLine);
	        }
            else if(TrimmedLine.StartsWith("&", StringComparison.Ordinal))
	        {
                blockIsOpen = true;
                newBlock.type = BlockType.CompilerDirective;
                newBlock.title = TrimmedLine;
                newBlock.Append(DocumentLine);
	        }
	        else if(TrimmedLine.StartsWith("#", StringComparison.Ordinal))
	        {
                blockIsOpen = true;
                newBlock.type = BlockType.Preprocessor;
                newBlock.title = TrimmedLine;
                newBlock.Append(DocumentLine);
	        }
            else if(TrimmedLine != String.Empty)
	        {
                blockIsOpen = true;
                newBlock.type = BlockType.OutOfMethod;
                newBlock.title = TrimmedLine;
                newBlock.Append(DocumentLine);
	        }
		
	
            if(blockIsOpen)
            {
                return newBlock;
            }
            else
            {
                return null;
            }

        }

        static private bool SwitchReaderContext(string DocumentLine, ref ParserBlock CurrentBlock)
        {
            bool blockIsIOpen = false;
	        var newBlock = OpenBlockForLine(DocumentLine);
            if(newBlock != null)
            {
                blockIsIOpen = true;
                CurrentBlock = newBlock.Value;
            }

            return blockIsIOpen;
        }

        struct ParserBlock
        {
            public BlockType type;
            private StringBuilder Builder;
            public string title;

            public ParserBlock()
            {
                Builder = new StringBuilder();
                type = BlockType.OutOfMethod;
                title = String.Empty;
            }

            public void Append(string line)
            {
                Builder.Append(line);
            }

        }

        enum BlockType
        {
            Comment,
            Procedure,
            Function,
            Preprocessor,
            CompilerDirective,
            OutOfMethod
        }

    }

    class V8ModulePart
    {
        public V8ModulePart(string title, string content)
        {
            Title = title;
            Content = content;
        }
        
        public V8RuntimeContextType RuntimeContext { get; set; }
        public V8ModulePartClass PartClass { get; set; }
        public string Content { get; set; }
        public string Title { get; private set; }

        public IList<V8ModulePart> ChildParts { get; protected set; }

    }

    enum V8ModulePartClass
    {
        VariablesDefinition,
        Method,
        StartupCode
    }

    enum V8RuntimeContextType
    {
        Default,
        Server,
        Client,
        WebClient,
        ThinClient,
        OrdinaryThickClient,
        ManagedThickClient,
        ExternalConnection
    }

}
