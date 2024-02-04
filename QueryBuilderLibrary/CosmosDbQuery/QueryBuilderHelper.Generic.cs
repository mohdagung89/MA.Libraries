/*
 * QueryBuilderHelper
 * Author: MohdAgung
 * Version: v1.2.0
 * Description: A powerful query building library for CosmosDb database.
 * Source: https://github.com/mohdagung89/MA.Libraries
 */

using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace QueryBuilderLibrary.CosmosDbQuery;

public interface IQuerySelect<T> : IQuerySelect
{
    IQueryBuilder<T> Select(Func<T, List<string>> delegateFunc);
    IQueryBuilder<T> SelectAs(Func<T, string> delegateFunc, string alias);
    IQueryBuilder<T> SelectAs<TOutput>(Func<T, string> delegateFunc, Func<TOutput, string> delegateFuncOutput);
    IQueryBuilder<T> SelectCount(Func<T, string>? delegateFunc = null);
    IQueryBuilder<T> SelectRowCount();
    IQueryBuilder<T> SelectDistinct(Func<T, List<string>> delegateFunc);
    IQueryBuilder<T> SelectSum(Func<T, List<string>> delegateFunc);
    new IQueryBuilder<T> SelectRawFunctionAs(string rawFunction, string alias);
    IQueryBuilder<T> SelectRawFunctionAs<TOutput>(string rawFunction, Func<TOutput, string> delegateFunc);
}
public interface IQueryWhere<T> : IQueryWhere
{
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, string value);
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, int value);
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, float value);
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, decimal value);
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, bool value);
    IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, DateTime value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, string value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, int value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, float value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, decimal value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, bool value);
    IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, DateTime value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, string value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, int value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, float value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, decimal value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, bool value);
    IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, DateTime value);
    IQueryBuilder<T> WhereIn(Func<T, string> delegateFunc, params string[] values);
    IQueryBuilder<T> WhereIgnoreCaseIs(Func<T, string> delegateFunc, string value);
    IQueryBuilder<T> WhereContains(Func<T, string> delegateFunc, string value);
    IQueryBuilder<T> WhereNotContains(Func<T, string> delegateFunc, string value);
}
public interface IQueryOrder<T> : IQueryOrder
{
    IQueryBuilder<T> OrderBy(Func<T, string> delegateFunc, string? orderMethod = SORTING_FILTER.ASCENDING);
}
public interface IQueryGroup<T> : IQueryGroup
{
    IQueryBuilder<T> GroupBy(Func<T, List<string>> delegateFunc);
}
public interface ISubQuery<T> : ISubQuery
{
    IQueryBuilder<T> FromSubQuery(Action<IQueryBuilder<T>> query);
}
public interface IQueryBuilder<T> : IQueryBuilder, IQuerySelect<T>, IQueryWhere<T>, IQueryGroup<T>, ISubQuery<T>
{
}

/// <summary>
/// QueryBuilderHelper class for building dynamic queries.
/// </summary>
[Description("v1.2.0")]
public class QueryBuilderHelper<T> : QueryBuilderHelper, IQueryBuilder<T>
{
    private T _model;
    private Type _modelType;
    private PropertyInfo[] _propertyInfo;

    private QueryBuilderHelper()
    {
        _model = Activator.CreateInstance<T>();
        _modelType = typeof(T);
        _propertyInfo = _modelType.GetProperties();
    }

    /// <summary>
    /// Create an instance of <see cref="QueryBuilderHelper{T}"/>
    /// </summary>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public static new QueryBuilderHelper<T> Initialize()
    {
        return new QueryBuilderHelper<T>();
    }

    #region ISubQuery
    /// <summary>
    /// [Beta Version] Creates a nested query using the FromSubQuery method.
    /// </summary>
    /// <param name="query">An Action that defines the nested query using a QueryBuilderHelper instance of type T.</param>
    /// <returns>The current QueryBuilderHelper instance with generic type T.</returns>
    public IQueryBuilder<T> FromSubQuery(Action<IQueryBuilder<T>> query)
    {
        var qb = Initialize();
        query(qb);
        _child = qb;
        return this;
    }
    #endregion

    #region IQuerySelect
    /// <summary>
    /// Register SELECT clause to select specific column that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Select(Func<T, List<string>> delegateFunc)
    {
        var columns = GetPropertyNames(delegateFunc(_model).ToArray());
        Select(columns);
        return this;
    }

    /// <summary>
    /// Registers a SELECT clause to specify a specific column with an optional alias for the result.
    /// </summary>
    /// <param name="delegateFunc">A delegate function that specifies the column to be selected based on the generic type T.</param>
    /// <param name="alias">The optional alias for the selected column.</param>
    /// <returns>The current QueryBuilderHelper instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the column is "*", as using an asterisk (*) is not allowed in the {nameof(SelectAs)} clause.</exception>
    public IQueryBuilder<T> SelectAs(Func<T, string> delegateFunc, string alias)
    {
        var column = GetPropertyName(delegateFunc(_model));
        SelectAs(column, alias);
        return this;
    }

    /// <summary>
    /// Registers a SELECT clause to specify a specific column with an optional alias for the result.
    /// </summary>
    /// <typeparam name="TOutput">The output type for the selected column.</typeparam>
    /// <param name="delegateFunc">A delegate function that specifies the column to be selected based on the generic type T.</param>
    /// <param name="delegateFuncOutput">A delegate function that specifies the output column based on the generic type TOutput.</param>
    /// <returns>The current QueryBuilderHelper instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the column is "*", as using an asterisk (*) is not allowed in the {nameof(SelectAs)} clause.</exception>
    public IQueryBuilder<T> SelectAs<TOutput>(Func<T, string> delegateFunc, Func<TOutput, string> delegateFuncOutput)
    {
        var column = GetPropertyName(delegateFunc(_model));
        var alias = GetPropertyName<TOutput>(delegateFuncOutput(Activator.CreateInstance<TOutput>()));
        SelectAs(column, alias);
        return this;
    }

    /// <summary>
    /// Register SELECT COUNT clause to count the data only
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> SelectCount(Func<T, string>? delegateFunc = null)
    {
        var column = delegateFunc?.Invoke(_model);
        SelectCount(column);
        return this;
    }

    /// <summary>
    /// Register SELECT COUNT clause to count the data only
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public new IQueryBuilder<T> SelectRowCount()
    {
        base.SelectRowCount();
        return this;
    }

    /// <summary>
    /// Register SELECT DISTINCT clause to select specific column that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> SelectDistinct(Func<T, List<string>> delegateFunc)
    {
        var columns = GetPropertyNames(delegateFunc(_model).ToArray());
        SelectDistinct(columns);
        return this;
    }

    /// <summary>
    /// Register SELECT SUM clause to select sum of number for specific column
    /// </summary>
    /// <param name="columns"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> SelectSum(Func<T, List<string>> delegateFunc)
    {
        var columns = GetPropertyNames(delegateFunc(_model).ToArray());
        SelectSum(columns);
        return this;
    }

    /// <summary>
    /// [Beta Version] Register SELECT raw, example rawFunction "sum(iif(c.score=1,1,0))" and alias "scoreTotal"
    /// </summary>
    /// <param name="rawFunction"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public new IQueryBuilder<T> SelectRawFunctionAs(string rawFunction, string alias)
    {
        base.SelectRawFunctionAs(rawFunction, alias);
        return this;
    }

    /// <summary>
    /// [Beta Version] Register SELECT raw, example rawFunction "sum(iif(c.score=1,1,0))" and alias "x => nameof(x.ScoreTotal)"
    /// </summary>
    /// <param name="rawFunction"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IQueryBuilder<T> SelectRawFunctionAs<TOutput>(string rawFunction, Func<TOutput, string> delegateFunc)
    {
        var alias = GetPropertyName<TOutput>(delegateFunc(Activator.CreateInstance<TOutput>()));
        base.SelectRawFunctionAs(rawFunction, alias);
        return this;
    }
    #endregion

    #region IQueryWhere
    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, string value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, int value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, float value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, decimal value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, bool value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> Where(Func<T, string> delegateFunc, Operation operation, DateTime value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        Where(column, operation, value);
        return this;
    }

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, string value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, int value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, float value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, decimal value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, bool value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIs(Func<T, string> delegateFunc, DateTime value) => Where(delegateFunc, Operation.Equal, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, string value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, int value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, float value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, decimal value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, bool value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE matched clause condition to filter not matched data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIsNot(Func<T, string> delegateFunc, DateTime value) => Where(delegateFunc, Operation.NotEqual, value);

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIn(Func<T, string> delegateFunc, string[] values)
    {
        var column = GetPropertyName(delegateFunc(_model));
        WhereIn(column, values);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned <br/>
    /// Ignore case sensitive
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereIgnoreCaseIs(Func<T, string> delegateFunc, string value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        WhereIgnoreCase(column, Operation.Equal, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned <br/>
    /// Ignore case sensitive
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereContains(Func<T, string> delegateFunc, string value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        WhereContains(column, value);
        return this;
    }

    /// <summary>
    /// Register WHERE clause condition to filter specific data that will be returned <br/>
    /// Ignore case sensitive
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> WhereNotContains(Func<T, string> delegateFunc, string value)
    {
        var column = GetPropertyName(delegateFunc(_model));
        WhereNotContains(column, value);
        return this;
    }
    #endregion

    #region IQueryOrder
    /// <summary>
    /// Register ORDER BY statement to sorting the data by specific column
    /// </summary>
    /// <param name="column"></param>
    /// <param name="orderMethode"></param>
    /// <returns></returns>
    public IQueryBuilder<T> OrderBy(Func<T, string> delegateFunc, string? orderMethode = SORTING_FILTER.ASCENDING)
    {

        var column = GetPropertyName(delegateFunc(_model));
        OrderBy(column, orderMethode);
        return this;
    }
    #endregion

    #region IQueryGroup
    /// <summary>
    /// Register GROUP BY statement to grouping the data by specific column
    /// </summary>
    /// <param name="delegateFunc"></param>
    /// <returns><see cref="QueryBuilderHelper{T}"/></returns>
    public IQueryBuilder<T> GroupBy(Func<T, List<string>> delegateFunc)
    {
        var columns = GetPropertyNames(delegateFunc(_model).ToArray());
        GroupBy(columns);
        return this;
    }
    #endregion

    #region Private Helper
    /// <summary>
    /// Get list attribute property name  if exist
    /// </summary>
    /// <param name="columns"></param>
    /// <returns>list of attribute property name or columns it self</returns>
    private string[] GetPropertyNames(params string[] columns) => columns.Select(x => GetPropertyName(x)).ToArray();

    /// <summary>
    /// Gets the attribute property name if it exists.
    /// </summary>
    /// <param name="column">The input column name.</param>
    /// <returns>An attribute property name if found, otherwise returns the input column name itself.</returns>
    private string GetPropertyName(string column)
    {
        var jsonProperty = _propertyInfo.FirstOrDefault(x => x.Name == column)?.GetCustomAttribute<JsonPropertyAttribute>();

        if (jsonProperty != null && jsonProperty.PropertyName != null)
        {
            return jsonProperty.PropertyName;
        }
        else
        {
            return column;
        }
    }

    /// <summary>
    /// Gets a list of attribute property names if they exist for a specified target type.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="columns">Array of input column names.</param>
    /// <returns>An array of attribute property names if found, otherwise returns the input column names themselves.</returns>
    private static string[] GetPropertyNames<TTarget>(params string[] columns)
    {
        PropertyInfo[] propertyInfo = QueryBuilderHelper<T>._GetPropertyInfo<TTarget>();

        return columns.Select(x => QueryBuilderHelper<T>._GetPropertyName(propertyInfo, x)).ToArray();
    }

    /// <summary>
    /// Gets the attribute property name for a specified target type if it exists.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="column">The input column name.</param>
    /// <returns>An attribute property name if found, otherwise returns the input column name itself.</returns>
    private static string GetPropertyName<TTarget>(string column)
    {
        PropertyInfo[] propertyInfo = QueryBuilderHelper<T>._GetPropertyInfo<TTarget>();

        return QueryBuilderHelper<T>._GetPropertyName(propertyInfo, column);
    }

    /// <summary>
    /// Gets the attribute property name for a specified target type if it exists.
    /// </summary>
    /// <param name="propertyInfos">Array of PropertyInfo for the target type.</param>
    /// <param name="column">The input column name.</param>
    /// <returns>An attribute property name if found, otherwise returns the input column name itself.</returns>
    private static string _GetPropertyName(PropertyInfo[] propertyInfo, string column)
    {
        var jsonProperty = propertyInfo.FirstOrDefault(x => x.Name == column)?.GetCustomAttribute<JsonPropertyAttribute>();

        if (jsonProperty != null && jsonProperty.PropertyName != null)
        {
            return jsonProperty.PropertyName;
        }
        else
        {
            return column;
        }
    }

    /// <summary>
    /// Gets an array of PropertyInfo for a specified target type.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <returns>An array of PropertyInfo for the target type.</returns>
    private static PropertyInfo[] _GetPropertyInfo<TTarget>()
    {
        return typeof(TTarget).GetProperties();
    }
    #endregion
}
