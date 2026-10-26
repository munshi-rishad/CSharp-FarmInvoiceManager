using System;
using BanglaProject.Helpers;
using BanglaProject.WindowsForm;

namespace BanglaProject
{
    // Employee dashboard: personal information, customers, add sales and sales report.
    public partial class FormEmployee : BaseForm
    {
        private readonly FormLogin loginForm;
        private readonly string userId;

        public FormEmployee()
        {
            InitializeComponent();
        }

        public FormEmployee(string fullName, string userId, FormLogin loginForm) : this()
        {
            this.userId = userId;
            this.loginForm = loginForm;
            lblEmployeeName.Text = "Welcome, " + fullName;
        }

        private void FormEmployee_Load(object sender, EventArgs e)
        {
            // the welcome page is shown first
            pnlShowCustomer.Visible = false;
            pnlMain.Visible = false;
            pnlMainAbout.Visible = true;
        }

        private void btnInformation_Click(object sender, EventArgs e)
        {
            Highlight(btnInformation, btnShowCustomer);
            pnlShowCustomer.Visible = false;
            pnlMainAbout.Visible = false;
            pnlMain.Visible = true;
            ShowPersonalInfo(userId);
        }

        private void btnShowCustomer_Click(object sender, EventArgs e)
        {
            Highlight(btnShowCustomer, btnInformation);
            pnlShowCustomer.Visible = true;
            pnlMainAbout.Visible = false;
            pnlMain.Visible = false;
            LoadCustomers(dgvCustomer, lblSerchFound, txtCustomer.Text.Trim());
        }

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {
            LoadCustomers(dgvCustomer, lblSerchFound, txtCustomer.Text.Trim());
        }

        private void btnAddSell_Click(object sender, EventArgs e)
        {
            GoTo(new FormAddSales(userId, this));
        }

        private void btnSellReport_Click(object sender, EventArgs e)
        {
            GoTo(new FromSalesReport(this));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            GoTo(new FormReset(userId, this));
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            GoTo(loginForm);
        }
    }
}
