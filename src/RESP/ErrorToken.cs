using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP;
internal class ErrorToken : RespToken {

    public string Value { get; set; } = "";

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        Value = resp[1..^2].FromAscii();
        endIndex = resp.Length;
        return this;
    }

    public override byte[] ToRESP() {
        List<byte> bytes = new();
        bytes.AddRange($"-{Value}\r\n".ToAscii());
        return bytes.ToArray();
    }
}
