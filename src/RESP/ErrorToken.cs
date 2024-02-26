using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP;
internal class ErrorToken : RespToken {

    public string Value { get; set; }

    public override RespToken FromRESP(string resp, out int endIndex) {
        Value = resp[1..^2];
        endIndex = resp.Length;
        return this;
    }

    public override string ToRESP() {
        return $"-{Value}\r\n";
    }
}
