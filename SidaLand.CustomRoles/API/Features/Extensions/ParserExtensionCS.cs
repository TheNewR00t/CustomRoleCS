using SidaLand.CustomRoles.API.Features.Interfaces;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace SidaLand.CustomRoles.API.Features.Extensions
{
    public static class ParserExtensionCS
    {

        public static bool TryFindMappingEntry(this ParsingEventBufferCS parser, Func<Scalar, bool> selector, out Scalar? key, out ParsingEvent? value)
        {
            parser.Consume<MappingStart>();
            do
            {
                switch (parser.Current)
                {
                    case Scalar scalar:
                        bool keyMatched = selector(scalar);
                        parser.MoveNext();
                        if (keyMatched)
                        {
                            value = parser.Current;
                            key = scalar;
                            return true;
                        }

                        parser.SkipThisAndNestedEvents();
                        break;
                    case MappingStart _:
                    case SequenceStart _:
                        parser.SkipThisAndNestedEvents();
                        break;
                    default:
                        parser.MoveNext();
                        break;
                }
            }
            while (parser.Current is not null);

            key = null;
            value = null;
            return false;
        }
    }
}
