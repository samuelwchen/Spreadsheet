/**********************************************
Samuel Chen
10728476
HW 6 - evaluate tree with parenthesis and operator precedence
**********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CptS321;


namespace ExprDemo
{
    class ExpDemo
    {
        static void Main(string[] args)
        {
            ExpTree tree = new ExpTree("");
            string option = "";
            string exp = "";
            string variable = "";
            double value = 0;
        

            for (;;)        // infiinite while loop
            {
                Console.WriteLine("Menu (Current expression :: " + tree.ToString() + ")");
                Console.WriteLine("  1 = Enter a new expression.");
                Console.WriteLine("  2 = Set a variable expression.");
                Console.WriteLine("  3 = Evaluate Tree.");
                Console.WriteLine("  4 = Quit.");

                option = Console.ReadLine();

                if(option == "1")           // create new tree from new expression
                {
                    Console.WriteLine("Enter new expression :: ");
                    exp = Console.ReadLine();
                    tree = new ExpTree(exp);
                }
                else if (option == "2")     // set variables
                {
                    Console.WriteLine("What is variable name? :: ");
                    variable = Console.ReadLine();
                    Console.WriteLine("What is the value? :: ");
                    double.TryParse(Console.ReadLine(), out value);
                    tree.SetVar(variable, value);
                }
                else if (option == "3")     // evaluate function
                {
                    Console.WriteLine("Answer = " + tree.Eval());
                }   
                else if (option == "4")     // exit
                {
                    return;
                }
            }
        }
    }
}
