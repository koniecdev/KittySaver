using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace KittySaver.Api.Shared.CollectionsQueriesFiltering;

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

public class StringPropertyFilter<TEntity>(Expression<Func<TEntity, string?>> propertySelector)
    : PropertyFilterBase<TEntity, string?>(propertySelector, ExpressionHelper.GetPropertyName(propertySelector)),
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

public class ComparablePropertyFilter<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
    : PropertyFilterBase<TEntity, TProperty>(propertySelector, ExpressionHelper.GetPropertyName(propertySelector)),
        IPropertyFilter<TEntity>
    where TProperty : IComparable<TProperty>
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
            _ => Expression.Equal(property, constantValue)
        };
    }

    public virtual IQueryable<TEntity> ApplyFilter(
        IQueryable<TEntity> query,
        FilterCriteria.FilterOperation operation,
        string value)
    {
        var typedValue = ParseValue(value);
        return ApplyFilterExpression(query, operation, typedValue);
    }

    protected virtual TProperty ParseValue(string value)
    {
        try
        {
            if (typeof(TProperty) == typeof(Guid))
            {
                return (TProperty)(object)Guid.Parse(value);
            }
            if (typeof(TProperty) == typeof(DateTimeOffset))
            {
                return (TProperty)(object)DateTimeOffset.Parse(value);
            }
            if (typeof(TProperty) == typeof(DateTime))
            {
                return (TProperty)(object)DateTime.Parse(value);
            }

            MethodInfo? parseMethod = typeof(TProperty).GetMethod("Parse", [typeof(string)]);
            if (parseMethod is not null)
            {
                return (TProperty)parseMethod.Invoke(null, [value])!;
            }

            throw new NotSupportedException($"No parsing method found for type {typeof(TProperty).Name}");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Cannot parse '{value}' as {typeof(TProperty).Name}", ex);
        }
    }
}

public class DateTimeOffsetPropertyFilter<TEntity>(Expression<Func<TEntity, DateTimeOffset>> propertySelector)
    : ComparablePropertyFilter<TEntity, DateTimeOffset>(propertySelector)
{
    protected override DateTimeOffset ParseValue(string value)
    {
        return value.ToLower() switch
        {
            _ => DateTimeOffset.Parse(value)
        };
    }
}

public class GuidPropertyFilter<TEntity>(Expression<Func<TEntity, Guid>> propertySelector)
    : ComparablePropertyFilter<TEntity, Guid>(propertySelector)
{
    protected override Guid ParseValue(string value)
    {
        return value.Equals("empty", StringComparison.CurrentCultureIgnoreCase) ? Guid.Empty : Guid.Parse(value);
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