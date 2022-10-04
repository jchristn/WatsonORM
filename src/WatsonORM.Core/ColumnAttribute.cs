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
        #region Public-Members

        /// <summary>
        /// Name of the column.
        /// </summary>
        public string Name
        { 
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
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

        #endregion

        #region Private-Members

        private string _Name = null;
        private bool _PrimaryKey = false;
        private DataTypes _Type = DataTypes.Nvarchar;
        private int? _MaxLength = null;
        private int? _Precision = null;
        private bool _Nullable = true;

        private List<DataTypes> _LengthAndPrecisionRequired = new List<DataTypes>
        {
            DataTypes.Decimal,
            DataTypes.Double
        };

        private List<DataTypes> _LengthRequired = new List<DataTypes>
        {
            DataTypes.Nvarchar,
            DataTypes.Varchar,
            DataTypes.Enum,
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="dataType">The data type of the column.</param> 
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(DataTypes dataType, bool isNullable = true)
        {
            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes both maximum length and precision is required for columns of type " + dataType.ToString() + ".");
            }

            if (_LengthRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes a maximum length is required for columns of type " + dataType.ToString() + ".");
            }

            _PrimaryKey = false;
            _Type = dataType;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(DataTypes dataType, int maxLength, bool isNullable = true)
        {
            if (maxLength < 1) throw new ArgumentException("Column must have a maximum length greater than zero.");

            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes both maximum length and precision is required for columns of type " + dataType.ToString() + ".");
            }

            _PrimaryKey = false;
            _Type = dataType;
            _MaxLength = maxLength;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="precision">The decimal precision of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(DataTypes dataType, int maxLength, int precision, bool isNullable = true)
        {
            if (maxLength < 1) throw new ArgumentException("Column must have a maximum length greater than zero.");
            if (precision < 1) throw new ArgumentException("Column must have a precision greater than zero.");

            _PrimaryKey = false;
            _Type = dataType;
            _MaxLength = maxLength;
            _Precision = precision;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param> 
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(bool primaryKey, DataTypes dataType, bool isNullable)
        {
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");

            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes both maximum length and precision is required for columns of type " + dataType.ToString() + ".");
            }

            if (_LengthRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes a maximum length is required for columns of type " + dataType.ToString() + ".");
            }

            _PrimaryKey = primaryKey;
            _Type = dataType;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(bool primaryKey, DataTypes dataType, int maxLength, bool isNullable)
        {
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");
            if (maxLength < 1) throw new ArgumentException("Column must have a maximum length greater than zero.");

            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("A column attribute that includes both maximum length and precision is required for columns of type " + dataType.ToString() + ".");
            }

            _PrimaryKey = primaryKey;
            _Type = dataType;
            _MaxLength = maxLength;
            _Nullable = isNullable;
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="primaryKey">Indicates whether or not the column is a primary key.</param>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The maximum length of the column.</param>
        /// <param name="precision">The decimal precision of the column.</param>
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(bool primaryKey, DataTypes dataType, int maxLength, int precision, bool isNullable)
        {
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");
            if (maxLength < 1) throw new ArgumentException("Column must have a maximum length greater than zero.");
            if (precision < 1) throw new ArgumentException("Column must have a precision greater than zero.");

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
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(string name, bool primaryKey, DataTypes dataType, bool isNullable)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");

            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("For column '" + name + "', please use a column attribute that includes both maximum length and precision.");
            }

            if (_LengthRequired.Contains(dataType))
            {
                throw new InvalidOperationException("For column '" + name + "', please use a column attribute that includes a maximum length.");
            }

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
        /// <param name="isNullable">Indicates whether or not the column allows a null value.</param>
        public ColumnAttribute(string name, bool primaryKey, DataTypes dataType, int maxLength, bool isNullable)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && isNullable) throw new ArgumentException("Primary key columns cannot be nullable.");
            if (maxLength < 1) throw new ArgumentException("Column '" + name + "' must have a maximum length greater than zero.");

            if (_LengthAndPrecisionRequired.Contains(dataType))
            {
                throw new InvalidOperationException("For column '" + name + "', please use a column attribute that includes both maximum length and precision.");
            }

            _Name = name;
            _PrimaryKey = primaryKey;
            _Type = dataType;
            _MaxLength = maxLength;
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
            if (maxLength < 1) throw new ArgumentException("Column '" + name + "' must have a maximum length greater than zero.");
            if (precision < 1) throw new ArgumentException("Column '" + name + "' must have a precision greater than zero.");

            _Name = name;
            _PrimaryKey = primaryKey;
            _Type = dataType;
            _MaxLength = maxLength;
            _Precision = precision;
            _Nullable = isNullable;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
