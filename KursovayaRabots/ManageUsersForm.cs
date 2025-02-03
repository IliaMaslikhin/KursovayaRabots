using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для управления пользователями.
    /// Позволяет загружать, добавлять, редактировать и удалять записи о пользователях.
    /// </summary>
    public class ManageUsersForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private DataGridView dataGridView; // Таблица для отображения пользователей
        private TableLayoutPanel mainLayout; // Основная компоновка элементов формы

        /// <summary>
        /// Конструктор формы управления пользователями.
        /// Принимает строку подключения к базе данных.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public ManageUsersForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация элементов пользовательского интерфейса
            LoadUsers(); // Загрузка списка пользователей
        }

        /// <summary>
        /// Инициализация пользовательского интерфейса.
        /// Создает основную таблицу компоновки, таблицу данных и кнопки управления.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Управление пользователями"; // Заголовок формы
            this.Width = 800; // Ширина формы
            this.Height = 600; // Высота формы

            // Создаем основную таблицу компоновки
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Занимает все пространство формы
                RowCount = 2, // Две строки: одна для таблицы данных, другая для кнопок
                ColumnCount = 1, // Одна колонка
                Padding = new Padding(10) // Внутренний отступ
            };

            // Определяем пропорции строк
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85)); // Таблица данных
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Панель кнопок

            // Инициализация таблицы данных
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill, // Занимает всё пространство строки
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // Колонки растягиваются по ширине
            };
            mainLayout.Controls.Add(dataGridView, 0, 0); // Добавляем таблицу в первую строку

            // Панель для кнопок управления
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Занимает всю строку
                ColumnCount = 3 // Три кнопки в одной строке
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33)); // Пропорции кнопок
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            // Создаем кнопки управления
            var addButton = new Button { Text = "Добавить", Dock = DockStyle.Fill };
            addButton.Click += AddButton_Click; // Обработчик нажатия кнопки "Добавить"

            var editButton = new Button { Text = "Редактировать", Dock = DockStyle.Fill };
            editButton.Click += EditButton_Click; // Обработчик нажатия кнопки "Редактировать"

            var deleteButton = new Button { Text = "Удалить", Dock = DockStyle.Fill };
            deleteButton.Click += DeleteButton_Click; // Обработчик нажатия кнопки "Удалить"

            // Добавляем кнопки на панель
            buttonPanel.Controls.Add(addButton, 0, 0);
            buttonPanel.Controls.Add(editButton, 1, 0);
            buttonPanel.Controls.Add(deleteButton, 2, 0);

            // Добавляем панель кнопок в основную компоновку
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            // Добавляем основную компоновку на форму
            this.Controls.Add(mainLayout);
        }

        /// <summary>
        /// Загружает список пользователей из базы данных и отображает их в таблице.
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для получения данных о пользователях с русскими названиями столбцов
                    var query = @"
                SELECT 
                    UserID AS 'ID пользователя',
                    FullName AS 'Полное имя',
                    Address AS 'Адрес',
                    Phone AS 'Телефон',
                    RegistrationDate AS 'Дата регистрации'
                FROM Users";

                    var adapter = new MySqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table); // Заполняем DataTable данными
                    dataGridView.DataSource = table; // Привязываем данные к таблице
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }


        /// <summary>
        /// Обработчик кнопки "Добавить".
        /// Открывает форму для добавления пользователя и обновляет таблицу после добавления.
        /// </summary>
        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddUserForm(connectionString); // Открываем форму добавления
            addForm.ShowDialog();
            LoadUsers(); // Обновляем таблицу после добавления
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать".
        /// Открывает форму для редактирования выбранного пользователя и обновляет таблицу.
        /// </summary>
        private void EditButton_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка в таблице
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int userId = Convert.ToInt32(selectedRow.Cells["UserID"].Value); // Получаем идентификатор пользователя

                var editForm = new EditUserForm(connectionString, userId); // Открываем форму редактирования
                editForm.ShowDialog();
                LoadUsers(); // Обновляем таблицу после редактирования
            }
            else
            {
                // Если пользователь не выбран
                MessageBox.Show("Выберите пользователя для редактирования.");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Удалить".
        /// Удаляет выбранного пользователя из базы данных и обновляет таблицу.
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка в таблице
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int userId = Convert.ToInt32(selectedRow.Cells["UserID"].Value); // Получаем идентификатор пользователя

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение

                        // SQL-запрос для удаления пользователя
                        var command = new MySqlCommand("DELETE FROM Users WHERE UserID = @UserID", connection);
                        command.Parameters.AddWithValue("@UserID", userId); // Передаем параметр
                        command.ExecuteNonQuery(); // Выполняем запрос
                    }
                    LoadUsers(); // Обновляем таблицу после удаления
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при удалении пользователя
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
            else
            {
                // Если пользователь не выбран
                MessageBox.Show("Выберите пользователя для удаления.");
            }
        }
    }
}