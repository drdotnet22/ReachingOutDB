using Microsoft.JSInterop;

namespace ReachingOutDB.Data
{
    public class YearQuarterService
    {
        private int _year = 0;
        private Quarter _quarter = Quarter.Q1;

        public event Action? OnYearQuarterChanged;

        public YearQuarterService() { }

        public int SelectedYear
        {
            get => _year;
            set => _year = value;
        }

        public Quarter SelectedQuarter
        {
            get => _quarter;
            set => _quarter = value;
        }

        public async Task SelectYearAsync(int year)
        {
            SelectedYear = year;
            OnYearQuarterChanged?.Invoke();
        }

        public async Task SelectQuarterAsync(Quarter quarter)
        {
            SelectedQuarter = quarter;
            OnYearQuarterChanged?.Invoke();
        }

        public async Task CalculateYearQuarterAsync()
        {
            int year = DateTime.UtcNow.Year;
            int monthInt = DateTime.UtcNow.Month;
            Quarter quarter;

            if (monthInt >= 2 && monthInt <= 4)
            {
                quarter = Quarter.Q2;
            }
            else if (monthInt >= 5 && monthInt <= 7)
            {
                quarter = Quarter.Q3;
            }
            else if (monthInt >= 8 && monthInt <= 10)
            {
                quarter = Quarter.Q4;
            }
            else if (monthInt >= 11)
            {
                quarter = Quarter.Q1;
                year++;
            }
            else
            {
                quarter = Quarter.Q1;
            }

            SelectedYear = year;
            SelectedQuarter = quarter;
            OnYearQuarterChanged?.Invoke();
        }
    }
}