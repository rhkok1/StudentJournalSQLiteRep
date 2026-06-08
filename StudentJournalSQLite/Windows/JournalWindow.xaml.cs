using Microsoft.Data.Sqlite;
using SQLitePCL;
using StudentJournalSQLite.Database;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentJournalSQLite.Windows
{
    public partial class JournalWindow : Window
    {
        private DataGridColumn selectedColumn;
        private int groupId;

        public JournalWindow(int groupId)
        {
            InitializeComponent();
            this.groupId = groupId;
            LoadTable();
        }

        private void LoadTable()
        {
            JournalGrid.Columns.Clear();

            List<StudentItem> students = new List<StudentItem>();
            List<LessonItem> lessons = new List<LessonItem>();

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                // студенты
                SqliteCommand cmd1 = con.CreateCommand();
                cmd1.CommandText = "SELECT Id, FullName FROM Students WHERE GroupId=" + groupId;

                SqliteDataReader r1 = cmd1.ExecuteReader();

                while (r1.Read())
                {
                    StudentItem s = new StudentItem();
                    s.Id = r1.GetInt32(0);
                    s.Name = r1.GetString(1);
                    students.Add(s);
                }

                r1.Close();

                // занятия
                SqliteCommand cmd2 = con.CreateCommand();
                cmd2.CommandText = "SELECT Id, Title FROM Lessons WHERE GroupId=" + groupId;

                SqliteDataReader r2 = cmd2.ExecuteReader();

                while (r2.Read())
                {
                    LessonItem l = new LessonItem();
                    l.Id = r2.GetInt32(0);
                    l.Title = r2.GetString(1);
                    lessons.Add(l);
                }
            }

            // колонка студент
            DataGridTextColumn studentCol = new DataGridTextColumn();
            studentCol.Header = "Студент";
            studentCol.Binding = new System.Windows.Data.Binding("[Student]");
            JournalGrid.Columns.Add(studentCol);

            // колонки занятий
            foreach (LessonItem lesson in lessons)
            {
                DataGridComboBoxColumn col = new DataGridComboBoxColumn();
                col.Header = lesson.Title;
                col.SelectedItemBinding = new System.Windows.Data.Binding("[" + lesson.Id + "]");
                col.ItemsSource = new List<string> { "", "н", "у", "2", "3", "4", "5" };

                JournalGrid.Columns.Add(col);
            }

            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

            foreach (StudentItem student in students)
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                row["Student"] = student.Name;

                foreach (LessonItem lesson in lessons)
                {
                    row[lesson.Id.ToString()] = "";
                }

                table.Add(row);
            }

            JournalGrid.ItemsSource = table;
        }

        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            string name = "Новый студент";

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText =
                    "INSERT INTO Students (FullName, GroupId) VALUES ('" + name + "', " + groupId + ")";
                cmd.ExecuteNonQuery();
            }

            LoadTable(); // обновление таблицы
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = JournalGrid.SelectedItem as Dictionary<string, object>;

            if (selectedRow == null)
            {
                MessageBox.Show("Выберите студента");
                return;
            }

            string studentName = selectedRow["Student"].ToString();

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                // получение ID
                SqliteCommand getId = con.CreateCommand();
                getId.CommandText = "SELECT Id FROM Students WHERE FullName='" + studentName + "' LIMIT 1";

                object result = getId.ExecuteScalar();
                if (result == null)
                {
                    MessageBox.Show("Студент не найден");
                    return;
                }

                int studentId = Convert.ToInt32(result);

                // удаление по ID
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "DELETE FROM Students WHERE Id=" + studentId;
                cmd.ExecuteNonQuery();
            }

            LoadTable();
        }

        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            string lessonName = "Занятие " + JournalGrid.Columns.Count;

            DataGridComboBoxColumn col = new DataGridComboBoxColumn();
            col.Header = lessonName;
            col.SelectedItemBinding = new System.Windows.Data.Binding("[" + lessonName + "]");
            col.ItemsSource = new List<string> { "", "н", "у", "2", "3", "4", "5" };

            JournalGrid.Columns.Add(col);

            var table = JournalGrid.ItemsSource as List<Dictionary<string, object>>;

            foreach (var row in table)
            {
                row[lessonName] = "";
            }

            JournalGrid.Items.Refresh();
        }

        private void DeleteLesson_Click(object sender, RoutedEventArgs e)
        {
            if (JournalGrid.SelectedCells.Count == 0)
            {
                MessageBox.Show("Кликните по любой ячейке нужного столбца");
                return;
            }

            var column = JournalGrid.SelectedCells[0].Column;

            string lessonTitle = column.Header.ToString();

            if (lessonTitle == "Студент")
            {
                MessageBox.Show("Нельзя удалить этот столбец");
                return;
            }

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand getId = con.CreateCommand();
                getId.CommandText = "SELECT Id FROM Lessons WHERE Title='" + lessonTitle + "' LIMIT 1";

                object result = getId.ExecuteScalar();

                if (result == null)
                {
                    MessageBox.Show("Занятие не найдено в БД");
                    return;
                }

                int lessonId = Convert.ToInt32(result);

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "DELETE FROM Lessons WHERE Id=" + lessonId;
                cmd.ExecuteNonQuery();
            }

            LoadTable();
        }

        // обработка клика по столбцу
        private void JournalGrid_ColumnHeaderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var header = e.OriginalSource as System.Windows.Controls.Primitives.DataGridColumnHeader;

            if (header != null)
            {
                selectedColumn = header.Column;
            }
        }

        private void JournalGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ComboBox combo = e.EditingElement as ComboBox;
            if (combo == null) return;

            string value = combo.Text;

            Dictionary<string, object> row = e.Row.Item as Dictionary<string, object>;
            if (row == null) return;

            string studentName = row["Student"].ToString();
            string lessonTitle = e.Column.Header.ToString();

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand cmd1 = con.CreateCommand();
                cmd1.CommandText = "SELECT Id FROM Students WHERE FullName='" + studentName + "'";
                int studentId = Convert.ToInt32(cmd1.ExecuteScalar());

                SqliteCommand cmd2 = con.CreateCommand();
                cmd2.CommandText = "SELECT Id FROM Lessons WHERE Title='" + lessonTitle + "'";
                int lessonId = Convert.ToInt32(cmd2.ExecuteScalar());

                SqliteCommand check = con.CreateCommand();
                check.CommandText =
                    "SELECT Id FROM Marks WHERE StudentId=" + studentId + " AND LessonId=" + lessonId;

                object exists = check.ExecuteScalar();

                if (exists == null)
                {
                    SqliteCommand insert = con.CreateCommand();
                    insert.CommandText =
                        "INSERT INTO Marks (StudentId, LessonId, Value) VALUES (" +
                        studentId + "," + lessonId + ",'" + value + "')";
                    insert.ExecuteNonQuery();
                }
                else
                {
                    SqliteCommand update = con.CreateCommand();
                    update.CommandText =
                        "UPDATE Marks SET Value='" + value + "' WHERE StudentId=" +
                        studentId + " AND LessonId=" + lessonId;
                    update.ExecuteNonQuery();
                }
            }
        }
    }

    public class StudentItem
    {
        public int Id;
        public string Name;
    }

    public class LessonItem
    {
        public int Id;
        public string Title;
    }

    public class SubjectItem
    {
        public int Id;
        public string Name;
    }
}