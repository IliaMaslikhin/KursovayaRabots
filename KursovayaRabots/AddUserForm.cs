using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для добавления нового пользователя в базу данных.
    /// Позволяет ввести имя, адрес и номер телефона пользователя,
    /// а затем сохранить их в базе данных.
    /// </summary>
    public class AddUserForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private TextBox fullNameBox, addressBox, phoneBox; // Поля ввода данных пользователя

        /// <summary>
        /// Конструктор формы добавления пользователя.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public AddUserForm(string connectionString)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            InitializeComponent(); // Инициализация пользовательского интерфейса
        }

        /// <summary>
        /// Метод для инициализации пользовательского интерфейса.
        /// Создает элементы управления для ввода данных и кнопку "Сохранить".
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Добавить пользователя"; // Заголовок окна
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

            // Поле для ввода полного имени
            layout.Controls.Add(new Label { Text = "Полное имя:", Anchor = AnchorStyles.Right }, 0, 0);
            fullNameBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(fullNameBox, 1, 0);

            // Поле для ввода адреса
            layout.Controls.Add(new Label { Text = "Адрес:", Anchor = AnchorStyles.Right }, 0, 1);
            addressBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(addressBox, 1, 1);

            // Поле для ввода телефона
            layout.Controls.Add(new Label { Text = "Телефон:", Anchor = AnchorStyles.Right }, 0, 2);
            phoneBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(phoneBox, 1, 2);

            // Кнопка "Сохранить"
            var saveButton = new Button { Text = "Сохранить", Dock = DockStyle.Right, Width = 100 };
            saveButton.Click += SaveButton_Click; // Обработчик нажатия кнопки "Сохранить"
            layout.Controls.Add(saveButton, 1, 3);

            // Добавляем основную компоновку на форму
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить".
        /// Проверяет корректность введенных данных и сохраняет пользователя в базе данных.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Валидация полного имени
            if (string.IsNullOrWhiteSpace(fullNameBox.Text))
            {
                MessageBox.Show("Полное имя не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка, что имя начинается с заглавной буквы
            if (!char.IsUpper(fullNameBox.Text[0]))
            {
                MessageBox.Show("Имя должно начинаться с заглавной буквы!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Валидация адреса (максимум 255 символов)
            if (!string.IsNullOrWhiteSpace(addressBox.Text) && addressBox.Text.Length > 255)
            {
                MessageBox.Show("Адрес не может быть длиннее 255 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Валидация номера телефона (должен начинаться с + и содержать только цифры)
            if (!Regex.IsMatch(phoneBox.Text, @"^\+\d+$"))
            {
                MessageBox.Show("Номер телефона должен начинаться с + и содержать только цифры!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Попытка сохранения данных в базу данных
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для вставки нового пользователя
                    var query = @"
                        INSERT INTO Users (FullName, Address, Phone, RegistrationDate)
                        VALUES (@FullName, @Address, @Phone, CURDATE());";

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FullName", fullNameBox.Text);
                    command.Parameters.AddWithValue("@Address", addressBox.Text);
                    command.Parameters.AddWithValue("@Phone", phoneBox.Text);

                    command.ExecuteNonQuery(); // Выполняем запрос

                    // Уведомляем пользователя об успешном добавлении
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Закрываем форму
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при выполнении запроса
                MessageBox.Show($"Ошибка добавления пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}