using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для выбора журнала, который будет выдан пользователю.
    /// Отображает список всех доступных журналов, позволяет выбрать журнал и зарегистрировать выдачу.
    /// </summary>
    public class SelectJournalForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private int userId; // Идентификатор пользователя, которому выдается журнал
        private DataGridView dataGridView; // Таблица для отображения списка журналов

        /// <summary>
        /// Конструктор формы. Принимает строку подключения и идентификатор пользователя.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="userId">Идентификатор пользователя</param>
        public SelectJournalForm(string connectionString, int userId)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            this.userId = userId; // Инициализация идентификатора пользователя
            InitializeComponent(); // Инициализация элементов пользовательского интерфейса
            LoadJournals(); // Загрузка списка журналов из базы данных
        }

        /// <summary>
        /// Метод для инициализации элементов пользовательского интерфейса.
        /// Создает таблицу для отображения журналов и кнопку для их выдачи.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Выберите журнал"; // Заголовок окна
            this.Width = 600; // Устанавливаем ширину окна
            this.Height = 400; // Устанавливаем высоту окна

            // Инициализация таблицы для отображения журналов
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill, // Занимает все доступное пространство
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // Автоматически растягивает колонки
            };

            // Создаем кнопку "Выдать"
            var selectButton = new Button
            {
                Text = "Выдать", // Текст кнопки
                Dock = DockStyle.Bottom // Располагается в нижней части формы
            };
            selectButton.Click += SelectButton_Click; // Обработчик нажатия на кнопку

            // Добавляем элементы управления на форму
            this.Controls.Add(dataGridView);
            this.Controls.Add(selectButton);
        }

        /// <summary>
        /// Загружает список журналов из базы данных и отображает их в таблице.
        /// </summary>
        private void LoadJournals()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для получения списка всех журналов
                    string query = "SELECT JournalID, Title, IssueNumber, PublicationYear FROM Journals;";

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
                // Обработка ошибки подключения или выполнения запроса
                MessageBox.Show($"Ошибка загрузки журналов: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Выдать".
        /// Регистрирует выдачу выбранного журнала в базе данных.
        /// </summary>
        private void SelectButton_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка в таблице
            if (dataGridView.SelectedRows.Count > 0)
            {
                // Получаем данные выбранной строки
                var selectedRow = dataGridView.SelectedRows[0];
                int journalId = Convert.ToInt32(selectedRow.Cells["JournalID"].Value); // Идентификатор журнала

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение с базой данных

                        // SQL-запрос для регистрации выдачи журнала
                        var query = @"
                            INSERT INTO Checkouts (UserID, BookID, JournalID, CheckoutDate)
                            VALUES (@UserID, NULL, @JournalID, CURDATE());";

                        var command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@UserID", userId); // Устанавливаем значение UserID
                        command.Parameters.AddWithValue("@JournalID", journalId); // Устанавливаем значение JournalID

                        command.ExecuteNonQuery(); // Выполняем запрос

                        // Сообщаем об успешной операции
                        MessageBox.Show("Журнал успешно выдан!");
                        this.Close(); // Закрываем форму
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибки выполнения SQL-запроса
                    MessageBox.Show($"Ошибка выдачи журнала: {ex.Message}");
                }
            }
            else
            {
                // Если строка не выбрана, уведомляем пользователя
                MessageBox.Show("Выберите журнал.");
            }
        }
    }
}

