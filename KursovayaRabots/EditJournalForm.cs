using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
        /// <summary>
        /// Форма для редактирования данных журнала.
        /// Позволяет загрузить текущие данные журнала из базы данных,
        /// внести изменения и сохранить их.
        /// </summary>
        public class EditJournalForm : Form
        {
            private string connectionString; // Строка подключения к базе данных
            private int journalId; // Идентификатор журнала, данные которого редактируются
            private TextBox titleBox, issueNumberBox, yearBox, copiesBox; // Поля ввода данных

            /// <summary>
            /// Конструктор формы редактирования журнала.
            /// </summary>
            /// <param name="connectionString">Строка подключения к базе данных</param>
            /// <param name="journalId">Идентификатор журнала</param>
            public EditJournalForm(string connectionString, int journalId)
            {
                this.connectionString = connectionString; // Инициализация строки подключения
                this.journalId = journalId; // Инициализация идентификатора журнала
                InitializeComponent(); // Инициализация пользовательского интерфейса
                LoadJournalData(); // Загрузка текущих данных журнала
            }

            /// <summary>
            /// Метод для инициализации пользовательского интерфейса.
            /// Создает элементы управления для ввода данных и кнопку "Сохранить".
            /// </summary>
            private void InitializeComponent()
            {
                this.Text = "Редактировать журнал"; // Заголовок окна
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

                layout.Controls.Add(new Label { Text = "Номер выпуска:", Anchor = AnchorStyles.Right }, 0, 1);
                issueNumberBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
                layout.Controls.Add(issueNumberBox, 1, 1);

                layout.Controls.Add(new Label { Text = "Год публикации:", Anchor = AnchorStyles.Right }, 0, 2);
                yearBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
                layout.Controls.Add(yearBox, 1, 2);

                layout.Controls.Add(new Label { Text = "Количество копий:", Anchor = AnchorStyles.Right }, 0, 3);
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
            /// Загружает текущие данные журнала из базы данных и заполняет поля ввода.
            /// </summary>
            private void LoadJournalData()
            {
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение с базой данных

                        // SQL-запрос для получения данных журнала
                        var query = "SELECT Title, IssueNumber, PublicationYear, CopiesAvailable FROM Journals WHERE JournalID = @JournalID;";
                        var command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@JournalID", journalId); // Передаем параметр JournalID

                        // Чтение данных из базы
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Заполняем поля ввода данными журнала
                                titleBox.Text = reader.GetString("Title");
                                issueNumberBox.Text = reader.GetInt32("IssueNumber").ToString();
                                yearBox.Text = reader.GetInt32("PublicationYear").ToString();
                                copiesBox.Text = reader.GetInt32("CopiesAvailable").ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при загрузке данных
                    MessageBox.Show($"Ошибка загрузки данных журнала: {ex.Message}");
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

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open(); // Открываем соединение

                        // SQL-запрос для обновления данных журнала
                        var query = @"
                    UPDATE Journals
                    SET Title = @Title, IssueNumber = @IssueNumber, PublicationYear = @PublicationYear, CopiesAvailable = @CopiesAvailable
                    WHERE JournalID = @JournalID;";
                        var command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Title", titleBox.Text);
                        command.Parameters.AddWithValue("@IssueNumber", issueNumber);
                        command.Parameters.AddWithValue("@PublicationYear", publicationYear);
                        command.Parameters.AddWithValue("@CopiesAvailable", copies);
                        command.Parameters.AddWithValue("@JournalID", journalId);

                        command.ExecuteNonQuery(); // Выполняем запрос

                        // Уведомляем об успешной операции
                        MessageBox.Show("Данные журнала успешно обновлены!");
                        this.Close(); // Закрываем форму
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при обновлении данных
                    MessageBox.Show($"Ошибка обновления журнала: {ex.Message}");
                }
            }
        }
    }

