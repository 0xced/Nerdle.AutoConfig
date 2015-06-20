using System;
using System.Xml.Linq;

namespace Nerdle.AutoConfig.Mappers
{
    public interface IMapper
    {
        object Map(XElement element, Type type);
    }

    interface IQueryableMapper : IMapper
    {
        bool CanMap(Type type);
    }
}