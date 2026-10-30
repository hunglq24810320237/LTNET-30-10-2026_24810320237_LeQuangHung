using CafePOS.BUS;
using System.Data;

namespace CafePOS.GUI;

public sealed class Form1 : Form
{
    private readonly CafeBus bus=new();
    private readonly FlowLayoutPanel tablePanel=new();
    private readonly DataGridView orderGrid=new();
    private readonly ComboBox cboCategory=new(),cboItem=new(),cboMenuCategory=new();
    private readonly NumericUpDown nudQty=new(),nudPrice=new();
    private readonly TextBox txtMenuName=new();
    private readonly DataGridView menuGrid=new();
    private int selectedTable,selectedMenu;

    public Form1()
    {
        Text="POS CAFE | Quản lý bàn & bán hàng"; StartPosition=FormStartPosition.CenterScreen; Size=new Size(1180,760); MinimumSize=new Size(1000,650); Font=new Font("Segoe UI",10);
        var tabs=new TabControl{Dock=DockStyle.Fill}; tabs.TabPages.Add(BuildPosTab()); tabs.TabPages.Add(BuildMenuTab()); Controls.Add(tabs);
        Shown+=(_,_)=>ReloadAll();
    }

    private TabPage BuildPosTab()
    {
        var page=new TabPage("Sơ đồ bàn / Gọi món");
        var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=400}; page.Controls.Add(split);
        split.Panel1.Controls.Add(new Label{Text="SƠ ĐỒ BÀN",Font=new Font(Font,FontStyle.Bold),Dock=DockStyle.Top,Height=35,TextAlign=ContentAlignment.MiddleLeft});
        tablePanel.Dock=DockStyle.Fill; tablePanel.Padding=new Padding(6); tablePanel.AutoScroll=true; split.Panel1.Controls.Add(tablePanel);
        var right=split.Panel2;
        var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=100,Padding=new Padding(8),WrapContents=true};
        bar.Controls.Add(new Label{Text="Danh mục",AutoSize=true,Padding=new Padding(0,7,0,0)}); cboCategory.Width=150; cboCategory.DropDownStyle=ComboBoxStyle.DropDownList; cboCategory.SelectedIndexChanged+=(_,_)=>LoadItems(); bar.Controls.Add(cboCategory);
        bar.Controls.Add(new Label{Text="Món",AutoSize=true,Padding=new Padding(6,7,0,0)}); cboItem.Width=190; cboItem.DropDownStyle=ComboBoxStyle.DropDownList; bar.Controls.Add(cboItem);
        bar.Controls.Add(new Label{Text="SL",AutoSize=true,Padding=new Padding(4,7,0,0)}); nudQty.Width=65; nudQty.Minimum=1; nudQty.Maximum=100; nudQty.Value=1; bar.Controls.Add(nudQty);
        var add=new Button{Text="Thêm món",Width=110,Height=32}; add.Click+=(_,_)=>AddItem(); bar.Controls.Add(add);
        right.Controls.Add(bar);
        orderGrid.Dock=DockStyle.Fill; orderGrid.ReadOnly=true; orderGrid.AllowUserToAddRows=false; orderGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; orderGrid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; orderGrid.BackgroundColor=Color.White; right.Controls.Add(orderGrid);
        var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=60,Padding=new Padding(8)};
        var pay=new Button{Text="Thanh toán bàn",Width=150,Height=38,BackColor=Color.SeaGreen,ForeColor=Color.White}; pay.Click+=(_,_)=>Pay(); footer.Controls.Add(pay);
        var refresh=new Button{Text="Tải lại",Width=100,Height=38}; refresh.Click+=(_,_)=>ReloadAll(); footer.Controls.Add(refresh); right.Controls.Add(footer);
        return page;
    }

    private TabPage BuildMenuTab()
    {
        var page=new TabPage("Quản lý thực đơn");
        var form=new FlowLayoutPanel{Dock=DockStyle.Top,Height=100,Padding=new Padding(8),WrapContents=true};
        form.Controls.Add(new Label{Text="Tên món",AutoSize=true,Padding=new Padding(0,7,0,0)}); txtMenuName.Width=190; form.Controls.Add(txtMenuName);
        form.Controls.Add(new Label{Text="Danh mục",AutoSize=true,Padding=new Padding(5,7,0,0)}); cboMenuCategory.Width=170; cboMenuCategory.DropDownStyle=ComboBoxStyle.DropDownList; form.Controls.Add(cboMenuCategory);
        form.Controls.Add(new Label{Text="Giá bán",AutoSize=true,Padding=new Padding(5,7,0,0)}); nudPrice.Width=130; nudPrice.Maximum=100000000; nudPrice.ThousandsSeparator=true; nudPrice.DecimalPlaces=0; form.Controls.Add(nudPrice);
        var save=new Button{Text="Thêm / Lưu sửa",Width=130,Height=34}; save.Click+=(_,_)=>SaveMenu(); form.Controls.Add(save);
        var delete=new Button{Text="Xóa món",Width=90,Height=34}; delete.Click+=(_,_)=>DeleteMenu(); form.Controls.Add(delete);
        var clear=new Button{Text="Làm mới",Width=90,Height=34}; clear.Click+=(_,_)=>ClearMenuForm(); form.Controls.Add(clear);
        page.Controls.Add(form); menuGrid.Dock=DockStyle.Fill; menuGrid.ReadOnly=true; menuGrid.AllowUserToAddRows=false; menuGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; menuGrid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; menuGrid.SelectionChanged+=(_,_)=>MenuSelected(); page.Controls.Add(menuGrid); return page;
    }

    private void ReloadAll()
    {
        try
        {
            var cats=bus.Categories(); cboCategory.DataSource=cats.Copy(); cboCategory.DisplayMember="TenDanhMuc"; cboCategory.ValueMember="MaDanhMuc";
            cboMenuCategory.DataSource=cats.Copy(); cboMenuCategory.DisplayMember="TenDanhMuc"; cboMenuCategory.ValueMember="MaDanhMuc";
            LoadItems(); ReloadTables(); ReloadMenu();
        } catch(Exception ex){MessageBox.Show("Kết nối CafePOSDB thất bại. Hãy chạy Database/CafePOSDB.sql trên localhost\\SQLEXPRESS.\n\n"+ex.Message,"Kết nối SQL Server",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
    private void ReloadTables()
    {
        tablePanel.Controls.Clear(); var dt=bus.Tables();
        foreach(DataRow row in dt.Rows)
        {
            int id=(int)row["MaBan"]; string status=(string)row["TrangThai"];
            var b=new Button{Text=$"{row["TenBan"]}\n{status}",Tag=id,Width=145,Height=86,Margin=new Padding(7),BackColor=status=="Trống"?Color.FromArgb(62,170,112):Color.FromArgb(218,79,79),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
            b.Click+=(_,_)=>SelectTable(id,b); tablePanel.Controls.Add(b);
        }
    }
    private void SelectTable(int id,Button b){selectedTable=id;foreach(Control c in tablePanel.Controls)if(c is Button button)button.FlatAppearance.BorderSize=0;b.FlatAppearance.BorderSize=3;b.FlatAppearance.BorderColor=Color.Gold;LoadOrder();}
    private void LoadItems(){if(cboCategory.SelectedValue is not int id)return;cboItem.DataSource=bus.MenuByCategory(id);cboItem.DisplayMember="TenMon";cboItem.ValueMember="MaMon";}
    private void LoadOrder(){if(selectedTable==0)return;orderGrid.DataSource=bus.CurrentOrder(selectedTable);}
    private void AddItem()
    {
        if(selectedTable==0||cboItem.SelectedValue is not int item){MessageBox.Show("Hãy chọn bàn và món trước.");return;}
        try{bus.AddOrder(selectedTable,item,(int)nudQty.Value);LoadOrder();ReloadTables();}catch(Exception ex){MessageBox.Show(ex.Message,"Không thể gọi món",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
    private void Pay()
    {
        if(selectedTable==0){MessageBox.Show("Hãy chọn bàn cần thanh toán.");return;}
        try{var total=bus.Settle(selectedTable);MessageBox.Show($"Thanh toán thành công.\nTổng tiền: {total:N0} đ","Hóa đơn",MessageBoxButtons.OK,MessageBoxIcon.Information);LoadOrder();ReloadTables();}
        catch(Exception ex){MessageBox.Show(ex.Message,"Thanh toán thất bại",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
    private void ReloadMenu()
    {
        menuGrid.DataSource=bus.Menu();
        SetMenuColumn("MaMon","Mã món");SetMenuColumn("TenMon","Tên món");SetMenuColumn("TenDanhMuc","Danh mục");
        SetMenuColumn("DonGia","Đơn giá","N0");
    }
    private void SetMenuColumn(string name,string header,string? format=null)
    {
        var column=menuGrid.Columns[name];if(column is null)return;column.HeaderText=header;if(format is not null)column.DefaultCellStyle.Format=format;
    }
    private void MenuSelected(){if(menuGrid.CurrentRow?.DataBoundItem is not DataRowView r)return;selectedMenu=Convert.ToInt32(r["MaMon"]);txtMenuName.Text=Convert.ToString(r["TenMon"])??"";cboMenuCategory.SelectedValue=Convert.ToInt32(r["MaDanhMuc"]);nudPrice.Value=Math.Clamp(Convert.ToDecimal(r["DonGia"]),nudPrice.Minimum,nudPrice.Maximum);}
    private void SaveMenu(){try{if(cboMenuCategory.SelectedValue is not int cat)throw new InvalidOperationException("Chọn danh mục món.");bus.SaveMenu(selectedMenu,txtMenuName.Text,cat,nudPrice.Value);ClearMenuForm();ReloadMenu();LoadItems();MessageBox.Show("Lưu thực đơn thành công.");}catch(Exception ex){MessageBox.Show(ex.Message,"Dữ liệu không hợp lệ",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void DeleteMenu(){if(selectedMenu==0){MessageBox.Show("Chọn món cần xóa.");return;}if(MessageBox.Show("Xóa món đang chọn?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;try{bus.DeleteMenu(selectedMenu);ClearMenuForm();ReloadMenu();LoadItems();}catch(Exception ex){MessageBox.Show("Không thể xóa món đã có trong hóa đơn.\n"+ex.Message);}}
    private void ClearMenuForm(){selectedMenu=0;txtMenuName.Clear();nudPrice.Value=0;menuGrid.ClearSelection();}
}
