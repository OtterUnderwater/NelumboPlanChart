using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PlanDiagram.Helpers;
using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using PlanDiagram.Services;

namespace PlanDiagram
{
    public partial class MainControl : UserControl, INotifyPropertyChanged
    {
        #region [Переменные класса]
        private IGanttChart _ganttChartService;
        private IEnumerable _leftColumnItems;
        private string _leftColumnTitle;
        private PlanRepository _planRepository;
        private List<ProcessData> _allProcess;
        private List<DateTime> _allWorkDays;
        private Item _selectedItem; // Добавляем поле для хранения выбранного изделия

        public List<ProductionOrder> ProductionOrders { get; private set; }
        public List<ClientOrder> ClientOrders { get; private set; }
        public List<Item> Items { get; private set; }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                UpdateSelectedFiltersDisplay();
                UpdatePlan();
            }
        }

        private Visibility _showColorIndicators = Visibility.Collapsed;
        public Visibility ShowColorIndicators
        {
            get => _showColorIndicators;
            set { _showColorIndicators = value; OnPropertyChanged(nameof(ShowColorIndicators)); }
        }

        public IEnumerable LeftColumnItems
        {
            get => _leftColumnItems;
            set { _leftColumnItems = value; OnPropertyChanged(nameof(LeftColumnItems)); }
        }

        public string LeftColumnTitle
        {
            get => _leftColumnTitle;
            set { _leftColumnTitle = value; OnPropertyChanged(nameof(LeftColumnTitle)); }
        }
        #endregion

        #region [Обработчики событий]
        private void BuildButton_Click(object sender, RoutedEventArgs e) => UpdatePlan();
        private void FilterChanged(object sender, RoutedEventArgs e) => UpdateSelectedFiltersDisplay();
        #endregion

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();
            DataContext = this;
            string connectionString = (string)parameters["ConnectionString"];
            int regID = (int)parameters["RegID"];
            int? orderID = (int?)parameters["OrderID"];

            _planRepository = new PlanRepository(connectionString, orderID);
            var result = _planRepository.GetPlanList();
            _allProcess = result.process;
            ProductionOrders = result.prodOrders;
            ClientOrders = result.clientOrders;
            Items = result.items;

            _allWorkDays = _planRepository.GetWorkingDaysFromCalendar();

            if (orderID == null)
            {
                _ganttChartService = new GanttChartByWorkCenter();
                ShowColorIndicators = Visibility.Collapsed;
            }
            else
            {
                _ganttChartService = new GanttChart();
                ShowColorIndicators = Visibility.Visible;
            }
            SetDefaultDates(orderID);
        }

        private void UpdateLeftColumn(List<ProcessData> sourceTasks = null)
        {
            if (sourceTasks == null) sourceTasks = _allProcess;

            if (_ganttChartService is GanttChartByWorkCenter)
            {
                var workCenters = sourceTasks
                    .Select(p => new { p.WorkCenterName, p.Sort })
                    .Distinct()
                    .OrderBy(w => w.Sort)
                    .Select(w => w.WorkCenterName)
                    .ToList();

                LeftColumnItems = workCenters;
                LeftColumnTitle = "Рабочее место";
            }
            else
            {
                LeftColumnItems = sourceTasks;
                LeftColumnTitle = "Процесс / Операция";
            }
        }

        private void SetDefaultDates(int? OrderID)
        {
            if (_allProcess == null || _allProcess.Count == 0 || OrderID == null)
            {
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(47);
                UpdateLeftColumn();
                return;
            }

            StartDatePicker.SelectedDate = _allProcess.Min(t => t.PlanStartTime);
            EndDatePicker.SelectedDate = _allProcess.Max(t => t.PlanEndTime);
            UpdatePlan();
        }

        private void UpdatePlan()
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

            // Применяем фильтры
            var filteredProc = _allProcess
                .Where(p => DateHelper.IsDateRange(p.PlanStartTime, p.PlanEndTime, startDate, endDate))
                .ToList();

            // фильтр по заказам производства
            var selectedProductionOrders = ProductionOrders?.Where(o => o.IsSelected).Select(o => o.OrderID).ToList();
            if (selectedProductionOrders != null && selectedProductionOrders.Any() && !selectedProductionOrders.Contains(0))
            {
                filteredProc = filteredProc.Where(p => selectedProductionOrders.Contains(p.OrderID)).ToList();
            }

            // фильтр по заказам клиента
            var selectedClientOrders = ClientOrders?.Where(c => c.IsSelected).Select(c => c.ClientOrderID).ToList();
            if (selectedClientOrders != null && selectedClientOrders.Any() && !selectedClientOrders.Contains(0))
            {
                filteredProc = filteredProc.Where(p => selectedClientOrders.Contains(p.ClientOrderID)).ToList();
            }

            // фильтр по изделию
            if (SelectedItem != null && SelectedItem.ItemID != 0)
            {
                filteredProc = filteredProc.Where(p => p.ItemID == SelectedItem.ItemID).ToList();
            }

            filteredProc = filteredProc.OrderBy(p => p.PlanStartTime).ThenBy(p => p.PlanEndTime).ToList();

            // Фильтруем рабочие дни по тому же диапазону
            var filteredWorkDays = _allWorkDays
                .Where(d => d >= startDate && d <= endDate)
                .OrderBy(d => d)
                .ToList();

            // Обновляем левую колонку
            UpdateLeftColumn(filteredProc);

            // Строим диаграмму
            GanttCanvas.Children.Clear();
            DateHeaderCanvas.Children.Clear();

            _ganttChartService.Build(filteredProc, filteredWorkDays, GanttCanvas);
            GanttHelper.DrawDateHeader(DateHeaderCanvas, filteredWorkDays);
            ResetScrollPositions();
        }

        private void UpdateSelectedFiltersDisplay()
        {
            var selected = new List<string>();

            if (ProductionOrders != null && ProductionOrders.Any(x => x.IsSelected))
            {
                var selectedOrders = ProductionOrders.Where(x => x.IsSelected).Select(x => x.DocNumber).ToList();
                if (selectedOrders.Contains("Все") || selectedOrders.Count == ProductionOrders.Count)
                    selected.Add("📦 Заказы П: Все");
                else if (selectedOrders.Count <= 3)
                    selected.Add($"📦 Заказы П: {string.Join(", ", selectedOrders)}");
                else
                    selected.Add($"📦 Заказы П: {selectedOrders.Count} шт.");
            }

            if (ClientOrders != null && ClientOrders.Any(x => x.IsSelected))
            {
                var selectedClients = ClientOrders.Where(x => x.IsSelected).Select(x => x.DocNumber).ToList();
                if (selectedClients.Contains("Все") || selectedClients.Count == ClientOrders.Count)
                    selected.Add("📋 Заказы К: Все");
                else if (selectedClients.Count <= 3)
                    selected.Add($"📋 Заказы К: {string.Join(", ", selectedClients)}");
                else
                    selected.Add($"📋 Заказы К: {selectedClients.Count} шт.");
            }

            if (SelectedItem != null && SelectedItem.ItemID != 0)
            {
                if (SelectedItem.ItemFullName == "Все")
                    selected.Add("🔧 Изделие: Все");
                else
                    selected.Add($"🔧 Изделие: {SelectedItem.ItemFullName}");
            }

            SelectedFiltersText.Text = selected.Any()
                ? string.Join(" | ", selected)
                : "🔍 Фильтры не выбраны (показаны все данные)";
        }

        private void OpenSearchWindow()
        {
            // Получаем текущие выбранные элементы
            var selectedProdOrders = ProductionOrders?.Where(o => o.IsSelected).ToList() ?? new List<ProductionOrder>();
            var selectedClientOrders = ClientOrders?.Where(c => c.IsSelected).ToList() ?? new List<ClientOrder>();
            var selectedItem = SelectedItem;

            var window = new SearchWindow(
                ProductionOrders ?? new List<ProductionOrder>(),
                ClientOrders ?? new List<ClientOrder>(),
                Items ?? new List<Item>(),
                selectedProdOrders,
                selectedClientOrders,
                selectedItem
            );

            if (window.ShowDialog() == true)
            {
                bool needUpdate = false;

                // Обновляем выбранные заказы производства
                if (ProductionOrders != null && window.SelectedProductionOrders != null)
                {
                    var selectedIds = window.SelectedProductionOrders.Select(o => o.OrderID).ToHashSet();
                    foreach (var order in ProductionOrders)
                    {
                        bool newSelected = selectedIds.Contains(order.OrderID);
                        if (order.IsSelected != newSelected)
                        {
                            order.IsSelected = newSelected;
                            needUpdate = true;
                        }
                    }
                }

                // Обновляем выбранные заказы клиента
                if (ClientOrders != null && window.SelectedClientOrders != null)
                {
                    var selectedIds = window.SelectedClientOrders.Select(c => c.ClientOrderID).ToHashSet();
                    foreach (var order in ClientOrders)
                    {
                        bool newSelected = selectedIds.Contains(order.ClientOrderID);
                        if (order.IsSelected != newSelected)
                        {
                            order.IsSelected = newSelected;
                            needUpdate = true;
                        }
                    }
                }

                // Обновляем изделие
                var newSelectedItem = window.SelectedItem;
                if ((newSelectedItem == null && SelectedItem != null) ||
                    (newSelectedItem != null && (SelectedItem == null || SelectedItem.ItemID != newSelectedItem.ItemID)))
                {
                    SelectedItem = newSelectedItem;
                    needUpdate = true;
                }

                // Обновляем отображение только если были изменения
                if (needUpdate)
                {
                    UpdateSelectedFiltersDisplay();
                    UpdatePlan();
                }
            }
        }

        private void OpenSearchButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSearchWindow();
        }

        #region Прокрутка
        private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            DateHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void DateHeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void ResetScrollPositions()
        {
            GanttScrollViewer?.ScrollToHome();
            DateHeaderScrollViewer?.ScrollToHome();
            LeftScrollViewer?.ScrollToTop();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}