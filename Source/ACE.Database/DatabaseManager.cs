using System;

using log4net;

namespace ACE.Database
{
    public static class DatabaseManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static AuthenticationDatabase Authentication { get; } = new AuthenticationDatabase();

        public static WorldDatabaseWithEntityCache World { get; } = new WorldDatabaseWithEntityCache();

        private static SerializedShardDatabase serializedShardDb;

        public static SerializedShardDatabase Shard { get; private set; }

        public static ShardConfigDatabase ShardConfig { get; } = new ShardConfigDatabase();

        public static void Initialize(bool autoRetry = true)
        {
            Authentication.Exists(true);

            if (Authentication.GetListofAccountsByAccessLevel(ACE.Entity.Enum.AccessLevel.Admin).Count == 0)
            {
                log.Warn("Database does not contain any admin accounts. The next account to be created will automatically be promoted to an Admin account.");
                AutoPromoteNextAccountToAdmin = true;
            }
            else
                AutoPromoteNextAccountToAdmin = false;

            World.Exists(true);

            var playerWeenieLoadTest = World.GetCachedWeenie("human");
            if (playerWeenieLoadTest == null)
                log.Fatal("Database does not contain the weenie for human (1). Characters cannot be created or logged into until the missing weenie is restored.");

            // By default, we hold on to player biotas a little bit longer to help with offline updates like pass-up xp, allegiance updates, etc...
            var shardDb = new ShardDatabaseWithCaching(TimeSpan.FromMinutes(31), TimeSpan.FromMinutes(11));
            serializedShardDb = new SerializedShardDatabase(shardDb);
            Shard = serializedShardDb;

            shardDb.Exists(true);
        }

        public static bool AutoPromoteNextAccountToAdmin { get; set; }

        public static void Start()
        {
            serializedShardDb.Start();
        }

        public static void Stop()
        {
            if (serializedShardDb != null)
                serializedShardDb.Stop();
        }
    }
}
