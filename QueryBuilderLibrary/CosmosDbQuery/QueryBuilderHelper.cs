using System.Text;

namespace QueryBuilderLibrary.CosmosDbQuery;

public interface IQuerySelect
{
    bool IsSelectCount();
    QueryBuilderHelper Distinct();
    QueryBuilderHelper Select(params string[] columns);
    QueryBuilderHelper SelectCount(string? column = null);
    QueryBuilderHelper SelectDistinct(params string[] columns);
    QueryBuilderHelper SelectSumNumberString(params string[] columns);
    QueryBuilderHelper SelectSum(params string[] columns);
    QueryBuilderHelper SelectMax(params string[] columns);
    QueryBuilderHelper SelectRight(int num, params string[] columns);
    QueryBuilderHelper SelectRightLeft(int numRight, int numLeft, params string[] columns);
}
public interface IQueryWhere
{
    QueryBuilderHelper Where(string column, string operation, string value);
    QueryBuilderHelper Where(string column, string operation, int value);
    QueryBuilderHelper Where(string column, string operation, bool value);
    QueryBuilderHelper WhereRight(int num, string column, string operation, string value);
    QueryBuilderHelper WhereRightLeft(int numRight, int numLeft, string column, string operation, string value);
    QueryBuilderHelper WhereNull(string column);
    QueryBuilderHelper WhereNotNull(string column);
    QueryBuilderHelper WhereContains(string column, string value);
    QueryBuilderHelper WhereNotContains(string column, string value);
    QueryBuilderHelper WhereIs(string column, string value);
    QueryBuilderHelper WhereIsNot(string column, string value);
    QueryBuilderHelper WhereIs(string column, int value);
    QueryBuilderHelper WhereIsNot(string column, int value);
    QueryBuilderHelper WhereIs(string column, bool value);
    QueryBuilderHelper WhereIsNot(string column, bool value);
    QueryBuilderHelper WhereRightIs(int num, string column, string value);
    QueryBuilderHelper WhereRightLeftIs(int numRight, int numLeft, string column, string value);
    QueryBuilderHelper WhereIn(string column, params string[] values);
    QueryBuilderHelper WhereIn(string column, params int[] values);
    QueryBuilderHelper WhereNotIn(string column, params string[] values);
    QueryBuilderHelper WhereNotIn(string column, params int[] values);
    QueryBuilderHelper WhereGroup(string operation, Action<IQueryWhere> expression);
}
public interface IQueryOrder
{
    QueryBuilderHelper OrderBy(string column, string? orderMethode = "ASC");
}
public interface IQueryGroup
{
    QueryBuilderHelper GroupBy(string column);
    QueryBuilderHelper GroupBy(params string[] columns);
}
public interface IQueryPagination
{
    QueryBuilderHelper Skip(int skip);
    QueryBuilderHelper Take(int take);
}
public class QueryBuilderHelper : IQuerySelect, IQueryWhere, IQueryOrder, IQueryGroup, IQueryPagination
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
    /// Return generated query string after method Build was executed
    /// </summary>
    public string Query
    {
        get { return _query; }
    }

    /// <summary>
    /// Create an instance of QueryBuilder
    /// </summary>
    /// <returns cref="QueryBuilderHelper"></returns>
    public static QueryBuilderHelper Initialize()
    {
        return new QueryBuilderHelper();
    }

    /// <summary>
    /// Distinct all result
    /// </summary>
    /// <returns></returns>
    public QueryBuilderHelper Distinct()
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
    public QueryBuilderHelper Select(params string[] columns)
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
    /// Register SELECT COUNT clause to count the data only
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public QueryBuilderHelper SelectCount(string? column = null)
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
    public QueryBuilderHelper SelectRowCount()
    {
        _isSelectRowCount = true;
        return this;
    }

    /// <summary>
    /// Register SELECT DISTINCT clause to select specific column that will be returned
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public QueryBuilderHelper SelectDistinct(params string[] columns)
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
    public QueryBuilderHelper SelectSumNumberString(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(SelectSumNumberString)} clause");
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
    public QueryBuilderHelper SelectSum(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(SelectSum)} clause");
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
    public QueryBuilderHelper SelectMax(params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(SelectMax)} clause");
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
    public QueryBuilderHelper SelectRight(int num, params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(SelectRight)} clause");
            else
                _selectMax.Add($"RIGHT({_initialTable}.{columns[i]}, {num}) as {columns[i]}");
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
    public QueryBuilderHelper SelectRightLeft(int numRight, int numLeft, params string[] columns)
    {
        _isSelectDefault = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(SelectRight)} clause");
            else
                _selectMax.Add($"LEFT(RIGHT({_initialTable}.{columns[i]}, {numRight}), {numLeft}) as {columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper Where(string column, string operation, string value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper Where(string column, string operation, int value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation} {value}");
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper Where(string column, string operation, bool value)
    {
        _whereList.Add($"{_initialTable}.{column} {operation} {value.ToString().ToLower()}");
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
    public QueryBuilderHelper WhereRight(int num, string column, string operation, string value)
    {
        _whereList.Add($"RIGHT({_initialTable}.{column}, {num}) {operation} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
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
    public QueryBuilderHelper WhereRightLeft(int numRight, int numLeft, string column, string operation, string value)
    {
        _whereList.Add($"LEFT(RIGHT({_initialTable}.{column}, {numRight}), {numLeft}) {operation} '{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}'");
        return this;
    }

    /// <summary>
    /// Register WHERE null clause condition to filter specific null data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereNull(string column)
    {
        _whereNullList.Add($"{_initialTable}.{column} = null");
        return this;
    }

    /// <summary>
    /// Register WHERE <> null clause condition to filter specific not null data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereNotNull(string column)
    {
        _whereNotNullList.Add($"{_initialTable}.{column} <> null");
        return this;
    }

    /// <summary>
    /// Register WHERE LIKE clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereContains(string column, string value)
    {
        _whereList.Add($"{_initialTable}.{column} LIKE '%{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}%'");
        return this;
    }

    /// <summary>
    /// Register WHERE NOT LIKE clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereNotContains(string column, string value)
    {
        _whereList.Add($"{_initialTable}.{column} NOT LIKE '%{value.Replace(@"\", @"\\").Replace(@"'", @"\'")}%'");
        return this;
    }

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIs(string column, string value) => Where(column, "=", value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIsNot(string column, string value) => Where(column, "<>", value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIs(string column, int value) => Where(column, "=", value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIsNot(string column, int value) => Where(column, "<>", value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIs(string column, bool value) => Where(column, "=", value);

    /// <summary>
    /// Register WHERE not matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIsNot(string column, bool value) => Where(column, "<>", value);

    /// <summary>
    /// Register WHERE clause condition to filter matched data for (n) last character value
    /// </summary>
    /// <param name="num"></param>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereRightIs(int num, string column, string value) => WhereRight(num, column, "=", value);

    /// <summary>
    /// Register WHERE clause condition to filter matched data for character value between (n) first character and (n) last character
    /// </summary>
    /// <param name="num"></param>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereRightLeftIs(int numRight, int numLeft, string column, string value) => WhereRightLeft(numRight, numLeft, column, "=", value);

    /// <summary>
    /// Register WHERE IN clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="column"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public QueryBuilderHelper WhereIn(string column, params string[] values)
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
    public QueryBuilderHelper WhereIn(string column, params int[] values)
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
    public QueryBuilderHelper WhereNotIn(string column, params string[] values)
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
    public QueryBuilderHelper WhereNotIn(string column, params int[] values)
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
    public QueryBuilderHelper WhereGroup(string operation, Action<IQueryWhere> expression)
    {
        var qb = Initialize();
        expression(qb);
        _whereList.Add("(" + string.Join($" {operation} ", qb._whereList) + ")");
        return this;
    }

    /// <summary>
    /// Register ORDER BY statement to sorting the data by specific column
    /// </summary>
    /// <param name="column"></param>
    /// <param name="orderMethode"></param>
    /// <returns></returns>
    public QueryBuilderHelper OrderBy(string column, string? orderMethode = "ASC")
    {
        orderMethode = (orderMethode ?? "ASC").ToUpper() == "ASC" || (orderMethode ?? "ASC").ToUpper() == "DESC" ? (orderMethode ?? "ASC").ToUpper() : "ASC";
        _orderList.Add($"{_initialTable}.{column} {orderMethode}");
        return this;
    }

    /// <summary>
    /// Register GROUP BY statement to sorting the data by specific column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public QueryBuilderHelper GroupBy(string column)
    {
        if (column == "*")
            throw new ArgumentException($"Cannot use asteric * from {nameof(GroupBy)} clause");
        else
            _groupByList.Add($"{_initialTable}.{column}");
        return this;
    }

    /// <summary>
    /// Register GROUP BY statement to grouping the data by specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public QueryBuilderHelper GroupBy(params string[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] == "*")
                throw new ArgumentException($"Cannot use asteric * from {nameof(GroupBy)} clause");
            else
                _groupByList.Add($"{_initialTable}.{columns[i]}");
        }
        return this;
    }

    /// <summary>
    /// Register OFFSET statement to skip number of record
    /// </summary>
    /// <param name="skip"></param>
    /// <returns></returns>
    public QueryBuilderHelper Skip(int skip)
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
    public QueryBuilderHelper Take(int take)
    {
        _isPaging = true;
        _take = take;
        return this;
    }

    /// <summary>
    /// Check if builder statement was registered for counting data or not
    /// </summary>
    /// <returns></returns>
    public bool IsSelectCount()
    {
        return _isSelectCount;
    }

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
            throw new ArgumentException($"Method {nameof(QueryBuilderHelper.Select)} cannot combine with method {nameof(QueryBuilderHelper.SelectCount)}");

        if (_isSelectCount && _isSelectDistinct)
            throw new ArgumentException($"Method {nameof(QueryBuilderHelper.SelectDistinct)} cannot combine with method {nameof(QueryBuilderHelper.SelectCount)}, please use method {nameof(QueryBuilderHelper.Select)} with {nameof(QueryBuilderHelper.GroupBy)} instead");

        if (_isSelectDefault && _isSelectDistinct)
            throw new ArgumentException($"Method {nameof(QueryBuilderHelper.Select)} cannot combine with method {nameof(QueryBuilderHelper.SelectDistinct)}, please use one of them only or use method {nameof(Distinct)} to make all distinct");

        if (_groupByList.Any() && !Enumerable.SequenceEqual(_groupByList, _select))
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
                sbSelect.AppendLine($"SELECT COUNT(1) _count FROM {_initialTable}");
            }
            else
            {
                _selectCount = _selectCount.EndsWith(",") ? _selectCount[..^1] : _selectCount;
                sbSelect.AppendLine($"SELECT {_selectCount} FROM {_initialTable}");
            }
        }
        else if (!_isSelectDistinct)
        {
            var select = new List<string>();
            select.AddRange(_select);
            select.AddRange(_selectSum);
            select.AddRange(_selectMax);
            if (!select.Any())
            {
                sbSelect.AppendLine($"SELECT * FROM {_initialTable}");
            }
            else
            {
                sbSelect.AppendLine($"SELECT {string.Join(",", select)} FROM {_initialTable}");
            }
        }
        else
        {
            var select = new List<string>();
            select.AddRange(_select);
            select.AddRange(_selectSum);
            select.AddRange(_selectMax);
            if (!select.Any())
            {
                sbSelect.AppendLine($"SELECT DISTINCT * FROM {_initialTable}");
            }
            else
            {
                sbSelect.AppendLine($"SELECT DISTINCT {string.Join(",", select)} FROM {_initialTable}");
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