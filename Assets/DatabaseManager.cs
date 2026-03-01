using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;
    private const string dbName = "MyGameDB.sqlite";

    public static DatabaseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
#if UNITY_EDITOR
        dbPath = Path.Combine(Application.dataPath, dbName);
#elif UNITY_STANDALONE
        dbPath = Path.Combine(Application.persistentDataPath, dbName);
#endif

        if (!File.Exists(dbPath))
        {
            CreateDatabase();
        }
        else
        {
            Debug.Log("Database already exists at: " + dbPath);
            // При необходимости можно добавить проверку и миграцию существующей БД
        }
    }

    private void CreateDatabase()
    {
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Таблица tests
                command.CommandText = @"
                CREATE TABLE tests (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE
                );";
                command.ExecuteNonQuery();

                // Таблица users
                command.CommandText = @"
                CREATE TABLE users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    age INTEGER,
                    email TEXT UNIQUE,
                    password TEXT,
                    registration_date DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
                command.ExecuteNonQuery();

                // Таблица test_results с добавленным столбцом score
                command.CommandText = @"
                CREATE TABLE test_results (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    test_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    avg_reaction_time_ms INTEGER,
                    omission_errors INTEGER,
                    commission_errors INTEGER,
                    reaction_time_variability REAL,
                    overall_accuracy REAL,
                    score REAL,  -- новый столбец для хранения результата теста
                    completion_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (test_id) REFERENCES tests (id) ON DELETE CASCADE,
                    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
                );";
                command.ExecuteNonQuery();

                // Добавляем тесты (включая RidersTest)
                command.CommandText = @"
                INSERT OR IGNORE INTO tests (name) VALUES 
                ('SplitMatch'), 
                ('PopTap'), 
                ('HealthMeter'),
                ('RidersTest');";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("Database created at: " + dbPath);
    }

    public bool TestConnection()
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                connection.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
            return false;
        }
    }

    public bool RegisterUser(string name, int age, string email, string password)
    {
        string hashedPassword = HashPassword(password); // В реальном проекте используйте безопасное хеширование

        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO users (name, age, email, password)
                        VALUES (@name, @age, @email, @password);";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@age", age);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", hashedPassword);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        catch (SqliteException e)
        {
            Debug.LogError("SQLite error: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError("Registration error: " + e.Message);
            return false;
        }
    }

    private string HashPassword(string password)
    {
        // Временная заглушка (небезопасно!). Замените на BCrypt или SHA256 с солью.
        return password;
    }

    public int ValidateUser(string email, string password)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, password FROM users WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            string storedPassword = reader.GetString(1);
                            if (password == storedPassword) // Сравнивайте хеши!
                            {
                                return userId;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ValidateUser error: " + e.Message);
        }
        return -1;
    }

    public static int CurrentUserId { get; set; } = -1;

    public (string name, int age, string email) GetUserInfo(int userId)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name, age, email FROM users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader.GetString(0);
                            int age = reader.GetInt32(1);
                            string email = reader.GetString(2);
                            return (name, age, email);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("GetUserInfo error: " + e.Message);
        }
        return (null, 0, null);
    }

    public float GetBestResult(int userId, string testName)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    SELECT MAX(tr.overall_accuracy) 
                    FROM test_results tr
                    INNER JOIN tests t ON tr.test_id = t.id
                    WHERE tr.user_id = @userId AND t.name = @testName";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@testName", testName);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToSingle(result);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetBestResult error for {testName}: " + e.Message);
        }
        return 0f;
    }

    // Обновлённый метод сохранения результата с параметром score
    public bool SaveTestResult(int userId, string testName,
                               float avgReactionTimeMs, int omissionErrors,
                               int commissionErrors, float reactionTimeVariability,
                               float overallAccuracy, float score)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();

                // Получаем ID теста по имени
                int testId;
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT id FROM tests WHERE name = @testName";
                    cmd.Parameters.AddWithValue("@testName", testName);
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        Debug.LogError($"Тест с именем {testName} не найден в БД");
                        return false;
                    }
                    testId = Convert.ToInt32(result);
                }

                // Вставляем результат со score
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                    INSERT INTO test_results 
                        (test_id, user_id, avg_reaction_time_ms, omission_errors, 
                         commission_errors, reaction_time_variability, overall_accuracy, score)
                    VALUES 
                        (@testId, @userId, @avgTime, @omission, @commission, @variability, @accuracy, @score);";

                    cmd.Parameters.AddWithValue("@testId", testId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@avgTime", avgReactionTimeMs);
                    cmd.Parameters.AddWithValue("@omission", omissionErrors);
                    cmd.Parameters.AddWithValue("@commission", commissionErrors);
                    cmd.Parameters.AddWithValue("@variability", reactionTimeVariability);
                    cmd.Parameters.AddWithValue("@accuracy", overallAccuracy);
                    cmd.Parameters.AddWithValue("@score", score);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SaveTestResult error: " + e.Message);
            return false;
        }
    }

    // Получить лучшее время реакции (минимальное)
    public float GetBestReactionTime(int userId, string testName)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT MIN(tr.avg_reaction_time_ms)
                FROM test_results tr
                INNER JOIN tests t ON tr.test_id = t.id
                WHERE tr.user_id = @userId AND t.name = @testName";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@testName", testName);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToSingle(result);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetBestReactionTime error for {testName}: " + e.Message);
        }
        return 0f;
    }

    // Получить лучший счёт (максимальный)
    public int GetBestScore(int userId, string testName)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT MAX(tr.score)
                FROM test_results tr
                INNER JOIN tests t ON tr.test_id = t.id
                WHERE tr.user_id = @userId AND t.name = @testName";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@testName", testName);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetBestScore error for {testName}: " + e.Message);
        }
        return 0;
    }
}