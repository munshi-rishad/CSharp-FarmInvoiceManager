using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BanglaProject.Data;
using BanglaProject.Helpers;

namespace BanglaProject.WindowsForm
{
    // Sales report: all invoices with totals and a chart.
    // Invoices can be filtered by date and by customer mobile number.
    // Click an invoice to see its items, double-click it to open the printable invoice.
    public partial class FromSalesReport : BaseForm
    {
        private readonly Form dashboard;   // admin or employee dashboard to go back to
        private DateTime? fromDate;        // null = no date filter
        private DateTime? toDate;

        public FromSalesReport()
        {
            InitializeComponent();
            dgvInvoices.SelectionChanged += dgvInvoices_SelectionChanged;
            dgvInvoices.CellClick += dgvInvoices_CellClick;
            dgvInvoices.CellDoubleClick += dgvInvoices_CellDoubleClick;
        }

        public FromSalesReport(Form dashboard) : this()
        {
            this.dashboard = dashboard;
        }

        private void FromSalesReport_Load(object sender, EventArgs e)
        {
            HideLabel(lblSearch);
            RefreshReport();
        }

        // Builds the WHERE part from the date filter and the mobile search box
        private string Filter(List<SqlParameter> parameters)
        {
            string where = " WHERE 1 = 1";

            if (fromDate != null && toDate != null)
            {
                where += " AND INVOICE_DATE BETWEEN @From AND @To";
                parameters.Add(Db.P("@From", fromDate.Value));
                parameters.Add(Db.P("@To", toDate.Value));
            }

            string mobile = txtSearchCustomer.Text.Trim();
            if (mobile != "")
            {
                where += " AND CUSTOMER_MOBILE LIKE @Mobile";
                parameters.Add(Db.P("@Mobile", "%" + mobile + "%"));
            }
            return where;
        }

        // Reloads the invoice list, the three totals and the chart with the current filter
        private void RefreshReport()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            DataTable invoices = LoadGrid(dgvInvoices,
                "SELECT * FROM InvoiceDetails" + Filter(parameters) + " ORDER BY INVOICE_DATE, INVOICE_ID",
                parameters.ToArray());

            dgvInvoiceDetails.DataSource = null;
            if (invoices == null) return;

            ShowTotals(invoices);
            LoadSalesChart(invoices);
        }

        private void ShowTotals(DataTable invoices)
        {
            decimal totalSales = 0;
            foreach (DataRow row in invoices.Rows)
                totalSales += Convert.ToDecimal(row["FINAL_AMOUNT"]);

            int count = invoices.Rows.Count;
            decimal average = count == 0 ? 0 : totalSales / count;

            lblTIAmount.Text = count.ToString();
            lblTSAmount.Text = "Tk " + totalSales.ToString("N2");
            lblASAmount.Text = "Tk " + average.ToString("N2");
        }

        // Column chart: total sales of each day
        private void LoadSalesChart(DataTable invoices)
        {
            SortedDictionary<DateTime, decimal> salesPerDay = new SortedDictionary<DateTime, decimal>();
            foreach (DataRow row in invoices.Rows)
            {
                DateTime day = Convert.ToDateTime(row["INVOICE_DATE"]).Date;
                if (!salesPerDay.ContainsKey(day)) salesPerDay[day] = 0;
                salesPerDay[day] += Convert.ToDecimal(row["FINAL_AMOUNT"]);
            }

            chartSales.Series.Clear();
            chartSales.ChartAreas.Clear();
            chartSales.Titles.Clear();

            ChartArea area = chartSales.ChartAreas.Add("Main");
            Font smallBold = new Font("Segoe UI", 8, FontStyle.Bold);
            area.AxisX.LabelStyle.Font = smallBold;
            area.AxisY.LabelStyle.Font = smallBold;
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;

            Series sales = chartSales.Series.Add("Sales");
            sales.ChartType = SeriesChartType.Column;
            sales.IsValueShownAsLabel = true;
            sales.Font = smallBold;

            foreach (KeyValuePair<DateTime, decimal> day in salesPerDay)
                sales.Points.AddXY(day.Key.ToString("dd-MMM"), (double)day.Value);

            foreach (Legend legend in chartSales.Legends)
            {
                legend.Font = smallBold;
                legend.IsTextAutoFit = false;
            }

            chartSales.Titles.Add("Sales Overview").Font = new Font("Segoe UI", 13, FontStyle.Bold);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                Msg.Warning("From Date cannot be after To Date.", "Invalid Date");
                return;
            }

            fromDate = dtpFromDate.Value.Date;
            toDate = dtpToDate.Value.Date;
            RefreshReport();
        }

        private void txtSearchCustomer_TextChanged(object sender, EventArgs e)
        {
            RefreshReport();

            if (txtSearchCustomer.Text.Trim() == "")
                HideLabel(lblSearch);
            else
                ShowFound(lblSearch, dgvInvoices.Rows.Count > 0);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            fromDate = null;
            toDate = null;
            txtSearchCustomer.Text = "";   // this also reloads the report
            RefreshReport();
            HideLabel(lblSearch);
        }

        // ---------- selected invoice ----------

        private string SelectedInvoiceId()
        {
            if (dgvInvoices.SelectedRows.Count != 1) return null;
            return dgvInvoices.SelectedRows[0].Cells["INVOICE_ID"].Value?.ToString();
        }

        private void dgvInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                dgvInvoices.Rows[e.RowIndex].Selected = true;
        }

        // Shows the items of the selected invoice in the lower grid
        private void dgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            string invoiceId = SelectedInvoiceId();
            if (invoiceId == null)
            {
                HideLabel(lblSearch);
                return;
            }

            ShowSelected(lblSearch, invoiceId, dgvInvoices.SelectedRows[0].Cells["CUSTOMER_NAME"].Value?.ToString());
            LoadGrid(dgvInvoiceDetails,
                "SELECT PRODUCT_ID, PRODUCT_NAME, QUANTITY, UNIT_PRICE, SUB_TOTAL FROM InvoiceItems WHERE INVOICE_ID = @ID",
                Db.P("@ID", invoiceId));
        }

        private void dgvInvoices_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string invoiceId = SelectedInvoiceId();
            if (e.RowIndex >= 0 && invoiceId != null)
                GoTo(new FormInvoicePrint(invoiceId, this));
        }

        private void btnDeleteInvoice_Click(object sender, EventArgs e)
        {
            string invoiceId = SelectedInvoiceId();
            if (invoiceId == null)
            {
                Msg.Info("Please select 1 row to delete.");
                return;
            }

            if (!Msg.Confirm("Are you sure you want to delete Invoice ID: " + invoiceId + "?", "Confirm Delete")) return;

            try
            {
                // items first, then the invoice itself - both or nothing
                Db.ExecuteInTransaction(new List<SqlCommand>
                {
                    Db.Command("DELETE FROM InvoiceItems WHERE INVOICE_ID = @ID", Db.P("@ID", invoiceId)),
                    Db.Command("DELETE FROM InvoiceDetails WHERE INVOICE_ID = @ID", Db.P("@ID", invoiceId))
                });

                Msg.Info("Invoice deleted successfully.", "Success");
                RefreshReport();
                HideLabel(lblSearch);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoTo(dashboard);
        }
    }
}
