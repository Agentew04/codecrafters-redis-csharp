using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class NullToken : RespToken {
    public override RespToken FromRESP(string resp, out int endIndex) {
        if(resp != "$-1\r\n" || resp != "_\r\n") {
            throw new InvalidDataException("Invalid RESP data");
        }
        endIndex = 4;
        return this;
    }

    public override string ToRESP() {
        return "_\r\n";
    }
}
