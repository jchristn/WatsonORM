using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM.Core
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
        /// <param name="left">The left term of the expression; can either be a string term or a nested DbExpression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested DbExpression.</param>
        public DbExpression(object left, DbOperators oper, object right)
        {
            LeftTerm = left;
            Operator = oper;
            RightTerm = right;
        }

        /// <summary>
        /// The left term of the expression; can either be a string term or a nested DbExpression.
        /// </summary>
        public object LeftTerm;

        /// <summary>
        /// The operator.
        /// </summary>
        public DbOperators Operator;

        /// <summary>
        /// The right term of the expression; can either be an object for comparison or a nested DbExpression.
        /// </summary>
        public object RightTerm;
        
        /// <summary>
        /// Display DbExpression in a human-readable string.
        /// </summary>
        /// <returns>String containing human-readable version of the DbExpression.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "(";

            if (LeftTerm is DbExpression) ret += ((DbExpression)LeftTerm).ToString();
            else ret += LeftTerm.ToString();

            ret += " " + Operator.ToString() + " ";

            if (RightTerm is DbExpression) ret += ((DbExpression)RightTerm).ToString();
            else ret += RightTerm.ToString();

            ret += ")";
            return ret;
        }

        /// <summary>
        /// Prepends a new DbExpression using the supplied left term, operator, and right term using an AND clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested DbExpression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested DbExpression.</param>
        public void PrependAnd(object left, DbOperators oper, object right)
        {
            DbExpression e = new DbExpression(left, oper, right);
            PrependAnd(e);
        }

        /// <summary>
        /// Prepends the DbExpression with the supplied DbExpression using an AND clause.
        /// </summary>
        /// <param name="prepend">The DbExpression to prepend.</param> 
        public void PrependAnd(DbExpression prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            DbExpression orig = new DbExpression(this.LeftTerm, this.Operator, this.RightTerm);
            DbExpression e = PrependAndClause(prepend, orig);
            LeftTerm = e.LeftTerm;
            Operator = e.Operator;
            RightTerm = e.RightTerm;

            return;
        }

        /// <summary>
        /// Prepends a new DbExpression using the supplied left term, operator, and right term using an OR clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested DbExpression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested DbExpression.</param>
        public void PrependOr(object left, DbOperators oper, object right)
        {
            DbExpression e = new DbExpression(left, oper, right);
            PrependOr(e);
        }

        /// <summary>
        /// Prepends the DbExpression with the supplied DbExpression using an OR clause.
        /// </summary>
        /// <param name="prepend">The DbExpression to prepend.</param> 
        public void PrependOr(DbExpression prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            DbExpression orig = new DbExpression(this.LeftTerm, this.Operator, this.RightTerm);
            DbExpression e = PrependOrClause(prepend, orig);
            LeftTerm = e.LeftTerm;
            Operator = e.Operator;
            RightTerm = e.RightTerm;

            return;
        }

        /// <summary>
        /// Prepends the DbExpression in prepend to the DbExpression original using an AND clause.
        /// </summary>
        /// <param name="prepend">The DbExpression to prepend.</param>
        /// <param name="original">The original DbExpression.</param>
        /// <returns>A new DbExpression.</returns>
        public static DbExpression PrependAndClause(DbExpression prepend, DbExpression original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            DbExpression ret = new DbExpression
            {
                LeftTerm = prepend,
                Operator = DbOperators.And,
                RightTerm = original
            };
            return ret;
        }

        /// <summary>
        /// Prepends the DbExpression in prepend to the DbExpression original using an OR clause.
        /// </summary>
        /// <param name="prepend">The DbExpression to prepend.</param>
        /// <param name="original">The original DbExpression.</param>
        /// <returns>A new DbExpression.</returns>
        public static DbExpression PrependOrClause(DbExpression prepend, DbExpression original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            DbExpression ret = new DbExpression
            {
                LeftTerm = prepend,
                Operator = DbOperators.Or,
                RightTerm = original
            };
            return ret;
        }

        /// <summary>
        /// Convert a List of DbExpression objects to a nested DbExpression containing AND between each DbExpression in the list. 
        /// </summary>
        /// <param name="exprList">List of DbExpression objects.</param>
        /// <returns>A nested DbExpression.</returns>
        public static DbExpression ListToNestedAndExpression(List<DbExpression> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            DbExpression ret = null;
            DbExpression left = null;
            List<DbExpression> remainder = new List<DbExpression>();

            if (exprList.Count == 1)
            {
                foreach (DbExpression curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (DbExpression curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new DbExpression();
                        left.LeftTerm = curr.LeftTerm;
                        left.Operator = curr.Operator;
                        left.RightTerm = curr.RightTerm;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new DbExpression();
                ret.LeftTerm = left;
                ret.Operator = DbOperators.And;
                DbExpression right = ListToNestedAndExpression(remainder);
                ret.RightTerm = right;

                return ret;
            }
        }

        /// <summary>
        /// Convert a List of DbExpression objects to a nested DbExpression containing OR between each DbExpression in the list. 
        /// </summary>
        /// <param name="exprList">List of DbExpression objects.</param>
        /// <returns>A nested DbExpression.</returns>
        public static DbExpression ListToNestedOrExpression(List<DbExpression> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            DbExpression ret = null;
            DbExpression left = null;
            List<DbExpression> remainder = new List<DbExpression>();

            if (exprList.Count == 1)
            {
                foreach (DbExpression curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (DbExpression curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new DbExpression();
                        left.LeftTerm = curr.LeftTerm;
                        left.Operator = curr.Operator;
                        left.RightTerm = curr.RightTerm;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new DbExpression();
                ret.LeftTerm = left;
                ret.Operator = DbOperators.Or;
                DbExpression right = ListToNestedOrExpression(remainder);
                ret.RightTerm = right;

                return ret;
            }
        }
    }
}
