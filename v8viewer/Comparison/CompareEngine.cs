﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Comparison
{

    interface IComparableItem
    {
        bool CompareTo(object Comparand);
    }

    interface IComparator
    {
        bool CompareObjects(object Compared, object Comparand);
    }

    class BasicComparator : IComparator
    {

        #region IComparator Members

        public bool CompareObjects(object Compared, object Comparand)
        {
            if (Compared == null)
                return Comparand == null;

            return Compared.Equals(Comparand);
        }

        #endregion
    }

    class ComparableHelperWrapper : IComparableItem
    {
        public ComparableHelperWrapper(object value, IComparator comparator)
        {
            m_Value = value;
            m_Comparator = comparator;
        }

        public object Value
        {
            get { return m_Value; }
        }

        private object m_Value;
        private IComparator m_Comparator;

        #region IComparableProperty Members

        public bool CompareTo(object Comparand)
        {
            return m_Comparator.CompareObjects(Value, Comparand);
        }

        #endregion
    }

    class ComparisonPerformer
    {
        public ComparisonPerformer(IMDTreeItem Left, IMDTreeItem Right)
        {
            m_Left = Left;
            m_Right = Right;
        }

        public ComparisonResult Perform()
        {
            return Perform(MatchingMode.ByID);
        }

        public ComparisonResult Perform(MatchingMode Mode)
        {
            ComparisonResult result = new ComparisonResult();
            m_CurrentMode = Mode;

            FillComparisonNode(m_Left, m_Right, result);

            return result;

        }

        private void FillComparisonNode(IMDTreeItem Left, IMDTreeItem Right, ComparisonItem node)
        {
            if ((Left != null && Right != null) && Left.GetType() != Right.GetType())
            {
                throw new InvalidOperationException("Compared components must be of the same type");
            }

            FillSide(node.Left, Left);
            FillSide(node.Right, Right);

            if (Left == null)
            {
                TraverseRightObject(Right, node);
                return;
            }

            if (Left is IMDPropertyProvider)
            {
                CompareProperties((IMDPropertyProvider)Left, (IMDPropertyProvider)Right, node);
            }

            if (Left is IComparableItem)
            {
                // Разница определяется самим объектом
                node.IsDiffer = ((IComparableItem)Left).CompareTo(Right);
            }
            else if (Left is MDObjectsCollection<MDBaseObject> || Left is StaticTreeNode)
            {

                var LeftItems = from item in Left.ChildItems orderby item.Key, item.Text select item;
                var RightItems = from item in Right.ChildItems orderby item.Key, item.Text select item;

                var LeftWalker = LeftItems.GetEnumerator();
                var RightWalker = RightItems.GetEnumerator();

                if (LeftItems.Count<IMDTreeItem>() != 0)
                {
                    
                    bool WalkRight = true;

                    while (LeftWalker.MoveNext())
                    {
                        
                        if (WalkRight)
                        {
                            if (RightWalker.MoveNext())
                            {
                                var LeftObj = LeftWalker.Current;
                                var RightObj = RightWalker.Current;

                                if (LeftObj.Key == RightObj.Key)
                                {
                                    // это один и тот же объект.
                                    AddAndFillNewNode(LeftObj, RightObj, node);
                                    // дальнейший траверс в штатном режиме
                                    WalkRight = true;
                                }
                                else
                                {
                                    // это разные объекты
                                    // значит, левый объект точно отсутствует справа
                                    AddAndFillNewNode(LeftObj, null, node);
                                    // при следующей итерации правый объект не двигаем.
                                    WalkRight = false;

                                }

                            }
                            else
                            {
                                // справа элементы закончились
                                AddAndFillNewNode(LeftWalker.Current, null, node);
                            }
                        }
                        else if (LeftWalker.Current.Key == RightWalker.Current.Key)
                        {
                            // это один и тот же объект.
                            AddAndFillNewNode(LeftWalker.Current, RightWalker.Current, node);
                            // дальнейший траверс в штатном режиме
                            WalkRight = true;
                        }
                        else
                        {
                            // это разные объекты
                            // значит, левый объект точно отсутствует справа
                            AddAndFillNewNode(LeftWalker.Current, null, node);
                            // при следующей итерации правый объект не двигаем.
                            WalkRight = false;
                        }

                    }
                    // слева элементов больше нет

                    if (RightWalker.Current != null)
                    {
                        AddAndFillNewNode(null, RightWalker.Current, node);
                    
                        while (RightWalker.MoveNext())
                        {
                            AddAndFillNewNode(null, RightWalker.Current, node);
                        }
                    }

                }
                else
                {
                    // слева элементов нет
                    while (RightWalker.MoveNext())
                    {
                        AddAndFillNewNode(null, RightWalker.Current, node);
                    }
                }

            }
            else if( Left.HasChildren() )
            {
                // это статичные свойства метаданных
                // порядок свойств будет соблюдаться в обоих объектах
                var LeftWalker = Left.ChildItems.GetEnumerator();

                IEnumerator<IMDTreeItem> RightWalker = null;
                if (Right != null)
                {
                    RightWalker = Right.ChildItems.GetEnumerator();
                }

                while (LeftWalker.MoveNext())
                {
                    var newNode = new ComparisonItem();

                    if (Right != null)
                    {
                        RightWalker.MoveNext();
                        FillComparisonNode(LeftWalker.Current, RightWalker.Current, newNode);
                    }
                    else
                    {
                        FillComparisonNode(LeftWalker.Current, null, newNode);
                    }

                    if (!(newNode.Left == null && newNode.Right == null))
                        node.Items.Add(newNode);
                }

            }

        }

        private void AddAndFillNewNode(IMDTreeItem Left, IMDTreeItem Right, ComparisonItem ParentNode)
        {
            ComparisonItem newNode = new ComparisonItem();
            FillComparisonNode(Left, Right, newNode);

            if (!(newNode.Left == null && newNode.Right == null))
                ParentNode.Items.Add(newNode);
        }

        private void TraverseRightObject(IMDTreeItem Right, ComparisonItem ParentNode)
        {
            if (Right.HasChildren())
            {
                foreach (var item in Right.ChildItems)
                {
                    AddAndFillNewNode(null, item, ParentNode);
                }

            }

            if (Right is IMDPropertyProvider)
            {
                var PropStub = ParentNode.AddStaticNode("Свойства");
                var PropProv = Right as IMDPropertyProvider;

                foreach (PropDef propDef in PropProv.Properties.Values)
                {
                    var propNode = new ComparisonItem(null, propDef.Value, propDef.Name);
                    propNode.IsDiffer = true;

                    PropStub.Items.Add(propNode);

                }

                PropStub.IsDiffer = true;

            }

        }

        private void FillSide(ComparisonSide SideObject, IMDTreeItem Value)
        {
            SideObject.Object = Value;
            if (Value == null)
            {
                SideObject.Presentation = "<отсутствует>";
            }
            else
            {
                SideObject.Presentation = Value.Text;
            }
        }

        private ComparisonItem CompareProperties(IMDPropertyProvider Left, IMDPropertyProvider Right, ComparisonItem parentNode)
        {
            var PropStub = parentNode.AddStaticNode("Свойства");
            
            bool result = true;

            foreach (PropDef propDef in Left.Properties.Values)
            {

                var LeftValComparable = GetAsComparable(propDef.Value);

                object RightVal = null;
                if (Right != null)
                    RightVal = Right.GetValue(propDef.Key);

                bool diff = LeftValComparable.CompareTo(RightVal);
                result = result && diff;

                var newNode = new ComparisonItem(propDef.Value, RightVal, propDef.Name);
                newNode.IsDiffer = diff;

                PropStub.Items.Add(newNode);

            }

            PropStub.IsDiffer = result;
            
            return PropStub;

        }

        private IComparableItem GetAsComparable(object Value)
        {
            if (Value is IComparableItem)
            {
                return (IComparableItem)Value;
            }
            else
            {
                return new ComparableHelperWrapper(Value, new BasicComparator());
            }

        }

        public enum MatchingMode
        {
            ByID,
            ByName
        }

        private IMDTreeItem m_Left;
        private IMDTreeItem m_Right;
        private MatchingMode m_CurrentMode;

    }

    class ComparisonItem
    {

        public ComparisonItem()
        {
            Left = new ComparisonSide();
            Right = new ComparisonSide();
            m_Items = new List<ComparisonItem>();
        }

        public ComparisonItem(object left, object right, string name)
        {
            Left = new ComparisonSide() { Object = left, Presentation = name };
            Right = new ComparisonSide { Object = right, Presentation = name };
            m_Items = new List<ComparisonItem>();
        }

        public ComparisonSide Left { get; private set; }
        public ComparisonSide Right { get; private set; }
        public List<ComparisonItem> Items { get { return m_Items; } }
        
        public bool IsDiffer { get; set; }

        public ComparisonItem AddStaticNode(string name)
        {
            var newNode = new ComparisonItem();
            newNode.Left.Presentation = name;
            newNode.Right.Presentation = name;
            this.Items.Add(newNode);

            return newNode;
        }

        private List<ComparisonItem> m_Items;
    }

    class ComparisonSide
    {
        private object m_Obj;

        public object Object
        {
            get { return m_Obj; }
            set { m_Obj = value; }
        }

        public string Presentation { get; set; }
        public override string ToString()
        {
            if (Presentation == String.Empty)
                return "< текст не задан >";

            return Presentation;
        }
    }

    class ComparisonResult : ComparisonItem
    {
        public ComparisonResult() : base()
        {
        }
    }

    class FileComparisonPerformer : IDisposable
    {

        public FileComparisonPerformer(string LeftFile, string RightFile)
        {
            m_LeftObject = MDDataProcessor.Create(LeftFile);
            m_RightObject = MDDataProcessor.Create(RightFile);
        }

        public ComparisonResult Perform()
        {
            return Perform(ComparisonPerformer.MatchingMode.ByID);
        }

        public ComparisonResult Perform(ComparisonPerformer.MatchingMode Mode)
        {
            m_Performer = new ComparisonPerformer(m_LeftObject, m_RightObject);
            return m_Performer.Perform(Mode);
        }

        private ComparisonPerformer m_Performer;
        private MDDataProcessor m_LeftObject;
        private MDDataProcessor m_RightObject;


        #region IDisposable Members

        public void Dispose()
        {
            LocalDispose(m_LeftObject);
            LocalDispose(m_RightObject);

            m_LeftObject  = null;
            m_RightObject = null;
        }

        private void LocalDispose(IDisposable Obj)
        {
            if (Obj != null) Obj.Dispose();
        }

        #endregion
    }

}
