using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class DatabaseManager
{
    [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
    private static extern int sqlite3_open(
        string filename,
        out IntPtr database
    );

    [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
    private static extern int sqlite3_close(
        IntPtr database
    );

    [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
    private static extern int sqlite3_exec(
        IntPtr database,
        IntPtr sql,
        IntPtr callback,
        IntPtr argument,
        out IntPtr errorMessage
    );

    [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void sqlite3_free(
        IntPtr pointer
    );

    private static string DatabasePath
    {
        get
        {
            string gameFolder =
                Directory.GetParent(UnityEngine.Application.dataPath).FullName;

            string saveFolder =
                Path.Combine(gameFolder, "Saves");

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            return Path.Combine(saveFolder, "bladerun.db");
        }
    }

    public static void InitializeDatabase()
    {
        IntPtr db;

        if (sqlite3_open(DatabasePath, out db) != 0)
        {
            UnityEngine.Debug.LogError("Could not open SQLite database.");
            return;
        }

        EnableForeignKeys(db);
        CreateTables(db);
        InsertDefaultData(db);

        sqlite3_close(db);
    }

    public static void SaveSlot(SaveData data)
    {
        IntPtr db;

        if (sqlite3_open(DatabasePath, out db) != 0)
        {
            UnityEngine.Debug.LogError("Could not open SQLite database.");
            return;
        }

        EnableForeignKeys(db);
        CreateTables(db);
        InsertDefaultData(db);

        string scene =
            (data.sceneName ?? "")
            .Replace("'", "''");

        string date =
            (data.lastSaved ?? "")
            .Replace("'", "''");

        string time =
            data.elapsedTime.ToString(CultureInfo.InvariantCulture);

        int levelId = GetLevelIdByScene(data.sceneName);

        string sql =
            "INSERT INTO SaveSlots " +
            "(slot, levelId, sceneName, elapsedTime, lastSaved) " +
            "VALUES (" +
            data.slot + ", " +
            levelId + ", '" +
            scene + "', " +
            time + ", '" +
            date + "') " +
            "ON CONFLICT(slot) DO UPDATE SET " +
            "levelId = excluded.levelId, " +
            "sceneName = excluded.sceneName, " +
            "elapsedTime = excluded.elapsedTime, " +
            "lastSaved = excluded.lastSaved;";

        Execute(db, sql);

        sqlite3_close(db);
    }

    public static void DeleteSlot(int slot)
    {
        IntPtr db;

        if (sqlite3_open(DatabasePath, out db) != 0)
            return;

        EnableForeignKeys(db);
        CreateTables(db);

        Execute(
            db,
            "DELETE FROM SaveSlots WHERE slot = " +
            slot + ";"
        );

        sqlite3_close(db);
    }

    private static void EnableForeignKeys(IntPtr db)
    {
        Execute(db, "PRAGMA foreign_keys = ON;");
    }

    private static void CreateTables(IntPtr db)
    {
        string biomesTable =
            "CREATE TABLE IF NOT EXISTS Biomes (" +
            "biomeId INTEGER NOT NULL PRIMARY KEY, " +
            "biomeName TEXT NOT NULL UNIQUE" +
            ");";

        string levelsTable =
            "CREATE TABLE IF NOT EXISTS Levels (" +
            "levelId INTEGER NOT NULL PRIMARY KEY, " +
            "sceneName TEXT NOT NULL UNIQUE, " +
            "levelName TEXT NOT NULL, " +
            "biomeId INTEGER NOT NULL, " +
            "levelOrder INTEGER NOT NULL, " +
            "FOREIGN KEY(biomeId) REFERENCES Biomes(biomeId)" +
            ");";

        string enemiesTable =
            "CREATE TABLE IF NOT EXISTS Enemies (" +
            "enemyId INTEGER NOT NULL PRIMARY KEY, " +
            "enemyName TEXT NOT NULL UNIQUE, " +
            "enemyType TEXT NOT NULL, " +
            "biomeId INTEGER NOT NULL, " +
            "FOREIGN KEY(biomeId) REFERENCES Biomes(biomeId)" +
            ");";

        string checkpointsTable =
            "CREATE TABLE IF NOT EXISTS Checkpoints (" +
            "checkpointId INTEGER NOT NULL PRIMARY KEY, " +
            "checkpointName TEXT NOT NULL UNIQUE, " +
            "levelId INTEGER NOT NULL, " +
            "checkpointOrder INTEGER NOT NULL, " +
            "FOREIGN KEY(levelId) REFERENCES Levels(levelId)" +
            ");";

        string saveSlotsTable =
            "CREATE TABLE IF NOT EXISTS SaveSlots (" +
            "slot INTEGER NOT NULL PRIMARY KEY, " +
            "levelId INTEGER NOT NULL, " +
            "sceneName TEXT NOT NULL, " +
            "elapsedTime REAL NOT NULL, " +
            "lastSaved TEXT, " +
            "FOREIGN KEY(levelId) REFERENCES Levels(levelId)" +
            ");";

        Execute(db, biomesTable);
        Execute(db, levelsTable);
        Execute(db, enemiesTable);
        Execute(db, checkpointsTable);
        Execute(db, saveSlotsTable);
    }

    private static void InsertDefaultData(IntPtr db)
    {
        Execute(
            db,
            "INSERT OR IGNORE INTO Biomes " +
            "(biomeId, biomeName) VALUES " +
            "(1, 'Grassland'), " +
            "(2, 'Cavern Layer'), " +
            "(3, 'Castle');"
        );

        Execute(
            db,
            "INSERT OR IGNORE INTO Levels " +
            "(levelId, sceneName, levelName, biomeId, levelOrder) VALUES " +
            "(1, '1Forest', 'Grassland 1', 1, 1), " +
            "(2, '2Forest', 'Grassland 2', 1, 2), " +
            "(3, '3Forest', 'Grassland 3', 1, 3), " +
            "(4, '4Cave', 'Cavern Layer', 2, 4), " +
            "(5, '5Castle', 'Castle 1', 3, 5), " +
            "(6, '6Castle', 'Castle 2', 3, 6);"
        );

        Execute(
            db,
            "INSERT OR IGNORE INTO Enemies " +
            "(enemyId, enemyName, enemyType, biomeId) VALUES " +
            "(1, 'Slime', 'Ground', 1), " +
            "(2, 'Bat', 'Flying', 2), " +
            "(3, 'Wolf', 'Ground', 3), " +
            "(4, 'KingBoss', 'Boss', 3);"
        );

        Execute(
            db,
            "INSERT OR IGNORE INTO Checkpoints " +
            "(checkpointId, checkpointName, levelId, checkpointOrder) VALUES " +
            "(1, 'Checkpoint_1Forest', 1, 1), " +
            "(2, 'Checkpoint_2Forest', 2, 1), " +
            "(3, 'Checkpoint_3Forest', 3, 1), " +
            "(4, 'Checkpoint_4Cave', 4, 1), " +
            "(5, 'Checkpoint_5Castle', 5, 1), " +
            "(6, 'Checkpoint_6Castle', 6, 1);"
        );
    }

    private static int GetLevelIdByScene(string sceneName)
    {
        switch (sceneName)
        {
            case "1Forest":
                return 1;

            case "2Forest":
                return 2;

            case "3Forest":
                return 3;

            case "4Cave":
                return 4;

            case "5Castle":
                return 5;

            case "6Castle":
                return 6;

            default:
                return 1;
        }
    }

    private static void Execute(
        IntPtr db,
        string sql
    )
    {
        byte[] bytes =
            Encoding.UTF8.GetBytes(
                sql + "\0"
            );

        IntPtr sqlPointer =
            Marshal.AllocHGlobal(
                bytes.Length
            );

        Marshal.Copy(
            bytes,
            0,
            sqlPointer,
            bytes.Length
        );

        IntPtr error;

        int result =
            sqlite3_exec(
                db,
                sqlPointer,
                IntPtr.Zero,
                IntPtr.Zero,
                out error
            );

        Marshal.FreeHGlobal(
            sqlPointer
        );

        if (result != 0)
        {
            string message =
                error != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(error)
                : "Unknown SQLite error";

            UnityEngine.Debug.LogError(
                "SQLite error: " + message
            );

            if (error != IntPtr.Zero)
                sqlite3_free(error);
        }
    }
}