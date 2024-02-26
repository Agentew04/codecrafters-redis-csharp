using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class BooleanToken : RespToken {

    public bool Value { get; set; }

    public override byte[] ToRESP() {
        List<byte> bytes = new();
        bytes.AddRange($":{(Value ? 't' : 'f')}\r\n".ToAscii());
        return bytes.ToArray();
    }

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        if (resp[0] != ':') {
            throw new InvalidDataException("Invalid RESP data");
        }
        byte value = resp[1];
        if (value != 't' && value != 'f') {
            throw new InvalidDataException($"Invalid RESP data. Boolean should be 't' or 'f'(was '{(char)value}')");
        }
        Value = resp[1] == 't';
        endIndex = 3;
        return this;
    }
}
