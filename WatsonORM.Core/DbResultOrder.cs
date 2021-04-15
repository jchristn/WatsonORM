using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseWrapper.Core;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Describe on which columns and in which direction results should be ordered.
    /// </summary>
    public class DbResultOrder
    {
        #region Public-Members

        /// <summary>
        /// Column name on which to order results.
        /// </summary>
        public string ColumnName { get; set; } = null;

        /// <summary>
        /// Direction by which results should be returned.
        /// </summary>
        public DbOrderDirection Direction { get; set; } = DbOrderDirection.Ascending;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DbResultOrder()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="columnName">Column name on which to order results.</param>
        /// <param name="direction">Direction by which results should be returned.</param>
        public DbResultOrder(string columnName, DbOrderDirection direction)
        {
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));
            ColumnName = columnName;
            Direction = direction;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Convert a WatsonORM DbResultOrder[] to a DatabaseWrapper ResultOrder[].
        /// </summary>
        /// <param name="resultOrder">DbResultOrder array.</param>
        /// <returns>ResultOrder array.</returns>
        public static ResultOrder[] ConvertToResultOrder(DbResultOrder[] resultOrder)
        {
            if (resultOrder == null || resultOrder.Length < 1) return null;
            ResultOrder[] ret = new ResultOrder[resultOrder.Length];
            for (int i = 0; i < resultOrder.Length; i++)
            {
                if (resultOrder[i].Direction == DbOrderDirection.Ascending)
                {
                    ret[i] = new ResultOrder(resultOrder[i].ColumnName, OrderDirection.Ascending);
                }
                else if (resultOrder[i].Direction == DbOrderDirection.Descending)
                {
                    ret[i] = new ResultOrder(resultOrder[i].ColumnName, OrderDirection.Descending);
                }
            }
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
