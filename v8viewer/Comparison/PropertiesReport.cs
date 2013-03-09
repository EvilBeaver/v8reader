using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;
using System.Windows.Documents;

namespace V8Reader.Comparison
{
    
    abstract class PropertiesReport
    {

        abstract public FlowDocument GenerateReport();
        abstract public Block GenerateContent();

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
                return new Run(content) { Style = DefaultStyle() };
            }

            public System.Windows.Style DefaultStyle()
            {
                return _defaultStyle;
            }

        }

    }
    
    class PropertiesReportSingle : PropertiesReport
    {
        public PropertiesReportSingle(IMDPropertyProvider PropertyProvider) : this(PropertyProvider, null)
        {
        }

        public PropertiesReportSingle(IMDPropertyProvider PropertyProvider, IPropertiesReportFormatter formatter)
        {
            _provider = PropertyProvider;
            Formatter = formatter;
        }

        public override FlowDocument GenerateReport()
        {
            FlowDocument doc = new FlowDocument();
            
            Paragraph Header = new Paragraph(Formatter.Header1("Отчет по свойствам"));

            doc.Blocks.Add(Header);

            Section contentSection = new Section();
            contentSection.Style = Formatter.DefaultStyle();
            contentSection.Blocks.Add(GenerateContent());

            doc.Blocks.Add(contentSection);

            return doc;

        }

        public override Block GenerateContent()
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

        private IMDPropertyProvider _provider;
        
    }

    class PropertiesReportCompare : PropertiesReport
    {
        public PropertiesReportCompare(IMDPropertyProvider left, IMDPropertyProvider right)
        {
            _left = left;
            _right = right;
        }

        IMDPropertyProvider _left;
        IMDPropertyProvider _right;

        public override FlowDocument GenerateReport()
        {
            FlowDocument doc = new FlowDocument();

            Paragraph Header = new Paragraph(Formatter.Header1("Отчет о сравнении объектов"));

            Paragraph legend = new Paragraph();
            legend.Inlines.Add(Formatter.MainText("-> свойства первого объекта"));
            legend.Inlines.Add(new LineBreak());
            legend.Inlines.Add(Formatter.MainText("<- свойства второго объекта"));

            doc.Blocks.Add(Header);
            doc.Blocks.Add(legend);

            Section contentSection = new Section();
            contentSection.Style = Formatter.DefaultStyle();
            contentSection.Blocks.Add(GenerateContent());

            doc.Blocks.Add(contentSection);

            return doc;
        }

        public override Block GenerateContent()
        {
            Section content = new Section();

            foreach (var PropDef in _left.Properties.Values)
            {
                Section propSection = new Section();

                var p = new Paragraph(Formatter.HeaderProperty(PropDef.Name));
                p.Margin = new System.Windows.Thickness(0, 2, 0, 1);

                propSection.Blocks.Add(p);
                
                var lp = new Paragraph(new Run("->")){Margin = new System.Windows.Thickness(0)};
                propSection.Blocks.Add(lp);
                propSection.Blocks.Add(PropDef.ValueVisualizer.FlowContent);

                var rightProp = _right.Properties[PropDef.Key];

                var rp = new Paragraph(new Run("<-")) { Margin = new System.Windows.Thickness(0) };
                propSection.Blocks.Add(rp);
                propSection.Blocks.Add(rightProp.ValueVisualizer.FlowContent);

                content.Blocks.Add(propSection);

            }

            return content;
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
