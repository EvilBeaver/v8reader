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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using V8Reader.Core;
using FastColoredTextBoxNS;

namespace V8Reader.Editors
{
    /// <summary>
    /// Логика взаимодействия для CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : Window
    {

        //styles
        TextStyle KeywordStyle = new TextStyle(System.Drawing.Brushes.Red, null, System.Drawing.FontStyle.Regular);
        TextStyle NumbersStyle = new TextStyle(System.Drawing.Brushes.Magenta, null, System.Drawing.FontStyle.Regular);
        TextStyle CommentStyle = new TextStyle(System.Drawing.Brushes.Green, null, System.Drawing.FontStyle.Regular);
        TextStyle StringStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);
        TextStyle PreprocStyle = new TextStyle(System.Drawing.Brushes.Brown, null, System.Drawing.FontStyle.Regular);

        public CodeEditor()
        {
            InitializeComponent();
            Utils.FormsSettingsManager.Register(this, "CodeEditor");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(codeTextBox);
        }

    }
}
