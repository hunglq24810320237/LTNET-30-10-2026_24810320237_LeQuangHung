using InventoryManagement.BUS;
using InventoryManagement.DTO;
using System.Data;

namespace InventoryManagement.GUI;
public sealed class Form1:Form
{
    private readonly InventoryBus bus=new();
    private readonly DataGridView materialGrid=new(),supplierGrid=new(),cartGrid=new(),voucherGrid=new(),reportGrid=new();
    private readonly TextBox txtSearch=new(),txtMaVT=new(),txtTenVT=new(),txtDVT=new(),txtNccName=new(),txtNccPhone=new(),txtNccAddress=new();
    private readonly ComboBox cboCategory=new(),cboSupplier=new(),cboType=new(),cboMaterial=new();
    private readonly NumericUpDown nudQty=new(),nudPrice=new(),nudThreshold=new();
    private readonly Label lblStockValue=new();
    private int supplierId;
    private readonly DataTable cart=new();
    public Form1()
    {
        Text="Inventory Management | Quản lý vật tư & phiếu kho";StartPosition=FormStartPosition.CenterScreen;Size=new Size(1250,790);MinimumSize=new Size(1080,680);Font=new Font("Segoe UI",10);
        cart.Columns.Add("MaVatTu",typeof(string));cart.Columns.Add("TenVatTu",typeof(string));cart.Columns.Add("SoLuong",typeof(decimal));cart.Columns.Add("DonGia",typeof(decimal));cart.Columns.Add("ThanhTien",typeof(decimal));
        cart.PrimaryKey=[cart.Columns["MaVatTu"]!];
        var tabs=new TabControl{Dock=DockStyle.Fill};tabs.TabPages.Add(BuildMaterialsTab());tabs.TabPages.Add(BuildSuppliersTab());tabs.TabPages.Add(BuildVoucherTab());tabs.TabPages.Add(BuildReportTab());Controls.Add(tabs);Shown+=(_,_)=>LoadAll();
    }
    private TabPage BuildMaterialsTab()
    {
        var page=new TabPage("Vật tư / CRUD");var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=145,Padding=new Padding(8),WrapContents=true};
        Add(top,"Tìm mã/tên",txtSearch,150);var search=new Button{Text="Tìm kiếm",Width=90};search.Click+=(_,_)=>LoadMaterials();top.Controls.Add(search);
        Add(top,"Mã vật tư",txtMaVT,140);Add(top,"Tên vật tư",txtTenVT,180);Add(top,"Danh mục",cboCategory,160);cboCategory.DropDownStyle=ComboBoxStyle.DropDownList;Add(top,"ĐVT",txtDVT,95);
        var save=new Button{Text="Thêm / Cập nhật",Width=135};save.Click+=(_,_)=>SaveMaterial();top.Controls.Add(save);var del=new Button{Text="Xóa",Width=70};del.Click+=(_,_)=>DeleteMaterial();top.Controls.Add(del);var clear=new Button{Text="Làm mới",Width=85};clear.Click+=(_,_)=>ClearMaterial();top.Controls.Add(clear);
        page.Controls.Add(top);ConfigureGrid(materialGrid);materialGrid.SelectionChanged+=(_,_)=>MaterialSelected();page.Controls.Add(materialGrid);return page;
    }
    private TabPage BuildSuppliersTab()
    {
        var page=new TabPage("Nhà cung cấp / CRUD");var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=100,Padding=new Padding(8),WrapContents=true};Add(top,"Tên nhà cung cấp",txtNccName,220);Add(top,"Điện thoại",txtNccPhone,150);Add(top,"Địa chỉ",txtNccAddress,240);
        var save=new Button{Text="Thêm / Cập nhật",Width=135};save.Click+=(_,_)=>SaveSupplier();top.Controls.Add(save);var del=new Button{Text="Xóa NCC",Width=90};del.Click+=(_,_)=>DeleteSupplier();top.Controls.Add(del);var clear=new Button{Text="Làm mới",Width=90};clear.Click+=(_,_)=>ClearSupplier();top.Controls.Add(clear);
        page.Controls.Add(top);ConfigureGrid(supplierGrid);supplierGrid.SelectionChanged+=(_,_)=>SupplierSelected();page.Controls.Add(supplierGrid);return page;
    }
    private TabPage BuildVoucherTab()
    {
        var page=new TabPage("Lập phiếu nhập / xuất");var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=112,Padding=new Padding(8),WrapContents=true};
        top.Controls.Add(new Label{Text="Loại phiếu",AutoSize=true,Padding=new Padding(0,8,0,0)});cboType.DropDownStyle=ComboBoxStyle.DropDownList;cboType.Items.AddRange(["NHAP","XUAT"]);cboType.SelectedIndex=0;cboType.Width=100;cboType.SelectedIndexChanged+=(_,_)=>UpdateSupplierEnabled();top.Controls.Add(cboType);
        top.Controls.Add(new Label{Text="Nhà cung cấp",AutoSize=true,Padding=new Padding(8,8,0,0)});cboSupplier.DropDownStyle=ComboBoxStyle.DropDownList;cboSupplier.Width=190;top.Controls.Add(cboSupplier);
        top.Controls.Add(new Label{Text="Vật tư",AutoSize=true,Padding=new Padding(8,8,0,0)});cboMaterial.DropDownStyle=ComboBoxStyle.DropDownList;cboMaterial.Width=240;top.Controls.Add(cboMaterial);
        top.Controls.Add(new Label{Text="SL",AutoSize=true,Padding=new Padding(8,8,0,0)});nudQty.DecimalPlaces=2;nudQty.Minimum=0.01M;nudQty.Maximum=100000000;nudQty.Value=1;nudQty.Width=100;top.Controls.Add(nudQty);
        top.Controls.Add(new Label{Text="Đơn giá",AutoSize=true,Padding=new Padding(8,8,0,0)});nudPrice.DecimalPlaces=2;nudPrice.Maximum=1000000000;nudPrice.Width=130;top.Controls.Add(nudPrice);
        var add=new Button{Text="Thêm dòng",Width=100};add.Click+=(_,_)=>AddCartLine();top.Controls.Add(add);
        page.Controls.Add(top);ConfigureGrid(cartGrid);cartGrid.DataSource=cart;page.Controls.Add(cartGrid);
        var bottom=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=60,Padding=new Padding(8)};var remove=new Button{Text="Xóa dòng chọn",Width=130};remove.Click+=(_,_)=>RemoveCartLine();bottom.Controls.Add(remove);var save=new Button{Text="Lưu phiếu kho (Transaction)",Width=220,BackColor=Color.SeaGreen,ForeColor=Color.White};save.Click+=(_,_)=>SaveVoucher();bottom.Controls.Add(save);page.Controls.Add(bottom);return page;
    }
    private TabPage BuildReportTab()
    {
        var page=new TabPage("Báo cáo / Cảnh báo tồn kho");var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=70,Padding=new Padding(12)};top.Controls.Add(new Label{Text="Ngưỡng an toàn",AutoSize=true,Padding=new Padding(0,8,0,0)});nudThreshold.DecimalPlaces=2;nudThreshold.Maximum=1000000;nudThreshold.Value=10;nudThreshold.Width=120;top.Controls.Add(nudThreshold);var load=new Button{Text="Lọc tồn thấp",Width=120};load.Click+=(_,_)=>LoadReport();top.Controls.Add(load);lblStockValue.AutoSize=true;lblStockValue.Padding=new Padding(15,7,0,0);top.Controls.Add(lblStockValue);var refresh=new Button{Text="Tải phiếu kho",Width=110};refresh.Click+=(_,_)=>LoadVouchers();top.Controls.Add(refresh);page.Controls.Add(top);
        var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=260};ConfigureGrid(reportGrid);ConfigureGrid(voucherGrid);split.Panel1.Controls.Add(reportGrid);split.Panel2.Controls.Add(voucherGrid);page.Controls.Add(split);return page;
    }
    private static void Add(FlowLayoutPanel p,string caption,Control control,int width){p.Controls.Add(new Label{Text=caption,AutoSize=true,Padding=new Padding(0,7,0,0)});control.Width=width;p.Controls.Add(control);}
    private static void ConfigureGrid(DataGridView g){g.Dock=DockStyle.Fill;g.ReadOnly=true;g.AllowUserToAddRows=false;g.MultiSelect=false;g.SelectionMode=DataGridViewSelectionMode.FullRowSelect;g.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;g.BackgroundColor=Color.White;}
    private void LoadAll(){try{cboCategory.DataSource=bus.Categories();cboCategory.DisplayMember="TenDanhMuc";cboCategory.ValueMember="MaDanhMuc";cboSupplier.DataSource=bus.Suppliers().Copy();cboSupplier.DisplayMember="TenNCC";cboSupplier.ValueMember="MaNCC";cboMaterial.DataSource=bus.Materials().Copy();cboMaterial.DisplayMember="TenVatTu";cboMaterial.ValueMember="MaVatTu";LoadMaterials();LoadSuppliers();LoadReport();LoadVouchers();UpdateSupplierEnabled();}catch(Exception ex){MessageBox.Show("Hãy chạy Database/InventoryDB.sql trên localhost\\SQLEXPRESS.\n\n"+ex.Message,"Kết nối SQL Server",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
    private void LoadMaterials(){materialGrid.DataSource=bus.Materials(txtSearch.Text);SetHeader(materialGrid,"MaVatTu","Mã vật tư");SetHeader(materialGrid,"TenVatTu","Tên vật tư");SetHeader(materialGrid,"TenDanhMuc","Danh mục");SetHeader(materialGrid,"DVT","ĐVT");SetHeader(materialGrid,"SoLuongTon","Tồn kho");}
    private void LoadSuppliers(){supplierGrid.DataSource=bus.Suppliers();SetHeader(supplierGrid,"MaNCC","Mã NCC");SetHeader(supplierGrid,"TenNCC","Tên nhà cung cấp");SetHeader(supplierGrid,"DienThoai","Điện thoại");SetHeader(supplierGrid,"DiaChi","Địa chỉ");}
    private void LoadReport(){reportGrid.DataSource=bus.LowStock(nudThreshold.Value);lblStockValue.Text=$"Tổng giá trị tồn kho: {bus.StockValue():N0} đ";LoadVouchers();}
    private void LoadVouchers(){voucherGrid.DataSource=bus.Vouchers();SetHeader(voucherGrid,"MaPhieu","Mã phiếu");SetHeader(voucherGrid,"LoaiPhieu","Loại");SetHeader(voucherGrid,"NgayLap","Ngày lập");SetHeader(voucherGrid,"TenNCC","Nhà cung cấp");SetHeader(voucherGrid,"TongTien","Tổng tiền");}
    private static void SetHeader(DataGridView g,string key,string title){if(g.Columns.Contains(key))g.Columns[key].HeaderText=title;}
    private void MaterialSelected(){if(materialGrid.CurrentRow?.DataBoundItem is not DataRowView r)return;txtMaVT.Text=Convert.ToString(r["MaVatTu"])??"";txtTenVT.Text=Convert.ToString(r["TenVatTu"])??"";txtDVT.Text=Convert.ToString(r["DVT"])??"";if(r["MaDanhMuc"]!=DBNull.Value)cboCategory.SelectedValue=Convert.ToInt32(r["MaDanhMuc"]);}
    private void SaveMaterial(){try{int? cat=cboCategory.SelectedValue is int id?id:null;bus.SaveMaterial(txtMaVT.Text,txtTenVT.Text,cat,txtDVT.Text);LoadAll();MessageBox.Show("Lưu vật tư thành công.");}catch(Exception ex){MessageBox.Show(ex.Message,"Không thể lưu vật tư",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void DeleteMaterial(){if(string.IsNullOrWhiteSpace(txtMaVT.Text))return;if(MessageBox.Show("Xóa vật tư đang chọn?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;try{bus.DeleteMaterial(txtMaVT.Text);ClearMaterial();LoadAll();}catch(Exception ex){MessageBox.Show("Vật tư đã được sử dụng trong phiếu kho nên không thể xóa.\n"+ex.Message);}}
    private void ClearMaterial(){txtMaVT.Clear();txtTenVT.Clear();txtDVT.Clear();}
    private void SupplierSelected(){if(supplierGrid.CurrentRow?.DataBoundItem is not DataRowView r)return;supplierId=Convert.ToInt32(r["MaNCC"]);txtNccName.Text=Convert.ToString(r["TenNCC"])??"";txtNccPhone.Text=Convert.ToString(r["DienThoai"])??"";txtNccAddress.Text=Convert.ToString(r["DiaChi"])??"";}
    private void SaveSupplier(){try{bus.SaveSupplier(supplierId,txtNccName.Text,txtNccPhone.Text,txtNccAddress.Text);supplierId=0;LoadAll();MessageBox.Show("Lưu nhà cung cấp thành công.");}catch(Exception ex){MessageBox.Show(ex.Message,"Không thể lưu nhà cung cấp",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void DeleteSupplier(){if(supplierId==0)return;if(MessageBox.Show("Xóa nhà cung cấp đang chọn?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;try{bus.DeleteSupplier(supplierId);ClearSupplier();LoadAll();}catch(Exception ex){MessageBox.Show("Nhà cung cấp đã gắn với phiếu nhập nên không thể xóa.\n"+ex.Message);}}
    private void ClearSupplier(){supplierId=0;txtNccName.Clear();txtNccPhone.Clear();txtNccAddress.Clear();}
    private void UpdateSupplierEnabled(){cboSupplier.Enabled=cboType.SelectedItem?.ToString()=="NHAP";}
    private void AddCartLine()
    {
        if(cboMaterial.SelectedValue is not string item||cboMaterial.SelectedItem is not DataRowView row)return;decimal qty=nudQty.Value,price=nudPrice.Value;string name=Convert.ToString(row["TenVatTu"])??item;
        var existing=cart.Rows.Find(item);if(existing is null){existing=cart.NewRow();existing["MaVatTu"]=item;existing["TenVatTu"]=name;existing["SoLuong"]=qty;existing["DonGia"]=price;existing["ThanhTien"]=qty*price;cart.Rows.Add(existing);}else{existing["SoLuong"]=(decimal)existing["SoLuong"]+qty;existing["DonGia"]=price;existing["ThanhTien"]=(decimal)existing["SoLuong"]*price;}
    }
    private void RemoveCartLine(){if(cartGrid.CurrentRow?.DataBoundItem is DataRowView row)cart.Rows.Remove(row.Row);}
    private void SaveVoucher()
    {
        try{var lines=cart.AsEnumerable().Select(r=>new StockLineDto(r.Field<string>("MaVatTu")!,r.Field<decimal>("SoLuong"),r.Field<decimal>("DonGia"))).ToList();int? supplier=cboSupplier.SelectedValue is int id?id:null;var idVoucher=bus.SaveVoucher(cboType.SelectedItem?.ToString()??"",supplier,lines);cart.Clear();LoadAll();MessageBox.Show($"Lưu phiếu kho #{idVoucher} thành công. Tồn kho đã cập nhật trong cùng transaction.","Phiếu kho",MessageBoxButtons.OK,MessageBoxIcon.Information);}catch(Exception ex){MessageBox.Show(ex.Message,"Phiếu kho đã rollback",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
}
