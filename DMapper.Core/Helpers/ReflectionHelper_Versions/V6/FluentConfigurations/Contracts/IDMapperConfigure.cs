using System.Linq.Expressions;

namespace DMapper.Helpers.FluentConfigurations.Contracts;

public interface IDMapperConfigure
{
    IDMapperConfigure Map<TProp>(Expression<Func<object, TProp>> destExpression, params string[] sourceKeys);
    Dictionary<string, List<string>> GetMappings();
}