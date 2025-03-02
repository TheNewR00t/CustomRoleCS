using System;

namespace SidaLand.CustomRoles.API.Features.Interfaces
{
    public interface ITypeDiscriminatorCS
    {
        Type BaseType { get; }
        bool TryResolve(ParsingEventBufferCS buffer, out Type? suggestedType);
    }
}
