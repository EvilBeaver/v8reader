using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;
using System.Windows.Documents;

namespace V8Reader.Comparison
{
    class PropertiesReport
    {
        public PropertiesReport(IMDPropertyProvider PropertyProvider) : this(PropertyProvider, null)
        {
        }

        public PropertiesReport(IMDPropertyProvider PropertyProvider, IPropertiesReportFormatter formatter)
        {
            _provider = PropertyProvider;
            Formatter = formatter;
        }

        public FlowDocument GenerateReport()
        {
            FlowDocument doc = new FlowDocument();
            //doc.Resources.Add("", Formatter.DefaultStyle());
            
            Paragraph Header = new Paragraph(Formatter.Header1("Отчет по свойствам"));

            doc.Blocks.Add(Header);

            Section contentSection = new Section();
            contentSection.Style = Formatter.DefaultStyle();
            contentSection.Blocks.Add(GenerateContent());

            doc.Blocks.Add(contentSection);

            return doc;

        }

        public Block GenerateContent()
        {
            Section content = new Section();

            foreach (var PropDef in _provider.Properties.Values)
            {
                Section propSection = new Section();

                var p = new Paragraph(Formatter.HeaderProperty(PropDef.Name));
                p.Margin = new System.Windows.Thickness(0,2,0,1);

                propSection.Blocks.Add(p);
                propSection.Blocks.Add(PropDef.ValueVisualizer.FlowContent);

                content.Blocks.Add(propSection);

            }

            return content;
        }

        public IPropertiesReportFormatter Formatter
        {
            get
            {
                if (_formatter == null)
                {
                    _formatter = new StandartFormatter();
                }

                return _formatter;
            }

            set
            {
                if (value != null)
                {
                    _formatter = value;
                }
                else
                {
                    _formatter = new StandartFormatter();
                }
            }
        }

        private IMDPropertyProvider _provider;
        private IPropertiesReportFormatter _formatter;

        private class StandartFormatter : IPropertiesReportFormatter
        {

            public StandartFormatter()
            {
                _defaultStyle = new System.Windows.Style(typeof(TextElement));
                _defaultStyle.Setters.Add(new System.Windows.Setter(Run.FontSizeProperty, (double)11.0));
                _defaultStyle.Setters.Add(new System.Windows.Setter(Run.FontFamilyProperty, new System.Windows.Media.FontFamily("Serif")));
            }

            private System.Windows.Style _defaultStyle;

            public Run Header1(string content)
            {
                var run = new Run(content);
                run.FontSize = 18;
                run.FontWeight = System.Windows.FontWeights.SemiBold;

                return run;
            }

            public Run Header2(string content)
            {
                var run = new Run(content);
                run.FontSize = 16;
                run.FontWeight = System.Windows.FontWeights.SemiBold;

                return run;
            }

            public Run Header3(string content)
            {
                var run = new Run(content);
                run.FontSize = 12;
                run.FontWeight = System.Windows.FontWeights.SemiBold;

                return run;
            }

            public Run HeaderProperty(string content)
            {
                return Header3(content);
            }

            public Run MainText(string content)
            {
                return new Run(content);
            }

            public System.Windows.Style DefaultStyle()
            {
                return _defaultStyle;
            }

        }

    }

    interface IPropertiesReportFormatter
    {
        Run Header1(string content);
        Run Header2(string content);
        Run Header3(string content);
        
        Run HeaderProperty(string content);
        Run MainText(string content);

        System.Windows.Style DefaultStyle();
    }

}
