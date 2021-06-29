using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;
namespace ConsoleApp1
{
    public class Query
    {
        public List<String> Fields { get; set; }
        public Filter Filter { get; set; }
        // function convert query object to string
        public string toSqlString()
        {
            StringBuilder query = new StringBuilder();
            var testTown = QueryConst.towns;
            // add SELECT to the query
            query.Append(QueryConst.select).Append(" ");
            // check if field is not null
            if (Fields != null && Fields.Count > 0)
            {
                // check if field exist in schema
                foreach (string fildName in Fields)
                {
                    if (!testTown.ContainsKey(fildName))
                    {
                        throw new Exception("Field '" + fildName + "' in SELECT Clause do not exist in 'Town' schema");
                    }
                }
                // concatenate the filds
                query.AppendLine(String.Join(",", Fields));
            }
            else
            {
                throw new Exception("Missing fields in query");
            }
            // add FROM Table to the query
            query.AppendLine(QueryConst.from);
            // convert filters to string if the query contains filters 
            if (Filter != null)
            {
                string filterString = Filter.toSqlString();
                if (filterString != null && filterString.Length > 0)
                {
                    // add WHERE to the query
                    query.Append(QueryConst.where).Append(" ");
                    // add FILTERS to the query
                    query.AppendLine(Filter.toSqlString());

                }

            }
            return query.ToString();
        }
    }

    public class Filter
    {
        public string LogicalOp { get; set; }
        public Filter Filters1 { get; set; }
        public Filter Filters2 { get; set; }
        public SingleFilter SingleFilter { get; set; }
        // recursive function to convert the filters object to a string
        internal string toSqlString()
        {
            StringBuilder queryPart = new StringBuilder();

            if (LogicalOp != null && SingleFilter == null)
            {
                // compounding filter 
                string Filter1 = Filters1.toSqlString();
                string Filter2 = Filters2.toSqlString();
                queryPart.Append("(").Append(Filter1).Append(" ");
                queryPart.Append(LogicalOp).Append(" ");
                queryPart.Append(Filter2).Append(")");

            }
            else
            {
                if (SingleFilter != null)
                {
                    // simple filter
                    string filterString = SingleFilter.toSqlString();
                    if (filterString != null && filterString.Length > 0)
                    {
                        queryPart.Append(SingleFilter.toSqlString());
                    }
                }
            }
            return queryPart.ToString();
        }
    }

    public class SingleFilter
    {
        public string field { get; set; }
        public string value { get; set; }
        public string predicate { get; set; }

        internal string toSqlString()
        {
            StringBuilder queryPart = new StringBuilder();
            if (predicate == null || predicate.Length == 0)
            {
                predicate = QueryConst.predicateEq;
            }
            // validation of SQL Query
            validation();
            // concate the filters
            queryPart.Append(field).Append(" ");
            queryPart.Append(convertPredicate(predicate)).Append(" ");
            if (QueryConst.predicateContains.Equals(predicate))
            {
                queryPart.Append("'%").Append(value).Append("%'");
            }
            else
            {
                queryPart.Append(value).Append(" ");
            }
            return queryPart.ToString();
        }
        void validation()
        {
            var testTown = QueryConst.towns;
            // check if field exist in schema
                if (!testTown.ContainsKey(field))
                {
                    throw new Exception("Field '" + field + "' in WHERE Clause do not exist in 'Town' schema");
                }
            // check if the type of field is integer or float and the value is int or float  when the predicate is '<', '>' or '='
            if (QueryConst.predicateEq.Equals(predicate) || QueryConst.predicateGt.Equals(predicate) || QueryConst.predicateLt.Equals(predicate))
            {
                // process the validation of the part where the field is average_age by itself
                if (!field.Equals(QueryConst.average_age))
                {
                    // exception when the filed type is not integer
                    if (!testTown[field].Equals(QueryConst.integer))
                    {
                        throw new Exception("Invalide operation: the predicate '" + convertPredicate(predicate) + "' is not valide with '" + field + "' " + testTown[field] + " field");
                    }
                    else
                    {
                        // exception when the value is not integer
                        int n;
                        if (!int.TryParse(value, out n))
                        {
                            throw new Exception("Invalide operation: comparison value should be " + testTown[field] + " when predicate is '" + convertPredicate(predicate) + "' (field '" + field + "')");
                        }
                    }
                }
                else
                {
                    // exception when the filed type is not float
                    if (!testTown[field].Equals(QueryConst.flottant))
                    {
                        throw new Exception("Invalide operation: the predicate '" + convertPredicate(predicate) + "' is not valide with '" + field + "' " + testTown[field] + " field");
                    }
                    else
                    {
                        // exception when the value is not float
                        float number;
                        var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                        ci.NumberFormat.NumberDecimalSeparator = ",";

                        if (!Single.TryParse(value, NumberStyles.Float, ci, out number))
                        {
                            ci.NumberFormat.NumberDecimalSeparator = ".";
                            if (!Single.TryParse(value, NumberStyles.Float, ci, out number))
                            {
                                throw new Exception("Invalide operation: comparison value should be " + testTown[field] + " when predicate is '" + convertPredicate(predicate) + "' (field '" + field + "')");
                            }
                        }

                    }
                }
            }
            // check if the type of field is varchar and the value is string when the predicate is LIKE
            else if (QueryConst.predicateContains.Equals(predicate))
            {
                // exception when the filed type is not varchar
                if (!testTown[field].Equals(QueryConst.varchar))
                {
                    throw new Exception("Invalide operation: the predicate '" + convertPredicate(predicate) + "' is not valide with '" + field + "' " + testTown[field] + " field");
                }
                else
                {
                    // exception when the value is not varchar
                    var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                    ci.NumberFormat.NumberDecimalSeparator = ",";
                    var cl = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                    cl.NumberFormat.NumberDecimalSeparator = ".";
                    int n;
                    float number;
                    if (int.TryParse(value, out n) || Single.TryParse(value, NumberStyles.Float, ci, out number) || Single.TryParse(value, NumberStyles.Float, cl, out number))
                    {
                        throw new Exception("Invalide operation: comparison value should be " + testTown[field] + " when predicate is '" + convertPredicate(predicate) + "' (field '" + field + "')");
                    }
                }
            }
            else
            {
                throw new Exception("Invalide operation: Only the folowing predicate are authorized: eq,gt,lt,contains");
            }
        }
        internal string convertPredicate(String predicate)
        {
            if (QueryConst.predicateEq.Equals(predicate))
            {
                return QueryConst.sqlEq;
            }

            if (QueryConst.predicateGt.Equals(predicate))
            {
                return QueryConst.sqlGt;
            }

            if (QueryConst.predicateLt.Equals(predicate))
            {
                return QueryConst.sqlLt;
            }

            if (QueryConst.predicateContains.Equals(predicate))
            {
                return QueryConst.sqlLike;
            }

            return null;
        }
    }

    public static class QueryConst
    {

        public static readonly string and = "and";
        public static readonly string or = "or";
        public static readonly string filter = "filter";
        public static readonly string field = "field";
        public static readonly string value = "value";
        public static readonly string predicate = "predicate";
        public static readonly string fields = "fields";
        public static readonly string filters = "filters";

        public static readonly string select = "SELECT";
        public static readonly string from = "FROM towns";
        public static readonly string where = "WHERE";

        public static readonly string predicateEq = "eq";
        public static readonly string predicateGt = "gt";
        public static readonly string predicateLt = "lt";
        public static readonly string predicateContains = "contains";

        public static readonly string sqlEq = "=";
        public static readonly string sqlGt = ">";
        public static readonly string sqlLt = "<";
        public static readonly string sqlLike = "LIKE";

        public static readonly string integer = "integer";
        public static readonly string varchar = "varchar";
        public static readonly string flottant = "float";

        public static readonly string average_age = "average_age";

        public static readonly Dictionary<string, string> towns = new Dictionary<string, string>()
        {
            { "id", "integer" },
            { "code", "integer" },
            { "name", "varchar" },
            { "population", "integer" },
            { "average_age", "float" },
            { "district_code", "integer" },
            { "department_code", "varchar" },
            { "region_code", "integer" },
            { "region_name", "varchar" },

        };
    }
}
