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

    // Синглтон для удобства доступа
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

    // Инициализация: создаём папку и файл БД, если их нет, и создаём таблицы
    private void InitializeDatabase()
    {
        // Определяем путь к базе данных в зависимости от платформы
#if UNITY_EDITOR
            dbPath = Path.Combine(Application.dataPath, dbName);
#elif UNITY_STANDALONE
            dbPath = Path.Combine(Application.persistentDataPath, dbName);
#endif

        // Если файл базы ещё не существует – создадим таблицы
        if (!File.Exists(dbPath))
        {
            CreateDatabase();
        }
        else
        {
            // Можно проверить структуру таблиц, но для простоты пропустим
            Debug.Log("Database already exists at: " + dbPath);
        }
    }

    // Создание таблиц по вашей схеме
    private void CreateDatabase()
    {
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Таблица tests с уникальным name
                command.CommandText = @"
                CREATE TABLE tests (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE
                );";
                command.ExecuteNonQuery();

                // Таблица users (без изменений)
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

                // Таблица test_results (новая структура)
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
                    completion_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (test_id) REFERENCES tests (id) ON DELETE CASCADE,
                    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
                );";
                command.ExecuteNonQuery();

                // Добавляем три теста (если их ещё нет)
                command.CommandText = @"
                INSERT OR IGNORE INTO tests (name) VALUES 
                ('SplitMatch'), 
                ('PopTap'), 
                ('HealthMeter');";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("Database created at: " + dbPath);
    }

    // Метод для проверки соединения (опционально)
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

    // Регистрация нового пользователя
    public bool RegisterUser(string name, int age, string email, string password)
    {
        // Хеширование пароля (обсудим позже)
        string hashedPassword = HashPassword(password);

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
            // Ошибка SQLite, например нарушение уникальности email
            Debug.LogError("SQLite error: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError("Registration error: " + e.Message);
            return false;
        }
    }

    // Простейшее хеширование пароля (для примера используем небезопасный GetHashCode,
    // в реальном проекте используйте BCrypt или как минимум SHA256 с солью)
    private string HashPassword(string password)
    {
        // ВАЖНО: Это только для демонстрации! Никогда не храните пароли в открытом виде.
        // Рекомендуется использовать BCrypt.Net или аналоги.
        // Пример с BCrypt (нужно установить пакет):
        // return BCrypt.Net.BCrypt.HashPassword(password);

        // Временная заглушка (небезопасно!)
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
                            string storedPassword = reader.GetString(1); // пока без хеша
                            if (password == storedPassword) // замените на BCrypt.Verify при необходимости
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
        return 0f; // если результатов нет
    }
}
