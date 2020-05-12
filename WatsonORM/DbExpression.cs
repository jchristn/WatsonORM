using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM
{
    /// <summary>
    /// Boolean expression.
    /// </summary>
    public class DbExpression
    {
        /// <summary>
        /// A structure in the form of term-operator-term that defines a boolean operation within a WHERE clause.
        /// </summary>
        public DbExpression()
        {
        }

        /// <summary>
        /// A structure in the form of term-operator-term that defines a boolean operation within a WHERE clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested Expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested Expression.</param>
        public DbExpression(object left, DbOperators oper, object right)
        {
            LeftTerm = left;
            Operator = oper;
            RightTerm = right;
        }

        /// <summary>
        /// The left term of the expression; can either be a string term or a nested Expression.
        /// </summary>
        public object LeftTerm;

        /// <summary>
        /// The operator.
        /// </summary>
        public DbOperators Operator;

        /// <summary>
        /// The right term of the expression; can either be an object for comparison or a nested Expression.
        /// </summary>
        public object RightTerm; 
    }
}
