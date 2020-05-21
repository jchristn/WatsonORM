using System;
using System.Collections.Generic;
using System.Text; 

namespace Watson.ORM.Core
{
    /// <summary>
    /// Links a class property to a column in a WatsonORM-managed database table.
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Name of the column.
        /// </summary>
        public string Name 
        { 
            get
            {
                return _Name;
            }
        }

        /// <summary>
        /// Indicates whether or not the column is a primary key.
        /// </summary>
        public bool PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
        }

        /// <summary>
        /// The data type of the column.
        /// </summary>
        public DataTypes Type
        {
            get
            {
                return _Type;
            }
        }

        /// <summary>
        /// The maximum length of the column.
        /// </summary>
        public int? MaxLength
        {
            get
            {
                return _MaxLength;
            }
        }

        /// <summary>
        /// The decimal precision of the column.
        /// </summary>
        public int? Precision
        {
            get
            {
                return _Precision;
            }
        }

        /// <summary>
        /// Indicates whether or not the column allows a null value.
        /// </summary>
        public bool Nullable
        {
            get
            {
                return _Nullable;
            }
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param> 
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(string name, bool primaryKey, DataTypes dataType, bool isNullable)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");

            _Name = name;
            _PrimaryKey = primaryKey;
            _Type = dataType; 
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="precision">The decimal precision of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(string name, bool primaryKey, DataTypes dataType, int maxLength, int precision, bool isNullable)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");

            _Name = name;
            _PrimaryKey = primaryKey;
            _Type = dataType;
            _MaxLength = maxLength;
            _Precision = precision;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(string name, bool primaryKey, DataTypes dataType, int maxLength, bool isNullable)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");

            _Name = name;
            _PrimaryKey = primaryKey;
            _Type = dataType;
            _MaxLength = maxLength; 
            _Nullable = isNullable;
        }

        private string _Name = null;
        private bool _PrimaryKey = false;
        private DataTypes _Type = DataTypes.Nvarchar;
        private int? _MaxLength = 64;
        private int? _Precision = null;
        private bool _Nullable = true;
    }
}
