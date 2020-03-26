using NetworkProcessMonitor.Helpers;
using NetworkProcessMonitor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkProcessMonitor.UI
{
    public partial class DataGridViewWithProcessDataListSource: System.Windows.Forms.DataGridView
    {
        private MainWindowForm ParentWindow;
        private ViewState RememberedViewState;

        private class ViewState
        {
            internal class SelectedRow
            {
                public UInt64 UID { get; set; }
                public Int32 CellIndex { get; set; }
            }
            public SelectedRow Selected { get; set; }
            public Int32 VisibleTopRowIndex { get; set; }
        }

        public DataGridViewWithProcessDataListSource(): base()
        {
        }

        public void SetParentWindow(MainWindowForm parent)
        {
            this.ParentWindow = parent;
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            // e.RowCount - amount of added rows (always one?)
            // e.RowIndex - index of added row
            base.OnRowsAdded(e);
            if (ParentWindow.ShouldHideRow((DataSource as StableSortableBindingList<ProcessData>)[e.RowIndex]))
            {
                UpdateRowVisibility(e.RowIndex, false);
            }
        }

        public void UpdateRowVisibility(int rowIndex, bool visible)
        {
            CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[this.DataSource];
            currencyManager1.SuspendBinding();
            this.Rows[rowIndex].Visible = visible;
            currencyManager1.ResumeBinding();
        }

        public void ScrollToTop()
        {
            for(int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Visible)
                {
                    FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }
        }

        public Int32 GetVisibleRowsCount()
        {
            int invisibleRows = 0;
            foreach (DataGridViewRow row in Rows)
            {
                if (!row.Visible) invisibleRows += 1;
            }
            return Rows.Count - invisibleRows;
        }

        protected override bool SetCurrentCellAddressCore(int columnIndex, int rowIndex, bool setAnchorCellAddress, bool validateCurrentCell, bool throughMouseClick)
        {
            try
            {
                return base.SetCurrentCellAddressCore(columnIndex, rowIndex, setAnchorCellAddress, validateCurrentCell, throughMouseClick);
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($"Got exception InvalidOperationException in protected override bool SetCurrentCellAddressCore: {e.Message}");
                if (!(MainWindowForm.ErrorLogger is null))
                {
                    MainWindowForm.ErrorLogger.LogObject(
                        Utils.GetCallerClassFuncName(), 
                        severity: 8, 
                        additionalInfo: "InvalidOperationException in protected override bool SetCurrentCellAddressCore, returning false", 
                        e);
                }
                return false;
            }
        }

        public override void Sort(DataGridViewColumn dataGridViewColumn, ListSortDirection direction)
        {
            base.Sort(dataGridViewColumn, direction);
            HideAllRowsFailingVisibilityCondition();
        }

        private void HideAllRowsFailingVisibilityCondition()
        {
            CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[this.DataSource];
            currencyManager1.SuspendBinding();
            for (int i = 0; i < Rows.Count; i++)
            {
                if (ParentWindow.ShouldHideRow((DataSource as StableSortableBindingList<ProcessData>)[i]))
                    Rows[i].Visible = false;
            }
            currencyManager1.ResumeBinding();
        }

        public void SaveAndSuspendCurrentView()
        {
            SuspendLayout();
            //Enabled = false;
            RememberedViewState = new ViewState();
            if (CurrentCell != null && Rows[CurrentCell.RowIndex].Cells["UniqueID"] != null)
            {
                RememberedViewState.Selected = new ViewState.SelectedRow()
                {
                    UID = (UInt64)Rows[CurrentCell.RowIndex].Cells["UniqueID"].Value,
                    CellIndex = CurrentCell.ColumnIndex
                };
            }
            RememberedViewState.VisibleTopRowIndex = FirstDisplayedScrollingRowIndex;
        }

        public void RestoreAndResumeCurrentView()
        {
            if (!(RememberedViewState is null))
            {
                RestoreSelectedCell();
                RestoreFirstVisibleRow();

                RememberedViewState = null;
            }
            //Enabled = true;
            ResumeLayout();
        }

        private void RestoreSelectedCell()
        {
            if (!(RememberedViewState.Selected is null))
            {
                ProcessData target = (DataSource as StableSortableBindingList<ProcessData>)
                                                                .FirstOrDefault(procesData => procesData.UniqueID == RememberedViewState.Selected.UID);
                if (!(target is null))
                {
                    int rowIndex = (DataSource as StableSortableBindingList<ProcessData>).IndexOf(target);
                    CurrentCell = Rows[rowIndex].Cells[(int)RememberedViewState.Selected.CellIndex];
                    Rows[rowIndex].Selected = true;
                }
            }
        }

        private void RestoreFirstVisibleRow()
        {
            try
            {
                if (Rows[RememberedViewState.VisibleTopRowIndex].Visible == true && Rows[RememberedViewState.VisibleTopRowIndex].Frozen == false)
                    FirstDisplayedScrollingRowIndex = RememberedViewState.VisibleTopRowIndex;
            }
            catch (Exception) { }
        }
    }
}
