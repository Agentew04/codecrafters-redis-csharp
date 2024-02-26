using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src {
    internal class Rdb {
        private const string emptyFileB64 = "UkVESVMwMDEx+glyZWRpcy12ZXIFNy4yLjD6CnJlZGlzLWJpdHPAQPoFY3RpbWXCbQi8ZfoIdXNlZC1tZW3CsMQQAPoIYW9mLWJhc2XAAP/wbjv+wP9aog==";
        public static byte[] EmptyFile => Convert.FromBase64String(emptyFileB64);

        internal static async Task SaveFile(byte[] content) {
            // TODO implement
        }
    }
}
