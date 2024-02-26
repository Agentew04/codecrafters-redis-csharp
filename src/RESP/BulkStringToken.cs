using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP;
public class BulkStringToken : RespToken {

    public int Length { get; set; }
    public string Value { get; set; } = "";

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        ReadSize(resp, 1, out int sizeEndIndex);
        
        byte[] sizeBytes = resp[1..sizeEndIndex];
        string sizeStr = sizeBytes.FromAscii();
        Length = int.Parse(sizeStr);
        byte[] valueBytes = resp[(sizeEndIndex + 2)..(sizeEndIndex + 2 + Length)];
        Value = valueBytes.FromAscii();
        endIndex = sizeEndIndex + 2 + Length + 2;
        return this;
    }

    public override byte[] ToRESP() {
        List<byte> bytes = new();
        bytes.AddRange($"${Length}\r\n".ToAscii());
        bytes.AddRange(Value.ToAscii());
        bytes.AddRange("\r\n".ToAscii());
        return bytes.ToArray();
    }

    public static BulkStringToken FromString(string value) {
        return new BulkStringToken() {
            Length = value.Length,
            Value = value
        };
    }
}
