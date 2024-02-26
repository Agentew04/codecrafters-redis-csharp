using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class IntegerToken : RespToken{

    public int Value { get; set; }

    public override string ToRESP() {
        return $":{Value}\r\n";
    }

    public override RespToken FromRESP(string resp, out int endIndex) {
        if (resp[0] != ':') {
            throw new InvalidDataException("Invalid RESP data");
        }
        Value = ReadSize(resp, 1, out int sizeEndIndex);
        endIndex = sizeEndIndex + 2;
        return this;
    }
}
