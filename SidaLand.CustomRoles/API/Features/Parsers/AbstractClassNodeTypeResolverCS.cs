namespace SidaLand.CustomRoles.API.Features.Parsers
{
    using SidaLand.CustomRoles.API.Features.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    public class AbstractClassNodeTypeResolverCS : INodeDeserializer
    {
        private readonly INodeDeserializer original;
        private readonly ITypeDiscriminatorCS[] typeDiscriminators;

        public AbstractClassNodeTypeResolverCS(INodeDeserializer original, params ITypeDiscriminatorCS[] discriminators)
        {
            this.original = original;
            typeDiscriminators = discriminators;
        }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (!reader.Accept<MappingStart>(out MappingStart? mapping))
            {
                value = null;
                return false;
            }

            IEnumerable<ITypeDiscriminatorCS> supportedTypes = typeDiscriminators.Where(t => t.BaseType == expectedType).ToArray();
            if (!supportedTypes.Any())
            {
                if (original.Deserialize(reader, expectedType, nestedObjectDeserializer, out value))
                {
                    Validator.ValidateObject(value!, new ValidationContext(value!, null, null), true);

                    return true;
                }

                return false;
            }

            Mark? start = reader.Current?.Start;
            Type? actualType;
            ParsingEventBufferCS buffer;
            try
            {
                buffer = new ParsingEventBufferCS(ReadNestedMapping(reader));
                actualType = CheckWithDiscriminators(expectedType, supportedTypes, buffer);
            }
            catch (Exception exception)
            {
                throw new YamlException(start ?? new(), reader.Current?.End ?? new(), "Failed when resolving abstract type", exception);
            }

            buffer.Reset();

            if (original.Deserialize(buffer, actualType!, nestedObjectDeserializer, out value))
            {
                Validator.ValidateObject(value!, new ValidationContext(value!, null, null), true);

                return true;
            }

            return false;
        }

        private static Type? CheckWithDiscriminators(Type expectedType, IEnumerable<ITypeDiscriminatorCS> supportedTypes, ParsingEventBufferCS buffer)
        {
            foreach (ITypeDiscriminatorCS discriminator in supportedTypes)
            {
                buffer.Reset();
                if (!discriminator.TryResolve(buffer, out Type? actualType))
                    continue;

                return actualType;
            }

            throw new Exception(
                $"None of the registered type discriminators could supply a child class for {expectedType}");
        }

        private static LinkedList<ParsingEvent> ReadNestedMapping(IParser reader)
        {
            LinkedList<ParsingEvent> result = new();
            result.AddLast(reader.Consume<MappingStart>());
            int depth = 0;
            do
            {
                ParsingEvent next = reader.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
                result.AddLast(next);
            }
            while (depth >= 0);

            return result;
        }
    }
}
