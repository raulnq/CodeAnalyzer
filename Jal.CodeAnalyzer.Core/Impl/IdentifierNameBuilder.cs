using System;
using Jal.CodeAnalyzer.Core.Interfaces;

namespace Jal.CodeAnalyzer.Core.Impl
{
    public class IdentifierNameBuilder : IIdentifierNameBuilder
    {
        public string Build()
        {
            return "temp" + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}