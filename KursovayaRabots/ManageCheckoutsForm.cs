using System;
using System.Data;
using System.Net;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;

namespace KursovayaRabots
{

    /// <summary>
    /// Класс формы "ManageCheckoutsForm".
    /// Отвечает за управление записями о выдаче книг и журналов.
    /// Позволяет:
    /// - Просматривать существующие записи выдач.
    /// - Выдавать книги и журналы.
    /// - Редактировать данные о выдачах.
    /// - Удалять записи.
    /// - Регистрировать возвраты с расчётом штрафов.
    /// </summary>
    /// <remarks>
    /// Основные элементы интерфейса:
    /// - Таблица DataGridView для отображения списка выдач.
    /// - ComboBox для выбора пользователей.
    /// - Кнопки для добавления, редактирования, удаления и регистрации возврата.
    /// 
    /// Подключается к базе данных MySQL для выполнения CRUD-операций.
    /// </remarks>
    public class ManageCheckoutsForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private DataGridView dataGridView; // Таблица для отображения выдач
        private ComboBox userComboBox; // Выпадающий список для выбора пользователя
        private Button addBookButton, addJournalButton, returnButton, deleteButton, editButton; // Кнопки управления

        /// <summary>
        /// Конструктор формы. Инициализирует интерфейс и загружает данные.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public ManageCheckoutsForm(string connectionString)
        {
            this.connectionString = connectionString; // Сохраняем строку подключения
            InitializeComponent(); // Инициализация элементов интерфейса
            LoadCheckouts(); // Загрузка данных о выдачах
            LoadUsers(); // Загрузка списка пользователей
        }


        /// <summary>
        /// Создает элементы интерфейса формы:
        /// - Таблицу для данных.
        /// - Выпадающий список пользователей.
        /// - Кнопки для выполнения операций.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Управление выдачей книг и журналов"; // Заголовок формы
            this.Width = 800; // Ширина формы
            this.Height = 600; // Высота формы

            // Основной макет формы
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Макет заполняет всю форму
                RowCount = 2, // Две строки: таблица и панель управления
                ColumnCount = 1, // Одна колонка
                Padding = new Padding(10)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // Таблица занимает 70% высоты
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Панель управления занимает 30%

            // Таблица для данных
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill, // Таблица занимает всю строку
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Столбцы автоматически масштабируются
                SelectionMode = DataGridViewSelectionMode.FullRowSelect // Разрешается выделение строк
            };
            mainLayout.Controls.Add(dataGridView, 0, 0); // Добавляем таблицу в первую строку

            // Панель управления
            var controlPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Панель заполняет всю строку
                RowCount = 3, // Три строки: метка + выпадающий список, кнопки управления
                ColumnCount = 3, // Три колонки для кнопок
                Padding = new Padding(5)
            };

            // Настройка строк и колонок
            controlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33)); // Каждая колонка занимает треть ширины
            controlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            controlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Поле выбора пользователя фиксированного размера
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Первая строка кнопок
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Вторая строка кнопок

            // Поле выбора пользователя
            var userLabel = new Label
            {
                Text = "Выберите пользователя:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            userComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };

            controlPanel.Controls.Add(userLabel, 0, 0); // Метка занимает первую строку
            controlPanel.Controls.Add(userComboBox, 1, 0); // Выпадающий список в центре
            controlPanel.SetColumnSpan(userComboBox, 2); // Список занимает две колонки

            // Кнопки управления
            addBookButton = new Button { Text = "Выдать книгу", Dock = DockStyle.Fill };
            addJournalButton = new Button { Text = "Выдать журнал", Dock = DockStyle.Fill };
            returnButton = new Button { Text = "Принять возврат", Dock = DockStyle.Fill };
            editButton = new Button { Text = "Редактировать выдачу", Dock = DockStyle.Fill };
            deleteButton = new Button { Text = "Удалить запись", Dock = DockStyle.Fill };

            // Обработчики событий
            addBookButton.Click += AddBookButton_Click;
            addJournalButton.Click += AddJournalButton_Click;
            returnButton.Click += ReturnButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;

            // Добавляем кнопки в панель
            controlPanel.Controls.Add(addBookButton, 0, 1);
            controlPanel.Controls.Add(addJournalButton, 1, 1);
            controlPanel.Controls.Add(returnButton, 2, 1);

            controlPanel.Controls.Add(editButton, 0, 2);
            controlPanel.Controls.Add(deleteButton, 1, 2);
            controlPanel.SetColumnSpan(deleteButton, 2); // Теперь кнопка занимает две колонки, но равного размера

            // Добавляем панель управления в основной макет
            mainLayout.Controls.Add(controlPanel, 0, 1);
            this.Controls.Add(mainLayout); // Добавляем основной макет на форму
        }






        /// <summary>
        /// Загружает данные о выдачах из базы данных в таблицу.
        /// </summary>
        private void LoadCheckouts()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение с базой данных

                    // SQL-запрос для получения данных о выдачах
                    string query = @"
                        SELECT 
                            CheckoutID AS 'Номер записи',
                            (SELECT FullName FROM Users WHERE Users.UserID = Checkouts.UserID) AS 'Пользователь',
                            (SELECT Title FROM Books WHERE Books.BookID = Checkouts.BookID) AS 'Книга',
                            (SELECT Title FROM Journals WHERE Journals.JournalID = Checkouts.JournalID) AS 'Журнал',
                            CheckoutDate AS 'Дата выдачи',
                            DueDate AS 'Выдать до',
                            ReturnDate AS 'Дата возврата',
                            OverdueFine AS 'Штраф'
                        FROM Checkouts";

                    var adapter = new MySqlDataAdapter(query, connection); // Заполняем адаптер
                    var table = new DataTable(); // Создаем таблицу для данных
                    adapter.Fill(table); // Заполняем таблицу данными

                    dataGridView.DataSource = table; // Устанавливаем таблицу как источник данных
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибки подключения или выполнения запроса
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает список пользователей из базы данных в выпадающий список (ComboBox).
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
                {
                    connection.Open(); // Открываем соединение

                    // SQL-запрос для получения списка пользователей
                    string userQuery = "SELECT UserID, FullName FROM Users;";
                    var userAdapter = new MySqlDataAdapter(userQuery, connection); // Используем адаптер для выполнения запроса
                    var userTable = new DataTable(); // Создаём таблицу для данных
                    userAdapter.Fill(userTable); // Заполняем таблицу результатами запроса

                    // Настраиваем ComboBox для отображения пользователей
                    userComboBox.DataSource = userTable; // Устанавливаем источник данных
                    userComboBox.DisplayMember = "FullName"; // Отображаемое имя
                    userComboBox.ValueMember = "UserID"; // Значение, связанное с выбранным элементом
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при подключении или выполнении запроса
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }


        /// <summary>
        /// Обработчик события изменения выделения строки в таблице.
        /// Включает или отключает кнопку редактирования в зависимости от статуса возврата.
        /// </summary>
        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0) // Проверяем, выбрана ли строка
            {
                var selectedRow = dataGridView.SelectedRows[0]; // Получаем выделенную строку
                var returnDate = selectedRow.Cells["Дата возврата"].Value; // Считываем значение столбца "Дата возврата"

                // Включаем кнопку "Редактировать", если возврат ещё не выполнен
                editButton.Enabled = returnDate == DBNull.Value || returnDate == null;
            }
        }



        /// <summary>
        /// Обработчик кнопки "Выдать книгу".
        /// Показывает диалог для выбора книги и добавляет запись о выдаче в базу данных.
        /// </summary>
        private void AddBookButton_Click(object sender, EventArgs e)
        {
            if (userComboBox.SelectedIndex < 0) // Проверяем, выбран ли пользователь
            {
                MessageBox.Show("Выберите пользователя."); // Если пользователь не выбран, показываем сообщение
                return;
            }

            int userId = Convert.ToInt32(userComboBox.SelectedValue); // Получаем ID выбранного пользователя

            using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
            {
                connection.Open(); // Открываем соединение

                // Запрос для получения доступных книг
                string query = "SELECT BookID, Title FROM Books WHERE CopiesAvailable > 0;";
                var bookTable = new DataTable(); // Таблица для хранения результатов запроса
                var adapter = new MySqlDataAdapter(query, connection); // Адаптер для выполнения запроса
                adapter.Fill(bookTable); // Заполняем таблицу

                if (bookTable.Rows.Count == 0) // Если нет доступных книг
                {
                    MessageBox.Show("Нет доступных книг для выдачи."); // Показываем сообщение
                    return;
                }

                // Создаём диалоговое окно для выбора книги
                var selectDialog = new Form
                {
                    Text = "Выберите книгу", // Заголовок окна
                    Width = 400,
                    Height = 300
                };

                // Компоненты диалога
                var bookComboBox = new ComboBox
                {
                    Dock = DockStyle.Top,
                    DataSource = bookTable,
                    DisplayMember = "Title",
                    ValueMember = "BookID"
                };

                var datePicker = new DateTimePicker
                {
                    Dock = DockStyle.Top,
                    Format = DateTimePickerFormat.Short,
                    MinDate = DateTime.Now // Минимальная дата — текущий день
                };

                var confirmButton = new Button
                {
                    Text = "Подтвердить",
                    Dock = DockStyle.Top
                };

                // Добавляем компоненты в диалог
                selectDialog.Controls.Add(confirmButton);
                selectDialog.Controls.Add(datePicker);
                selectDialog.Controls.Add(bookComboBox);

                // Переменные для хранения результатов выбора
                int? selectedBookId = null;
                DateTime? dueDate = null;

                // Обработчик подтверждения выбора
                confirmButton.Click += (s, args) =>
                {
                    selectedBookId = (int?)bookComboBox.SelectedValue;
                    dueDate = datePicker.Value;
                    selectDialog.DialogResult = DialogResult.OK;
                    selectDialog.Close();
                };

                // Показываем диалог и обрабатываем результаты
                if (selectDialog.ShowDialog() != DialogResult.OK || selectedBookId == null)
                {
                    MessageBox.Show("Выбор книги отменён."); // Если выбор отменён, показываем сообщение
                    return;
                }

                // Добавляем запись о выдаче книги в базу данных
                try
                {
                    string insertQuery = @"
                    INSERT INTO Checkouts (UserID, BookID, CheckoutDate, DueDate, ReturnDate, FineCalculationDate)
                    VALUES (@UserID, @BookID, CURDATE(), @DueDate, NULL, CURDATE());";

                    using (var command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId); // ID пользователя
                        command.Parameters.AddWithValue("@BookID", selectedBookId.Value); // ID книги
                        command.Parameters.AddWithValue("@DueDate", dueDate); // Дата возврата
                        command.ExecuteNonQuery(); // Выполняем команду
                    }

                    // Обновляем количество доступных экземпляров книги
                    string updateQuery = "UPDATE Books SET CopiesAvailable = CopiesAvailable - 1 WHERE BookID = @BookID;";
                    using (var command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BookID", selectedBookId.Value);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Книга успешно выдана!"); // Показываем сообщение об успехе
                    LoadCheckouts(); // Обновляем данные в таблице
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выдачи книги: {ex.Message}"); // Показываем сообщение об ошибке
                }
            }
        }




        /// <summary>
        /// Обработчик кнопки "Выдать журнал".
        /// Показывает диалог для выбора журнала и добавляет запись о выдаче в базу данных.
        /// </summary>
        private void AddJournalButton_Click(object sender, EventArgs e)
        {
            if (userComboBox.SelectedIndex < 0) // Проверяем, выбран ли пользователь
            {
                MessageBox.Show("Выберите пользователя."); // Если пользователь не выбран, показываем сообщение
                return;
            }

            int userId = Convert.ToInt32(userComboBox.SelectedValue); // Получаем ID выбранного пользователя

            try
            {
                using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
                {
                    connection.Open(); // Открываем соединение

                    // Запрос для получения доступных журналов
                    string journalQuery = "SELECT JournalID, Title FROM Journals WHERE CopiesAvailable > 0;";
                    var journalAdapter = new MySqlDataAdapter(journalQuery, connection); // Адаптер для выполнения запроса
                    var journalTable = new DataTable(); // Таблица для хранения результатов запроса
                    journalAdapter.Fill(journalTable); // Заполняем таблицу

                    if (journalTable.Rows.Count == 0) // Если нет доступных журналов
                    {
                        MessageBox.Show("Нет доступных журналов для выдачи."); // Показываем сообщение
                        return;
                    }

                    // Создаём диалоговое окно для выбора журнала
                    var dialog = new Form
                    {
                        Text = "Выбор журнала", // Заголовок окна
                        Width = 400,
                        Height = 200,
                        StartPosition = FormStartPosition.CenterParent // Центрируем окно
                    };

                    // Настраиваем макет диалога
                    var layout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 2,
                        Padding = new Padding(10)
                    };
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Первая колонка для меток
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Вторая колонка для компонентов

                    // Добавляем метку и выпадающий список для журналов
                    layout.Controls.Add(new Label { Text = "Журнал:", Anchor = AnchorStyles.Right }, 0, 0);
                    var journalComboBox = new ComboBox
                    {
                        DataSource = journalTable, // Устанавливаем источник данных
                        DisplayMember = "Title", // Отображаемое имя
                        ValueMember = "JournalID", // Значение, связанное с элементом
                        Dock = DockStyle.Fill
                    };
                    layout.Controls.Add(journalComboBox, 1, 0);

                    // Добавляем метку и выбор даты возврата
                    layout.Controls.Add(new Label { Text = "Выдать до:", Anchor = AnchorStyles.Right }, 0, 1);
                    var dueDatePicker = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Short, // Формат даты
                        MinDate = DateTime.Now, // Минимальная дата — текущий день
                        Dock = DockStyle.Fill
                    };
                    layout.Controls.Add(dueDatePicker, 1, 1);

                    // Кнопка подтверждения
                    var confirmButton = new Button
                    {
                        Text = "Подтвердить",
                        Dock = DockStyle.Bottom
                    };
                    dialog.Controls.Add(layout); // Добавляем макет в окно
                    dialog.Controls.Add(confirmButton); // Добавляем кнопку

                    confirmButton.Click += (s, args) =>
                    {
                        if (journalComboBox.SelectedValue == null) // Проверяем, выбран ли журнал
                        {
                            MessageBox.Show("Выберите журнал."); // Если журнал не выбран, показываем сообщение
                            return;
                        }

                        int journalId = Convert.ToInt32(journalComboBox.SelectedValue); // Получаем ID журнала
                        DateTime dueDate = dueDatePicker.Value; // Получаем дату возврата

                        try
                        {
                            // Добавляем запись о выдаче в таблицу Checkouts
                            var insertQuery = @"
                        INSERT INTO Checkouts (UserID, JournalID, CheckoutDate, DueDate, ReturnDate, FineCalculationDate)
                        VALUES (@UserID, @JournalID, CURDATE(), @DueDate, NULL, CURDATE());";
                            var insertCommand = new MySqlCommand(insertQuery, connection);
                            insertCommand.Parameters.AddWithValue("@UserID", userId);
                            insertCommand.Parameters.AddWithValue("@JournalID", journalId);
                            insertCommand.Parameters.AddWithValue("@DueDate", dueDate);
                            insertCommand.ExecuteNonQuery();

                            // Уменьшаем количество доступных экземпляров журнала
                            var updateQuery = "UPDATE Journals SET CopiesAvailable = CopiesAvailable - 1 WHERE JournalID = @JournalID;";
                            var updateCommand = new MySqlCommand(updateQuery, connection);
                            updateCommand.Parameters.AddWithValue("@JournalID", journalId);
                            updateCommand.ExecuteNonQuery();

                            MessageBox.Show("Журнал успешно выдан!"); // Сообщаем об успешной операции
                            dialog.Close(); // Закрываем диалог
                            LoadCheckouts(); // Обновляем данные в таблице
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка выдачи журнала: {ex.Message}"); // Сообщаем об ошибке
                        }
                    };

                    dialog.ShowDialog(); // Показываем диалог
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}"); // Обработка ошибок подключения
            }
        }





        /// <summary>
        /// Обработчик кнопки "Редактировать выдачу".
        /// Показывает диалог для редактирования данных о выдаче книги или журнала.
        /// </summary>
        private void EditButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) // Проверяем, выбрана ли запись
            {
                MessageBox.Show("Выберите запись для редактирования."); // Если нет, показываем сообщение
                return;
            }

            var selectedRow = dataGridView.SelectedRows[0]; // Получаем выделенную строку
            int checkoutId = Convert.ToInt32(selectedRow.Cells["Номер записи"].Value); // Получаем ID выдачи

            try
            {
                using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
                {
                    connection.Open(); // Открываем соединение

                    // Запрос для получения данных о выдаче
                    var query = @"
                SELECT CheckoutDate, DueDate
                FROM Checkouts
                WHERE CheckoutID = @CheckoutID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CheckoutID", checkoutId);

                    DateTime checkoutDate = DateTime.MinValue;
                    DateTime dueDate = DateTime.MinValue;

                    using (var reader = command.ExecuteReader()) // Выполняем запрос и считываем результаты
                    {
                        if (reader.Read()) // Если найдена запись
                        {
                            checkoutDate = reader["CheckoutDate"] != DBNull.Value ? (DateTime)reader["CheckoutDate"] : DateTime.Now;
                            dueDate = reader["DueDate"] != DBNull.Value ? (DateTime)reader["DueDate"] : DateTime.Now.AddDays(14);
                        }
                    }

                    // Создаём диалог для редактирования
                    var dialog = new Form
                    {
                        Text = "Редактировать выдачу", // Заголовок
                        Width = 400,
                        Height = 200
                    };

                    // Макет для компонентов
                    var layout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 2,
                        Padding = new Padding(10)
                    };
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

                    // Компоненты диалога: поля для дат выдачи и возврата
                    layout.Controls.Add(new Label { Text = "Дата выдачи:", Anchor = AnchorStyles.Right }, 0, 0);
                    var checkoutDatePicker = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Short,
                        Value = checkoutDate
                    };
                    layout.Controls.Add(checkoutDatePicker, 1, 0);

                    layout.Controls.Add(new Label { Text = "Выдать до:", Anchor = AnchorStyles.Right }, 0, 1);
                    var dueDatePicker = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Short,
                        Value = dueDate,
                        MinDate = checkoutDate // Минимальная дата — дата выдачи
                    };
                    layout.Controls.Add(dueDatePicker, 1, 1);

                    var saveButton = new Button
                    {
                        Text = "Сохранить",
                        Dock = DockStyle.Bottom
                    };

                    // Обработчик кнопки "Сохранить"
                    saveButton.Click += (s, args) =>
                    {
                        try
                        {
                            // Обновляем запись в таблице Checkouts
                            var updateQuery = @"
                        UPDATE Checkouts
                        SET CheckoutDate = @CheckoutDate, DueDate = @DueDate
                        WHERE CheckoutID = @CheckoutID;";
                            var updateCommand = new MySqlCommand(updateQuery, connection);
                            updateCommand.Parameters.AddWithValue("@CheckoutDate", checkoutDatePicker.Value);
                            updateCommand.Parameters.AddWithValue("@DueDate", dueDatePicker.Value);
                            updateCommand.Parameters.AddWithValue("@CheckoutID", checkoutId);
                            updateCommand.ExecuteNonQuery();

                            MessageBox.Show("Данные успешно обновлены!"); // Сообщаем об успешной операции
                            dialog.Close(); // Закрываем диалог
                            LoadCheckouts(); // Обновляем данные в таблице
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка обновления: {ex.Message}"); // Сообщаем об ошибке
                        }
                    };

                    layout.Controls.Add(saveButton, 1, 2); // Добавляем кнопку в макет
                    dialog.Controls.Add(layout); // Добавляем макет в диалог
                    dialog.ShowDialog(); // Показываем диалог
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}"); // Сообщаем об ошибке
            }
        }


        /// <summary>
        /// Обработчик кнопки "Принять возврат".
        /// Помечает запись о выдаче как возвращённую и рассчитывает штраф при необходимости.
        /// </summary>
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) // Проверяем, выбрана ли запись
            {
                MessageBox.Show("Выберите запись для возврата."); // Если запись не выбрана, показываем сообщение
                return;
            }

            var selectedRow = dataGridView.SelectedRows[0]; // Получаем выделенную строку
            int checkoutId = Convert.ToInt32(selectedRow.Cells["Номер записи"].Value); // Получаем ID выдачи

            try
            {
                using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
                {
                    connection.Open(); // Открываем соединение

                    // Проверяем, была ли запись уже возвращена
                    var query = "SELECT ReturnDate FROM Checkouts WHERE CheckoutID = @CheckoutID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CheckoutID", checkoutId);

                    var returnDate = command.ExecuteScalar(); // Получаем значение ReturnDate
                    if (returnDate != DBNull.Value) // Если запись уже имеет ReturnDate
                    {
                        MessageBox.Show("Эта запись уже была возвращена ранее."); // Сообщаем об этом
                        return;
                    }

                    // Устанавливаем ReturnDate в текущую дату
                    query = "UPDATE Checkouts SET ReturnDate = CURDATE() WHERE CheckoutID = @CheckoutID;";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CheckoutID", checkoutId);
                    command.ExecuteNonQuery(); // Обновляем запись

                    LoadCheckouts(); // Обновляем таблицу записей

                    // Рассчитываем и выводим штраф (если есть)
                    decimal fine = 0;
                    query = "SELECT OverdueFine FROM Checkouts WHERE CheckoutID = @CheckoutID;";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CheckoutID", checkoutId);
                    fine = Convert.ToDecimal(command.ExecuteScalar()); // Получаем значение штрафа

                    MessageBox.Show($"Возврат успешно зарегистрирован! Штраф: {fine} руб.",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка возврата: {ex.Message}"); // Обрабатываем ошибку
            }
        }


        /// <summary>
        /// Обработчик кнопки "Удалить запись".
        /// Удаляет выбранную запись о выдаче из базы данных.
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) // Проверяем, выбрана ли запись
            {
                MessageBox.Show("Выберите запись для удаления."); // Если запись не выбрана, показываем сообщение
                return;
            }

            var selectedRow = dataGridView.SelectedRows[0]; // Получаем выделенную строку
            int checkoutId = Convert.ToInt32(selectedRow.Cells["ID"].Value); // Получаем ID выдачи

            try
            {
                using (var connection = new MySqlConnection(connectionString)) // Устанавливаем соединение с базой данных
                {
                    connection.Open(); // Открываем соединение

                    // Удаляем запись из таблицы Checkouts
                    string query = "DELETE FROM Checkouts WHERE CheckoutID = @CheckoutID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CheckoutID", checkoutId);
                    command.ExecuteNonQuery(); // Выполняем запрос

                    MessageBox.Show("Запись успешно удалена!"); // Сообщаем об успешной операции
                    LoadCheckouts(); // Обновляем таблицу записей
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления записи: {ex.Message}"); // Обрабатываем ошибку
            }
        }
    }
}


