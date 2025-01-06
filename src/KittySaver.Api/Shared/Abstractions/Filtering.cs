using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using KittySaver.Api.Shared.Contracts;

namespace KittySaver.Api.Shared.Abstractions;

public interface IPropertyFilter<TEntity>
{
    bool MatchesProperty(string propertyName);
    IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value);
}

public abstract class PropertyFilterBase<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, string name)
{
    public bool MatchesProperty(string propertyName) =>
        string.Equals(name, propertyName, StringComparison.CurrentCultureIgnoreCase);

    protected abstract Expression CreateFilterExpression(
        Expression property,
        FilterCriteria.FilterOperation operation,
        Expression constantValue);

    protected IQueryable<TEntity> ApplyFilterExpression(
        IQueryable<TEntity> query,
        FilterCriteria.FilterOperation operation,
        TProperty value)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
        InvocationExpression property = Expression.Invoke(propertySelector, parameter);
        ConstantExpression constant = Expression.Constant(value);

        Expression comparison = CreateFilterExpression(property, operation, constant);
        Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        
        return query.Where(lambda);
    }
}

public class StringPropertyFilter<TEntity>(Expression<Func<TEntity, string>> propertySelector)
    : PropertyFilterBase<TEntity, string>(propertySelector, ExpressionHelper.GetPropertyName(propertySelector)),
        IPropertyFilter<TEntity>
{
    protected override Expression CreateFilterExpression(
        Expression property,
        FilterCriteria.FilterOperation operation,
        Expression constantValue)
    {
        return operation switch
        {
            FilterCriteria.FilterOperation.Eq => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Neq => Expression.NotEqual(property, constantValue),
            FilterCriteria.FilterOperation.In => Expression.Call(property,
                typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!,
                constantValue),
            FilterCriteria.FilterOperation.Nin => Expression.Not(Expression.Call(property,
                typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!,
                constantValue)),
            FilterCriteria.FilterOperation.Gt => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Gte => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Lt => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Lte => Expression.Equal(property, constantValue),
            _ => throw new ArgumentException($"Unsupported string operation: {operation}")
        };
    }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value)
        => ApplyFilterExpression(query, operation, value);
}

public class NumericPropertyFilter<TEntity, TNumber>(Expression<Func<TEntity, TNumber>> propertySelector)
    : PropertyFilterBase<TEntity, TNumber>(propertySelector, ExpressionHelper.GetPropertyName(propertySelector)),
        IPropertyFilter<TEntity>
    where TNumber : INumber<TNumber>
{
    protected override Expression CreateFilterExpression(
        Expression property,
        FilterCriteria.FilterOperation operation,
        Expression constantValue)
    {
        return operation switch
        {
            FilterCriteria.FilterOperation.Eq => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Neq => Expression.NotEqual(property, constantValue),
            FilterCriteria.FilterOperation.Gt => Expression.GreaterThan(property, constantValue),
            FilterCriteria.FilterOperation.Gte => Expression.GreaterThanOrEqual(property, constantValue),
            FilterCriteria.FilterOperation.Lt => Expression.LessThan(property, constantValue),
            FilterCriteria.FilterOperation.Lte => Expression.LessThanOrEqual(property, constantValue),
            FilterCriteria.FilterOperation.In => Expression.Equal(property, constantValue),
            FilterCriteria.FilterOperation.Nin => Expression.NotEqual(property, constantValue),
            _ => throw new ArgumentException($"Unsupported numeric operation: {operation}")
        };
    }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value)
    {
        return !TNumber.TryParse(value, CultureInfo.InvariantCulture, out TNumber? parsedValue) 
            ? query 
            : ApplyFilterExpression(query, operation, parsedValue);
    }
}
public static class ExpressionHelper
{
    public static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        return expression.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression { Operand: MemberExpression memberExpr } => memberExpr.Member.Name,
            _ => ""
        };
    }
}

public static class FilterExtensions
{
    public static IQueryable<TEntity> ApplyFilters<TEntity>(
        this IQueryable<TEntity> query,
        IEnumerable<FilterCriteria> filterCriteria,
        IEnumerable<IPropertyFilter<TEntity>> filters)
    {
        List<IPropertyFilter<TEntity>> availableFilters = filters.ToList();

        foreach (FilterCriteria criteria in filterCriteria)
        {
            IPropertyFilter<TEntity>? filter = availableFilters.FirstOrDefault(f => f.MatchesProperty(criteria.PropertyName));
            if (filter == null)
            {
                continue;
            }

            query = filter.ApplyFilter(query, criteria.Operation, criteria.Value);
        }

        return query;
    }
}