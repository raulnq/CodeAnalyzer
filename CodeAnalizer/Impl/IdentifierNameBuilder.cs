using System;
using CodeAnalizer.Interfaces;

namespace CodeAnalizer.Impl
{
    public class IdentifierNameBuilder : IIdentifierNameBuilder
    {
        public string Build()
        {
            return "temp" + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}