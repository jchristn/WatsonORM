﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DatabaseWrapper.Core;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Methods to assist 
    /// </summary>
    public static class ReflectionHelper
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

        /// <summary>
        /// Determine if an object has an attribute.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="attr">Attribute.</param>
        /// <returns>True if the attribute exists.</returns>
        public static bool HasAttribute(object obj, Attribute attr)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            Attribute[] attrs = Attribute.GetCustomAttributes(obj.GetType(), attr.GetType());
            if (attrs != null && attrs.Length > 0) return true;
            return false;
        }

        /// <summary>
        /// Get database table name from a given type.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Database table name.</returns>
        public static string GetTableNameFromType(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            TableAttribute attr = (TableAttribute)Attribute.GetCustomAttribute(t, typeof(TableAttribute));
            if (attr != null) return attr.TableName;
            return null;
        }

        /// <summary>
        /// Get database table name from a given object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Database table name.</returns>
        public static string GetTableNameFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetTableNameFromType(obj.GetType());
        }

        /// <summary>
        /// Get list of columns from a given type.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>List of Column.</returns>
        public static List<DatabaseWrapper.Core.Column> GetColumnsFromType(Type t)
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
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

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

        /// <summary>
        /// Get list of columns from a given object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>List of Column.</returns>
        public static List<DatabaseWrapper.Core.Column> GetColumnsFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetColumnsFromType(obj.GetType());
        }

        /// <summary>
        /// Get table column for a given property.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Column.</returns>
        public static DatabaseWrapper.Core.Column GetColumnForProperty(Type t)
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
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

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

        /// <summary>
        /// Get table column for a given property by name.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column.</returns>
        public static DatabaseWrapper.Core.Column GetColumnForProperty<T>(string propName)
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
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

                        DatabaseWrapper.Core.Column col = new DatabaseWrapper.Core.Column(
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

        /// <summary>
        /// Get table column name for a given property name.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column name.</returns>
        public static string GetColumnNameForPropertyName<T>(string propName)
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
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

                        return colAttr.Name;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get property name from a given column name.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <param name="columnName">Column name.</param>
        /// <returns>Property name.</returns>
        public static string GetPropertyNameFromColumnName(Type t, string columnName)
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
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

                        if (colAttr.Name.Equals(columnName)) return prop.Name;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get primary key column name for a given type.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Primary key column name.</returns>
        public static string GetPrimaryKeyColumnName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null && colAttr.PrimaryKey)
                    {
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

                        return colAttr.Name;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get property name of the primary key column.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Property name.</returns>
        public static string GetPrimaryKeyPropertyName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null && colAttr.PrimaryKey)
                    {
                        if (String.IsNullOrEmpty(colAttr.Name)) colAttr.Name = prop.Name;

                        return prop.Name;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walk an object using reflection and display its properties on the console.
        /// </summary>
        /// <param name="obj">Object.</param>
        public static void WalkObject(object obj)
        {
            Console.WriteLine("");
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Console.WriteLine("  " + prop.Name + ": " + prop.GetValue(obj) + " [" + prop.PropertyType.ToString() + "]");
            }
            Console.WriteLine("");
        }

        /// <summary>
        /// Retrieve a property value by property name.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="propName">Property name.</param>
        /// <returns>Property value.</returns>
        public static object GetPropertyValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }
         
        private static DataTypeEnum DbTypeConverter(DataTypes dt)
        {
            switch (dt)
            {
                case DataTypes.Boolean:
                    return DataTypeEnum.TinyInt;
                case DataTypes.Enum:
                    return DataTypeEnum.Nvarchar;
                case DataTypes.Varchar:
                    return DataTypeEnum.Varchar;
                case DataTypes.Nvarchar:
                    return DataTypeEnum.Nvarchar;
                case DataTypes.TinyInt:
                    return DataTypeEnum.TinyInt;
                case DataTypes.Int:
                    return DataTypeEnum.Int;
                case DataTypes.Long:
                    return DataTypeEnum.Long;
                case DataTypes.Decimal:
                    return DataTypeEnum.Decimal;
                case DataTypes.Double:
                    return DataTypeEnum.Double;
                case DataTypes.DateTime:
                    return DataTypeEnum.DateTime;
                case DataTypes.DateTimeOffset:
                    return DataTypeEnum.DateTimeOffset;
                case DataTypes.Blob:
                    return DataTypeEnum.Blob;
                case DataTypes.Guid:
                    return DataTypeEnum.Nvarchar;
                default:
                    throw new ArgumentException("Unknown data type '" + dt.ToString() + "'.");
            }
        }
    }
}
