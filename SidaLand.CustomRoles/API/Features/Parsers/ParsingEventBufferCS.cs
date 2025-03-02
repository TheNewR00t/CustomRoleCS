using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace SidaLand.CustomRoles.API.Features.Interfaces
{
    public class ParsingEventBufferCS : IParser
    {
        private readonly LinkedList<ParsingEvent> buffer;

        private LinkedListNode<ParsingEvent>? current;
        public ParsingEventBufferCS(LinkedList<ParsingEvent> events)
        {
            buffer = events;
            current = events.First;
        }
        public ParsingEvent? Current => current?.Value;

        public bool MoveNext()
        {
            current = current?.Next;
            return current is not null;
        }
        public void Reset()
        {
            current = buffer.First;
        }
    }
}
