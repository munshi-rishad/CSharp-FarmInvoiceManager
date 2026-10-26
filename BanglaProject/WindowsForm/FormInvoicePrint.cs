using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;

namespace BanglaProject.WindowsForm
{
    // Shows a saved invoice as a receipt. "Print" sends it to a printer (or "Microsoft Print to PDF").
    public partial class FormInvoicePrint : BaseForm
    {
        private readonly string invoiceId;
        private readonly Form previousForm;

        public FormInvoicePrint()
        {
            InitializeComponent();
            btnPrint.Click += btnPrint_Click;
        }

        public FormInvoicePrint(string invoiceId, Form previousForm) : this()
        {
            this.invoiceId = invoiceId;
            this.previousForm = previousForm;
        }

        private void FormInvoicePrint_Load(object sender, EventArgs e)
        {
            txtInvoice.Font = new Font("Consolas", 10);
            txtInvoice.ScrollBars = ScrollBars.Vertical;

            try
            {
                txtInvoice.Text = BuildInvoiceText();
            }
            catch (Exception ex)
            {
                Msg.Error("An error has occurred while generating the invoice.\n" + ex.Message);
            }
        }

        // Builds the receipt text: header, one line per item and the totals
        private string BuildInvoiceText()
        {
            DataTable details = Db.GetTable(
                "SELECT CUSTOMER_NAME, CUSTOMER_MOBILE, TOTAL_AMOUNT, DISCOUNT_AMOUNT, FINAL_AMOUNT, INVOICE_DATE " +
                "FROM InvoiceDetails WHERE INVOICE_ID = @ID", Db.P("@ID", invoiceId));

            if (details.Rows.Count == 0) return "Invoice " + invoiceId + " was not found.";
            DataRow invoice = details.Rows[0];

            DataTable items = Db.GetTable(
                "SELECT PRODUCT_NAME, QUANTITY, UNIT_PRICE, SUB_TOTAL FROM InvoiceItems WHERE INVOICE_ID = @ID ORDER BY INVOICE_ITEM_ID",
                Db.P("@ID", invoiceId));

            const string line = "------------------------------------------------";
            const string doubleLine = "================================================";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(doubleLine);
            sb.AppendLine("                FARM INVOICE MANAGER            ");
            sb.AppendLine(doubleLine);
            sb.AppendLine("Invoice ID      : " + invoiceId);
            sb.AppendLine("Customer Name   : " + invoice["CUSTOMER_NAME"]);
            sb.AppendLine("Customer Mobile : " + invoice["CUSTOMER_MOBILE"]);
            sb.AppendLine("Date            : " + Convert.ToDateTime(invoice["INVOICE_DATE"]).ToString("dd/MM/yyyy"));
            sb.AppendLine(doubleLine);
            sb.AppendLine("SL  ITEM                 QTY    PRICE     TOTAL");
            sb.AppendLine(line);

            int sl = 1;
            foreach (DataRow item in items.Rows)
            {
                string name = item["PRODUCT_NAME"].ToString();
                if (name.Length > 19) name = name.Substring(0, 19);   // keep the columns straight

                sb.AppendLine(
                    sl.ToString().PadLeft(2) + "  " +
                    name.PadRight(20) +
                    item["QUANTITY"].ToString().PadLeft(5) +
                    Convert.ToDecimal(item["UNIT_PRICE"]).ToString("0.00").PadLeft(9) +
                    Convert.ToDecimal(item["SUB_TOTAL"]).ToString("0.00").PadLeft(10));
                sl++;
            }

            sb.AppendLine(line);
            sb.AppendLine(TotalLine("Total Amount", invoice["TOTAL_AMOUNT"]));
            sb.AppendLine(TotalLine("Discount", invoice["DISCOUNT_AMOUNT"]));
            sb.AppendLine(line);
            sb.AppendLine(TotalLine("Final Amount", invoice["FINAL_AMOUNT"]));
            sb.AppendLine(doubleLine);
            sb.AppendLine("           Thank you for your purchase!         ");
            sb.AppendLine(doubleLine);

            return sb.ToString();
        }

        private static string TotalLine(string title, object amount)
        {
            return title.PadRight(32) + Convert.ToDecimal(amount).ToString("0.00").PadLeft(16);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDocument document = new PrintDocument();
            document.DocumentName = "Invoice " + invoiceId;
            document.PrintPage += (s, args) =>
            {
                using (Font font = new Font("Consolas", 10))
                {
                    args.Graphics.DrawString(txtInvoice.Text, font, Brushes.Black, args.MarginBounds.Left, args.MarginBounds.Top);
                }
            };

            using (PrintDialog dialog = new PrintDialog { Document = document, UseEXDialog = true })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    document.Print();
                }
                catch (Exception ex)
                {
                    Msg.Error("The invoice could not be printed.\n" + ex.Message);
                }
            }
        }

        // the "Close" button goes back to the Add Sales form
        private void button1_Click(object sender, EventArgs e)
        {
            GoTo(previousForm);
        }
    }
}
