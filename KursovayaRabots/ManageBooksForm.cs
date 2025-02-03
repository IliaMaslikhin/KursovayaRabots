using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для управления книгами.
    /// Позволяет загружать, добавлять, редактировать и удалять записи о книгах.
    /// </summary>
    public class ManageBooksForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private DataGridView dataGridView; // Таблица для отображения списка книг
        private TableLayoutPanel mainLayout; // Основная компоновка элементов формы

        /// <summary>
        /// Конструктор формы управления книгами.
        /// Принимает строку подключения к базе данных.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public ManageBooksForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация пользовательского интерфейса
            LoadBooks(); // Загрузка списка книг
        }

        /// <summary>
        /// Метод для инициализации пользовательского интерфейса.
        /// Создает основную компоновку, таблицу данных и кнопки управления.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Управление книгами"; // Заголовок окна
            this.Width = 800; // Устанавливаем ширину окна
            this.Height = 600; // Устанавливаем высоту окна

            // Создаем основную таблицу компоновки
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Занимает всё пространство формы
                RowCount = 2, // Две строки: одна для таблицы данных, другая для кнопок
                ColumnCount = 1, // Одна колонка
                Padding = new Padding(10) // Внутренний отступ
            };

            // Определяем пропорции строк
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85)); // Таблица данных
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Панель кнопок

            // Инициализация таблицы для отображения данных
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
        /// Загружает список книг из базы данных и отображает их в таблице.
        /// </summary>
        /// <summary>
        /// Загружает список книг из базы данных и отображает их в таблице.
        /// </summary>
        private void LoadBooks()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для загрузки данных о книгах с русскими названиями
                    var query = @"
                    SELECT 
                        BookID AS 'ID книги',
                        Title AS 'Название',
                        Author AS 'Автор',
                        PublicationYear AS 'Год публикации',
                        CopiesAvailable AS 'Количество'
                    FROM Books";

                    var adapter = new MySqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);
                    dataGridView.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

    /// <summary>
    /// Обработчик кнопки "Добавить".
    /// Открывает форму для добавления книги и обновляет таблицу после завершения.
    /// </summary>
    private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddBookForm(connectionString); // Открываем форму добавления
            addForm.ShowDialog();
            LoadBooks(); // Обновляем таблицу
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать".
        /// Открывает форму для редактирования выбранной книги и обновляет таблицу.
        /// </summary>
        private void EditButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0) // Проверяем, выбрана ли строка
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int bookId = Convert.ToInt32(selectedRow.Cells["BookID"].Value); // Получаем ID книги

                var editForm = new EditBookForm(connectionString, bookId); // Открываем форму редактирования
                editForm.ShowDialog();
                LoadBooks(); // Обновляем таблицу
            }
            else
            {
                // Если книга не выбрана
                MessageBox.Show("Выберите книгу для редактирования.");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Удалить".
        /// Удаляет выбранную книгу из базы данных и обновляет таблицу.
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0) // Проверяем, выбрана ли строка
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int bookId = Convert.ToInt32(selectedRow.Cells["BookID"].Value); // Получаем ID книги

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение

                        // SQL-запрос для удаления книги
                        var command = new MySqlCommand("DELETE FROM Books WHERE BookID = @BookID", connection);
                        command.Parameters.AddWithValue("@BookID", bookId); // Устанавливаем параметр
                        command.ExecuteNonQuery(); // Выполняем запрос
                    }
                    LoadBooks(); // Обновляем таблицу
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при удалении книги
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
            else
            {
                // Если книга не выбрана
                MessageBox.Show("Выберите книгу для удаления.");
            }
        }
    }
}


