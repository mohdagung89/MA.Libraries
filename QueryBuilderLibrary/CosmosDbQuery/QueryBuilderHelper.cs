/*
 * QueryBuilderHelper
 * Author: MohdAgung
 * Version: v1.2.0
 * Description: A powerful query building library for CosmosDb database.
 * Source: https://github.com/mohdagung89/MA.Libraries
 */

using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace QueryBuilderLibrary.CosmosDbQuery;

/// <summary>
/// An enum Operation
/// </summary>
public enum Operation
{
    /// <summary>
    /// Equal operation "="
    /// </summary>
    [Description("=")]
    Equal,

    /// <summary>
    /// Not equal operation "<>"
    /// </summary>
    [Description("<>")]
    NotEqual,

    /// <summary>
    /// Greater than operation ">"
    /// </summary>
    [Description(">")]
    GreaterThan,

    /// <summary>
    /// Greater than or equal operation ">="
    /// </summary>
    [Description(">=")]
    GreaterThanOrEqual,

    /// <summary>
    /// Less than operation "<"
    /// </summary>
    [Description("<")]
    LessThan,

    /// <summary>
    /// Less than or equal operation "<="
    /// </summary>
    [Description("<=")]
    LessThanOrEqual,

    /// <summary>
    /// Less than or equal operation "IN"
    /// </summary>
    [Description("IN")]
    Contains
}

public class SORTING_FILTER
{
    public const string ASCENDING = "ASC";
    public const string DESCENDING = "DESC";
}

/// <summary>
/// A class enum extensions
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get description value of enum
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        return descriptionAttribute?.Length > 0 ? descriptionAttribute[0].Description : value.ToString();
    }
}

public interface IQuerySelect
{
    bool IsSelectCount();
    IQueryBuilder Distinct();
    IQueryBuilder Select(params string[] columns);
    IQueryBuilder SelectAs(string column, string alias);
    IQueryBuilder SelectCount(string? column = null);
    IQueryBuilder SelectDistinct(params string[] columns);
    IQueryBuilder SelectSumNumberString(params string[] columns);
    IQueryBuilder SelectSum(params string[] columns);
    IQueryBuilder SelectMax(params string[] columns);
    IQueryBuilder SelectRight(int num, params string[] columns);
    IQueryBuilder SelectLeft(int num, params string[] columns);
    IQueryBuilder SelectRightLeft(int numRight, int numLeft, params string[] columns);
    IQueryBuilder SelectRawFunctionAs(string rawFunction, string alias);
}
public interface IQueryWhere
{
    IQueryBuilder Where(string column, Operation operation, string value);
    IQueryBuilder Where(string column, Operation operation, int value);
    IQueryBuilder Where(string column, Operation operation, float value);
    IQueryBuilder Where(string column, Operation operation, decimal value);
    IQueryBuilder Where(string column, Operation operation, bool value);
    IQueryBuilder Where(string column, Operation operation, DateTime value);
    IQueryBuilder WhereIgnoreCase(string column, Operation operation, string value);
    IQueryBuilder WhereRight(int num, string column, Operation operation, string value);
    IQueryBuilder WhereRightLeft(int numRight, int numLeft, string column, Operation operation, string value);
    IQueryBuilder WhereNull(string column);
    IQueryBuilder WhereNotNull(string column);
    IQueryBuilder WhereContains(string column, string value);
    IQueryBuilder WhereNotContains(string column, string value);
    IQueryBuilder WhereIs(string column, string value);
    IQueryBuilder WhereIsNot(string column, string value);
    IQueryBuilder WhereIs(string column, int value);
    IQueryBuilder WhereIs(string column, float value);
    IQueryBuilder WhereIs(string column, decimal value);
    IQueryBuilder WhereIsNot(string column, int value);
    IQueryBuilder WhereIs(string column, bool value);
    IQueryBuilder WhereIsNot(string column, bool value);
    IQueryBuilder WhereIgnoreCaseIs(string column, string value);
    IQueryBuilder WhereRightIs(int num, string column, string value);
    IQueryBuilder WhereRightLeftIs(int numRight, int numLeft, string column, string value);
    IQueryBuilder WhereIn(string column, params string[] values);
    IQueryBuilder WhereIn(string column, params int[] values);
    IQueryBuilder WhereNotIn(string column, params string[] values);
    IQueryBuilder WhereNotIn(string column, params int[] values);
    IQueryBuilder WhereGroup(string operation, Action<IQueryWhere> expression);
}
public interface IQueryOrder
{
    IQueryBuilder OrderBy(string column, string? orderMethod = "ASC");
}
public interface IQueryGroup
{
    IQueryBuilder GroupBy(string column);
    IQueryBuilder GroupBy(params string[] columns);
}
public interface IQueryPagination
{
    IQueryBuilder Skip(int skip);
    IQueryBuilder Take(int take);
}
public interface ISubQuery
{
    IQueryBuilder FromSubQuery(Action<IQueryBuilder> query);
}
public interface IQueryBuilder : IQuerySelect, IQueryWhere, IQueryOrder, IQueryGroup, IQueryPagination, ISubQuery
{
    string InitialTable { get; }
    string Query { get; }
    string Build();
}

/// <summary>
/// QueryBuilderHelper class for building dynamic queries.
/// </summary>
[Description("v1.2.0")]
public class QueryBuilderHelper : IQueryBuilder
{
    /// <summary>
    /// Initial for COUNT method
    /// </summary>
    public const string COUNT_INITIAL = "_COUNT";

    private string _initialTable = "C";
    private string _selectCount = string.Empty;
    private List<string> _select = new();
    private List<string> _selectSum = new();
    private List<string> _selectMax = new();
    private List<string> _selectRawFunction = new();
    private List<string> _whereList = new();
    private List<string> _whereNullList = new();
    private List<string> _whereNotNullList = new();
    private List<string> _orderList = new();
    private List<string> _groupByList = new();
    private bool _isPaging = false;
    private int _skip = 0;
    private int _take = 10;

    private bool _isSelectDefault = false;
    private bool _isSelectCount = false;
    private bool _isSelectRowCount = false;
    private bool _isSelectDistinct = false;

    private string _query = string.Empty;

    /// <summary>
    /// Represents a child QueryBuilderHelper used for creating nested queries.
    /// </summary>
    protected IQueryBuilder? _child = null;

    /// <summary>
    /// Create an instance of QueryBuilder
    /// </summary>
    /// <returns cref="QueryBuilderHelper"></returns>
    public static QueryBuilderHelper Initialize()
    {
        return new QueryBuilderHelper();
    }

    #region IQueryBuilder
    /// <summary>
    /// Return generated query string after method Build was executed
    /// </summary>
    public string Query
    {
        get { return _query; }
    }

    /// <summary>
    /// Get initial table
    /// </summary>
    public string InitialTable
    {
        get { return _initialTable; }
    }
    #endregion

    #region ISubQuery
    /// <summary>
    /// [Beta Version] Creates a nested query using the FromSubQuery method.
    /// </summary>
    /// <param name="query">An Action that defines the nested query using a QueryBuilderHelper instance.</param>
    /// <returns>The current QueryBuilderHelper instance.</returns>
    public IQueryBuilder FromSubQuery(Action<IQueryBuilder> query)
    {
        var qb = Initialize();
        query(qb);
        _child = qb;
        return this;
    }
    #endregion

    #region IQuerySelect
    /// <summary>
    /// Check if builder statement was registered for counting data or not
    /// </summary>
    /// <returns></returns>
    public bool IsSelectCount()
    {
        return _isSelectCount;
    }

    /// <summary>
    /// Distinct all result
    /// </summary>
    /// <returns></returns>
    public IQueryBuilder Distinct()
    {
        _isSelectDefault = false;
        _isSelectDistinct = true;
        return this;
    }

    /// <summary>
    /// Register SELECT clause to select specific column that will be returned
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder Select(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                _select.Add(columns[i]);
            else
                _select.Add($"{_initialTable}.{columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Registers a SELECT clause to specify a specific column with an optional alias for the result.
    /// </summary>
    /// <param name="column">The column to be selected.</param>
    /// <param name="alias">The optional alias for the selected column.</param>
    /// <returns>The current QueryBuilderHelper instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the column is "*", as using asterisk (*) is not allowed in the {nameof(SelectAs)} clause.</exception>
    public IQueryBuilder SelectAs(string column, string alias)
    {
        _isSelectDefault = true;
        if (column == "*")
            throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectAs)} clause");
        else
            _select.Add($"{_initialTable}.{column} AS {alias}");
        return this;
    }

    /// <summary>
    /// Register SELECT COUNT clause to count the data only
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IQueryBuilder SelectCount(string? column = null)
    {
        _isSelectCount = true;
        _selectCount += string.Format("COUNT({0}) {1}", !string.IsNullOrWhiteSpace(column) ? (_initialTable + "." + column) : ("1"), COUNT_INITIAL);
        return this;
    }

    /// <summary>
    /// Register SELECT COUNT clause to count the data only
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IQueryBuilder SelectRowCount()
    {
        _isSelectRowCount = true;
        return this;
    }

    /// <summary>
    /// Register SELECT DISTINCT clause to select specific column that will be returned
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder SelectDistinct(params string[] columns)
    {
        _isSelectDistinct = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                _select.Add("*");
            else
                _select.Add($"{_initialTable}.{columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT SUM clause for number string to select sum of number for specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder SelectSumNumberString(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectSumNumberString)} clause");
            else
                _selectSum.Add($"SUM(StringToNumber({_initialTable}.{columns[i]})) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT SUM clause to select sum of number for specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder SelectSum(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectSum)} clause");
            else
                _selectSum.Add($"SUM({_initialTable}.{columns[i]}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT MAX clause to select max of number/date for specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder SelectMax(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectMax)} clause");
            else
                _selectMax.Add($"MAX({_initialTable}.{columns[i]}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT clause to select last (n) character only for specific column
    /// </summary>
    /// <param name="num"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IQueryBuilder SelectRight(int num, params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectRight)} clause");
            else
                _selectMax.Add($"RIGHT({_initialTable}.{columns[i]}, {num}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT clause to select last (n) character only for specific column
    /// </summary>
    /// <param name="num"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IQueryBuilder SelectLeft(int num, params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectRight)} clause");
            else
                _selectMax.Add($"LEFT({_initialTable}.{columns[i]}, {num}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register SELECT clause to select data between (n) first character and (n) last character only for specific column
    /// </summary>
    /// <param name="num"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IQueryBuilder SelectRightLeft(int numRight, int numLeft, params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectRight)} clause");
            else
                _selectMax.Add($"LEFT(RIGHT({_initialTable}.{columns[i]}, {numRight}), {numLeft}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// [Beta Version] Register SELECT raw, example rawFunction "sum(iif(c.score=1,1,0))" and alias "scoreTotal"
    /// </summary>
    /// <param name="rawFunction"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IQueryBuilder SelectRawFunctionAs(string rawFunction, string alias)
    {
        _isSelectDefault = true;
        if (rawFunction == "*")
            throw new ArgumentException($"Cannot use asterisk (*) from {nameof(SelectRawFunctionAs)} clause");
        else if (rawFunction.Contains(" as ", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Alias \"as\" must be defined in the alias param when use {nameof(SelectRawFunctionAs)} clause");
        else
            _selectRawFunction.Add($"{rawFunction} as {alias}");
        return this;
    }
    #endregion

    #region IQueryWhere
    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, string value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, int value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} {value}");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, float value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} {value.ToString(CultureInfo.InvariantCulture)}");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, decimal value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} {value.ToString(CultureInfo.InvariantCulture)}");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, bool value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} {value.ToString().ToLower()}");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder Where(string column, Operation operation, DateTime value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation.GetDescription()} '{value.ToString("yyyy-MM-dd HH:mm:ss")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned <br/>
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIgnoreCase(string column, Operation operation, string value)
    {
        _whereList.Add($"LOWER({_initialTable}.{column}) {operation.GetDescription()} '{value.ToLower().Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter matched data for (n) last character value
    /// </summary>
    /// <param name="num"></param>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereRight(int num, string column, Operation operation, string value)
    {
        _whereList.Add($"RIGHT({_initialTable}.{column}, {num}) {operation.GetDescription()} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter matched data for character value between (n) first character and (n) last character
    /// </summary>
    /// <param name="numRight"></param>
    /// <param name="numLeft"></param>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereRightLeft(int numRight, int numLeft, string column, Operation operation, string value)
    {
        _whereList.Add($"LEFT(RIGHT({_initialTable}.{column}, {numRight}), {numLeft}) {operation.GetDescription()} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE null clause condition to filter specific null data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IQueryBuilder WhereNull(string column)
    {
        _whereNullList.Add($"{_initialTable}.{column} {Operation.Equal.GetDescription()} null");
        return this;
    }

    /// <summary>
    /// Register WHERE <> null clause condition to filter specific not null data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IQueryBuilder WhereNotNull(string column)
    {
        _whereNotNullList.Add($"{_initialTable}.{column} {Operation.NotEqual.GetDescription()} null");
        return this;
    }

    /// <summary>
    /// Register WHERE LIKE clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereContains(string column, string value)
    {
        _whereList.Add($"lower({_initialTable}.{column}) LIKE '%{value.Replace(@"\", @"\\").Replace(@"'", @"\'").ToLower()}%'");
        return this;
    }

    /// <summary>
    /// Register WHERE NOT LIKE clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereNotContains(string column, string value)
    {
        _whereList.Add($"lower({_initialTable}.{column}) NOT LIKE '%{value.Replace(@"\", @"\\").Replace(@"'", @"\'").ToLower()}%'");
        return this;
    }

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIs(string column, string value) => Where(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIsNot(string column, string value) => Where(column, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIs(string column, int value) => Where(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIs(string column, float value) => Where(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIs(string column, decimal value) => Where(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIsNot(string column, int value) => Where(column, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIs(string column, bool value) => Where(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIsNot(string column, bool value) => Where(column, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned <br/>
    /// Ignore case sensitive
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIgnoreCaseIs(string column, string value) => WhereIgnoreCase(column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE clause condition to filter matched data for (n) last character value
    /// </summary>
    /// <param name="num"></param>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereRightIs(int num, string column, string value) => WhereRight(num, column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE clause condition to filter matched data for character value between (n) first character and (n) last character
    /// </summary>
    /// <param name="num"></param>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IQueryBuilder WhereRightLeftIs(int numRight, int numLeft, string column, string value) => WhereRightLeft(numRight, numLeft, column, Operation.Equal, value);

    /// <summary>
    /// Register WHERE IN clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIn(string column, params string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Replace(@"\", @"\\").Replace(@"'", @"\'");
        }
        _whereList.Add($"{_initialTable}.{column} IN ('{string.Join("','", values)}')");
        return this;
    }

    /// <summary>
    /// Register WHERE IN clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public IQueryBuilder WhereIn(string column, params int[] values)
    {
        _whereList.Add($"{_initialTable}.{column} IN ({string.Join(',', values)})");
        return this;
    }

    /// <summary>
    /// Register WHERE NOT IN clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public IQueryBuilder WhereNotIn(string column, params string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Replace(@"\", @"\\").Replace(@"'", @"\'");
        }
        _whereList.Add($"{_initialTable}.{column} NOT IN ('{string.Join("','", values)}')");
        return this;
    }

    /// <summary>
    /// Register WHERE NOT IN clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public IQueryBuilder WhereNotIn(string column, params int[] values)
    {
        _whereList.Add($"{_initialTable}.{column} NOT IN ({string.Join(',', values)})");
        return this;
    }

    /// <summary>
    /// Register nested WHERE clause, it will has bracket group for nested where
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public IQueryBuilder WhereGroup(string operation, Action<IQueryWhere> expression)
    {
        var qb = Initialize();
        expression(qb);
        _whereList.Add("(" + string.Join($" {operation} ", qb._whereList) + ")");
        return this;
    }
    #endregion

    #region IQueryOrder
    /// <summary>
    /// Register ORDER BY statement to sorting the data by specific column
    /// </summary>
    /// <param name="column"></param>
    /// <param name="orderMethod"></param>
    /// <returns></returns>
    public IQueryBuilder OrderBy(string column, string? orderMethod = "ASC")
    {
        orderMethod = (orderMethod ?? "ASC").ToUpper() == "ASC" || (orderMethod ?? "ASC").ToUpper() == "DESC" ? (orderMethod ?? "ASC").ToUpper() : "ASC";
        _orderList.Add($"{_initialTable}.{column} {orderMethod}");
        return this;
    }
    #endregion

    #region IQueryGroup
    /// <summary>
    /// Register GROUP BY statement to sorting the data by specific column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IQueryBuilder GroupBy(string column)
    {
        if (column == "*")
            throw new ArgumentException($"Cannot use asterisk (*) from {nameof(GroupBy)} clause");
        else
            _groupByList.Add($"{_initialTable}.{column}");
        return this;
    }

    /// <summary>
    /// Register GROUP BY statement to grouping the data by specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public IQueryBuilder GroupBy(params string[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asterisk (*) from {nameof(GroupBy)} clause");
            else
                _groupByList.Add($"{_initialTable}.{columns[i]}");
        }
        return this;
    }
    #endregion

    #region IQueryPagination
    /// <summary>
    /// Register OFFSET statement to skip number of record
    /// </summary>
    /// <param name="skip"></param>
    /// <returns></returns>
    public IQueryBuilder Skip(int skip)
    {
        _isPaging = true;
        _skip = skip;
        return this;
    }

    /// <summary>
    /// Register LIMIT statement to take specific number of data that will be returned
    /// </summary>
    /// <param name="take"></param>
    /// <returns></returns>
    public IQueryBuilder Take(int take)
    {
        _isPaging = true;
        _take = take;
        return this;
    }
    #endregion

    /// <summary>
    /// Generate sql query string from registered clause
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string Build()
    {
        if (!_isSelectDefault && !_isSelectDistinct)
            _isSelectDefault = true;

        if (_isSelectCount && _select.Any())
            throw new ArgumentException($"Method {nameof(Select)} cannot combine with method {nameof(SelectCount)}");

        if (_isSelectCount && _isSelectDistinct)
            throw new ArgumentException($"Method {nameof(SelectDistinct)} cannot combine with method {nameof(SelectCount)}, please use method {nameof(Select)} with {nameof(QueryBuilderHelper.GroupBy)} instead");

        if (_isSelectDefault && _isSelectDistinct)
            throw new ArgumentException($"Method {nameof(Select)} cannot combine with method {nameof(SelectDistinct)}, please use one of them only or use method {nameof(Distinct)} to make all distinct");

        var filterSelectGroup = _select.Select(x => x.Split(" AS ").First()).ToList();
        if (_groupByList.Any() && !Enumerable.SequenceEqual(_groupByList, filterSelectGroup))
            throw new ArgumentException($"Registered columns in method {nameof(GroupBy)} must exist all in method {nameof(Select)}");

        StringBuilder sbSelect = new();
        StringBuilder sbWhere = new();
        StringBuilder sbGroup = new();
        StringBuilder sbOrder = new();
        StringBuilder sbLimit = new();

        StringBuilder sbOpenBracket = new();
        StringBuilder sbCloseBracket = new();

        sbOpenBracket.AppendLine("SELECT * FROM (");
        sbCloseBracket.AppendLine($") AS {_initialTable}");

        if (_isSelectCount)
        {
            if (string.IsNullOrEmpty(_selectCount))
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    sbSelect.AppendLine($"SELECT COUNT(1) {COUNT_INITIAL} FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    sbSelect.AppendLine($"SELECT COUNT(1) {COUNT_INITIAL} FROM {_initialTable}");
                }
            }
            else
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    _selectCount = _selectCount.EndsWith(",") ? _selectCount[..^1] : _selectCount;
                    sbSelect.AppendLine($"SELECT {_selectCount} FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    _selectCount = _selectCount.EndsWith(",") ? _selectCount[..^1] : _selectCount;
                    sbSelect.AppendLine($"SELECT {_selectCount} FROM {_initialTable}");
                }
            }
        }
        else if (!_isSelectDistinct)
        {
            var select = new List<string>();
            select.AddRange(_select);
            select.AddRange(_selectSum);
            select.AddRange(_selectMax);
            select.AddRange(_selectRawFunction);
            if (!select.Any())
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    sbSelect.AppendLine($"SELECT * FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    sbSelect.AppendLine($"SELECT * FROM {_initialTable}");
                }
            }
            else
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    sbSelect.AppendLine($"SELECT {string.Join(",", select)} FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    sbSelect.AppendLine($"SELECT {string.Join(",", select)} FROM {_initialTable}");
                }
            }
        }
        else
        {
            var select = new List<string>();
            select.AddRange(_select);
            select.AddRange(_selectSum);
            select.AddRange(_selectMax);
            select.AddRange(_selectRawFunction);
            if (!select.Any())
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    sbSelect.AppendLine($"SELECT DISTINCT * FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    sbSelect.AppendLine($"SELECT DISTINCT * FROM {_initialTable}");
                }
            }
            else
            {
                if (_child != null)
                {
                    var query = _child.Build();
                    sbSelect.AppendLine($"SELECT DISTINCT {string.Join(",", select)}  FROM ({query}) AS {_initialTable}");
                }
                else
                {
                    sbSelect.AppendLine($"SELECT DISTINCT {string.Join(",", select)} FROM {_initialTable}");
                }
            }
        }

        if (_whereList.Any() || _whereNullList.Any() || _whereNotNullList.Any())
        {
            var where = new List<string>();
            where.AddRange(_whereList);
            where.AddRange(_whereNullList);
            where.AddRange(_whereNotNullList);
            sbWhere.AppendLine($"WHERE {string.Join(" AND ", where)}");
        }

        if (_groupByList.Any())
        {
            sbGroup.AppendLine($"GROUP BY {string.Join(",", _groupByList)}");
        }

        if (_orderList.Any())
        {
            sbOrder.AppendLine($"ORDER BY {string.Join(",", _orderList)}");
        }

        if (_isPaging)
        {
            sbLimit.AppendLine($"OFFSET {_skip} LIMIT {_take}");
        }

        _query = string.Concat(
            //sbOpenBracket.ToString(),
            sbSelect.ToString(),
            sbWhere.ToString(),
            sbGroup.ToString(),
            //sbCloseBracket.ToString(),
            sbOrder.ToString(),
            sbLimit.ToString());

        if (_isSelectRowCount)
        {
            _query = $"SELECT COUNT({COUNT_INITIAL}) {COUNT_INITIAL} FROM ({_query}) as {COUNT_INITIAL}";
        }

        return _query;
    }
}