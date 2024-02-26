using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class NullToken : RespToken {
    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        if (resp[0] != '_' || resp[1] != '\r' || resp[2] != '\n') {
            throw new InvalidDataException("Invalid RESP data");
        }
        endIndex = 3;
        return this;
    }

    public override byte[] ToRESP() {
        return "_\r\n".ToAscii();
    }
}
