using MySql.Data.MySqlClient; // Подключение библиотеки для работы с MySQL
using System;
using System.Drawing; // Используется для задания шрифтов и цветов
using System.Windows.Forms; // Пространство имен для создания оконных приложений Windows Forms

namespace KursovayaRabots
{
    /// <summary>
    /// Главная форма программы для управления библиотекой.
    /// Здесь создается пользовательский интерфейс, который предоставляет
    /// доступ к различным функциям, таким как управление пользователями,
    /// книгами, журналами, выдачами, а также очистка базы данных.
    /// </summary>
    public class MainForm : Form
    {
        private string connectionString; // Строка подключения к базе данных MySQL

        /// <summary>
        /// Конструктор формы, принимающий строку подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных MySQL</param>
        public MainForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация элементов интерфейса
        }

        private Button clearDatabaseButton; // Кнопка для очистки базы данных

        /// <summary>
        /// Метод инициализации пользовательского интерфейса.
        /// Создает основную компоновку, кнопки и другие элементы управления.
        /// </summary>
        private void InitializeComponent()
        {
            // Устанавливаем свойства главной формы
            this.Text = "Программа для управления библиотекой";
            this.Width = 800;
            this.Height = 600;

            // Главная компоновка формы
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всё пространство формы
                RowCount = 6, // Задаем 6 строк в компоновке
                ColumnCount = 1, // Одна колонка
                Padding = new Padding(10) // Внутренний отступ в пикселях
            };

            // Устанавливаем пропорции строк
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Первая строка для ASCII-смайлика
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Вторая строка для заголовка
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Остальные строки для кнопок
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));

            // Добавляем ASCII-смайлик
            var smileyLabel = new Label
            {
                Text = @"
             _____  
           /      \  
          |  O  O | 
          \   ∆   /  
           -------  
          ", // Текст смайлика в виде ASCII-арт
                Dock = DockStyle.Fill, // Заполняет пространство ячейки
                TextAlign = ContentAlignment.MiddleCenter, // Выравнивание текста по центру
                Font = new Font("Consolas", 16), // Моноширинный шрифт для лучшего отображения ASCII
                AutoSize = true
            };
            layout.Controls.Add(smileyLabel);

            // Добавляем заголовок
            var titleLabel = new Label
            {
                Text = "Программа для управления базой данных библиотеки",
                Dock = DockStyle.Fill, // Заполняет ячейку
                TextAlign = ContentAlignment.MiddleCenter, // Центрирование текста
                Font = new Font("Arial", 14, FontStyle.Bold) // Используем жирный шрифт
            };
            layout.Controls.Add(titleLabel);

            // Создаем кнопки для различных функций
            var usersButton = new Button { Text = "Управление пользователями", Dock = DockStyle.Fill };
            usersButton.Click += (sender, args) => new ManageUsersForm(connectionString).ShowDialog();

            var booksButton = new Button { Text = "Управление книгами", Dock = DockStyle.Fill };
            booksButton.Click += (sender, args) => new ManageBooksForm(connectionString).ShowDialog();

            var journalsButton = new Button { Text = "Управление журналами", Dock = DockStyle.Fill };
            journalsButton.Click += (sender, args) => new ManageJournalsForm(connectionString).ShowDialog();

            var checkoutsButton = new Button { Text = "Управление выдачами", Dock = DockStyle.Fill };
            checkoutsButton.Click += (sender, args) => new ManageCheckoutsForm(connectionString).ShowDialog();

            var importExportButton = new Button { Text = "Импорт и экспорт базы данных", Dock = DockStyle.Fill };
            importExportButton.Click += ImportExportButton_Click;

            // Создаем кнопку для очистки базы данных
            clearDatabaseButton = new Button { Text = "Очистить базу данных", Dock = DockStyle.Fill, BackColor = Color.Red, ForeColor = Color.White };
            clearDatabaseButton.Click += ClearDatabaseButton_Click;

            // Добавляем кнопки в компоновку
            layout.Controls.Add(usersButton);
            layout.Controls.Add(booksButton);
            layout.Controls.Add(journalsButton);
            layout.Controls.Add(checkoutsButton);
            layout.Controls.Add(importExportButton);
            layout.Controls.Add(clearDatabaseButton);

            // Добавляем компоновку в форму
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Метод для обработки события нажатия на кнопку очистки базы данных.
        /// Запрашивает подтверждение у пользователя, затем очищает все данные из таблиц базы данных.
        /// </summary>
        private void ClearDatabaseButton_Click(object sender, EventArgs e)
        {
            // Подтверждение действия у пользователя
            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить базу данных? Все данные будут удалены, и это действие нельзя будет отменить.",
                "Подтверждение очистки",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение с базой данных

                        // SQL-запрос для удаления всех данных из таблиц и сброса автоинкремента
                        string deleteDataQuery = @"
                        DELETE FROM Checkouts;
                        DELETE FROM Books;
                        DELETE FROM Journals;
                        DELETE FROM Users;

                        -- Сбрасываем AUTO_INCREMENT для всех таблиц
                        ALTER TABLE Checkouts AUTO_INCREMENT = 1;
                        ALTER TABLE Books AUTO_INCREMENT = 1;
                        ALTER TABLE Journals AUTO_INCREMENT = 1;
                        ALTER TABLE Users AUTO_INCREMENT = 1;
                    ";

                        var command = new MySqlCommand(deleteDataQuery, connection);
                        command.ExecuteNonQuery(); // Выполняем запрос

                        // Сообщаем об успешной очистке
                        MessageBox.Show("База данных успешно очищена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при выполнении SQL-запроса
                    MessageBox.Show($"Ошибка очистки базы данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Метод для обработки события нажатия на кнопку импорта/экспорта базы данных.
        /// Открывает форму для работы с импортом и экспортом.
        /// </summary>
        private void ImportExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                var importExportForm = new ImportExportForm(connectionString);
                importExportForm.ShowDialog(); // Открываем форму импорта/экспорта
            }
            catch (Exception ex)
            {
                // Обработка ошибок при открытии формы
                MessageBox.Show($"Ошибка при открытии формы импорта/экспорта: {ex.Message}");
            }
        }
    }
}




