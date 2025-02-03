using System; // Базовые классы .NET
using System.Data; // Для работы с объектами DataTable
using System.Windows.Forms; // Пространство имен для создания Windows Forms
using MySql.Data.MySqlClient; // Подключение для работы с MySQL

namespace KursovayaRabots
{
    /// <summary>
    /// Форма выбора книги для выдачи пользователю.
    /// Отображает список доступных книг и позволяет выбрать одну для выдачи.
    /// </summary>
    public class SelectBookForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private int userId; // Идентификатор пользователя, для которого осуществляется выдача книги
        private DataGridView dataGridView; // Таблица для отображения доступных книг

        /// <summary>
        /// Конструктор формы.
        /// Принимает строку подключения к базе данных и идентификатор пользователя.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="userId">Идентификатор пользователя</param>
        public SelectBookForm(string connectionString, int userId)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            this.userId = userId; // Инициализация идентификатора пользователя
            InitializeComponent(); // Инициализация пользовательского интерфейса
            LoadBooks(); // Загрузка списка доступных книг
        }

        /// <summary>
        /// Инициализация элементов пользовательского интерфейса.
        /// Создает таблицу для отображения книг и кнопку для выбора книги.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Выберите книгу"; // Заголовок формы
            this.Width = 600; // Ширина формы
            this.Height = 400; // Высота формы

            // Инициализация таблицы
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill, // Занимает все доступное пространство
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // Автоматически растягивает колонки
            };

            // Кнопка "Выдать"
            var selectButton = new Button
            {
                Text = "Выдать", // Текст кнопки
                Dock = DockStyle.Bottom // Располагается в нижней части формы
            };
            selectButton.Click += SelectButton_Click; // Обработчик нажатия кнопки

            // Добавляем элементы управления на форму
            this.Controls.Add(dataGridView);
            this.Controls.Add(selectButton);
        }

        /// <summary>
        /// Загрузка списка доступных книг из базы данных.
        /// Показывает только книги, где количество доступных экземпляров больше 0.
        /// </summary>
        private void LoadBooks()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // Запрос для получения списка доступных книг
                    string query = "SELECT BookID, Title, CopiesAvailable FROM Books WHERE CopiesAvailable > 0;";

                    // Используем адаптер для заполнения DataTable
                    var adapter = new MySqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    // Привязываем DataTable к DataGridView
                    dataGridView.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок подключения или выполнения запроса
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Выдать".
        /// Проверяет выбранную строку в таблице, затем добавляет запись в таблицу Checkouts
        /// и уменьшает количество доступных экземпляров книги.
        /// </summary>
        private void SelectButton_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка в таблице
            if (dataGridView.SelectedRows.Count > 0)
            {
                // Получаем данные выбранной строки
                var selectedRow = dataGridView.SelectedRows[0];
                int bookId = Convert.ToInt32(selectedRow.Cells["BookID"].Value); // Идентификатор книги

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение

                        // Запрос для добавления выдачи и уменьшения количества экземпляров
                        var query = @"
                            INSERT INTO Checkouts (UserID, BookID, JournalID, CheckoutDate)
                            VALUES (@UserID, @BookID, NULL, CURDATE());
                            UPDATE Books SET CopiesAvailable = CopiesAvailable - 1 WHERE BookID = @BookID;";

                        var command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@UserID", userId); // Передаем параметр UserID
                        command.Parameters.AddWithValue("@BookID", bookId); // Передаем параметр BookID

                        command.ExecuteNonQuery(); // Выполняем запрос

                        // Сообщаем об успешной операции
                        MessageBox.Show("Книга успешно выдана!");
                        this.Close(); // Закрываем форму
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок SQL-запроса
                    MessageBox.Show($"Ошибка выдачи книги: {ex.Message}");
                }
            }
            else
            {
                // Если книга не выбрана
                MessageBox.Show("Выберите книгу.");
            }
        }
    }
}