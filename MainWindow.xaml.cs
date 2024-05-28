using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DbContextOptions<TradeCompletedContext> _options;
        private ObservableCollection<Product> _products;
        private ListCollectionView _productsView;
        private ObservableCollection<Product> _filteredProducts;
        private bool _sortBy;
        private int _totalRecordsCount;
        public int TotalRecordsCount
        {
            get { return _totalRecordsCount; }
            set
            {
                _totalRecordsCount = value;
                OnPropertyChanged(nameof(TotalRecordsCount));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _options = new DbContextOptionsBuilder<TradeCompletedContext>()
                .UseSqlServer(@"Data Source = DESKTOP-DRC69FM\SERVERKM; Initial Catalog = Trade_completed; Integrated Security = true; TrustServerCertificate = true")
                .Options;
            LoadData();
            FilteredProducts = _products;
            _productsView = (ListCollectionView)CollectionViewSource.GetDefaultView(FilteredProducts);

           
        }

        private void LoadData()
        {
            using (var context = new TradeCompletedContext(_options))
            {
                _products = new ObservableCollection<Product>(context.Products.ToList());
                TotalRecordsCount = _products.Count;
                var manufacturers = context.Products.Select(p => p.Manufacturer).Distinct().ToList();
                manufacturers.Insert(0, "Все производители");
                ManufacturerComboBox.ItemsSource = manufacturers;
            }

            DataContext = this;

        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void ManufacturerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public ObservableCollection<Product> FilteredProducts
        {
            get { return _filteredProducts; }
            set
            {
                _filteredProducts = value;
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        private void FilterProducts()
        {
            var searchText = SearchTextBox.Text;
            var selectedManufacturer = ManufacturerComboBox.SelectedItem as string;


            if (string.IsNullOrEmpty(searchText) && (selectedManufacturer == "Все производители" || selectedManufacturer == null))
            {
                // No filtering required
                FilteredProducts = new ObservableCollection<Product>(_products);

            }
            else
            {
                // Apply filters
                FilteredProducts = new ObservableCollection<Product>(_products.Where(p =>
                    (string.IsNullOrEmpty(searchText) || p.Name.Contains(searchText) || p.Manufacturer.Contains(searchText)) &&
                    (selectedManufacturer == "Все производители" || p.Manufacturer == selectedManufacturer)
                ));
            }
            
            _productsView = (ListCollectionView)CollectionViewSource.GetDefaultView(FilteredProducts); 
            OnPropertyChanged(nameof(FilteredProducts));
            SortByCost(_sortBy);
        }

        private void SortByCost(bool ascending)
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Cost", ascending ? ListSortDirection.Ascending : ListSortDirection.Descending));
        }

        private void AscendingSortButton_Click(object sender, RoutedEventArgs e)
        {
            _sortBy = true;
            SortByCost(_sortBy);
        }

        private void DescendingSortButton_Click(object sender, RoutedEventArgs e)
        {
            _sortBy = false;
            SortByCost(_sortBy);
        }
    }
}

