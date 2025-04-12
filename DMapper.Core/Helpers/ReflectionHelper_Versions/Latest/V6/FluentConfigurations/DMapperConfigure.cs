using System.Linq.Expressions;
using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Helpers.FluentConfigurations;

public class DMapperConfigure : IDMapperConfigure
{
    private readonly Dictionary<string, List<string>> _mappings = new(StringComparer.OrdinalIgnoreCase);

    public IDMapperConfigure Map<TProp>(
        Expression<Func<object, TProp>> destExpression,
        params string[] sourceKeys)
    {
        if (destExpression.Body is MemberExpression memberExpr)
        {
            string destPath = GetFullPropertyPath(memberExpr);
            // Now destPath should be "Source2.DestinationName3" instead of just "DestinationName3"
            _mappings[destPath] = sourceKeys.ToList();
        }
        return this;
    }

    public Dictionary<string, List<string>> GetMappings() => _mappings;
    
    private string GetFullPropertyPath(Expression expression)
    {
        if (expression is MemberExpression memberExpr)
        {
            string parentPath = GetFullPropertyPath(memberExpr.Expression);
            if (!string.IsNullOrEmpty(parentPath))
            {
                return $"{parentPath}.{memberExpr.Member.Name}";
            }
            return memberExpr.Member.Name;
        }
        else if (expression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
        {
            // Unwrap the conversion
            return GetFullPropertyPath(unaryExpr.Operand);
        }
        return string.Empty;
    }
}