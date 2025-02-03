using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    /// <summary>
    /// Класс для управления импортом и экспортом базы данных MySQL.
    /// Позволяет пользователю сохранять текущую базу данных в файл и загружать базу данных из файла.
    /// </summary>
    public class ImportExportForm : Form
    {
        private readonly string connectionString; // Строка подключения к базе данных.

        /// <summary>
        /// Конструктор класса.
        /// Принимает строку подключения и инициализирует интерфейс формы.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        public ImportExportForm(string connectionString)
        {
            this.connectionString = connectionString; // Сохраняем строку подключения.
            InitializeComponent(); // Инициализация элементов интерфейса.
        }

        /// <summary>
        /// Метод для создания элементов интерфейса формы.
        /// </summary>
        private void InitializeComponent()
        {
            // Устанавливаем основные параметры окна.
            this.Text = "Импорт и экспорт базы данных"; // Заголовок окна.
            this.Width = 400; // Ширина окна.
            this.Height = 200; // Высота окна.

            // Создаем основную компоновку (TableLayoutPanel).
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всё пространство окна.
                RowCount = 2, // Две строки: для кнопки экспорта и кнопки импорта.
                ColumnCount = 1, // Одна колонка.
                Padding = new Padding(10) // Внутренние отступы.
            };

            // Определяем размеры строк (по 50% высоты на каждую кнопку).
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Создаем кнопку экспорта.
            var exportButton = new Button
            {
                Text = "Экспорт базы данных", // Текст на кнопке.
                Dock = DockStyle.Fill // Занимает всю строку.
            };
            exportButton.Click += ExportDatabase; // Привязываем метод обработки клика.

            // Создаем кнопку импорта.
            var importButton = new Button
            {
                Text = "Импорт базы данных", // Текст на кнопке.
                Dock = DockStyle.Fill // Занимает всю строку.
            };
            importButton.Click += ImportDatabase; // Привязываем метод обработки клика.

            // Добавляем кнопки в компоновку.
            layout.Controls.Add(exportButton);
            layout.Controls.Add(importButton);

            // Добавляем компоновку на форму.
            this.Controls.Add(layout);
        }

        /// <summary>
        /// Метод для экспорта базы данных в файл.
        /// </summary>
        private void ExportDatabase(object sender, EventArgs e)
        {
            // Открываем диалог сохранения файла.
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql", // Указываем фильтр для файлов SQL.
                Title = "Сохранить базу данных" // Заголовок диалога.
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK) // Если пользователь выбрал файл.
                {
                    string filePath = saveDialog.FileName; // Путь к файлу, указанный пользователем.

                    try
                    {
                        using (var connection = new MySqlConnection(connectionString)) // Открываем соединение с базой данных.
                        {
                            connection.Open(); // Открываем соединение.

                            using (var cmd = new MySqlCommand()) // Создаем команду MySQL.
                            {
                                cmd.Connection = connection; // Привязываем команду к соединению.

                                // Используем MySqlBackup для экспорта данных.
                                using (var mb = new MySqlBackup(cmd))
                                {
                                    mb.ExportInfo.AddCreateDatabase = true; // Включаем команду создания базы данных в экспорт.
                                    mb.ExportToFile(filePath); // Сохраняем базу данных в файл.
                                }
                            }
                        }

                        // Уведомляем пользователя об успешном экспорте.
                        MessageBox.Show("Экспорт базы данных выполнен успешно!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Уведомляем пользователя об ошибке.
                        MessageBox.Show($"Ошибка экспорта базы данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Метод для импорта базы данных из файла.
        /// </summary>
        private void ImportDatabase(object sender, EventArgs e)
        {
            // Открываем диалог выбора файла.
            using (var openDialog = new OpenFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql", // Указываем фильтр для файлов SQL.
                Title = "Выберите файл базы данных" // Заголовок диалога.
            })
            {
                if (openDialog.ShowDialog() == DialogResult.OK) // Если пользователь выбрал файл.
                {
                    string filePath = openDialog.FileName; // Путь к выбранному файлу.

                    try
                    {
                        using (var connection = new MySqlConnection(connectionString)) // Открываем соединение с базой данных.
                        {
                            connection.Open(); // Открываем соединение.

                            using (var cmd = new MySqlCommand()) // Создаем команду MySQL.
                            {
                                cmd.Connection = connection; // Привязываем команду к соединению.

                                // Используем MySqlBackup для импорта данных.
                                using (var mb = new MySqlBackup(cmd))
                                {
                                    mb.ImportFromFile(filePath); // Импортируем базу данных из файла.
                                }
                            }
                        }

                        // Уведомляем пользователя об успешном импорте.
                        MessageBox.Show("Импорт базы данных выполнен успешно!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Уведомляем пользователя об ошибке.
                        MessageBox.Show($"Ошибка импорта базы данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}