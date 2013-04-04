using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    sealed class V8Type
    {

        public V8Type(String name, String id)
        {
            Name = name;
            ID = id;
        }

        public override string ToString()
        {
            return Name;
        }

        public String Name { get; private set; }
        public String ID { get; private set; }

    }

    sealed class V8StringQualifier
    {

        public V8StringQualifier(int len, AvailableLengthType lenType = V8StringQualifier.AvailableLengthType.Variable)
        {
            Lenght = len;
            AvailableLength = lenType;
        }
        
        public int Lenght { get; private set; }
        public AvailableLengthType AvailableLength { get; private set;}

        public enum AvailableLengthType
        {
            Variable,
            Fixed
        }

        public override string ToString()
        {
            if (AvailableLength == AvailableLengthType.Fixed)
            {
                return String.Format("str({0},fixed)", Lenght.ToString());
            }
            else
            {
                return String.Format("str({0})", Lenght.ToString());
            }
        }
    }

    sealed class V8NumberQualifier
    {

        public V8NumberQualifier(int IntegerPart, int Fraction = 0, bool NonNeg = false)
        {
            IntegerDigits = IntegerPart;
            FractionDigits = Fraction;
            NonNegative = NonNeg;
        }
        
        public int IntegerDigits { get; private set; }
        public int FractionDigits { get; private set; }
        public bool NonNegative { get; private set; }

        public override string ToString()
        {
            return String.Format("num({0},{1})", IntegerDigits, FractionDigits);
        }
    }

    sealed class V8DateQualifier
    {

        public V8DateQualifier(DateFractionsType dateFractions)
        {
            DateFractions = dateFractions;
        }

        public DateFractionsType DateFractions { get; private set; }

        public enum DateFractionsType
        {
            DateAndTime,
            Date,
            Time
        }

        public override string ToString()
        {
            switch (DateFractions)
            {
                case DateFractionsType.Date:
                    return "date()";
                case DateFractionsType.Time:
                    return "time()";
                default:
                    return ""; // datetime не требует доп. пояснений
            }
        }
    }

    sealed class V8TypeDescription
    {

        public V8TypeDescription(V8Type[] types, V8NumberQualifier numberQualifier = null, V8StringQualifier stringQualifier = null, V8DateQualifier dateQualifier = null)
        {
            m_types = new V8Type[types.Length];

            types.CopyTo(m_types, 0);
            NumberQualifier = numberQualifier;
            StringQualifier = stringQualifier;
            DateQualifier = dateQualifier;
        }

        public V8NumberQualifier NumberQualifier { get; private set; }
        public V8StringQualifier StringQualifier { get; private set; }
        public V8DateQualifier DateQualifier { get; private set; }

        public V8Type[] Types()
        {
            return (V8Type[])m_types.Clone();
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            if (m_types.Length > 0)
            {                
                for (int i = 0; i < m_types.Length; i++)
                {
                    sb.Append(',');
                    sb.Append(m_types[i].ToString());
                }
                
            }
            else
            {
                return "";
            }

            if (NumberQualifier != null)
            {
                sb.Append(',');
                sb.Append(NumberQualifier.ToString());
            }

            if (StringQualifier != null)
            {
                sb.Append(',');
                sb.Append(StringQualifier.ToString());
            }

            if (DateQualifier != null)
            {
                var qs = StringQualifier.ToString();
                if (qs != "")
                {
                    sb.Append(',');
                    sb.Append(qs);
                }
            }

            sb.Remove(0, 1);
            return sb.ToString();

        }

        private V8Type[] m_types;

        #region Static part. List Deserializtion

        public static V8TypeDescription ReadFromList(SerializedList pattern)
        {

            if (pattern.Items[0].ToString() != "Pattern")
                throw new ArgumentException("Wrong pattern stream");

            V8Type[] types = new V8Type[pattern.Items.Count - 1];
            V8NumberQualifier numQ = null;
            V8StringQualifier strQ = null;
            V8DateQualifier dateQ = null;

            for (int i = 1; i < pattern.Items.Count; i++)
            {
                SerializedList item = (SerializedList)pattern.Items[i];

                if (item.Items[0].ToString() == "#")
                {
                    V8Type newType = new V8Type(item.Items[1].ToString(), item.Items[1].ToString()); // пока статичные id разбирать не будем
                    types[i - 1] = newType;
                }
                else
                {
                    String typeToken = item.Items[0].ToString();
                    
                    switch (typeToken)
                    {
                        case "N":
                            types[i - 1] = V8BasicTypes.Number;

                            if (item.Items.Count > 1)
                            {
                                // указан квалификатор
                                numQ = new V8NumberQualifier(Int32.Parse(item.Items[1].ToString()),
                                        Int32.Parse(item.Items[2].ToString()), item.Items[3].ToString() == "1");
                            }
                            
                            break;
                        
                        case "S":
                            
                            types[i - 1] = V8BasicTypes.String;

                            if (item.Items.Count > 1)
                            {
                                // указан квалификатор
                                strQ = new V8StringQualifier(Int32.Parse(item.Items[1].ToString()),
                                        (item.Items[2].ToString() == "0") ? V8StringQualifier.AvailableLengthType.Fixed : V8StringQualifier.AvailableLengthType.Variable);
                            }

                            break;

                        case "D":

                            types[i - 1] = V8BasicTypes.Date;

                            if (item.Items.Count > 1)
                            {
                                // указан квалификатор
                                dateQ = new V8DateQualifier((item.Items[1].ToString() == "T") ? V8DateQualifier.DateFractionsType.Time : V8DateQualifier.DateFractionsType.Date);
                            }
                            else
                            {
                                dateQ = new V8DateQualifier(V8DateQualifier.DateFractionsType.DateAndTime);
                            }

                            break;

                        case "B":

                            types[i - 1] = V8BasicTypes.Boolean;
                            break;

                        default:

                            V8Type newType = new V8Type("Unknown", "U"); // пока не знаю про U
                            types[i - 1] = newType;

                            break;

                    }
                }

            }

            V8TypeDescription result = new V8TypeDescription(types, numQ, strQ, dateQ);
            return result;

        }

        #endregion

    }

    static class V8BasicTypes
    {

        public static readonly V8Type Number = new V8Type("Число", "N");
        public static readonly V8Type String = new V8Type("Строка", "S");
        public static readonly V8Type Boolean = new V8Type("Булево", "B");
        public static readonly V8Type Date = new V8Type("Дата", "D");

    }

}
