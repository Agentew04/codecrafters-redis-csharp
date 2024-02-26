using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class FileToken : RespToken{
    public int Length { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        Length = ReadSize(resp, 1, out int sizeEndIndex);

        Content = resp[(sizeEndIndex + 2)..(sizeEndIndex + 2 + Length)];
        endIndex = sizeEndIndex + 2 + Length;
        return this;
    }

    public override byte[] ToRESP() {
        List<byte> bytes = new();
        bytes.AddRange($"${Length}\r\n".ToAscii());
        bytes.AddRange(Content);
        return bytes.ToArray();
    }
}
