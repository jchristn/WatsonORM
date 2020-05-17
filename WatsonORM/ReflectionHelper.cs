using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DatabaseWrapper;

namespace Watson.ORM
{
    internal static class ReflectionHelper
    {
        /*
         * These methods do not rely on internal caching like the methods in WatsonORM.cs do.
         * Many of these are unused but remain as reference because they may be useful later.
         * 
         * Some helpful links:
         * 
         * https://docs.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes
         * https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/attributes
         * https://stackoverflow.com/questions/6637679/reflection-get-attribute-name-and-value-on-property
         * https://stackoverflow.com/questions/1226161/test-if-a-class-has-an-attribute
         * https://stackoverflow.com/questions/61723748/retrieve-name-and-value-of-property-has-a-specific-attribute-value
         * 
         */

        internal static bool HasAttribute(object obj, Attribute attr)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            Attribute[] attrs = Attribute.GetCustomAttributes(obj.GetType(), attr.GetType());
            if (attrs != null && attrs.Length > 0) return true;
            return false;
        }

        internal static string GetTableNameFromType(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            TableAttribute attr = (TableAttribute)Attribute.GetCustomAttribute(t, typeof(TableAttribute));
            if (attr != null) return attr.TableName;
            return null;
        }

        internal static string GetTableNameFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetTableNameFromType(obj.GetType());
        }

        internal static List<Column> GetColumnsFromType(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            
            List<Column> ret = new List<Column>();
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        Column col = new Column(
                            colAttr.Name,
                            colAttr.PrimaryKey,
                            DbTypeConverter(colAttr.Type),
                            colAttr.MaxLength,
                            colAttr.Precision,
                            colAttr.Nullable);

                        ret.Add(col);
                    }
                }
            }
             
            return ret;
        }

        internal static List<Column> GetColumnsFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetColumnsFromType(obj.GetType());
        }

        internal static Column GetColumnForProperty(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        Column col = new Column(
                            colAttr.Name,
                            colAttr.PrimaryKey,
                            DbTypeConverter(colAttr.Type),
                            colAttr.MaxLength,
                            colAttr.Precision,
                            colAttr.Nullable);

                        return col;
                    }
                }
            }

            return null;
        }

        internal static Column GetColumnForProperty<T>(string propName)
        {
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        Column col = new Column(
                            colAttr.Name,
                            colAttr.PrimaryKey,
                            DbTypeConverter(colAttr.Type),
                            colAttr.MaxLength,
                            colAttr.Precision,
                            colAttr.Nullable);

                        return col;
                    }
                }
            }

            return null;
        }

        internal static string GetColumnNameForPropertyName<T>(string propName)
        {
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        return colAttr.Name;
                    }
                }
            }

            return null;
        }

        internal static string GetPropertyNameFromColumnName(Type t, string columnName)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    { 
                        if (colAttr.Name.Equals(columnName)) return prop.Name;
                    }
                }
            }

            return null;
        }
          
        internal static string GetPrimaryKeyColumnName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null && colAttr.PrimaryKey) return colAttr.Name;
                }
            }

            return null;
        }

        internal static string GetPrimaryKeyPropertyName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null && colAttr.PrimaryKey) return prop.Name;
                }
            }

            return null;
        }

        internal static void WalkObject(object obj)
        {
            Console.WriteLine("");
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Console.WriteLine("  " + prop.Name + ": " + prop.GetValue(obj) + " [" + prop.PropertyType.ToString() + "]");
            }
            Console.WriteLine("");
        }

        internal static object GetPropertyValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }
         
        private static DataType DbTypeConverter(DataTypes dt)
        {
            switch (dt)
            {
                case DataTypes.Boolean:
                    return DataType.Int;
                case DataTypes.Enum:
                    return DataType.Nvarchar;
                case DataTypes.Varchar:
                    return DataType.Varchar;
                case DataTypes.Nvarchar:
                    return DataType.Nvarchar;
                case DataTypes.Int:
                    return DataType.Int;
                case DataTypes.Long:
                    return DataType.Long;
                case DataTypes.Decimal:
                    return DataType.Decimal;
                case DataTypes.Double:
                    return DataType.Double;
                case DataTypes.DateTime:
                    return DataType.DateTime;
                case DataTypes.Blob:
                    return DataType.Blob;
                default:
                    throw new ArgumentException("Unknown data type '" + dt.ToString() + "'.");
            }
        }
    }
}
