using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.DataTransferObject
{
    public class IncomeExpenseDTO : INotifyPropertyChanged
    {
        public int TransactionId { get; set; }
        public double Amount { get; set; }
        public string CurrencySymbol { get; set; }
        public string AmountText => string.Format(new CultureInfo(CurrencySymbol), "{0:C0}", Amount);
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
        public double TaxAmountCut { get; set; }
        public string OwnerName { get; set; }
        public string CategoryName { get; set; }
        public string Remarks { get; set; }
        public string Mode { get; set; }
        public string FileName { get; set; }
        public Color ModeColor => !string.IsNullOrEmpty(Mode) ? (Mode.ToLower() == "file_upload" ? Colors.Red : null) : null;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
