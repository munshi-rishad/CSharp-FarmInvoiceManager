using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.WindowsForm;

namespace BanglaProject
{
    // Admin dashboard: manage users, see customers, personal information
    // and open products, sales and the sales report.
    public partial class FormAdminView : BaseForm
    {
        private readonly FormLogin loginForm;
        private readonly string adminUserId;
        private readonly string fullName;

        public FormAdminView()
        {
            InitializeComponent();
            lblsearch.Text = "";

            // the user list is the default page
            pnlCustomer.Visible = false;
            pnlInformation.Visible = false;
            pnlShowCustomer.Visible = false;

            dgvUserInfo.CellClick += dgvUserInfo_CellClick;
            dgvUserInfo.SelectionChanged += dgvUserInfo_SelectionChanged;
        }

        public FormAdminView(string fullName, string adminUserId, FormLogin loginForm) : this()
        {
            this.fullName = fullName;
            this.adminUserId = adminUserId;
            this.loginForm = loginForm;

            lblAdminName.Text = "Welcome, " + fullName;
            LoadUsers();
        }

        private void FormAdminView_Load(object sender, EventArgs e)
        {
            foreach (DataGridView grid in new[] { dgvUserInfo, dgvCustomer })
            {
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.MultiSelect = false;
                grid.ClearSelection();
                grid.CurrentCell = null;
            }
            HideLabel(lblSerchFound);
        }

        // Also called by the Add User and Edit User forms after saving
        public void LoadUsers()
        {
            LoadGrid(dgvUserInfo, "SELECT * FROM UserInfo ORDER BY USER_ID");
        }

        private string SelectedUserId()
        {
            if (dgvUserInfo.SelectedRows.Count != 1) return null;
            return dgvUserInfo.SelectedRows[0].Cells["USER_ID"].Value.ToString();
        }

        // ---------- left menu ----------

        private void btnManageUser_Click(object sender, EventArgs e)
        {
            Highlight(btnManageUser, btnInformation, btnShowCustomer);
            ShowPage(pnlGrid);
            pnlButtonHide.Visible = true;
        }

        private void btnInformation_Click(object sender, EventArgs e)
        {
            Highlight(btnInformation, btnManageUser, btnShowCustomer);
            ShowPage(pnlInformation);
            ShowPersonalInfo(adminUserId);
        }

        private void btnShowCustomer_Click(object sender, EventArgs e)
        {
            Highlight(btnShowCustomer, btnManageUser, btnInformation);
            ShowPage(pnlShowCustomer);
            pnlCustomer.Visible = true;
            txtCustomer.Clear();
            LoadCustomers(dgvCustomer, lblSerchFound);
        }

        // Shows one of the three pages (users, information, customers) and hides the others
        private void ShowPage(Panel page)
        {
            pnlGrid.Visible = page == pnlGrid;
            pnlInformation.Visible = page == pnlInformation;
            pnlShowCustomer.Visible = page == pnlShowCustomer;
            pnlCustomer.Visible = false;
            pnlButtonHide.Visible = false;
        }

        private void btnManageProduct_Click(object sender, EventArgs e)
        {
            GoTo(new FormManageProducts(fullName, adminUserId, this));
        }

        private void btnAddSell_Click(object sender, EventArgs e)
        {
            GoTo(new FormAddSales(adminUserId, this));
        }

        private void btnSellReport_Click(object sender, EventArgs e)
        {
            GoTo(new FromSalesReport(this));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            GoTo(new FormReset(adminUserId, this));
        }

        private void btnLogOut_Click_1(object sender, EventArgs e)
        {
            GoTo(loginForm);
        }

        // ---------- users ----------

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadUsers();
        }

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();

            if (search == "")
            {
                LoadUsers();
                HideLabel(lblsearch);
                return;
            }

            DataTable users = LoadGrid(dgvUserInfo,
                "SELECT * FROM UserInfo WHERE FULL_NAME LIKE @Search OR USERNAME LIKE @Search OR USER_ID LIKE @Search ORDER BY USER_ID",
                Db.P("@Search", "%" + search + "%"));

            if (users != null)
                ShowFound(lblsearch, users.Rows.Count > 0);
        }

        private void dgvUserInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                dgvUserInfo.Rows[e.RowIndex].Selected = true;
        }

        private void dgvUserInfo_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUserInfo.SelectedRows.Count == 1)
            {
                DataGridViewRow row = dgvUserInfo.SelectedRows[0];
                ShowSelected(lblsearch, row.Cells["USER_ID"].Value?.ToString(), row.Cells["FULL_NAME"].Value?.ToString());
            }
            else
            {
                HideLabel(lblsearch);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            GoTo(new FormAddUser(this));
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string userId = SelectedUserId();
            if (userId == null)
            {
                Msg.Warning("Please select a user first.", "No User Selected");
                return;
            }
            GoTo(new FormEditUser(userId, this));
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string userId = SelectedUserId();
            if (userId == null)
            {
                Msg.Warning("Please select a row first.");
                return;
            }

            if (userId == adminUserId)
            {
                Msg.Warning("You cannot delete your own account while you are logged in.", "Delete Not Allowed");
                return;
            }

            if (!Msg.Confirm("Are you sure you want to delete this user?", "Confirm Delete")) return;

            try
            {
                Db.Execute("DELETE FROM UserInfo WHERE USER_ID = @ID", Db.P("@ID", userId));
                Msg.Info("User deleted successfully!", "Success");
                LoadUsers();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // 547 = foreign key error: products or invoices still point to this user
                Msg.Warning("This user cannot be deleted because products or invoices are linked to this user.", "Delete Not Allowed");
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        // ---------- customers ----------

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {
            LoadCustomers(dgvCustomer, lblSerchFound, txtCustomer.Text.Trim());
        }

        private void btnCustomerRefresh_Click(object sender, EventArgs e)
        {
            txtCustomer.Clear();
            LoadCustomers(dgvCustomer, lblSerchFound);
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvCustomer.Rows[e.RowIndex];
            row.Selected = true;
            ShowSelected(lblSerchFound, row.Cells["CUSTOMER_ID"].Value?.ToString(), row.Cells["CUSTOMER_NAME"].Value?.ToString());
        }

        private void btnCustomerDekete_Click(object sender, EventArgs e)
        {
            if (dgvCustomer.SelectedRows.Count != 1)
            {
                Msg.Warning("Please select a customer first.", "No Selection");
                return;
            }

            string customerId = dgvCustomer.SelectedRows[0].Cells["CUSTOMER_ID"].Value.ToString();
            if (!Msg.Confirm("Are you sure you want to delete this customer?", "Confirm Delete")) return;

            try
            {
                Db.Execute("DELETE FROM CustomerInfo WHERE CUSTOMER_ID = @ID", Db.P("@ID", customerId));
                Msg.Info("Customer deleted successfully!", "Success");
                txtCustomer.Clear();
                LoadCustomers(dgvCustomer, lblSerchFound);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }
    }
}
