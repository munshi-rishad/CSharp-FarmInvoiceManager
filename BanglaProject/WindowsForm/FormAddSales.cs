using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;
using BanglaProject.WindowsForm;

namespace BanglaProject
{
    // Make a new sale: choose the customer, add products to the cart,
    // give a discount and save the invoice. Saving also reduces the product stock.
    public partial class FormAddSales : BaseForm
    {
        private readonly Form dashboard;   // admin or employee dashboard to go back to
        private readonly string userId;
        private bool isFormLoaded = false;
        private bool isPremiumCustomer = false;

        public FormAddSales()
        {
            InitializeComponent();
            lblMobileError.Text = "";
            lblFullNameError.Text = "";
        }

        public FormAddSales(string userId, Form dashboard) : this()
        {
            this.userId = userId;
            this.dashboard = dashboard;
        }

        private void FormAddSales_Load(object sender, EventArgs e)
        {
            dgvProductInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductInfo.MultiSelect = false;

            HideLabel(lblSearch);
            lblPremiumMember.Visible = false;
            lblDateDefault.Text = DateTime.Today.ToShortDateString();

            try
            {
                LoadProducts();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
            isFormLoaded = true;
        }

        // ---------- combo boxes ----------

        // Fills a combo box from a query and leaves nothing selected.
        // isFormLoaded is switched off meanwhile, so the change events do not run.
        private void FillComboBox(ComboBox comboBox, string sql, string display, string value)
        {
            bool wasLoaded = isFormLoaded;
            isFormLoaded = false;

            comboBox.DataSource = Db.GetTable(sql);
            comboBox.DisplayMember = display;
            comboBox.ValueMember = value;
            comboBox.SelectedIndex = -1;

            isFormLoaded = wasLoaded;
        }

        // only products that are available and still in stock can be sold
        private void LoadProducts()
        {
            FillComboBox(cmbProduct,
                "SELECT PRODUCT_ID, PRODUCT_NAME FROM ProductInfo WHERE IS_AVAILABLE = 1 AND QUANTITY > 0 ORDER BY PRODUCT_NAME",
                "PRODUCT_NAME", "PRODUCT_ID");
        }

        private void LoadCustomers()
        {
            FillComboBox(cmbNumber, "SELECT CUSTOMER_ID, CUSTOMER_MOBILE FROM CustomerInfo ORDER BY CUSTOMER_MOBILE",
                         "CUSTOMER_MOBILE", "CUSTOMER_ID");
        }

        // Called by the Add Customer form after a new premium customer is saved
        public void ReloadCustomers(string mobileToSelect)
        {
            LoadCustomers();
            cmbNumber.Text = mobileToSelect;   // fills the name and shows "Premium Member"
        }

        private string SelectedProductId()
        {
            if (cmbProduct.SelectedIndex < 0 || cmbProduct.SelectedValue == null) return null;
            return cmbProduct.SelectedValue.ToString();
        }

        // ---------- customer ----------

        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {
            UserValidator.ValidateFullName(txtCustomerName, lblFullNameError);
        }

        // When the mobile number belongs to a saved customer, the name is filled in
        // and the customer is shown as a premium member.
        private void cmbNumber_TextChanged(object sender, EventArgs e)
        {
            if (!isFormLoaded) return;

            isPremiumCustomer = false;
            txtCustomerName.Text = "";
            lblPremiumMember.Visible = false;

            if (!UserValidator.ValidateMobileComboBox(cmbNumber, lblMobileError)) return;

            try
            {
                DataTable customer = Db.GetTable("SELECT CUSTOMER_NAME FROM CustomerInfo WHERE CUSTOMER_MOBILE = @Mobile",
                                                 Db.P("@Mobile", cmbNumber.Text.Trim()));
                isPremiumCustomer = customer.Rows.Count > 0;

                if (isPremiumCustomer)
                    txtCustomerName.Text = customer.Rows[0]["CUSTOMER_NAME"].ToString();

                lblPremiumMember.Text = isPremiumCustomer ? "Premium Member" : "Non Premium Member";
                lblPremiumMember.ForeColor = isPremiumCustomer ? Color.Green : Color.Red;
                lblPremiumMember.Visible = true;
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            GoTo(new FormAddCustomer(this));
        }

        // ---------- product and cart ----------

        // How many pieces of a product are already in the cart
        private int QuantityInCart(string productId)
        {
            DataGridViewRow row = FindCartRow(productId);
            return row == null ? 0 : Convert.ToInt32(row.Cells["PRODUCT_QUANTITY"].Value);
        }

        private DataGridViewRow FindCartRow(string productId)
        {
            foreach (DataGridViewRow row in dgvProductInfo.Rows)
            {
                if (row.Cells["PRODUCT_ID"].Value?.ToString() == productId)
                    return row;
            }
            return null;
        }

        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            string productId = SelectedProductId();
            if (!isFormLoaded || productId == null) return;

            try
            {
                DataTable product = Db.GetTable("SELECT UNITPRICE, QUANTITY FROM ProductInfo WHERE PRODUCT_ID = @ID", Db.P("@ID", productId));
                if (product.Rows.Count == 0) return;

                // stock that is still free = stock in the database - pieces already in the cart
                int stockLeft = Convert.ToInt32(product.Rows[0]["QUANTITY"]) - QuantityInCart(productId);

                txtUnitPrice.Text = Convert.ToDecimal(product.Rows[0]["UNITPRICE"]).ToString("0.##");
                txtAvailableStocks.Text = stockLeft.ToString();
                txtQuantity_TextChanged(sender, e);   // update the item total
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        // only digits can be typed in the quantity box
        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            txtItemTotal.Text = "";
            if (txtQuantity.Text == "") return;

            if (SelectedProductId() == null)
            {
                Msg.Warning("Please select a product first.", "Product Required");
                txtQuantity.Text = "";
                return;
            }

            int.TryParse(txtQuantity.Text, out int quantity);
            int.TryParse(txtAvailableStocks.Text, out int stock);
            decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice);

            if (quantity > stock)
            {
                Msg.Warning("The entered quantity exceeds the available stock (" + stock + ").\n\nPlease enter a smaller quantity.", "Invalid Quantity");
                txtQuantity.Text = "";
                return;
            }

            txtItemTotal.Text = (quantity * unitPrice).ToString("0.##");
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            string productId = SelectedProductId();
            if (productId == null)
            {
                Msg.Warning("Please select a product before adding an item.", "Product Required");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                Msg.Warning("Please enter the quantity to add.", "Quantity Required");
                return;
            }

            try
            {
                DataTable product = Db.GetTable(
                    "SELECT PRODUCT_ID, PRODUCT_NAME, UNITPRICE, QUANTITY, CREATED_DATE FROM ProductInfo WHERE PRODUCT_ID = @ID",
                    Db.P("@ID", productId));

                if (product.Rows.Count != 1)
                {
                    Msg.Error("The selected product could not be found. Please refresh and try again.", "Product Not Found");
                    return;
                }

                DataRow p = product.Rows[0];
                decimal unitPrice = Convert.ToDecimal(p["UNITPRICE"]);
                DataGridViewRow cartRow = FindCartRow(productId);

                if (cartRow != null)
                {
                    // the product is already in the cart: just add to its quantity
                    int newQuantity = Convert.ToInt32(cartRow.Cells["PRODUCT_QUANTITY"].Value) + quantity;
                    cartRow.Cells["PRODUCT_QUANTITY"].Value = newQuantity;
                    cartRow.Cells["TOTAL"].Value = newQuantity * unitPrice;
                }
                else
                {
                    // columns: ID, name, stock, unit price, created date, quantity sold, total
                    dgvProductInfo.Rows.Add(p["PRODUCT_ID"], p["PRODUCT_NAME"], p["QUANTITY"], unitPrice,
                                            Convert.ToDateTime(p["CREATED_DATE"]).ToShortDateString(),
                                            quantity, quantity * unitPrice);
                }

                CalculateTotals();
                ClearProductInputs();
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
            dgvProductInfo.ClearSelection();
        }

        private void ClearProductInputs()
        {
            cmbProduct.SelectedIndex = -1;
            txtUnitPrice.Text = "";
            txtQuantity.Text = "";
            txtAvailableStocks.Text = "";
            txtItemTotal.Text = "";
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvProductInfo.SelectedRows.Count == 0)
            {
                Msg.Info("Please select an item from the list before removing it.", "No Item Selected");
                return;
            }

            int index = dgvProductInfo.SelectedRows[0].Index;
            dgvProductInfo.Rows.RemoveAt(index);
            CalculateTotals();

            // select the next item so several items can be removed quickly
            dgvProductInfo.ClearSelection();
            if (dgvProductInfo.Rows.Count > 0)
                dgvProductInfo.Rows[Math.Min(index, dgvProductInfo.Rows.Count - 1)].Selected = true;

            UpdateSelectedLabel();
        }

        private void dgvProductInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            dgvProductInfo.ClearSelection();
            dgvProductInfo.Rows[e.RowIndex].Selected = true;
            UpdateSelectedLabel();
        }

        private void UpdateSelectedLabel()
        {
            if (dgvProductInfo.SelectedRows.Count == 1)
            {
                DataGridViewRow row = dgvProductInfo.SelectedRows[0];
                ShowSelected(lblSearch, row.Cells["PRODUCT_ID"].Value?.ToString(), row.Cells["PRODUCT_NAME"].Value?.ToString());
            }
            else
            {
                HideLabel(lblSearch);
            }
        }

        // ---------- totals ----------

        private decimal SubTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvProductInfo.Rows)
                total += Convert.ToDecimal(row.Cells["TOTAL"].Value);
            return total;
        }

        private decimal DiscountPercent()
        {
            decimal.TryParse(txtDiscount.Text, out decimal percent);
            return percent;
        }

        private void CalculateTotals()
        {
            decimal subTotal = SubTotal();
            decimal discountAmount = subTotal * DiscountPercent() / 100;

            txtTotalAmount.Text = subTotal.ToString("0.00");
            txtFinalAmount.Text = (subTotal - discountAmount).ToString("0.00");
        }

        // discount is a percentage between 0 and 100
        private void txtDiscount_TextChanged_1(object sender, EventArgs e)
        {
            if (txtDiscount.Text != "")
            {
                if (!decimal.TryParse(txtDiscount.Text, out decimal discount))
                {
                    txtDiscount.Text = "";
                    return;   // clearing the box runs this method again
                }

                if (discount < 0 || discount > 100)
                {
                    Msg.Warning("Discount must be a value between 0 and 100.", "Invalid Discount");
                    txtDiscount.Text = "";
                    return;
                }
            }
            CalculateTotals();
        }

        // ---------- save ----------

        private void btnSaveInvoice_Click(object sender, EventArgs e)
        {
            if (!UserValidator.ValidateMobileComboBox(cmbNumber, lblMobileError))
            {
                Msg.Warning("Please select or enter a correct customer mobile number.", "Customer Mobile Required");
                return;
            }

            if (!UserValidator.ValidateFullName(txtCustomerName, lblFullNameError))
            {
                Msg.Warning("Please enter the customer's name correctly.", "Customer Name Required");
                return;
            }

            if (dgvProductInfo.Rows.Count == 0)
            {
                Msg.Warning("Please add at least one product item before saving the invoice.", "No Items Added");
                return;
            }

            try
            {
                if (!EnoughStock()) return;

                string invoiceId = SaveInvoice();
                Msg.Info("Invoice saved successfully and stock quantities have been updated.", "Saved Successfully");

                ClearAllFields();
                LoadProducts();   // stock has changed
                GoTo(new FormInvoicePrint(invoiceId, this));
            }
            catch (Exception ex)
            {
                Msg.Error("The invoice could not be saved. Nothing was changed.\n\nDetails: " + ex.Message, "Save Failed");
            }
        }

        // Checks the stock again just before saving (another user may have sold the same product)
        private bool EnoughStock()
        {
            foreach (DataGridViewRow row in dgvProductInfo.Rows)
            {
                string productId = row.Cells["PRODUCT_ID"].Value.ToString();
                int wanted = Convert.ToInt32(row.Cells["PRODUCT_QUANTITY"].Value);
                int stock = Convert.ToInt32(Db.Scalar("SELECT QUANTITY FROM ProductInfo WHERE PRODUCT_ID = @ID", Db.P("@ID", productId)));

                if (wanted > stock)
                {
                    Msg.Warning("Only " + stock + " left of " + row.Cells["PRODUCT_NAME"].Value + ". Please change the quantity.", "Not Enough Stock");
                    return false;
                }
            }
            return true;
        }

        // Saves the invoice, its items and the new stock in one transaction and returns the invoice ID
        private string SaveInvoice()
        {
            string mobile = cmbNumber.Text.Trim();
            string invoiceId = Db.NextId("InvoiceDetails", "INVOICE_ID", "INV-");

            // premium customers keep their PC-xxx ID, other customers get a new RC-xxx ID
            string customerId = isPremiumCustomer
                ? Db.Scalar("SELECT CUSTOMER_ID FROM CustomerInfo WHERE CUSTOMER_MOBILE = @Mobile", Db.P("@Mobile", mobile)).ToString()
                : Db.NextId("InvoiceDetails", "CUSTOMER_ID", "RC-");

            decimal subTotal = SubTotal();
            decimal discountAmount = subTotal * DiscountPercent() / 100;

            List<SqlCommand> commands = new List<SqlCommand>();

            commands.Add(Db.Command(
                "INSERT INTO InvoiceDetails (INVOICE_ID, USER_ID, CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_MOBILE, TOTAL_AMOUNT, DISCOUNT_AMOUNT, FINAL_AMOUNT, INVOICE_DATE) " +
                "VALUES (@Invoice, @User, @Customer, @Name, @Mobile, @Total, @Discount, @Final, @Date)",
                Db.P("@Invoice", invoiceId), Db.P("@User", userId), Db.P("@Customer", customerId),
                Db.P("@Name", txtCustomerName.Text.Trim()), Db.P("@Mobile", mobile),
                Db.P("@Total", subTotal), Db.P("@Discount", discountAmount),
                Db.P("@Final", subTotal - discountAmount), Db.P("@Date", DateTime.Today)));

            foreach (DataGridViewRow row in dgvProductInfo.Rows)
            {
                object productId = row.Cells["PRODUCT_ID"].Value;
                object quantity = row.Cells["PRODUCT_QUANTITY"].Value;

                commands.Add(Db.Command(
                    "INSERT INTO InvoiceItems (INVOICE_ID, PRODUCT_ID, PRODUCT_NAME, QUANTITY, UNIT_PRICE, SUB_TOTAL) " +
                    "VALUES (@Invoice, @Product, @Name, @Qty, @Price, @SubTotal)",
                    Db.P("@Invoice", invoiceId), Db.P("@Product", productId), Db.P("@Name", row.Cells["PRODUCT_NAME"].Value),
                    Db.P("@Qty", quantity), Db.P("@Price", row.Cells["UNITPRICE"].Value), Db.P("@SubTotal", row.Cells["TOTAL"].Value)));

                commands.Add(Db.Command(
                    "UPDATE ProductInfo SET QUANTITY = QUANTITY - @Qty WHERE PRODUCT_ID = @Product",
                    Db.P("@Qty", quantity), Db.P("@Product", productId)));
            }

            Db.ExecuteInTransaction(commands);
            return invoiceId;
        }

        // ---------- clear and back ----------

        private void ClearAllFields()
        {
            isFormLoaded = false;   // so clearing the mobile box does not search again
            cmbNumber.SelectedIndex = -1;
            cmbNumber.Text = "";
            isFormLoaded = true;

            txtCustomerName.Text = "";
            isPremiumCustomer = false;
            lblPremiumMember.Visible = false;
            ValidationLabel.Reset(lblFullNameError, lblMobileError);

            ClearProductInputs();
            dgvProductInfo.Rows.Clear();
            txtDiscount.Text = "";
            txtTotalAmount.Text = "";
            txtFinalAmount.Text = "";
            HideLabel(lblSearch);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearAllFields();
        }

        private void btnBack_Click_1(object sender, EventArgs e)
        {
            GoTo(dashboard);
        }
    }
}
