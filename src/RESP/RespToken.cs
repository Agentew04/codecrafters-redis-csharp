using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public abstract class RespToken {

    /// <summary>
    /// Exports the RESP representation of this token.
    /// Should end with a \r\n
    /// </summary>
    /// <returns></returns>
    public abstract byte[] ToRESP();

    /// <summary>
    /// Populates current RESP Token from a RESP string.
    /// The return value is the same of the caller(for chaining)
    /// </summary>
    /// <param name="resp">The resp data</param>
    /// <exception cref="InvalidDataException">Should be thrown when the wrong resp data is given</exception>
    public abstract RespToken FromRESP(byte[] resp, out int endIndex);

    public static RespToken Parse(byte[] resp, out int endIndex) {
        if (resp[0] == '+') {
            return new SimpleStringToken().FromRESP(resp, out endIndex);
        }
        if (resp[0] == '-') {
            return new ErrorToken().FromRESP(resp, out endIndex);
        }
        if (resp[0] == ':') {
            return new IntegerToken().FromRESP(resp, out endIndex);
        }
        if (resp[0] == '$') {
            return new BulkStringToken().FromRESP(resp, out endIndex);
        }
        if (resp[0] == '*') {
            return new ArrayToken().FromRESP(resp, out endIndex);
        }
        if (resp[0] == '#') {
            return new BooleanToken().FromRESP(resp, out endIndex);
        }

        throw new InvalidDataException("Invalid RESP data");
    }

    /// <summary>
    /// Reads a string number from a RESP string
    /// </summary>
    /// <param name="resp">The resp code</param>
    /// <param name="startIndex">The index where the function starts reading the int</param>
    /// <param name="sizeEndIndex">Index where the int ended</param>
    /// <returns>The size read</returns>
    protected static int ReadSize(byte[] resp, int startIndex, out int sizeEndIndex) {
        // read string numbers until \r\n
        while (resp[startIndex] >= '0' && resp[startIndex] <= '9') {
            startIndex++;
        }

        byte[] sizeBytes = resp[1..startIndex];
        string sizeStr = Encoding.ASCII.GetString(sizeBytes);
        sizeEndIndex = startIndex;
        return int.Parse(sizeStr);
    }
}
