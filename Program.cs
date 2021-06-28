using System;
using System.Collections.Generic;
using System.Xml;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // upload the xml file
            string xmlPath = null;
            if (args != null && args.Length == 1)
            {
                xmlPath = args[0];
                if (xmlPath != null && xmlPath.Length > 0)
                {
                    // show the help commande
                    if (xmlPath.Equals("help"))
                    {
                        System.Console.WriteLine("Commande example:  ");
                        System.Console.WriteLine("DSLQuery.exe C:/app/sample.xml");
                    }
                    else
                    {
                        Query query = null;
                        try
                        {
                            // load xml query file to an object
                            query = loadQuery(xmlPath);
                        }
                        catch (Exception e)
                        {
                            // exception xml error
                            Console.WriteLine("XML Validation Error: " + e.Message);
                        }
                        try
                        {
                            if (query != null)
                            {
                                // convert from query object to string
                                Console.WriteLine(query.toSqlString());
                            }
                            else
                            {
                                Console.WriteLine("SQL Validation Error: The query is empty");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("SQL Validation Error: " + e.Message);
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine("the path is empty or there are many path");
                }
            }
            else
            {
                System.Console.WriteLine("Error: The input arguments must be one");
            }




        }

        static Query loadQuery(String xmlPath)
        {
            Query query = new Query();
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            // retrieve the child nodes
            XmlNodeList childNodes = doc.DocumentElement.ChildNodes;
            if (childNodes.Count > 0)
            {
                var queryFields = new List<string>();
                var filter = new Filter();
                // iterate the child nodes 
                foreach (XmlNode node in childNodes)
                {
                    // retrieve the fields
                    string text = node.Name;
                    if (text.Equals("fields"))
                    {
                        XmlNodeList fieldsNodes = node.ChildNodes;
                        if (fieldsNodes != null && fieldsNodes.Count > 0)
                        {
                            foreach (XmlNode fieldsNode in fieldsNodes)
                            {
                                string fieldName = fieldsNode.InnerText;
                                if (fieldName != null && fieldName.Length > 0)
                                {
                                    queryFields.Add(fieldName);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Field found with empty name");
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No Fields found in xml");
                        }
                    }
                    else if (text.Equals("filters"))
                    {
                        // retrieve the filters
                        XmlNodeList filtersNodes = node.ChildNodes;
                        if (filtersNodes != null && filtersNodes.Count > 0)
                        {
                            XmlNode firstNode = filtersNodes.Item(0);
                            string nodeName = firstNode.Name;
                            if (filtersNodes.Count == 1)
                            {

                                if (nodeName != null && nodeName.Length > 0)
                                {
                                    if (nodeName.Equals(QueryConst.and) || nodeName.Equals(QueryConst.or))
                                    {
                                        filter = initFilters(node);
                                    }
                                    else
                                    {

                                        throw new InvalidOperationException(" XML tag '" + nodeName + "' is not authorized, only 'and' and 'or' logical operator are acceptable in filters node");
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Empty Filter node in xml");
                                }
                            }
                            else
                            {
                                if (nodeName.Equals(QueryConst.field) || nodeName.Equals(QueryConst.value) || nodeName.Equals(QueryConst.predicate))
                                {
                                    filter = initFilters(node);
                                }
                                else
                                {
                                    throw new InvalidOperationException(" It is mandatory to have only the following xml child node under 'filters', this child must be 'and' , 'or', 'field', 'value' or 'predicate' xml tag");
                                }

                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No Filters found in xml");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(" XML tag '" + text + "' is not authorized");
                    }
                }
                query.Fields = queryFields;
                query.Filter = filter;
            }
            else
            {
                throw new InvalidOperationException("Empty Query element in xml");
            }
            return query;
        }
        // recursive function return filter object from xml node
        private static Filter initFilters(XmlNode filterNode)
        {
            var filter = new Filter();
            XmlNodeList nodes = filterNode.ChildNodes;
            if (nodes != null && nodes.Count > 0)
            {
                if (nodes.Count == 1)
                {
                    XmlNode firstNode = nodes.Item(0);
                    string nodeName = firstNode.Name;
                    if (nodeName != null && nodeName.Length > 0)
                    {
                        if (nodeName.Equals(QueryConst.and) || nodeName.Equals(QueryConst.or))
                        {
                            if (nodeName.Equals(QueryConst.and))
                            {
                                filter.LogicalOp = QueryConst.and;
                            }
                            else if (nodeName.Equals(QueryConst.or))
                            {
                                filter.LogicalOp = QueryConst.or;
                            }

                            XmlNodeList fNodes = firstNode.ChildNodes;
                            if (fNodes != null && fNodes.Count == 2)
                            {
                                var filter1 = initFilters(fNodes.Item(0));
                                var filter2 = initFilters(fNodes.Item(1));
                                filter.Filters1 = filter1;
                                filter.Filters2 = filter2;
                            }
                            else
                            {
                                throw new InvalidOperationException("It is mandatory to have two 'filter' nodes after 'and / 'or' logical operator");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(" XML tag '" + nodeName + "' is not authorized, only 'and' and 'or' logical operator are acceptable in filters node");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Empty Filter node in xml");
                    }
                }
                else
                {
                    var singleFilter = new SingleFilter();
                    foreach (XmlNode n in nodes)
                    {
                        if (n != null && QueryConst.field.Equals(n.Name))
                        {
                            singleFilter.field = n.InnerText;
                        }

                        if (n != null && QueryConst.value.Equals(n.Name))
                        {
                            singleFilter.value = n.InnerText;
                        }

                        if (n != null && QueryConst.predicate.Equals(n.Name))
                        {
                            singleFilter.predicate = n.InnerText;
                        }
                    }
                    if (singleFilter.field == null || singleFilter.field.Length == 0)
                    {
                        throw new InvalidOperationException("Missing field attribute in 'filter' node");
                    }

                    if (singleFilter.value == null || singleFilter.value.Length == 0)
                    {
                        throw new InvalidOperationException("Missing value attribute in 'filter' node");
                    }

                    filter.SingleFilter = singleFilter;
                }
            }
            return filter;
        }
    }
}
