using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Parses and executes PlayerPrefs commands and variables from a script.
/// </summary>
public class TwineScriptParser
{
    // List of supported operators
    private static readonly List<string> OperatorTokens = new List<string>
    {
        "+", "-", "*", "/", "%", "&", "|", "^", "&&", "||"
    };

    // Constants for operator types
    private const string ADD = "+";
    private const string SUBTRACT = "-";
    private const string MULTIPLY = "*";
    private const string DIVIDE = "/";
    private const string MODULO = "%";
    private const string AND = "&";
    private const string OR = "|";
    private const string XOR = "^";
    private const string ANDALSO = "&&";
    private const string ORELSE = "||";

    private enum ParsingState
    {
        None,
        Command,
        Argument
    }

    public enum ValueType
    {
        Int,
        Float,
        String
    }

    private Dictionary<string, Action<List<string>>> commandHandlers;
    private Dictionary<string, AstResult> variables;

    public TwineScriptParser()
    {
        // Initialize command handlers for PlayerPrefs functions
        commandHandlers = new Dictionary<string, Action<List<string>>>
        {
            { "PlayerPrefs.SetInt", HandleSetInt },
            { "PlayerPrefs.SetFloat", HandleSetFloat },
            { "PlayerPrefs.SetString", HandleSetString },
            { "PlayerPrefs.GetInt", HandleGetInt },
            { "PlayerPrefs.GetFloat", HandleGetFloat },
            { "PlayerPrefs.GetString", HandleGetString }
        };

        // Initialize variables storage
        variables = new Dictionary<string, AstResult>();
    }

    /// <summary>
    /// Parses a script and executes PlayerPrefs commands and variable assignments.
    /// </summary>
    /// <param name="script">The script to parse and execute.</param>
    public void ParseAndExecute(string script)
    {
        var lines = script.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            Debug.Log($"ParseAndExecute - Line: [{line}]");
            var ast = ParseLineToAST(line);
            Debug.Log($"ParseAndExecute - AST: [{ast}]");
            ExecuteAST(ast);
        }
    }

    /// <summary>
    /// Parses a single line of script into an AST.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>An AST representing the command and arguments.</returns>
    private List<string> ParseLineToAST(string line)
    {
        var ast = new List<string>();
        var matches = Regex.Matches(line, @"(\w+\.\w+|\w+\(.*?\)|\d+|"".*?""|\+|\-|\*|\/|\%|\&|\||\^|&&|\|\|)");
        foreach (Match match in matches)
        {
            ast.Add(match.Value);
        }
        return ast;
    }

    /// <summary>
    /// Executes an AST.
    /// </summary>
    /// <param name="ast">The AST to execute.</param>
    private void ExecuteAST(List<string> ast)
    {
        if (ast.Count == 0) {
            Debug.Log($"ExecuteAST - AST Count Zero: [{ast}]");
            return;
        }
            
        if (ast[0].Contains("PlayerPrefs"))
        {
            var command = ast[0];
            Debug.Log($"ExecuteAST - PlayerPrefs Command: [{command}]");
            if (commandHandlers.ContainsKey(command))
            {
                var args = ast.GetRange(1, ast.Count - 1);
                commandHandlers[command].Invoke(args);
            }
            else
            {
                Debug.LogError($"ExecuteAST - Unknown command: {command}");
            }
        }
        else
        {
            HandleVariableAssignment(ast);
        }
    }

    #region Command Handlers

    private void HandleSetInt(List<string> args)
    {
        if (args.Count == 2)
        {
            var key = args[0].Trim('"');
            if (int.TryParse(args[1], out int value))
            {
                PlayerPrefs.SetInt(key, value);
                Debug.Log($"PlayerPrefs.SetInt - Key: {key}, Value: {value}");
            }
        }
    }

    private void HandleSetFloat(List<string> args)
    {
        if (args.Count == 2)
        {
            var key = args[0].Trim('"');
            if (float.TryParse(args[1], out float value))
            {
                PlayerPrefs.SetFloat(key, value);
                Debug.Log($"PlayerPrefs.SetFloat - Key: {key}, Value: {value}");
            }
        }
    }

    private void HandleSetString(List<string> args)
    {
        if (args.Count == 2)
        {
            var key = args[0].Trim('"');
            var value = args[1].Trim('"');
            PlayerPrefs.SetString(key, value);
            Debug.Log($"PlayerPrefs.SetString - Key: {key}, Value: {value}");
        }
    }

    private void HandleGetInt(List<string> args)
    {
        if (args.Count == 1)
        {
            var key = args[0].Trim('"');
            var value = PlayerPrefs.GetInt(key, 0);
            Debug.Log($"PlayerPrefs.GetInt - Key: {key}, Value: {value}");
        }
    }

    private void HandleGetFloat(List<string> args)
    {
        if (args.Count == 1)
        {
            var key = args[0].Trim('"');
            var value = PlayerPrefs.GetFloat(key, 0f);
            Debug.Log($"PlayerPrefs.GetFloat - Key: {key}, Value: {value}");
        }
    }

    private void HandleGetString(List<string> args)
    {
        if (args.Count == 1)
        {
            var key = args[0].Trim('"');
            var value = PlayerPrefs.GetString(key, string.Empty);
            Debug.Log($"PlayerPrefs.GetString - Key: {key}, Value: {value}");
        }
    }

    #endregion

    #region Variable Handlers

    /// <summary>
    /// Handles variable assignments.
    /// </summary>
    /// <param name="ast">The AST representing the variable assignment.</param>
    private void HandleVariableAssignment(List<string> ast)
    {
        if (ast.Count >= 3 && ast[1] == "=")
        {
            var varName = ast[0];
            var expr = ast.GetRange(2, ast.Count - 2);
            var exprResult = EvaluateExpression(expr);
            Debug.Log($"HandleVariableAssignment - EvaluateExpression exprResult: [{exprResult}] varName: [{varName}]");
            if (exprResult != null)
            {
                variables[varName] = ((ExprResult)exprResult).Ast;
                Debug.Log($"HandleVariableAssignment - Variable assigned - Name: {varName}, Value: {variables[varName].Value}, Type: {variables[varName].Type}");
            }
        }
    }

    /// <summary>
    /// Evaluates an expression represented by an AST.
    /// </summary>
    /// <param name="expr">The AST representing the expression.</param>
    /// <returns>The result of the expression evaluation.</returns>
    private ExprResult? EvaluateExpression(List<string> expr)
    {
        Stack<AstResult> stack = new Stack<AstResult>();

        foreach (var token in expr)
        {
            if (int.TryParse(token, out int intResult))
            {
                stack.Push(new AstResult(ValueType.Int, intResult));
            }
            else if (float.TryParse(token, out float floatResult))
            {
                stack.Push(new AstResult(ValueType.Float, floatResult));
            }
            else if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                stack.Push(new AstResult(ValueType.String, token.Trim('"')));
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else if (IsOperator(token))
            {
                var right = stack.Pop();
                var left = stack.Pop();
                var result = ApplyOperator(left, right, token);
                stack.Push(result);
            }
            else
            {
                Debug.LogError($"Unknown token: {token}");
                return null;
            }
        }

        return stack.Count == 1 ? new ExprResult { Ast = stack.Pop(), Success = true } : (ExprResult?)null;
    }

    /// <summary>
    /// Evaluates an AST.
    /// </summary>
    /// <param name="ast">The AST to evaluate.</param>
    /// <returns>The result of the AST evaluation.</returns>
    private AstResult? EvaluateAst(List<string> ast)
    {
        return EvaluateExpression(ast)?.Ast;
    }

    /// <summary>
    /// Determines if a token is an operator.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token is an operator, false otherwise.</returns>
    private bool IsOperator(string token)
    {
        return OperatorTokens.Contains(token);
    }

    /// <summary>
    /// Applies an operator to two operands.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="op">The operator.</param>
    /// <returns>The result of applying the operator to the operands.</returns>
    private AstResult ApplyOperator(AstResult left, AstResult right, string op)
    {
        // Implement operator logic for int, float, and string types
        if (left.Type == ValueType.Int && right.Type == ValueType.Int)
        {
            int l = (int)left.Value;
            int r = (int)right.Value;
            return op switch
            {
                ADD => new AstResult(ValueType.Int, l + r),
                SUBTRACT => new AstResult(ValueType.Int, l - r),
                MULTIPLY => new AstResult(ValueType.Int, l * r),
                DIVIDE => new AstResult(ValueType.Int, l / r),
                MODULO => new AstResult(ValueType.Int, l % r),
                AND => new AstResult(ValueType.Int, l & r),
                OR => new AstResult(ValueType.Int, l | r),
                XOR => new AstResult(ValueType.Int, l ^ r),
                ANDALSO => new AstResult(ValueType.Int, (l != 0 && r != 0) ? 1 : 0),
                ORELSE => new AstResult(ValueType.Int, (l != 0 || r != 0) ? 1 : 0),
                _ => throw new InvalidOperationException($"Unknown operator: {op}"),
            };
        }
        else if (left.Type == ValueType.Float && right.Type == ValueType.Float)
        {
            float l = (float)left.Value;
            float r = (float)right.Value;
            return op switch
            {
                ADD => new AstResult(ValueType.Float, l + r),
                SUBTRACT => new AstResult(ValueType.Float, l - r),
                MULTIPLY => new AstResult(ValueType.Float, l * r),
                DIVIDE => new AstResult(ValueType.Float, l / r),
                MODULO => new AstResult(ValueType.Float, l % r),
                _ => throw new InvalidOperationException($"Unknown operator: {op}"),
            };
        }
        else if (left.Type == ValueType.String || right.Type == ValueType.String)
        {
            string l = left.Value.ToString();
            string r = right.Value.ToString();
            return op switch
            {
                ADD => new AstResult(ValueType.String, l + r),
                _ => throw new InvalidOperationException($"Unknown operator: {op}"),
            };
        }
        else
        {
            throw new InvalidOperationException($"Type mismatch or unknown type: {left.Type} and {right.Type}");
        }
    }

    #endregion
}

/// <summary>
/// Represents the result of evaluating an AST node.
/// </summary>
public struct AstResult
{
    public TwineScriptParser.ValueType Type { get; set; }
    public object Value { get; set; }

    public AstResult(TwineScriptParser.ValueType type, object value)
    {
        Type = type;
        Value = value;
    }
}

/// <summary>
/// Represents the result of an expression evaluation.
/// </summary>
public struct ExprResult
{
    public AstResult Ast { get; set; }
    public bool Success { get; set; }
}
