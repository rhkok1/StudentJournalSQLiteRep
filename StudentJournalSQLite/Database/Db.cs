using System;
using Microsoft.Data.Sqlite;

namespace StudentJournalSQLite.Database
{
    public static class Db
    {
        public static string ConnectionString = "Data Source=journal.db";

        public static void Initialize()
        {
            using (SqliteConnection connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();

                cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS Groups (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "Name TEXT);" +

                "CREATE TABLE IF NOT EXISTS Subjects (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "Name TEXT);" +

                "CREATE TABLE IF NOT EXISTS Students (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "FullName TEXT," +
                "GroupId INTEGER);" +

                "CREATE TABLE IF NOT EXISTS Lessons (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "Title TEXT," +
                "GroupId INTEGER," +
                "SubjectId INTEGER);" +

                "CREATE TABLE IF NOT EXISTS Marks (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "StudentId INTEGER," +
                "LessonId INTEGER," +
                "Value TEXT);";

                cmd.ExecuteNonQuery();

                Seed(connection);
            }
        }

        private static void Seed(SqliteConnection connection)
        {
            SqliteCommand checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Groups";

            long count = (long)checkCmd.ExecuteScalar();

            if (count > 0)
                return;

            SqliteCommand cmd = connection.CreateCommand();

            cmd.CommandText =
            "INSERT INTO Groups (Name) VALUES ('22ИСП1');" +
            "INSERT INTO Groups (Name) VALUES ('22ИСП2');" +
            "INSERT INTO Groups (Name) VALUES ('22ИСС1');" +

            "INSERT INTO Subjects (Name) VALUES ('Математика');" +
            "INSERT INTO Subjects (Name) VALUES ('Русский язык');" +

            "INSERT INTO Students (FullName, GroupId) VALUES ('Иванов И.И.',1);" +
            "INSERT INTO Students (FullName, GroupId) VALUES ('Петров П.П.',1);" +
            "INSERT INTO Students (FullName, GroupId) VALUES ('Сидоров С.С.',1);" +
            "INSERT INTO Students (FullName, GroupId) VALUES ('Кузнецов К.К.',2);" +

            "INSERT INTO Lessons (Title, GroupId) VALUES ('Занятие 1',1);" +
            "INSERT INTO Lessons (Title, GroupId) VALUES ('Занятие 2',1);" +
            "INSERT INTO Lessons (Title, GroupId) VALUES ('Занятие 3',1);" +

            "INSERT INTO Lessons (Title, GroupId) VALUES ('Занятие 1',2);" +
            "INSERT INTO Lessons (Title, GroupId) VALUES ('Занятие 2',2);";

            cmd.ExecuteNonQuery();
        }
    }
}