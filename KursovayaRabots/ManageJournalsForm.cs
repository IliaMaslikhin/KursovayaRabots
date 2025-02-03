using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Форма для управления журналами.
    /// Поддерживает два режима работы:
    /// 1. Управление журналами (добавление, редактирование, удаление).
    /// 2. Выбор журнала (возвращает ID выбранного журнала).
    /// </summary>
    public class ManageJournalsForm : Form
    {
        private string connectionString; // Строка подключения к базе данных
        private DataGridView dataGridView; // Таблица для отображения журналов
        private Button addButton, editButton, deleteButton, selectButton; // Кнопки управления
        private bool isSelectionMode; // Флаг для проверки режима работы (выбор или управление)

        /// <summary>
        /// Возвращает ID выбранного журнала в режиме выбора.
        /// </summary>
        public int SelectedJournalId { get; private set; }

        /// <summary>
        /// Конструктор формы управления журналами.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="isSelectionMode">Режим работы: true для выбора журнала, false для управления</param>
        public ManageJournalsForm(string connectionString, bool isSelectionMode = false)
        {
            this.connectionString = connectionString; // Инициализация строки подключения
            this.isSelectionMode = isSelectionMode; // Устанавливаем режим работы

            InitializeComponent(); // Инициализация пользовательского интерфейса
            LoadJournals(); // Загрузка списка журналов
        }

        /// <summary>
        /// Метод инициализации пользовательского интерфейса.
        /// Создает таблицу данных, кнопки управления и настраивает режим отображения.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = isSelectionMode ? "Выбор журнала" : "Управление журналами"; // Заголовок окна
            this.Width = 800; // Устанавливаем ширину окна
            this.Height = 600; // Устанавливаем высоту окна

            // Основная компоновка элементов
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Занимает всё пространство формы
                RowCount = isSelectionMode ? 2 : 2, // В режиме выбора 2 строки, иначе 2
                ColumnCount = 1,
                Padding = new Padding(10) // Отступы внутри формы
            };

            // Настройка размеров строк
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85)); // Таблица данных (85% высоты)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Панель кнопок (15% высоты)

            // Инициализация таблицы для отображения данных
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill, // Занимает всё пространство строки
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Растягиваем колонки
                SelectionMode = DataGridViewSelectionMode.FullRowSelect // Полная строка выделяется
            };
            mainLayout.Controls.Add(dataGridView, 0, 0); // Добавляем таблицу в первую строку

            // Панель для кнопок управления
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Панель заполняет всю строку
                ColumnCount = isSelectionMode ? 1 : 3 // Если режим выбора, одна кнопка, иначе три
            };

            // Настройка пропорций колонок
            if (!isSelectionMode)
            {
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33)); // Первая кнопка занимает 33%
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33)); // Вторая кнопка занимает 33%
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33)); // Третья кнопка занимает 33%
            }

            // Если режим выбора
            if (isSelectionMode)
            {
                selectButton = new Button { Text = "Выбрать", Dock = DockStyle.Fill };
                selectButton.Click += SelectButton_Click; // Обработчик нажатия
                buttonPanel.Controls.Add(selectButton, 0, 0); // Кнопка занимает всю панель
            }
            else
            {
                // Кнопка "Добавить"
                addButton = new Button { Text = "Добавить", Dock = DockStyle.Fill };
                addButton.Click += AddButton_Click; // Обработчик нажатия
                buttonPanel.Controls.Add(addButton, 0, 0); // Кнопка в первой колонке

                // Кнопка "Редактировать"
                editButton = new Button { Text = "Редактировать", Dock = DockStyle.Fill };
                editButton.Click += EditButton_Click; // Обработчик нажатия
                buttonPanel.Controls.Add(editButton, 1, 0); // Кнопка во второй колонке

                // Кнопка "Удалить"
                deleteButton = new Button { Text = "Удалить", Dock = DockStyle.Fill };
                deleteButton.Click += DeleteButton_Click; // Обработчик нажатия
                buttonPanel.Controls.Add(deleteButton, 2, 0); // Кнопка в третьей колонке
            }

            // Добавляем панель кнопок на форму
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            // Добавляем основную компоновку на форму
            this.Controls.Add(mainLayout);
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

                    // SQL-запрос для получения данных о журналах с русскими названиями столбцов
                    string query = @"
                SELECT 
                    JournalID AS 'ID журнала',
                    Title AS 'Название',
                    IssueNumber AS 'Номер выпуска',
                    PublicationYear AS 'Год публикации',
                    CopiesAvailable AS 'Количество'
                FROM Journals";

                    var adapter = new MySqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table); // Заполняем таблицу данными
                    dataGridView.DataSource = table; // Привязываем данные к DataGridView
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки загрузки данных
                MessageBox.Show($"Ошибка загрузки журналов: {ex.Message}");
            }
        }


        /// <summary>
        /// Обработчик кнопки "Добавить".
        /// Открывает форму для добавления журнала и обновляет таблицу после завершения.
        /// </summary>
        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddJournalForm(connectionString); // Открываем форму добавления
            addForm.ShowDialog();
            LoadJournals(); // Обновляем таблицу
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать".
        /// Открывает форму для редактирования выбранного журнала и обновляет таблицу.
        /// </summary>
        private void EditButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите журнал для редактирования.");
                return;
            }

            // Получаем ID выбранного журнала
            var selectedRow = dataGridView.SelectedRows[0];
            int journalId = Convert.ToInt32(selectedRow.Cells["JournalID"].Value);

            var editForm = new EditJournalForm(connectionString, journalId); // Открываем форму редактирования
            editForm.ShowDialog();
            LoadJournals(); // Обновляем таблицу
        }

        /// <summary>
        /// Обработчик кнопки "Удалить".
        /// Удаляет выбранный журнал из базы данных и обновляет таблицу.
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите журнал для удаления.");
                return;
            }

            // Получаем ID выбранного журнала
            var selectedRow = dataGridView.SelectedRows[0];
            int journalId = Convert.ToInt32(selectedRow.Cells["JournalID"].Value);

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open(); // Открываем соединение

                    // SQL-запрос для удаления журнала
                    var query = "DELETE FROM Journals WHERE JournalID = @JournalID;";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@JournalID", journalId);
                    command.ExecuteNonQuery(); // Выполняем запрос

                    MessageBox.Show("Журнал успешно удалён!");
                    LoadJournals(); // Обновляем таблицу
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки удаления
                MessageBox.Show($"Ошибка удаления журнала: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Выбрать" в режиме выбора.
        /// Сохраняет ID выбранного журнала и закрывает форму.
        /// </summary>
        private void SelectButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите журнал.");
                return;
            }

            // Получаем ID выбранного журнала
            var selectedRow = dataGridView.SelectedRows[0];
            SelectedJournalId = Convert.ToInt32(selectedRow.Cells["JournalID"].Value);

            this.DialogResult = DialogResult.OK; // Возвращаем результат OK
            this.Close(); // Закрываем форму
        }
    }
}
