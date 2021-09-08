//Nick Sells, 2021
//Program.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace dicalc {
	class Program {

		//a regex expression matcher for dice notation algebra
		private static readonly Regex regex = new Regex(@"(?:[\^*/+\-()])|(?:\d+|\[\d+d\d+\])",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		//a dictionary that maps each operator to it's precedence
		private static readonly Dictionary<string, int> opPrecedence = new Dictionary<string, int>() {
			{ "^", 3 },
			{ "*", 2 },
			{ "/", 2 },
			{ "+", 1 },
			{ "-", 1 },
			{ "(", 0 } //NOTE: only needs left parenthesis. right is treated like a null terminator
		};

		//our friendly neighborhood prng
		private static readonly Random rng = new Random();

		static void Main(string[] args) {
			if(args.Length >= 1)
				Console.WriteLine($"{args[0]} = {EvaluatePostfix(InfixToPostfix(args[0]))}");
			else
				Console.WriteLine("error: no expression provided");
		}

		//tokenizes a mathematical expression
		private static MatchCollection TokenizeInfix(string expr) {
			return regex.Matches(expr);
		}

		//accepts an infix mathematical expression and converts it to postfix
		private static string[] InfixToPostfix(string expr) {

			MatchCollection tokens = TokenizeInfix(expr);
			Stack<string> operators = new Stack<string>();
			List<string> postfix = new List<string>();

			foreach(Match token in tokens) {
				switch(token.Value) {
					
					//remove any operators with >= precedence to the current operator, append the current operator
					case "^":
					case "*":
					case "/":
					case "+":
					case "-":
						while(operators.Count > 0 && opPrecedence[operators.Peek()] >= opPrecedence[token.Value]) {
							postfix.Add(operators.Pop());
						}
						operators.Push(token.Value);
						break;

					//push any opening parenthesis to the opStack
					case "(":
						operators.Push(token.Value);
						break;

					//closing parenthesis
					//pop the opStack until the matching opening parenthesis is popped
					//TODO: turn this into a do-while
					case ")":
						var top = operators.Pop();
						while(top != "(") {
							postfix.Add(top);
							top = operators.Pop();
						}
						break;

					//append any operands to the postfix expression
					default:
						postfix.Add(token.Value);
						break;
				}
			}

			//pop and append all operators on the opStack until it is empty
			while(operators.Count > 0) {
				postfix.Add(operators.Pop());
			}

			return postfix.ToArray();
		}

		//roll a dice notation expression and report on it
		private static int RollDice(int rolls, int faces) {
			
			var str = $"[{rolls}d{faces}] = ";
			
			var sum = 0;
			for(int i = 0; i < rolls; i++) {
				var roll = rng.Next(0, faces) + 1;
				sum += roll;

				str += $"{roll}";
				if(i < rolls - 1)
					str += " + ";
			}

			Console.WriteLine(str + $" = {sum}");

			return sum;
		}

		//converts a algebraic term into it's representative number value
		private static int ParseTerm(string expr) {
			//if the term is dice notation
			if(expr.StartsWith("[") && expr.EndsWith("]")) {
				//split it into its operands, parse them, and then roll them
				var operands = expr.Substring(1, expr.Length - 2).Split("d");
				var rolls = Int32.Parse(operands[0]);
				var faces = Int32.Parse(operands[1]);
				return RollDice(rolls, faces);
			}
			//if the term is a constant
			else
				return Int32.Parse(expr);
		}

		//evaluates a postfix expression
		private static int EvaluatePostfix(string[] tokens) {

			Stack<int> operands = new Stack<int>();

			foreach(string token in tokens) {
				switch(token) {
					//operator
					case "^":
					case "*":
					case "/":
					case "+":
					case "-":
						var opRight = operands.Pop();
						var opLeft = operands.Pop();
						operands.Push(MathEvaluate(token, opLeft, opRight));
						break;

					//operand
					default:
						operands.Push(ParseTerm(token));
						break;
				}
			}

			return operands.Pop();
		}

		//evaluates a mathematical operation on two operands, using the operator represented by the string
		//NOTE: damn you, operator keyword
		private static int MathEvaluate(string operatr, int opLeft, int opRight) {
			switch(operatr) {
				case "^":
					return (int) Math.Pow(opLeft, opRight);
				case "*":
					return opLeft * opRight;
				case "/":
					return opLeft / opRight;
				case "+":
					return opLeft + opRight;
				case "-":
					return opLeft - opRight;
				default:
					//TODO: handle this somehow
					return 0;
			}
		}
	}
}
