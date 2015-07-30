using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;


namespace WpfFront.Common.Query
{

    public class Operators
    {

        public static StringDictionary GetStrOperator()
        {
            StringDictionary result = new StringDictionary();            
            //result.Add("Between (Range)", " BETWEEN %_val AND %_val1");
            //result.Add("EndsWith", " LIKE - %_val");
            //result.Add("StartsWith", " LIKE - _val%");
            //result.Add("Contains", " LIKE - %_val%");
            //result.Add("NotContains", " NOT LIKE - %_val%");
            //result.Add("Equal", " = - _val");

            result.Add("Between (Range)", " BETWEEN %_val AND %_val1");
            result.Add("EndsWith", " LIKE %_val");
            result.Add("StartsWith", " LIKE _val%");
            result.Add("Contains", " LIKE %_val%");
            result.Add("NotContains", " NOT_LIKE %_val%");
            result.Add("Equal", " = _val");
            //result.Add("None", "");

            return result;
            
        }


        public static StringDictionary GetNumOperator()
        {
            StringDictionary result = new StringDictionary();
            result.Add("Equal", " = _val");
            result.Add("GreatherThan", " > _val");
            result.Add("LowerThan", " < _val");
            result.Add("GreatOrEq", " >= _val");
            result.Add("LowerOrEq", " <= _val");
            //result.Add("None", "");

            return result;
        }


         public static StringDictionary GetAggregation()
        {
            StringDictionary result = new StringDictionary();
            result.Add("Sum", " Sum(_val) ");
            result.Add("Average", " Avg(_val) ");
            result.Add("Min", " Min(_val) ");
            result.Add("Max", " Max(_val) ");
            result.Add("Count", " Count(_val) ");
            //result.Add("None", "");
      
            return result;
        }


         public static StringDictionary GetABCRank()
         {
             StringDictionary result = new StringDictionary();
             result.Add("A", "1");
             result.Add("B", "2");
             result.Add("C", "3");
             result.Add("D", "4");

             return result;
         }

    }

}
