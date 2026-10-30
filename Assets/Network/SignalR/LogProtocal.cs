using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using NUnit;
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using UnityEngine;

namespace FunBoardGames.Network.SignalR
{
    public class LogProtocal : IHubProtocol
    {
        public LogProtocal(IHubProtocol usedProtocal)
        {
            _usedProtocal = usedProtocal;
        }

        IHubProtocol _usedProtocal;

        static readonly JsonSerializerOptions _prettyOptions = new JsonSerializerOptions { WriteIndented = true };

        public string Name => _usedProtocal.Name;

        public int Version => _usedProtocal.Version;

        public TransferFormat TransferFormat => _usedProtocal.TransferFormat;

        public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
        {
            return _usedProtocal.GetMessageBytes(message);
        }

        public bool IsVersionSupported(int version) => _usedProtocal.IsVersionSupported(version);

        public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
        {
            var before = input;
            if (!_usedProtocal.TryParseMessage(ref input, binder, out message))
                return false;

            if(message is InvocationMessage invokeMessage)
            {
                var consumed = before.Slice(before.Start, input.Start);
                var bytes = consumed.ToArray();
                // drop the trailing 0x1E record separator
                var len = bytes.Length > 0 && bytes[bytes.Length - 1] == 0x1E ? bytes.Length - 1 : bytes.Length;

                using var doc = JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes, 0, len));
                if (doc.RootElement.TryGetProperty("arguments", out var args))
                    Debug.Log($"[<color=#4FC3F7>SignalR Receive</color>] [<color=#FFB74D>{invokeMessage.Target}</color>]\n" + JsonSerializer.Serialize(args, _prettyOptions));

            }

            return true;
        }

        public void WriteMessage(HubMessage message, IBufferWriter<byte> output)
        {
            if (message is InvocationMessage invokeMessage)
            {
                try
                {
                    Debug.Log($"[<color=#81C784>SignalR Send</color>] [<color=#FFB74D>{invokeMessage.Target}</color>]\n" + JsonSerializer.Serialize(invokeMessage.Arguments, _prettyOptions));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SignalR Send] [{invokeMessage.Target}] could not serialize arguments for logging: {ex.Message}");
                }
            }

            _usedProtocal.WriteMessage(message, output);
        }
    }
}