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
using ICSharpCode.AvalonEdit.Folding;
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

            //DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            //foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            //foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            //foldingUpdateTimer.Start();

        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
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
		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			List<NewFolding> newFoldings = new List<NewFolding>();
			
            //Stack<int> startOffsets = new Stack<int>();
            //int lastNewLineOffset = 0;
            //char openingBrace = this.OpeningBrace;
            //char closingBrace = this.ClosingBrace;
            //for (int i = 0; i < document.TextLength; i++) {
            //    char c = document.GetCharAt(i);
            //    if (c == openingBrace) {
            //        startOffsets.Push(i);
            //    } else if (c == closingBrace && startOffsets.Count > 0) {
            //        int startOffset = startOffsets.Pop();
            //        // don't fold if opening and closing brace are on the same line
            //        if (startOffset < lastNewLineOffset) {
            //            newFoldings.Add(new NewFolding(startOffset, i + 1));
            //        }
            //    } else if (c == '\n' || c == '\r') {
            //        lastNewLineOffset = i + 1;
            //    }
            //}
            //newFoldings.Sort((a,b) => a.StartOffset.CompareTo(b.StartOffset));

			return newFoldings;
		}
	}

}
