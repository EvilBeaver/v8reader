using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Search;

using System.Windows.Threading;

namespace V8Reader.Controls
{
    /// <summary>
    /// Логика взаимодействия для CodeControl.xaml
    /// </summary>
    public partial class CodeControl : UserControl
    {
        public CodeControl()
        {
            var Res = Application.GetResourceStream(new Uri("pack://application:,,,/v8viewer;component/controls/1CV8Syntax.xshd", UriKind.Absolute));

            IHighlightingDefinition v8Highlighting;

            using (var s = Res.Stream)
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    v8Highlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            HighlightingManager.Instance.RegisterHighlighting("1CV8", new string[] { ".v8m" }, v8Highlighting);

            InitializeComponent();

            editor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(editor.TextArea));
            editor.ShowLineNumbers = true;

            foldingManager = FoldingManager.Install(editor.TextArea);

            editor.TextChanged += editor_TextChanged;
            editor.TextArea.Options.EnableHyperlinks = false;
            editor.TextArea.Options.EnableVirtualSpace = true;
            editor.TextArea.Options.EnableRectangularSelection = true;
            editor.TextArea.SelectionCornerRadius = 0;
            
            foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

        }

        void editor_TextChanged(object sender, EventArgs e)
        {
           // m_ModifyFlag = true;
            if (foldingUpdateTimer!= null && !foldingUpdateTimer.IsEnabled)
            {
                foldingUpdateTimer.Start();
            }
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            ((DispatcherTimer)sender).Stop();
            //m_ModifyFlag = false;
        }

        public String Text 
        { 
            get 
            {
                return editor.Text; 
            }
            set
            {
                editor.Text = value;
            }
        }

        FoldingManager foldingManager;
        AbstractFoldingStrategy foldingStrategy = new V8ModuleFoldingStrategy();
        //bool m_ModifyFlag;
        DispatcherTimer foldingUpdateTimer;
    }

    class V8ModuleFoldingStrategy : AbstractFoldingStrategy
	{
		
        public V8ModuleFoldingStrategy()
		{
			
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>

        private struct TextFragment
        {
            public int offset;
            public int len;
        }


		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			List<NewFolding> newFoldings = new List<NewFolding>();

            int startPos = 0;
            int len = document.TextLength;

            int currentStart = 0;

            bool MethodIsOpen = false;
            string EndToken = null;

            int PreCommentStart = -1;

            var Reader = document.CreateReader();
            
            do
            {

                string lineText = Reader.ReadLine();
                if (lineText == null)
                {
                    break;
                }
                
                TextFragment tf = new TextFragment();
                tf.offset = startPos;
                tf.len = lineText.Length;

                startPos += lineText.Length + 2;

                if (!MethodIsOpen)
                {
                    bool CommentBreak = false;
                    
                    if (lineText.StartsWith("//"))
                    {
                        if (PreCommentStart < 0)
                        {
                            PreCommentStart = tf.offset + tf.len;
                        }
                    }
                    else
                    {
                        CommentBreak = true;
                    }
                    
                    if (lineText.StartsWith("ПРОЦЕДУРА", StringComparison.OrdinalIgnoreCase))
                    {
                        MethodIsOpen = true;
                        EndToken = "КОНЕЦПРОЦЕДУРЫ";
                    }
                    else if(lineText.StartsWith("ФУНКЦИЯ", StringComparison.OrdinalIgnoreCase))
                    {
                        MethodIsOpen = true;
                        EndToken = "КОНЕЦФУНКЦИИ";
                    }

                    if (MethodIsOpen)
                    {
                        currentStart = tf.offset + tf.len;

                        if (PreCommentStart >= 0)
                        {
                            var Folding = new NewFolding(PreCommentStart, tf.offset - 2);
                            newFoldings.Add(Folding);
                            PreCommentStart = -1;
                        }
                    }
                    else if(CommentBreak)
                    {
                        PreCommentStart = -1;
                    }
                    
                }
                else if (lineText.StartsWith(EndToken, StringComparison.OrdinalIgnoreCase))
                {
                    var Folding = new NewFolding(currentStart, tf.offset + tf.len);
                    newFoldings.Add(Folding);

                    MethodIsOpen = false;
                }
                
            }
            while (true);

			return newFoldings;
		}
	}

}
