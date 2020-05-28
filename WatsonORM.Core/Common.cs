using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using DatabaseWrapper.Core;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Common methods shared amongst WatsonORM modules.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Convert a WatsonORM DbOperators to a DatabaseWrapper Core Operators.
        /// </summary>
        /// <param name="oper">WatsonORM DbOperators.</param>
        /// <returns>DatabaseWrapper Core Operators.</returns>
        public static Operators DbOperatorsConverter(DbOperators oper)
        {
            switch (oper)
            {
                case DbOperators.And:
                    return Operators.And;
                case DbOperators.Or:
                    return Operators.Or;
                case DbOperators.Equals:
                    return Operators.Equals;
                case DbOperators.NotEquals:
                    return Operators.NotEquals;
                case DbOperators.In:
                    return Operators.In;
                case DbOperators.NotIn:
                    return Operators.NotIn;
                case DbOperators.Contains:
                    return Operators.Contains;
                case DbOperators.ContainsNot:
                    return Operators.ContainsNot;
                case DbOperators.StartsWith:
                    return Operators.StartsWith;
                case DbOperators.EndsWith:
                    return Operators.EndsWith;
                case DbOperators.GreaterThan:
                    return Operators.GreaterThan;
                case DbOperators.GreaterThanOrEqualTo:
                    return Operators.GreaterThanOrEqualTo;
                case DbOperators.LessThan:
                    return Operators.LessThan;
                case DbOperators.LessThanOrEqualTo:
                    return Operators.LessThanOrEqualTo;
                case DbOperators.IsNull:
                    return Operators.IsNull;
                case DbOperators.IsNotNull:
                    return Operators.IsNotNull;
                default:
                    throw new ArgumentException("Unknown operator '" + oper.ToString() + "'.");
            }
        }

        /// <summary>
        /// Convert a WatsonORM DbExpression to a DatabaseWrapper Expression.
        /// </summary>
        /// <param name="expr">WatsonORM DbExpression.</param>
        /// <returns>DatabaseWrapper Core Expression.</returns>
        public static Expression DbExpressionConverter(DbExpression expr)
        {
            if (expr == null) return null;

            return new Expression(
                (expr.LeftTerm is DbExpression ? DbExpressionConverter((DbExpression)expr.LeftTerm) : expr.LeftTerm),
                DbOperatorsConverter(expr.Operator),
                (expr.RightTerm is DbExpression ? DbExpressionConverter((DbExpression)expr.RightTerm) : expr.RightTerm));
        } 
    }
}
