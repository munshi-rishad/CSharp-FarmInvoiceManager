using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;

namespace BanglaProject
{
    // Product list for the admin: search, add, update and delete products
    public partial class FormManageProducts : BaseForm
    {
        private readonly FormAdminView adminView;
        private readonly string adminUserId;

        public FormManageProducts()
        {
            InitializeComponent();
        }

        public FormManageProducts(string fullName, string adminUserId, FormAdminView adminView) : this()
        {
            this.adminUserId = adminUserId;
            this.adminView = adminView;
            lblAdminName.Text = "Welcome, " + fullName;
            LoadProducts();
        }

        private void FormManageProducts_Load(object sender, EventArgs e)
        {
            lblsearch.Visible = false;
            dgvProductInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductInfo.MultiSelect = false;
            dgvProductInfo.CellClick += dgvProductInfo_CellClick;
        }

        // Also called by the Add Product and Update Product forms after saving
        public void LoadProducts()
        {
            LoadGrid(dgvProductInfo, "SELECT * FROM ProductInfo ORDER BY PRODUCT_ID");
        }

        private string SelectedProductId()
        {
            if (dgvProductInfo.SelectedRows.Count != 1) return null;
            return dgvProductInfo.SelectedRows[0].Cells["PRODUCT_ID"].Value.ToString();
        }

        private void ShowSelectedProduct()
        {
            if (dgvProductInfo.SelectedRows.Count != 1) return;

            DataGridViewRow row = dgvProductInfo.SelectedRows[0];
            ShowSelected(lblsearch, row.Cells["PRODUCT_ID"].Value?.ToString(), row.Cells["PRODUCT_NAME"].Value?.ToString());
        }

        private void dgvProductInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            dgvProductInfo.Rows[e.RowIndex].Selected = true;
            ShowSelectedProduct();
        }

        private void dgvProductInfo_DoubleClick(object sender, EventArgs e)
        {
            ShowSelectedProduct();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();

            if (search == "")
            {
                LoadProducts();
                HideLabel(lblsearch);
                return;
            }

            DataTable products = LoadGrid(dgvProductInfo,
                "SELECT * FROM ProductInfo WHERE PRODUCT_NAME LIKE @Search OR PRODUCT_ID LIKE @Search ORDER BY PRODUCT_ID",
                Db.P("@Search", "%" + search + "%"));

            if (products != null)
                ShowFound(lblsearch, products.Rows.Count > 0);
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadProducts();
            HideLabel(lblsearch);
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            GoTo(new FormAddProduct(adminUserId, this));
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string productId = SelectedProductId();
            if (productId == null)
            {
                Msg.Warning("Please select a product first.", "No Product Selected");
                return;
            }
            GoTo(new FormUpdateProduct(productId, this));
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string productId = SelectedProductId();
            if (productId == null)
            {
                Msg.Warning("Please select a row first.");
                return;
            }

            if (!Msg.Confirm("Are you sure you want to delete this product?", "Confirm Delete")) return;

            try
            {
                Db.Execute("DELETE FROM ProductInfo WHERE PRODUCT_ID = @ID", Db.P("@ID", productId));
                Msg.Info("Product deleted successfully!", "Success");
                LoadProducts();
                HideLabel(lblsearch);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // 547 = foreign key error: the product is already used in an invoice
                Msg.Warning("This product cannot be deleted because it is used in saved invoices.\n" +
                            "You can untick \"Active\" in Update Product instead.", "Delete Not Allowed");
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoTo(adminView);
        }
    }
}
