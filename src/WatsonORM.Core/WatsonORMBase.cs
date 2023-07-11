using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper.Core;
using ExpressionTree;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Base class for WatsonORM.
    /// </summary>
    public abstract class WatsonORMBase
    {
        #region Public-Methods

        /// <summary>
        /// Initialize the database client.
        /// If the client is already initialized, it will first be disposed.
        /// </summary>
        public abstract void InitializeDatabase();

        /// <summary>
        /// Validate a series of tables to determine if any errors or warnings exist.
        /// </summary>
        /// <param name="types">List of classes for which tables should be validated.</param>
        /// <param name="errors">List of human-readable errors.</param>
        /// <param name="warnings">List of human-readable warnings.</param>
        /// <returns>True if the table will initialize successfully.</returns>
        public abstract bool ValidateTables(List<Type> types, out List<string> errors, out List<string> warnings);

        /// <summary>
        /// Validate a table to determine if any errors or warnings exist.
        /// </summary>
        /// <param name="t">Class for which a table should be validated.</param>
        /// <param name="errors">List of human-readable errors.</param>
        /// <param name="warnings">List of human-readable warnings.</param>
        /// <returns>True if the table will initialize successfully.</returns>
        public abstract bool ValidateTable(Type t, out List<string> errors, out List<string> warnings);

        /// <summary>
        /// Create tables (if they don't exist) for a given set of classes.
        /// </summary>
        /// <param name="types">List of classes for which a table should be created.</param>
        public abstract void InitializeTables(List<Type> types);

        /// <summary>
        /// Create table (if it doesn't exist) for a given class.
        /// Adding a table that has already been added will throw an ArgumentException.
        /// </summary>
        /// <param name="t">Class for which a table should be created.</param>
        public abstract void InitializeTable(Type t);

        /// <summary>
        /// Drop table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public abstract void DropTable(Type t);

        /// <summary>
        /// Drop table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task DropTableAsync(Type t, CancellationToken token = default);

        /// <summary>
        /// Truncate table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public abstract void TruncateTable(Type t);

        /// <summary>
        /// Truncate table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task TruncateTableAsync(Type t, CancellationToken token = default);

        /// <summary>
        /// INSERT an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to INSERT.</param>
        /// <returns>INSERTed object.</returns>
        public abstract T Insert<T>(T obj) where T : class, new();

        /// <summary>
        /// INSERT an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to INSERT.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>INSERTed object.</returns>
        public abstract Task<T> InsertAsync<T>(T obj, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// INSERT multiple records.
        /// This operation will iteratively call Insert on each individual object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        /// <returns>List of objects.</returns>
        public abstract List<T> InsertMany<T>(List<T> objs) where T : class, new();

        /// <summary>
        /// INSERT multiple records.
        /// This operation will iteratively call Insert on each individual object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>List of objects.</returns>
        public abstract Task<List<T>> InsertManyAsync<T>(List<T> objs, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// INSERT multiple records.
        /// This operation performs the INSERT using a single database query, and does not return a list of inserted objects.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        public abstract void InsertMultiple<T>(List<T> objs) where T : class, new();

        /// <summary>
        /// INSERT multiple records.
        /// This operation performs the INSERT using a single database query, and does not return a list of inserted objects.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task InsertMultipleAsync<T>(List<T> objs, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// UPDATE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to UPDATE.</param>
        /// <returns>UPDATEd object.</returns>
        public abstract T Update<T>(T obj) where T : class, new();

        /// <summary>
        /// UPDATE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to UPDATE.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>UPDATEd object.</returns>
        public abstract Task<T> UpdateAsync<T>(T obj, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// UPDATE multiple rows.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="updateVals">Update values.</param>
        public abstract void UpdateMany<T>(Expr expr, Dictionary<string, object> updateVals) where T : class, new();

        /// <summary>
        /// UPDATE multiple rows.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="updateVals">Update values.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task UpdateManyAsync<T>(Expr expr, Dictionary<string, object> updateVals, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to DELETE.</param>
        public abstract void Delete<T>(T obj) where T : class, new();

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to DELETE.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task DeleteAsync<T>(T obj, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// DELETE an object by its primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        public abstract void DeleteByPrimaryKey<T>(object id) where T : class, new();

        /// <summary>
        /// DELETE an object by its primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task DeleteByPrimaryKeyAsync<T>(object id, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        public abstract void DeleteMany<T>(Expr expr) where T : class, new();

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract Task DeleteManyAsync<T>(Expr expr, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// SELECT an object by id.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <returns>Object.</returns>
        public abstract T SelectByPrimaryKey<T>(object id) where T : class, new();

        /// <summary>
        /// SELECT an object by id.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Object.</returns>
        public abstract Task<T> SelectByPrimaryKeyAsync<T>(object id, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// SELECT the first instance of an object matching a given expression.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="expr">Expression by which SELECT should be filtered (i.e. WHERE clause).</param> 
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>Object.</returns>
        public abstract T SelectFirst<T>(Expr expr, ResultOrder[] ro = null) where T : class, new();

        /// <summary>
        /// SELECT the first instance of an object matching a given expression.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="expr">Expression by which SELECT should be filtered (i.e. WHERE clause).</param> 
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Object.</returns>
        public abstract Task<T> SelectFirstAsync<T>(Expr expr, ResultOrder[] ro = null, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// SELECT multiple rows.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>List of objects.</returns>
        public abstract List<T> SelectMany<T>(Expr expr = null, ResultOrder[] ro = null) where T : class, new();

        /// <summary>
        /// SELECT multiple rows.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>List of objects.</returns>
        public abstract Task<List<T>> SelectManyAsync<T>(Expr expr = null, ResultOrder[] ro = null, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// SELECT multiple rows with pagination.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>List of objects.</returns>
        public abstract List<T> SelectMany<T>(int? indexStart, int? maxResults, Expr expr = null, ResultOrder[] ro = null) where T : class, new();

        /// <summary>
        /// SELECT multiple rows with pagination.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>List of objects.</returns>
        public abstract Task<List<T>> SelectManyAsync<T>(int? indexStart, int? maxResults, Expr expr = null, ResultOrder[] ro = null, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// Retrieve the table name for a given type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Table name.</returns>
        public abstract string GetTableName(Type type);

        /// <summary>
        /// Retrieve the column name for a given property.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column name.</returns>
        public abstract string GetColumnName<T>(string propName) where T : class, new();

        /// <summary>
        /// Check if objects of a given type exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>True if exists.</returns>
        public abstract bool Exists<T>(Expr expr);

        /// <summary>
        /// Check if objects of a given type exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public abstract Task<bool> ExistsAsync<T>(Expr expr, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// Determine the number of objects of a given type that exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>Number of matching records.</returns>
        public abstract long Count<T>(Expr expr) where T : class, new();

        /// <summary>
        /// Determine the number of objects of a given type that exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Number of matching records.</returns>
        public abstract Task<long> CountAsync<T>(Expr expr, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// Add the contents of the specified column from objects that match the supplied expression.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="columnName"></param>
        /// <param name="expr">Expression.</param>
        /// <returns></returns>
        public abstract decimal Sum<T>(string columnName, Expr expr) where T : class, new();

        /// <summary>
        /// Add the contents of the specified column from objects that match the supplied expression.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="columnName"></param>
        /// <param name="expr">Expression.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public abstract Task<decimal> SumAsync<T>(string columnName, Expr expr, CancellationToken token = default) where T : class, new();

        /// <summary>
        /// Execute a query directly against the database.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns>DataTable.</returns>
        public abstract DataTable Query(string query);

        /// <summary>
        /// Execute a query directly against the database.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>DataTable.</returns>
        public abstract Task<DataTable> QueryAsync(string query, CancellationToken token = default);

        /// <summary>
        /// Retrieve a timestamp formatted for the database.
        /// </summary>
        /// <param name="dt">DateTime.</param>
        /// <returns>Formatted DateTime string.</returns>
        public abstract string Timestamp(DateTime dt);

        /// <summary>
        /// Retrieve a timestamp with offset formatted for the database.
        /// </summary>
        /// <param name="dt">DateTimeOffset.</param>
        /// <returns>Formatted DateTime string.</returns>
        public abstract string TimestampOffset(DateTimeOffset dt);

        /// <summary>
        /// Returns a sanitized string useful in a raw query.
        /// If null is supplied, null is returned.
        /// </summary>
        /// <param name="str">String.</param>
        /// <returns>String.</returns>
        public abstract string Sanitize(string str);

        #endregion
    }
}
