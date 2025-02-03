using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для редактирования данных книги.
    /// Позволяет загрузить текущие данные книги из базы данных,
    /// внести изменения и сохранить их.
    /// </summary>
    public class EditBookForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private int bookId; // Идентификатор книги, данные которой редактируются
        private TextBox titleBox, authorBox, yearBox, copiesBox; // Поля для ввода данных

        /// <summary>
        /// Конструктор формы редактирования книги.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="bookId">Идентификатор книги</param>
        public EditBookForm(string connectionString, int bookId)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            this.bookId = bookId; // Инициализация идентификатора книги
            InitializeComponent(); // Инициализация пользовательского интерфейса
            LoadBookData(); // Загрузка данных книги
        }

        /// <summary>
        /// Метод для инициализации пользовательского интерфейса.
        /// Создает элементы управления для ввода данных и кнопку "Сохранить".
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Редактировать книгу"; // Заголовок окна
            this.Width = 400; // Устанавливаем ширину окна
            this.Height = 300; // Устанавливаем высоту окна

            // Основная компоновка формы
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всё пространство формы
                ColumnCount = 2, // Две колонки: для меток и полей ввода
                Padding = new Padding(10) // Внутренний отступ
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Первая колонка для меток
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Вторая колонка для полей ввода

            // Поля ввода данных и соответствующие метки
            layout.Controls.Add(new Label { Text = "Название:", Anchor = AnchorStyles.Right }, 0, 0);
            titleBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(titleBox, 1, 0);

            layout.Controls.Add(new Label { Text = "Автор:", Anchor = AnchorStyles.Right }, 0, 1);
            authorBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(authorBox, 1, 1);

            layout.Controls.Add(new Label { Text = "Год публикации:", Anchor = AnchorStyles.Right }, 0, 2);
            yearBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(yearBox, 1, 2);

            layout.Controls.Add(new Label { Text = "Количество экземпляров:", Anchor = AnchorStyles.Right }, 0, 3);
            copiesBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(copiesBox, 1, 3);

            // Кнопка "Сохранить"
            var saveButton = new Button { Text = "Сохранить", Dock = DockStyle.Right, Width = 100 };
            saveButton.Click += SaveButton_Click; // Обработчик нажатия кнопки
            layout.Controls.Add(saveButton, 1, 4);

            // Добавляем основную компоновку на форму
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Загружает данные книги из базы данных и заполняет поля ввода.
        /// </summary>
        private void LoadBookData()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для получения данных книги
                    var query = "SELECT Title, Author, PublicationYear, CopiesAvailable FROM Books WHERE BookID = @BookID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@BookID", bookId); // Передаем параметр BookID

                    // Чтение данных из базы
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполняем поля ввода данными книги
                            titleBox.Text = reader.GetString("Title");
                            authorBox.Text = reader.GetString("Author");
                            yearBox.Text = reader.GetInt32("PublicationYear").ToString();
                            copiesBox.Text = reader.GetInt32("CopiesAvailable").ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show($"Ошибка загрузки данных книги: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить".
        /// Проверяет корректность введенных данных и сохраняет изменения в базе.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(titleBox.Text))
            {
                MessageBox.Show("Название книги не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(authorBox.Text))
            {
                MessageBox.Show("Автор книги не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(yearBox.Text, out int publicationYear) || publicationYear < 1901 || publicationYear > DateTime.Now.Year)
            {
                MessageBox.Show("Укажите корректный год публикации (от 1901 до текущего года)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(copiesBox.Text, out int copiesAvailable) || copiesAvailable < 0)
            {
                MessageBox.Show("Количество экземпляров должно быть положительным числом!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обновление данных в базе данных
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение

                    // SQL-запрос для обновления данных книги
                    var query = @"
                    UPDATE Books
                    SET Title = @Title, Author = @Author, PublicationYear = @PublicationYear, CopiesAvailable = @CopiesAvailable
                    WHERE BookID = @BookID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", titleBox.Text);
                    command.Parameters.AddWithValue("@Author", authorBox.Text);
                    command.Parameters.AddWithValue("@PublicationYear", publicationYear);
                    command.Parameters.AddWithValue("@CopiesAvailable", copiesAvailable);
                    command.Parameters.AddWithValue("@BookID", bookId);

                    command.ExecuteNonQuery(); // Выполняем запрос

                    // Уведомляем об успешной операции
                    MessageBox.Show("Изменения успешно сохранены!");
                    this.Close(); // Закрываем форму
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при обновлении данных
                MessageBox.Show($"Ошибка сохранения изменений: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


