using System.Collections.Generic;
using System.Linq;

namespace AzureDataLake.ODataQuery
{
    public enum CompareOps
    {
        Equals,
        GreaterThan,
        GreaterThanOrEquals,
        LesserThan,
        LesserThanOrEquals
    }

    public abstract class Expr
    {

        public abstract void ToExprString(System.Text.StringBuilder sb);
    }

    public class ExprAnd : Expr
    {
        public List<Expr> Items;
        public ExprAnd(params Expr[] items)
        {
            this.Items = new List<Expr>();
            this.Items.AddRange(items);
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            if (this.Items.Count < 1)
            {
                return;
            }

            sb.Append("(");
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" and ");
                }

                this.Items[i].ToExprString(sb);
            }

            sb.Append(")");
        }
    }

    public class ExprOr : Expr
    {
        public List<Expr> Items;
        public ExprOr(params Expr[] items)
        {
            this.Items = new List<Expr>();
            this.Items.AddRange(items);
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            if (this.Items.Count < 1)
            {
                return;
            }
            sb.Append("(");
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" or ");
                }

                this.Items[i].ToExprString(sb);
            }

            sb.Append(")");
        }
    }

    public class ExprParens : Expr
    {
        public Expr Item;
        public ExprParens( Expr item)
        {
            this.Item = item;
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            sb.Append("(");
            this.Item.ToExprString(sb);
            sb.Append(")");
        }
    }

    public class ExprStringEquals : Expr
    {
        public string Column;
        public string Value;
        public ExprStringEquals(string col, string val)
        {
            this.Column = col;
            this.Value = val;
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            sb.Append(string.Format("{0} eq '{1}'", this.Column, this.Value));
        }
    }

    public class ExprDateTimeComparison : Expr
    {
        public string Column;
        public System.DateTime Value;
        public CompareOps Op;
        public ExprDateTimeComparison(string col, System.DateTime val, CompareOps op)
        {
            this.Column = col;
            this.Value = val;
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            string datestring = this.Value.ToString("O");

            // due to issue: https://github.com/Azure/autorest/issues/975,
            // date time offsets must be explicitly escaped before being passed to the filter

            var escaped_datestring = System.Uri.EscapeDataString(datestring);

            var op = FilterUtils.OpToString(this.Op);
            sb.Append(string.Format("{0} {1} datetimeoffset'{2}'", this.Column, op, escaped_datestring));
        }


    }

    public class FilterUtils
    {
        public static string OpToString(CompareOps Op)
        {
            string op = "ge";
            if (Op == CompareOps.GreaterThan)
            {
                op = "gt";
            }
            else if (Op == CompareOps.GreaterThanOrEquals)
            {
                op = "ge";
            }
            else if (Op == CompareOps.LesserThan)
            {
                op = "lt";
            }
            else if (Op == CompareOps.LesserThanOrEquals)
            {
                op = "le";
            }
            else if (Op == CompareOps.Equals)
            {
                op = "eq";
            }
            else
            {
                throw new System.ArgumentOutOfRangeException();
            }
            return op;
        }
    }

    public class ExprRaw : Expr
    {
        public string Item;
        public ExprRaw(string s)
        {
            this.Item = s;
        }

        public override void ToExprString(System.Text.StringBuilder sb)
        {
            sb.Append(this.Item);
        }
    }

}
