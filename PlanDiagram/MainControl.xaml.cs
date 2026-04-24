using PlanDiagram.Models;
using PlanDiagram.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PlanDiagram
{
    public partial class MainControl : UserControl
    {
        #region [Переменные класса]
        private GanttChart _ganttChartService;
        private List<ProcessData> _allTasks;
        private bool _isSyncing = false;
        #endregion

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();

            // Получаем процессы
            _allTasks = DataProvider.GetListProcesses();
            _ganttChartService = new GanttChart(GanttCanvas, DateHeaderCanvas);

            // Устанавливаем даты по умолчанию
            SetDefaultDates();

            // Устанавливаем DataContext для списка процессов
            TasksList.ItemsSource = _allTasks;
        }

        private void SetDefaultDates()
        {
            if (_allTasks == null || _allTasks.Count == 0)
            {
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);
                return;
            }

            StartDatePicker.SelectedDate = _allTasks.Min(t => t.PlanStartDate);
            EndDatePicker.SelectedDate = _allTasks.Max(t => t.PlanEndDate);
        }

        /// <summary>
        /// Обработка события нажатия на кнопку
        /// </summary>
        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите начальную и конечную дату", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Фильтруем задачи по датам для отображения в списке
            var filteredTasks = _allTasks
                .Where(t => t.PlanEndDate >= startDate && t.PlanStartDate <= endDate)
                .OrderBy(t => t.PlanStartDate)
                .ToList();

            TasksList.ItemsSource = filteredTasks;

            // Обновляем диаграмму
            _ganttChartService.Build(startDate, endDate, filteredTasks);

            // Сбрасываем прокрутку после построения
            ResetScrollPositions();
        }

        #region [Прокрутка диаграммы] 

        // Горизонтальная и вертикальная синхронизация
        private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing)
            {
                _isSyncing = true;

                // Синхронизация горизонтальной прокрутки с заголовком дат
                if (e.HorizontalChange != 0)
                {
                    DateHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                }

                // Синхронизация вертикальной прокрутки с левым списком
                if (e.VerticalChange != 0)
                {
                    LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                }

                _isSyncing = false;
            }
        }

        // Заголовки двигают диаграмму (только горизонтально)
        private void DateHeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing && e.HorizontalChange != 0)
            {
                _isSyncing = true;
                GanttScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                _isSyncing = false;
            }
        }

        // Вертикальная синхронизация (левый список двигает диаграмму)
        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing && e.VerticalChange != 0)
            {
                _isSyncing = true;
                GanttScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                _isSyncing = false;
            }
        }

        private void ResetScrollPositions()
        {
            GanttScrollViewer.ScrollToHome();
            DateHeaderScrollViewer.ScrollToHome();
            LeftScrollViewer.ScrollToTop();
        }

        #endregion
    }
}