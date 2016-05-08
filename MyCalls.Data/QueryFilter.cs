using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalls.Data
{
    /// <summary>
    /// Define a property that can be used to filter our Dataset
    /// </summary>
    public class FilterPropertyInfo
    {
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public FilterPropertyType FilterType { get; set; }
        public object Data { get; set; }
    }


    public class FilterProperty
    {
        public FilterPropertyInfo Info { get; set; }
        public FilterPropertyCondition Condition { get; set; }
        public FilterPropertyCompareMode CompareMode { get; set; }
        public object Value { get; set; }
    }

    public enum FilterPropertyCondition
    {
        AND,
        OR
    }

    public enum FilterPropertyCompareMode
    {
        Equals,
        GreaterThan,
        LesserThan,
        Contains,
        Like
    }

    public enum FilterPropertyType
    {
        String,
        Number,
        Date,
        List,
    }


    public class QueryFilter
    {
        public QueryFilter()
        {
            Properties = new List<FilterProperty>();
        }

        public List<FilterProperty> Properties { get; set; }

        public string GenerateFilter()
        {
            var filter = "1=1";

            foreach (var filterProperty in Properties)
            {
                switch (filterProperty.Info.FilterType)
                {
                    case FilterPropertyType.List:
                    case FilterPropertyType.String:
                    case FilterPropertyType.Date:
                        filter += GetComparasion(filterProperty, true);
                        break;
                    case FilterPropertyType.Number:
                        filter += GetComparasion(filterProperty, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return filter;
        }



        private string GetComparasion(FilterProperty filterProperty, bool useQuotes)
        {
            var condition = $" {filterProperty.Condition} {filterProperty.Info.PropertyName}";

            var value = useQuotes ? $"\"{filterProperty.Value}\"" : $"{filterProperty.Value}";

            switch (filterProperty.CompareMode)
            {
                case FilterPropertyCompareMode.Equals:
                    condition += $" = {value}";
                    break;
                case FilterPropertyCompareMode.GreaterThan:
                    condition += $" > {value}";
                    break;
                case FilterPropertyCompareMode.LesserThan:
                    condition += $" < {value}";
                    break;
                case FilterPropertyCompareMode.Like:
                    condition += $" like %{value}%";
                    break;
                case FilterPropertyCompareMode.Contains:
                    var pathParts = filterProperty.Info.PropertyName.Split('.');
                    var path = String.Join(".", pathParts.Take(2));
                    var prop = pathParts.Skip(2).Take(1).First();
                    condition = $" {filterProperty.Condition} {path}.Any({prop} == {value})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return condition;
        }
    }

}
