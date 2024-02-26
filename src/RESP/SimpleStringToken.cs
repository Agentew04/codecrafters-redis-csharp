using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
internal class SimpleStringToken : RespToken {

    public string Value { get; set; } = "";

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        Value = resp[1..^2].FromAscii();
        endIndex = resp.Length;
        return this;
    }

    public override byte[] ToRESP() {
        return $"+{Value}\r\n".ToAscii();
    }
}
