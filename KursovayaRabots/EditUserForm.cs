using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для редактирования данных пользователя.
    /// Позволяет загрузить текущие данные пользователя из базы данных,
    /// внести изменения и сохранить их.
    /// </summary>
    public class EditUserForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private int userId; // Идентификатор пользователя, данные которого редактируются
        private TextBox fullNameBox, addressBox, phoneBox; // Поля для ввода данных

        /// <summary>
        /// Конструктор формы редактирования пользователя.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="userId">Идентификатор пользователя</param>
        public EditUserForm(string connectionString, int userId)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            this.userId = userId; // Инициализация идентификатора пользователя
            InitializeComponent(); // Инициализация пользовательского интерфейса
            LoadUserData(); // Загрузка текущих данных пользователя
        }

        /// <summary>
        /// Метод инициализации пользовательского интерфейса.
        /// Создает элементы управления для ввода данных и кнопку "Сохранить".
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Редактировать пользователя"; // Заголовок окна
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
            saveButton.Click += SaveButton_Click; // Обработчик нажатия кнопки
            layout.Controls.Add(saveButton, 1, 3);

            // Добавляем основную компоновку на форму
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Загружает текущие данные пользователя из базы данных и заполняет поля ввода.
        /// </summary>
        private void LoadUserData()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для получения данных пользователя
                    var query = "SELECT FullName, Address, Phone FROM Users WHERE UserID = @UserID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserID", userId); // Передаем параметр UserID

                    // Чтение данных из базы
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполняем поля данными пользователя
                            fullNameBox.Text = reader.GetString("FullName");
                            addressBox.Text = reader.GetString("Address");
                            phoneBox.Text = reader.GetString("Phone");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить".
        /// Проверяет корректность введенных данных и сохраняет изменения в базе.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(fullNameBox.Text))
            {
                MessageBox.Show("Полное имя не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!char.IsUpper(fullNameBox.Text[0]))
            {
                MessageBox.Show("Имя должно начинаться с заглавной буквы!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(addressBox.Text) && addressBox.Text.Length > 255)
            {
                MessageBox.Show("Адрес не может быть длиннее 255 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(phoneBox.Text, @"^\+\d+$")) // Проверяем формат телефона
            {
                MessageBox.Show("Номер телефона должен начинаться с + и содержать только цифры!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение

                    // SQL-запрос для обновления данных пользователя
                    var query = @"
                        UPDATE Users
                        SET FullName = @FullName, Address = @Address, Phone = @Phone
                        WHERE UserID = @UserID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FullName", fullNameBox.Text);
                    command.Parameters.AddWithValue("@Address", addressBox.Text);
                    command.Parameters.AddWithValue("@Phone", phoneBox.Text);
                    command.Parameters.AddWithValue("@UserID", userId);

                    command.ExecuteNonQuery(); // Выполняем запрос

                    // Уведомляем об успешной операции
                    MessageBox.Show("Данные пользователя успешно обновлены!");
                    this.Close(); // Закрываем форму
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при обновлении данных
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}");
            }
        }
    }
}


