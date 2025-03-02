using Exiled.API.Features;
using SidaLand.CustomRoles.API.Features.Extensions;
using SidaLand.CustomRoles.API.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace SidaLand.CustomRoles.API.Features.Parsers
{
    public class AggregateExceptionTypeResolveCS<T> : ITypeDiscriminatorCS
        where T : class
    {
        private const string TargetKey = nameof(CustomAbilityCS.AbilityType);
        private readonly string targetKey;
        private readonly Dictionary<string, Type?> typeLookup;

        public AggregateExceptionTypeResolveCS(INamingConvention namingConvention)
        {
            targetKey = namingConvention.Apply(TargetKey);
            typeLookup = new Dictionary<string, Type?>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type? t in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T))))
                        typeLookup.Add(t.Name, t);
                }
                catch (Exception e)
                {
                    Log.Error($"Error loading types for {assembly.FullName}. It can be ignored if it's not using Exiled.CustomRoles.");
                    Log.Debug(e);
                }
            }
        }

        public Type BaseType => throw new NotImplementedException();

        public bool TryResolve(ParsingEventBufferCS buffer, out Type? suggestedType)
        {
            if (buffer.TryFindMappingEntry(scalar => targetKey == scalar.Value,out Scalar? key,out ParsingEvent? value))
            {
                if (value is Scalar valueScalar)
                {
                    suggestedType = CheckName(valueScalar.Value);

                    return true;
                }

                FailEmpty();
            }

            suggestedType = null;
            return false;
        }

        private void FailEmpty()
        {
            throw new Exception($"Could not determine expectation type, {targetKey} has an empty value");
        }

        private Type? CheckName(string value)
        {
            if (typeLookup.TryGetValue(value, out Type? childType))
                return childType;

            string known = string.Join(",", typeLookup.Keys);
            throw new Exception(
                $"Could not match `{targetKey}: {value}` to a known expectation. Expecting one of: {known}");
        }
    }
}
