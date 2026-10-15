using System.Data;
using Dapper;

namespace src.Data.TypeHandlers;

internal sealed class ListTypeHandler<T> : SqlMapper.TypeHandler<List<T>>
{
    public override List<T> Parse(object value)
    {
        return ((T[])value).ToList();
    }

    public override void SetValue(IDbDataParameter parameter, List<T>? value)
    {
        parameter.Value = value?.ToArray();
    }
}
