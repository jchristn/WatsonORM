using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DatabaseWrapper.Core;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Type metadata manager for WatsonORM.
    /// </summary>
    public class TypeMetadataManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly object _MetadataLock = new object();
        private Dictionary<Type, TypeMetadata> _Metadata = new Dictionary<Type, TypeMetadata>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TypeMetadataManager()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add a TypeMetadata entry for a given Type.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <param name="md">TypeMetadata.</param>
        public void Add(Type t, TypeMetadata md)
        {
            lock (_MetadataLock)
            {
                _Metadata.Add(t, md);
            }
        }

        /// <summary>
        /// Get TypeMetadata for a given Type, or throw an InvalidOperationException.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>TypeMetadata.</returns>
        public TypeMetadata GetTypeMetadata(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            lock (_MetadataLock)
            {
                if (_Metadata.ContainsKey(t)) return _Metadata[t];
            }

            throw new InvalidOperationException("Type '" + t.Name + "' has not been initialized into the ORM.");
        }

        /// <summary>
        /// Get the database table name from a given Type, or throw an InvalidOperationException.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Database table name.</returns>
        public string GetTableNameFromType(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);

            return md.TableName;
        }

        /// <summary>
        /// Get the table name from a given object, or throw an InvalidOperationException.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Database table name.</returns>
        public string GetTableNameFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetTableNameFromType(obj.GetType());
        }

        /// <summary>
        /// Get the primary key column name from a given type, or throw an InvalidOperationException.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Primary key column name.</returns>
        public string GetPrimaryKeyColumnName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);

            foreach (Column col in md.Columns)
            {
                if (col.PrimaryKey) return col.Name;
            }

            throw new InvalidOperationException("Type '" + t.Name + "' does not have a property with the 'PrimaryKey' attribute.");
        }

        /// <summary>
        /// Get the primary key property name from a given Type, or throw an InvalidOperationException.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Primary key property name.</returns>
        public string GetPrimaryKeyPropertyName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);

            return md.PrimaryKeyPropertyName;
        }

        /// <summary>
        /// Get the primary key value from a given object given the object and the property name.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="propName">Property name.</param>
        /// <returns>Value.</returns>
        public object GetPrimaryKeyValue(object obj, string propName)
        {
            object val = GetPropertyValue(obj, propName);
            if (val == null) throw new InvalidOperationException("Property '" + propName + "' cannot be null if decorated as a primary key.");
            return val;
        }

        /// <summary>
        /// Get the value of a given property from an object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="propName">Property name.</param>
        /// <returns>Value.</returns>
        public object GetPropertyValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }

        /// <summary>
        /// Get the database table column name for a given property name.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column name.</returns>
        public string GetColumnNameForPropertyName<T>(string propName)
        {
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name.Equals(propName))
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
            }

            throw new ArgumentException("No 'Column' attribute found for property name '" + propName + "'.");
        }

        /// <summary>
        /// Get the property name from the database table column name.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <param name="columnName">Column name.</param>
        /// <returns>Property name.</returns>
        public string GetPropertyNameFromColumnName(Type t, string columnName)
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

            throw new ArgumentException("No property found for column name '" + columnName + "'.");
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
