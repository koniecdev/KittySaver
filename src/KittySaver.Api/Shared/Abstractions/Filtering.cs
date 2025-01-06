using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Contracts;

namespace KittySaver.Api.Shared.Abstractions;

public interface IPropertyFilter<TEntity>
{
    bool MatchesProperty(string propertyName);
    IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value);
}
public abstract class PropertyFilterBase<TEntity, TProperty>
{
    protected readonly Expression<Func<TEntity, TProperty>> PropertySelector;
    protected readonly string PropertyName;

    protected PropertyFilterBase(Expression<Func<TEntity, TProperty>> propertySelector, string propertyName)
    {
        PropertySelector = propertySelector;
        PropertyName = propertyName;
    }

    public bool MatchesProperty(string propertyName) =>
        string.Equals(PropertyName, propertyName, StringComparison.CurrentCultureIgnoreCase);

    protected abstract Expression CreateFilterExpression(
        Expression property,
        FilterCriteria.FilterOperation operation,
        Expression constantValue);

    protected IQueryable<TEntity> ApplyFilterExpression(
        IQueryable<TEntity> query,
        FilterCriteria.FilterOperation operation,
        TProperty value)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var property = Expression.Invoke(PropertySelector, parameter);
        var constant = Expression.Constant(value);

        var comparison = CreateFilterExpression(property, operation, constant);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        
        return query.Where(lambda);
    }
}

// Implementation for string properties
public class StringPropertyFilter<TEntity> : PropertyFilterBase<TEntity, string>, IPropertyFilter<TEntity>
{
    public StringPropertyFilter(Expression<Func<TEntity, string>> propertySelector)
        : base(propertySelector, ExpressionHelper.GetPropertyName(propertySelector))
    {
    }

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
                typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                constantValue),
            FilterCriteria.FilterOperation.Nin => Expression.Not(Expression.Call(property,
                typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                constantValue)),
            _ => throw new ArgumentException($"Unsupported string operation: {operation}")
        };
    }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value)
        => ApplyFilterExpression(query, operation, value);
}

// Implementation for numeric properties
public class NumericPropertyFilter<TEntity, TNumber> : PropertyFilterBase<TEntity, TNumber>, IPropertyFilter<TEntity>
    where TNumber : INumber<TNumber>
{
    public NumericPropertyFilter(Expression<Func<TEntity, TNumber>> propertySelector)
        : base(propertySelector, ExpressionHelper.GetPropertyName(propertySelector))
    {
    }

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
            _ => throw new ArgumentException($"Unsupported numeric operation: {operation}")
        };
    }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, FilterCriteria.FilterOperation operation, string value)
    {
        if (!TNumber.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
        {
            throw new ArgumentException($"Cannot parse '{value}' as {typeof(TNumber).Name}");
        }
        
        return ApplyFilterExpression(query, operation, parsedValue);
    }
}
public static class ExpressionHelper
{
    public static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        return expression.Body switch
        {
            // Handle the case of direct member access
            MemberExpression memberExpression => memberExpression.Member.Name,
            // Handle the case of conversions (like implicit numeric conversions)
            UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression memberExpr => memberExpr
                .Member.Name,
            _ => throw new ArgumentException("Expression must be a simple property access", nameof(expression))
        };
    }
}