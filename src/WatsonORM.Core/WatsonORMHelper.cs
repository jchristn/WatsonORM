using System;
using System.Collections.Generic;
using System.Text;
using ExpressionTree;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Helper methods for Watson ORM.
    /// </summary>
    public static class WatsonORMHelper
    {
        #region Public-Methods

        /// <summary>
        /// Preprocess an expression, primarily to convert Boolean to integer.
        /// </summary>
        /// <param name="expr">Expression.</param>
        /// <returns>Expression.</returns>
        public static Expr PreprocessExpression(Expr expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            Expr ret = new Expr();

            if (expr.Left is Expr)
            {
                ret.Left = PreprocessExpression((Expr)expr.Left);
            }
            else if (expr.Left is bool)
            {
                if ((bool)expr.Left)
                {
                    ret.Left = 1;
                }
                else
                {
                    ret.Left = 0;
                }
            }
            else
            {
                ret.Left = expr.Left;
            }

            ret.Operator = expr.Operator;

            if (expr.Right is Expr)
            {
                ret.Right = PreprocessExpression((Expr)expr.Right);
            }
            else if (expr.Right is bool)
            {
                if ((bool)expr.Right)
                {
                    ret.Right = 1;
                }
                else
                {
                    ret.Right = 0;
                }
            }
            else
            {
                ret.Right = expr.Right;
            }

            return ret;
        }

        #endregion
    }
}
