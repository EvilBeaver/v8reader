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
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace V8Reader.Controls
{
    /// <summary>
    /// Логика взаимодействия для CodeControl.xaml
    /// </summary>
    public partial class CodeControl : UserControl
    {
        public CodeControl()
        {
            InitializeComponent();
        }

        public String Text 
        { 
            get 
            { 
                return codeTextBox.Text; 
            }
            set
            {
                codeTextBox.Text = value;
            }
        }

        private void FastColoredTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {

            //clear style of changed range
            e.ChangedRange.ClearStyle(KeywordStyle, CommentStyle, NumbersStyle, StringStyle);

            //comment highlighting
            e.ChangedRange.SetStyle(CommentStyle, @"//.*$", RegexOptions.Multiline);

            //preprocessor highlighting
            e.ChangedRange.SetStyle(PreprocStyle, @"\#.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(PreprocStyle, @"&.*$", RegexOptions.Multiline);

            //string highlighting
            e.ChangedRange.SetStyle(StringStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");

            //number highlighting
            e.ChangedRange.SetStyle(NumbersStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");

            //keyword highlighting
            e.ChangedRange.SetStyle(KeywordStyle, @"\b(Процедура|КонецПроцедуры|Функция|КонецФункции|Если|Тогда|ИначеЕсли|Иначе|КонецЕсли|Для|Пока|Каждого|По|Из|Цикл|КонецЦикла|Прервать|Продолжить|Возврат|Попытка|Исключение|КонецПопытки|ВызватьИсключение|Истина|Ложь|Лев|Прав|Сред|СокрЛ|СокрП|СокрЛП|Дата|Булево|Строка|Число|Новый|Перем|Экспорт)\b");
            e.ChangedRange.SetStyle(KeywordStyle, @"[\(|\)|,|;|.|+|-|*|\/|%|=|<|>]");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();
            //set folding markers
            e.ChangedRange.SetFoldingMarkers("Процедура", "КонецПроцедуры", RegexOptions.IgnoreCase);
            e.ChangedRange.SetFoldingMarkers("Функция", "КонецФункции", RegexOptions.IgnoreCase);

        }

        //styles
        TextStyle KeywordStyle = new TextStyle(System.Drawing.Brushes.Red, null, System.Drawing.FontStyle.Regular);
        TextStyle NumbersStyle = new TextStyle(System.Drawing.Brushes.Magenta, null, System.Drawing.FontStyle.Regular);
        TextStyle CommentStyle = new TextStyle(System.Drawing.Brushes.Green, null, System.Drawing.FontStyle.Regular);
        TextStyle StringStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);
        TextStyle PreprocStyle = new TextStyle(System.Drawing.Brushes.Brown, null, System.Drawing.FontStyle.Regular);

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(WFHost);
        }

    }
}
