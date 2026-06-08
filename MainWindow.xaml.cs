using Microsoft.Data.Sqlite;
using StudentJournalSQLite.Database;
using StudentJournalSQLite.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;

namespace StudentJournalSQLite
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            List<GroupItem> list = new List<GroupItem>();

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT * FROM Groups";

                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GroupItem g = new GroupItem();
                    g.Id = reader.GetInt32(0);
                    g.Name = reader.GetString(1);

                    list.Add(g);
                }
            }

            GroupBox.ItemsSource = list;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Введите название группы:", "Добавление группы", "");

            if (string.IsNullOrWhiteSpace(name))
                return;

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO Groups (Name) VALUES ('" + name + "')";
                cmd.ExecuteNonQuery();
            }

            LoadGroups();
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            GroupItem selected = GroupBox.SelectedItem as GroupItem;

            if (selected == null)
            {
                MessageBox.Show("Выберите группу");
                return;
            }

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                // сначала удаляем связанные данные
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText =
                    "DELETE FROM Students WHERE GroupId=" + selected.Id + ";" +
                    "DELETE FROM Lessons WHERE GroupId=" + selected.Id + ";" +
                    "DELETE FROM Groups WHERE Id=" + selected.Id;

                cmd.ExecuteNonQuery();
            }

            LoadGroups();
        }

        private void RenameGroup_Click(object sender, RoutedEventArgs e)
        {
            GroupItem selected = GroupBox.SelectedItem as GroupItem;

            if (selected == null)
            {
                MessageBox.Show("Выберите группу");
                return;
            }

            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите новое название группы:",
                "Переименование",
                selected.Name);

            if (string.IsNullOrWhiteSpace(newName))
                return;

            using (SqliteConnection con = new SqliteConnection(Db.ConnectionString))
            {
                con.Open();

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText =
                    "UPDATE Groups SET Name='" + newName + "' WHERE Id=" + selected.Id;

                cmd.ExecuteNonQuery();
            }

            LoadGroups();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            GroupItem group = GroupBox.SelectedItem as GroupItem;

            if (group != null)
            {
                JournalWindow w = new JournalWindow(group.Id);
                w.Show();
            }
        }
    }

    public class GroupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}