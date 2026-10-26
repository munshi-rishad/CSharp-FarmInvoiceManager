using System;
using System.Data;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject
{
    // Admin changes the selected product
    public partial class FormUpdateProduct : BaseForm
    {
        private readonly FormManageProducts manageProducts;
        private readonly string productId;

        public FormUpdateProduct()
        {
            InitializeComponent();
        }

        public FormUpdateProduct(string productId, FormManageProducts manageProducts) : this()
        {
            this.productId = productId;
            this.manageProducts = manageProducts;
        }

        private void FormUpdateProduct_Load(object sender, EventArgs e)
        {
            ResetErrors();
            LoadProduct();
        }

        private void ResetErrors()
        {
            ValidationLabel.Reset(lblNameError, lblQuantityError, lblCategoryError, lblPriceError, lblDateError);
        }

        private void LoadProduct()
        {
            try
            {
                DataTable table = Db.GetTable("SELECT * FROM ProductInfo WHERE PRODUCT_ID = @ID", Db.P("@ID", productId));
                if (table.Rows.Count == 0)
                {
                    Msg.Error("Product not found!");
                    return;
                }

                DataRow product = table.Rows[0];
                txtProductName.Text = product["PRODUCT_NAME"].ToString();
                txtQunatity.Text = product["QUANTITY"].ToString();
                txtCategory.Text = product["CATEGORY_NAME"].ToString();
                txtPrice.Text = Convert.ToInt32(product["UNITPRICE"]).ToString();
                dtpDate.Value = Convert.ToDateTime(product["CREATED_DATE"]);
                chkIsActive.Checked = Convert.ToBoolean(product["IS_AVAILABLE"]);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        // live validation while typing
        private void txtProductName_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateName(txtProductName, lblNameError);
        private void txtQunatity_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateQuantity(txtQunatity, lblQuantityError);
        private void txtCategory_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateCategory(txtCategory, lblCategoryError);
        private void txtPrice_TextChanged(object sender, EventArgs e) => ProductValidator.ValidatePrice(txtPrice, lblPriceError);
        private void dtpDate_ValueChanged(object sender, EventArgs e) => ProductValidator.ValidateDate(dtpDate, lblDateError);

        private bool AllFieldsValid()
        {
            bool ok = true;
            ok &= ProductValidator.ValidateName(txtProductName, lblNameError);
            ok &= ProductValidator.ValidateQuantity(txtQunatity, lblQuantityError);
            ok &= ProductValidator.ValidateCategory(txtCategory, lblCategoryError);
            ok &= ProductValidator.ValidatePrice(txtPrice, lblPriceError);
            ok &= ProductValidator.ValidateDate(dtpDate, lblDateError);
            return ok;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!AllFieldsValid())
            {
                Msg.Warning("Please fix the highlighted errors before updating.", "Validation Error");
                return;
            }

            try
            {
                int result = Db.Execute(
                    "UPDATE ProductInfo SET PRODUCT_NAME = @Name, QUANTITY = @Qty, CATEGORY_NAME = @Category, " +
                    "UNITPRICE = @Price, CREATED_DATE = @Date, IS_AVAILABLE = @Available WHERE PRODUCT_ID = @ID",
                    Db.P("@Name", txtProductName.Text.Trim()),
                    Db.P("@Qty", int.Parse(txtQunatity.Text.Trim())),
                    Db.P("@Category", txtCategory.Text.Trim()),
                    Db.P("@Price", int.Parse(txtPrice.Text.Trim())),
                    Db.P("@Date", dtpDate.Value.Date),
                    Db.P("@Available", chkIsActive.Checked),
                    Db.P("@ID", productId));

                if (result > 0)
                {
                    Msg.Info("Product updated successfully!", "Success");
                    manageProducts.LoadProducts();
                    GoTo(manageProducts);
                }
                else
                {
                    Msg.Error("Update failed!");
                }
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtProductName.Clear();
            txtQunatity.Clear();
            txtCategory.Clear();
            txtPrice.Clear();
            dtpDate.Value = DateTime.Today;
            chkIsActive.Checked = false;
            ResetErrors();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            GoTo(manageProducts);
        }
    }
}
