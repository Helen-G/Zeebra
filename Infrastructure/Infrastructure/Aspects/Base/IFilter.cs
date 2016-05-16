using System;

namespace AFT.RegoV2.Infrastructure.Aspects.Base
{
    public interface IFilter<T>
    {
        T Filter(T data, Guid userId);
    }
}
