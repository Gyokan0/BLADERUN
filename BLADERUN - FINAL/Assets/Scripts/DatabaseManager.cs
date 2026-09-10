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
                Directory.GetParent(
                    UnityEngine.Application.dataPath
                ).FullName;

            string saveFolder =
                Path.Combine(gameFolder, "Saves");

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            return Path.Combine(
                saveFolder,
                "bladerun.db"
            );
        }
    }

    public static void SaveSlot(SaveData data)
    {
        IntPtr db;

        if (sqlite3_open(DatabasePath, out db) != 0)
        {
            UnityEngine.Debug.LogError("Could not open SQLite database.");
            return;
        }

        CreateTable(db);

        string scene =
            data.sceneName.Replace("'", "''");

        string date =
            data.lastSaved.Replace("'", "''");

        string time =
            data.elapsedTime.ToString(
                CultureInfo.InvariantCulture
            );

        string sql =
            "INSERT INTO SaveSlots " +
            "(slot, sceneName, elapsedTime, lastSaved) " +
            "VALUES (" +
            data.slot + ", '" +
            scene + "', " +
            time + ", '" +
            date + "') " +
            "ON CONFLICT(slot) DO UPDATE SET " +
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

        CreateTable(db);

        Execute(
            db,
            "DELETE FROM SaveSlots WHERE slot = " +
            slot + ";"
        );

        sqlite3_close(db);
    }

    private static void CreateTable(IntPtr db)
    {
        string sql =
            "CREATE TABLE IF NOT EXISTS SaveSlots (" +
            "slot INTEGER NOT NULL PRIMARY KEY, " +
            "sceneName TEXT, " +
            "elapsedTime REAL, " +
            "lastSaved TEXT" +
            ");";

        Execute(db, sql);
    }

    private static void Execute(
        IntPtr db,
        string sql
    )
    {
        byte[] bytes =
            Encoding.UTF8.GetBytes(sql + "\0");

        IntPtr sqlPointer =
            Marshal.AllocHGlobal(bytes.Length);

        Marshal.Copy(
            bytes,
            0,
            sqlPointer,
            bytes.Length
        );

        IntPtr error;

        int result = sqlite3_exec(
            db,
            sqlPointer,
            IntPtr.Zero,
            IntPtr.Zero,
            out error
        );

        Marshal.FreeHGlobal(sqlPointer);

        if (result != 0)
        {
            string message =
                Marshal.PtrToStringAnsi(error);

            UnityEngine.Debug.LogError("SQLite error: " + message);

            if (error != IntPtr.Zero)
                sqlite3_free(error);
        }
    }
}