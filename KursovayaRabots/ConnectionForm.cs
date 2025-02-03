using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    public class ConnectionForm : Form
    {
        private TextBox serverBox, userBox, passwordBox; // Поля ввода для данных подключения
        private Button connectButton; // Кнопка для подключения
        private Label statusLabel; // Метка для отображения состояния подключения

        public ConnectionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация элементов пользовательского интерфейса.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Подключение к базе данных"; // Заголовок окна
            this.Width = 400; // Устанавливаем ширину окна
            this.Height = 250; // Устанавливаем высоту окна
            this.StartPosition = FormStartPosition.CenterScreen; // Окно отображается в центре экрана

            // Основная компоновка
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всё пространство формы
                ColumnCount = 2, // Две колонки: для меток и полей ввода
                Padding = new Padding(10) // Внутренний отступ
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Первая колонка для меток
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Вторая колонка для полей ввода


            // Поле ввода для сервера
            layout.Controls.Add(new Label { Text = "Сервер:", Anchor = AnchorStyles.Right }, 0, 0);
            serverBox = new TextBox { Text = "localhost", Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(serverBox, 1, 0);

            // Поле ввода для пользователя
            layout.Controls.Add(new Label { Text = "Пользователь:", Anchor = AnchorStyles.Right }, 0, 1);
            userBox = new TextBox { Text = "root", Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(userBox, 1, 1);

            // Поле ввода для пароля
            layout.Controls.Add(new Label { Text = "Пароль:", Anchor = AnchorStyles.Right }, 0, 2);
            passwordBox = new TextBox { PasswordChar = '*', Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(passwordBox, 1, 2);

            // Кнопка подключения
            connectButton = new Button { Text = "Подключиться", Dock = DockStyle.Fill };
            connectButton.Click += ConnectButton_Click;
            layout.Controls.Add(connectButton, 1, 3);

            // Статус подключения
            statusLabel = new Label
            {
                Text = "Статус: Ожидание подключения",
                Dock = DockStyle.Top,
                ForeColor = SystemColors.GrayText
            };
            layout.Controls.Add(statusLabel, 0, 4);
            layout.SetColumnSpan(statusLabel, 2);

            this.Controls.Add(layout);
        }

        /// <summary>
        /// Обработчик нажатия кнопки подключения. Проверяет соединение и создаёт базу данных.
        /// </summary>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string server = serverBox.Text;
            string user = userBox.Text;
            string password = passwordBox.Text;

            string connectionString = $"Server={server};User ID={user};Password={password};";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    statusLabel.Text = "Статус: Подключение успешно!";
                    statusLabel.ForeColor = SystemColors.Highlight;

                    // Создаём базу данных и таблицы
                    CreateDatabase(connection);

                    // Переход к главной форме
                    this.Hide();
                    var mainForm = new MainForm(connectionString + "Database=LibraryDB;");
                    mainForm.ShowDialog();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Ошибка: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Создаёт базу данных LibraryDB и её таблицы, если она ещё не существуют.
        /// </summary>
        /// <param name="connection">Открытое соединение MySQL</param>
        private void CreateDatabase(MySqlConnection connection)
        {
            // Запросы для создания базы данных и таблиц
            string createDbQuery = "CREATE DATABASE IF NOT EXISTS LibraryDB;";
            string useDbQuery = "USE LibraryDB;";

            // Текущая структура таблиц
            string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    FullName VARCHAR(255) NOT NULL,
                    Address VARCHAR(255),
                    Phone VARCHAR(20),
                    RegistrationDate DATE NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Books (
                    BookID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    Author VARCHAR(255) NOT NULL,
                    PublicationYear YEAR NOT NULL,
                    CopiesAvailable INT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Journals (
                    JournalID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    IssueNumber INT NOT NULL,
                    PublicationYear YEAR NOT NULL,
                    CopiesAvailable INT NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Checkouts (
                    CheckoutID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    UserID INT DEFAULT NULL,
                    BookID INT DEFAULT NULL,
                    JournalID INT DEFAULT NULL,
                    CheckoutDate DATE NOT NULL,
                    DueDate DATE DEFAULT NULL,
                    ReturnDate DATE DEFAULT NULL,
                    FineCalculationDate DATE DEFAULT (CURDATE()),
                    OverdueFine DECIMAL(10, 2) GENERATED ALWAYS AS (
                        CASE
                            WHEN ReturnDate IS NOT NULL AND ReturnDate > DueDate
                            THEN (DATEDIFF(ReturnDate, DueDate) * 10)
                            ELSE 0
                        END
                    ) STORED,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID),
                    FOREIGN KEY (BookID) REFERENCES Books(BookID),
                    FOREIGN KEY (JournalID) REFERENCES Journals(JournalID)
                );
            ";

            // Выполнение запросов
            using (var command = new MySqlCommand($"{createDbQuery} {useDbQuery} {createTablesQuery}", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}



