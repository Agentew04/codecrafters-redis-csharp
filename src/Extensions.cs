using codecrafters_redis.src.RESP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src; 
public static class Extensions {
    public static byte[] ToAscii(this string str) {
        return Encoding.ASCII.GetBytes(str);
    }

    public static byte[] ToAscii(this char str) {
        return Encoding.ASCII.GetBytes(str.ToString());
    }

    public static string FromAscii(this byte[] bytes) {
        return Encoding.ASCII.GetString(bytes);
    }

    public static async Task WriteAsync(this Stream s, RespToken token) {
        if (!s.CanWrite) {
            throw new InvalidOperationException("Stream is not writable");
        }

        await s.WriteAsync(token.ToRESP());
    }
}
