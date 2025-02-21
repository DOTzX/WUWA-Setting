using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace WUWA_Tweaker {
    public class LocalStorageEntry {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    // Insert or Update
    //LocalStorageEntry entry = new LocalStorageEntry { Key = "username", Value = "JohnDoe" };
    //storageRepo.InsertOrUpdate(entry);

    // Retrieve by key
    //LocalStorageEntry retrievedEntry = storageRepo.GetByKey("username");
    //Console.WriteLine($"Key: {retrievedEntry.Key}, Value: {retrievedEntry.Value}");

    // Retrieve all entries
    //List<LocalStorageEntry> allEntries = storageRepo.GetAll();
    //foreach (LocalStorageEntry e in allEntries) {
    //    Console.WriteLine($"Key: {e.Key}, Value: {e.Value}");
    //}

    public class LocalStorageRepository {
        private readonly string _connectionString;
        public LocalStorageRepository (string databasePath) {
            _connectionString = $"Data Source={databasePath};Version=3;";
        }
        public void InsertOrUpdate (LocalStorageEntry entry) {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                string query = @"
                INSERT INTO LocalStorage (key, value) 
                VALUES (@key, @value)
                ON CONFLICT(key) DO UPDATE SET value = excluded.value;";

                using (var command = new SQLiteCommand(query, connection)) {
                    command.Parameters.AddWithValue("@key", entry.Key);
                    command.Parameters.AddWithValue("@value", entry.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public LocalStorageEntry GetByKey (string key) {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                string query = "SELECT key, value FROM LocalStorage WHERE key = @key";

                using (var command = new SQLiteCommand(query, connection)) {
                    command.Parameters.AddWithValue("@key", key);
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            return new LocalStorageEntry {
                                Key = reader["key"].ToString(),
                                Value = reader["value"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }
        public List<LocalStorageEntry> GetAll () {
            var entries = new List<LocalStorageEntry>();

            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                string query = "SELECT key, value FROM LocalStorage";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        entries.Add(new LocalStorageEntry {
                            Key = reader["key"].ToString(),
                            Value = reader["value"].ToString()
                        });
                    }
                }
            }

            return entries;
        }
    }

}
