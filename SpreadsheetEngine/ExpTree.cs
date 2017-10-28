/**********************************************
Samuel Chen
10728476
**********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace CptS321
{
    public class ExpTree
    {

        private Node mRoot;
        private Dictionary<string, double> m_vars;
        private string rawExpression;

        /************************************************************* 
        constructor.  requires a string expression
        *************************************************************/
        public ExpTree(string exp)
        {
            if (exp.Length != 0)
                if (exp[0] == '=')
                    exp = exp.Substring(1);
            m_vars = new Dictionary<string, double>();
            mRoot = Compile(exp, m_vars);
            rawExpression = exp;

        }

        /************************************************************* 
        to print expression of tree
        *************************************************************/
        public override string ToString()
        {
            //return mRoot.ToString();
            return rawExpression;
        }

        private abstract class Node
        {
            public abstract double eval();
        }

        // for nodes with constants
        private class ConstNode : Node
        {
            private double Value;
            public override double eval()
            {
                return Value;
            }
            public override string ToString()
            {
                return Value.ToString();
            }

            public ConstNode(double exp)
            {
                Value = exp;
            }
        }

        // for nodes with variables
        private class VarNode : Node
        {
            private string Name;
            private Dictionary<string, double> _mvars;
            public override double eval()
            {
                if (_mvars.ContainsKey(Name))
                    return _mvars[Name];
                return double.NaN;
            }
            public override string ToString()
            {
                return Name.ToString();
            }

            public VarNode(string exp, Dictionary<string, double> m_vars)
            {
                Name = exp;
                _mvars = m_vars;
            }
        }

        // looks for +,-,*,/
        private class OpNode : Node
        {
            private char Op;
            private Node Left;
            private Node Right;
            public override double eval()
            {
                switch (Op)
                {
                    case '+':
                        return Left.eval() + Right.eval();
                    case '-':
                        return Left.eval() - Right.eval();
                    case '*':
                        return Left.eval() * Right.eval();
                    case '/':
                        return Left.eval() / Right.eval();
                    default:
                        return 0;
                }
            }

            public override string ToString()
            {
                return Left.ToString() + Op.ToString() + Right.ToString();
            }

            public OpNode(char op, Node expLeft, Node expRight)
            {
                Op = op;
                Left = expLeft;
                Right = expRight;
            }
        }

        // setting variables
        public void SetVar(string varName, double varValue)
        {
            if(!m_vars.ContainsKey(varName))
                m_vars.Add(varName, varValue);
        }

        // evaluating expression tree
        public double Eval()
        {
            return mRoot.eval();
        }

        // creates a tree based on the expression
        private static Node Compile(string exp, Dictionary<string, double> m_vars)
        {
            exp = exp.Replace(" ", "");

            int parenCount = 0;

            if (exp.Length > 0)
            {
                if (exp[0] == '(')
                {
                    for (int j = 0; j < exp.Length; j++)
                    {
                        if (exp[j] == '(')
                            parenCount++;
                        else if (exp[j] == ')')
                        {
                            parenCount--;
                            if (parenCount == 0)
                            {
                                if (j == exp.Length - 1)
                                {
                                    return Compile(exp.Substring(1, exp.Length - 2), m_vars);
                                }
                                break;
                            }
                        }
                    }
                }
                parenCount = 0;
                int index = GetLow(exp);

                if(index >= 0)      // there is an operator
                    return new OpNode(exp[index], Compile(exp.Substring(0, index), m_vars), Compile(exp.Substring(index + 1), m_vars));
            }
            return BuildSimple(exp, m_vars);
        }

        // gets called from Compile.  builds an end node (either const or var node)
        private static Node BuildSimple(string exp, Dictionary<string, double> m_vars)
        {
            double temp = 0;
            if (double.TryParse(exp, out temp))
            {
                return new ConstNode(temp);
            }
            else
                return new VarNode(exp, m_vars);
        }

        private static int GetLow(string exp)
        {
            int parenCounter = 0;
            int index = -1;

            for (int i = exp.Length - 1; i >= 0; i--)
            {
                switch (exp[i])
                {
                    case ')':
                        parenCounter++;
                        break;
                    case '(':
                        parenCounter--;
                        break;
                    case '+':
                        if (parenCounter == 0)
                            return i;
                        break;
                    case '-':
                        if (parenCounter == 0)
                            return i;
                        break;
                    case '*':
                        if (parenCounter == 0 && index == -1)
                            index = i;
                        break;
                    case '/':
                        if (parenCounter == 0 && index == -1)
                            index = i;
                        break;
                }
            }
            return index;
        }
    }
}

