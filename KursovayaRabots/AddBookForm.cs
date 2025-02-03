using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для добавления новой книги в базу данных.
    /// Позволяет ввести название книги, автора, год публикации и количество экземпляров.
    /// </summary>
    public class AddBookForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private TextBox titleBox, authorBox, yearBox, copiesBox; // Поля ввода данных

        /// <summary>
        /// Конструктор формы добавления книги.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public AddBookForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация пользовательского интерфейса
        }

        /// <summary>
        /// Метод для создания элементов интерфейса формы.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Добавить книгу"; // Заголовок формы
            this.Width = 400; // Ширина формы
            this.Height = 300; // Высота формы
            this.StartPosition = FormStartPosition.CenterScreen; // Центрирование формы на экране

            // Создаем макет формы с двумя колонками
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполнение всей формы
                ColumnCount = 2, // Две колонки: метки и текстовые поля
                Padding = new Padding(10) // Внутренние отступы
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Ширина для меток
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Ширина для полей ввода

            // Поле для ввода названия книги
            layout.Controls.Add(new Label { Text = "Название:", Anchor = AnchorStyles.Right }, 0, 0);
            titleBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(titleBox, 1, 0);

            // Поле для ввода автора книги
            layout.Controls.Add(new Label { Text = "Автор:", Anchor = AnchorStyles.Right }, 0, 1);
            authorBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(authorBox, 1, 1);

            // Поле для ввода года публикации
            layout.Controls.Add(new Label { Text = "Год публикации:", Anchor = AnchorStyles.Right }, 0, 2);
            yearBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(yearBox, 1, 2);

            // Поле для ввода количества экземпляров
            layout.Controls.Add(new Label { Text = "Количество экземпляров:", Anchor = AnchorStyles.Right }, 0, 3);
            copiesBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(copiesBox, 1, 3);

            // Кнопка "Сохранить"
            var saveButton = new Button { Text = "Сохранить", Dock = DockStyle.Right, Width = 100 };
            saveButton.Click += SaveButton_Click; // Привязываем обработчик нажатия
            layout.Controls.Add(saveButton, 1, 4);

            this.Controls.Add(layout); // Добавляем макет на форму
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить". Проверяет корректность данных и сохраняет новую книгу в базу данных.
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

            if (!int.TryParse(yearBox.Text, out int publicationYear) || publicationYear < 1000 || publicationYear > DateTime.Now.Year)
            {
                MessageBox.Show("Укажите корректный год публикации (от 1000 до текущего года)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(copiesBox.Text, out int copiesAvailable) || copiesAvailable < 0)
            {
                MessageBox.Show("Количество экземпляров должно быть положительным числом!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Попытка добавления данных в базу данных
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для добавления книги
                    var query = @"
                        INSERT INTO Books (Title, Author, PublicationYear, CopiesAvailable)
                        VALUES (@Title, @Author, @PublicationYear, @CopiesAvailable);";

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", titleBox.Text); // Название книги
                    command.Parameters.AddWithValue("@Author", authorBox.Text); // Автор книги
                    command.Parameters.AddWithValue("@PublicationYear", publicationYear); // Год публикации
                    command.Parameters.AddWithValue("@CopiesAvailable", copiesAvailable); // Количество копий

                    command.ExecuteNonQuery(); // Выполняем SQL-запрос

                    MessageBox.Show("Книга успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Закрываем форму после успешного сохранения
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при добавлении данных
                MessageBox.Show($"Ошибка добавления книги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


