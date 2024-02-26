using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src; 
public static partial class Server {

    #region Basic Config

    private static int port;
    private static readonly Dictionary<string, (string value, DateTime? expiry)> _data = new();

    #endregion

    #region Replication

    private static bool isMaster = true;
    private static string masterHost;
    private static int masterPort;
    private static string masterReplId = "";
    private static long masterReplOffset = 0;

    #endregion
}
