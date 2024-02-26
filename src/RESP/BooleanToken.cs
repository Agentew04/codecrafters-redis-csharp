using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class BooleanToken : RespToken {

    public bool Value { get; set; }

    public override string ToRESP() {
        return $":{(Value ? 't' : 'f')}\r\n";
    }

    public override RespToken FromRESP(string resp, out int endIndex) {
        if (resp[0] != ':') {
            throw new InvalidDataException("Invalid RESP data");
        }
        Value = resp[1] == 't';
        endIndex = 3;
        return this;
    }
}
