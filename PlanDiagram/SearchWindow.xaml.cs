using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PlanDiagram.Models;

namespace PlanDiagram
{
    public partial class SearchWindow : Window
    {
        public List<ProductionOrder> SelectedProductionOrders => GetSelected<ProductionOrder>(ProductionOrdersList);
        public List<ClientOrder> SelectedClientOrders => GetSelected<ClientOrder>(ClientOrdersList);
        public Item SelectedItem => ItemsList.SelectedItem as Item; // Исправлено для Single режима

        private List<ProductionOrder> _allProductionOrders;
        private List<ClientOrder> _allClientOrders;
        private List<Item> _allItems;

        public SearchWindow(List<ProductionOrder> prodOrders, List<ClientOrder> clientOrders, List<Item> items,
                           List<ProductionOrder> selectedProd = null, List<ClientOrder> selectedClient = null, Item selectedItem = null)
        {
            InitializeComponent();

            _allProductionOrders = prodOrders ?? new List<ProductionOrder>();
            _allClientOrders = clientOrders ?? new List<ClientOrder>();
            _allItems = items ?? new List<Item>();

            // Заполняем списки
            ProductionOrdersList.ItemsSource = _allProductionOrders;
            ClientOrdersList.ItemsSource = _allClientOrders;
            ItemsList.ItemsSource = _allItems;

            // Выделяем предварительно выбранные элементы для множественного выбора
            if (selectedProd != null && selectedProd.Any())
            {
                foreach (var item in selectedProd)
                    ProductionOrdersList.SelectedItems.Add(item);
            }

            // Выделяем предварительно выбранные элементы для множественного выбора
            if (selectedClient != null && selectedClient.Any())
            {
                foreach (var item in selectedClient)
                    ClientOrdersList.SelectedItems.Add(item);
            }

            // Выделяем предварительно выбранный элемент для одиночного выбора
            if (selectedItem != null)
            {
                ItemsList.SelectedItem = selectedItem;
            }

            UpdateSelectedText();
        }

        private List<T> GetSelected<T>(ListBox listBox) where T : class
        {
            return listBox.SelectedItems.Cast<T>().ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                ProductionOrdersList.ItemsSource = _allProductionOrders;
                ClientOrdersList.ItemsSource = _allClientOrders;
                ItemsList.ItemsSource = _allItems;
            }
            else
            {
                ProductionOrdersList.ItemsSource = _allProductionOrders
                    .Where(x => x.DocNumber?.ToLower().Contains(searchText) == true).ToList();
                ClientOrdersList.ItemsSource = _allClientOrders
                    .Where(x => x.DocNumber?.ToLower().Contains(searchText) == true).ToList();
                ItemsList.ItemsSource = _allItems
                    .Where(x => x.ItemFullName?.ToLower().Contains(searchText) == true).ToList();
            }
        }

        private void UpdateSelectedText()
        {
            var parts = new List<string>();

            var selectedProd = SelectedProductionOrders;
            if (selectedProd.Any())
                parts.Add($"{selectedProd.Count} зак. произв.");

            var selectedClient = SelectedClientOrders;
            if (selectedClient.Any())
                parts.Add($"{selectedClient.Count} зак. клиента");

            var selectedItem = SelectedItem;
            if (selectedItem != null)
                parts.Add($"{selectedItem.ItemFullName}");

            SelectedText.Text = parts.Any() ? string.Join(" | ", parts) : "Ничего не выбрано";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionOrdersList.SelectedItems.Clear();
            ClientOrdersList.SelectedItems.Clear();
            ItemsList.SelectedItem = null; // Для одиночного выбора используем SelectedItem
            UpdateSelectedText();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}