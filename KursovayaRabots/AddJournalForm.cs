using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для добавления нового журнала в базу данных.
    /// Позволяет ввести название, номер выпуска, год публикации и количество копий.
    /// </summary>
    public class AddJournalForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private TextBox titleBox, issueNumberBox, yearBox, copiesBox; // Поля ввода данных журнала

        /// <summary>
        /// Конструктор формы добавления журнала.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public AddJournalForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация пользовательского интерфейса
        }

        /// <summary>
        /// Метод для инициализации пользовательского интерфейса.
        /// Создает поля ввода данных и кнопку "Сохранить".
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Добавить журнал"; // Заголовок окна
            this.Width = 400; // Устанавливаем ширину окна
            this.Height = 300; // Устанавливаем высоту окна

            // Основная компоновка формы
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всё пространство формы
                ColumnCount = 2, // Две колонки: для меток и полей ввода
                Padding = new Padding(10) // Внутренний отступ
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Ширина первой колонки
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Ширина второй колонки

            // Поле для ввода названия журнала
            layout.Controls.Add(new Label { Text = "Название:", Anchor = AnchorStyles.Right }, 0, 0);
            titleBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(titleBox, 1, 0);

            // Поле для ввода номера выпуска
            layout.Controls.Add(new Label { Text = "Номер выпуска:", Anchor = AnchorStyles.Right }, 0, 1);
            issueNumberBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(issueNumberBox, 1, 1);

            // Поле для ввода года публикации
            layout.Controls.Add(new Label { Text = "Год публикации:", Anchor = AnchorStyles.Right }, 0, 2);
            yearBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(yearBox, 1, 2);

            // Поле для ввода количества копий
            layout.Controls.Add(new Label { Text = "Количество копий:", Anchor = AnchorStyles.Right }, 0, 3);
            copiesBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(copiesBox, 1, 3);

            // Кнопка "Сохранить"
            var saveButton = new Button { Text = "Сохранить", Dock = DockStyle.Right, Width = 100 };
            saveButton.Click += SaveButton_Click; // Привязка обработчика событий
            layout.Controls.Add(saveButton, 1, 4);

            // Добавляем основную компоновку на форму
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить".
        /// Проверяет корректность введенных данных и добавляет новый журнал в базу данных.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Валидация данных перед сохранением
            if (string.IsNullOrWhiteSpace(titleBox.Text))
            {
                MessageBox.Show("Название журнала не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(issueNumberBox.Text, out int issueNumber) || issueNumber <= 0)
            {
                MessageBox.Show("Номер выпуска должен быть положительным числом!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(yearBox.Text, out int publicationYear) || publicationYear < 1901 || publicationYear > DateTime.Now.Year)
            {
                MessageBox.Show("Укажите корректный год публикации (от 1901 до текущего года)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(copiesBox.Text, out int copies) || copies < 0)
            {
                MessageBox.Show("Количество копий должно быть положительным числом или нулём!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Попытка добавления данных в базу
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для вставки данных о новом журнале
                    var query = @"
                        INSERT INTO Journals (Title, IssueNumber, PublicationYear, CopiesAvailable)
                        VALUES (@Title, @IssueNumber, @PublicationYear, @CopiesAvailable);";

                    // Подготовка команды и параметров
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", titleBox.Text);
                    command.Parameters.AddWithValue("@IssueNumber", issueNumber);
                    command.Parameters.AddWithValue("@PublicationYear", publicationYear);
                    command.Parameters.AddWithValue("@CopiesAvailable", copies);

                    command.ExecuteNonQuery(); // Выполняем SQL-запрос

                    // Уведомление об успешном добавлении
                    MessageBox.Show("Журнал успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Закрываем форму
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при выполнении запроса
                MessageBox.Show($"Ошибка добавления журнала: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

