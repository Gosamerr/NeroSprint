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
                // Таблица tests
                command.CommandText = @"
                    CREATE TABLE tests (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL
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

                // Таблица results
                command.CommandText = @"
                    CREATE TABLE results (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        test_id INTEGER NOT NULL,
                        user_id INTEGER NOT NULL,
                        res_start_time DATETIME,
                        res_end_time DATETIME,
                        score REAL,
                        FOREIGN KEY (test_id) REFERENCES tests (id) ON DELETE CASCADE,
                        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
                    );";
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

    public bool ValidateUser(string email, string password)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT password FROM users WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.GetString(0);
                            // Здесь нужно сравнить пароль с хешем
                            // Пока используем прямое сравнение (для демонстрации)
                            // В реальности: return BCrypt.Verify(password, storedHash);
                            return password == storedHash; // временно, пока не внедрили хеширование
                        }
                        else
                        {
                            return false; // пользователь не найден
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Validation error: " + e.Message);
            return false;
        }
    }
}
