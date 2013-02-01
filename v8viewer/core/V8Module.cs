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
            return null;
        }

        private static bool BlockCanHaveComments(BlockType blockType)
        {
            return (blockType == BlockType.Function
                || blockType == BlockType.Procedure
                || blockType == BlockType.CompilerDirective
                || blockType == BlockType.OutOfMethod);
            
        }

        private static void CloseBlock(ParserBlock Block, IList<V8ModulePart> Parts)
        {
            
        }

        private static void AddLineToBlock(ParserBlock currentBlock,string currentLine)
        {
 	        currentBlock.AppendLine(currentLine);
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

            public void Append(string line)
            {
                Builder.Append(line);
            }

            public void AppendLine(string line)
            {
                Builder.AppendLine(line);
            }

            public string Content
            {
                get
                {
                    return Builder.ToString();
                }
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
            RuntimeContext = V8RuntimeContextType.Default;
        }
        
        public V8RuntimeContextType RuntimeContext { get; set; }
        public V8ModulePartClass PartClass { get; set; }

        public string Content { get; set; }
        public string Title { get; private set; }

    }

    enum V8ModulePartClass
    {
        VariableDefinition,
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
