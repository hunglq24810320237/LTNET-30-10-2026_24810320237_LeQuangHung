using HotelManagement.BUS;
using System.Data;

namespace HotelManagement.GUI;
public sealed class Form1:Form
{
    private readonly HotelBus bus=new();
    private readonly DataGridView roomsGrid=new(),staysGrid=new(),roomCrudGrid=new();
    private readonly TextBox txtName=new(),txtCCCD=new(),txtPhone=new(),txtRoomCode=new();
    private readonly ComboBox cboRoom=new(),cboService=new(),cboType=new(),cboStatus=new(),cboRoomType=new();
    private readonly NumericUpDown nudQty=new(),nudRoomRate=new();
    private DataTable? roomTable;
    public Form1()
    {
        Text="Hotel Management | Lễ tân - Phòng - Dịch vụ";StartPosition=FormStartPosition.CenterScreen;Size=new Size(1180,760);MinimumSize=new Size(1000,650);Font=new Font("Segoe UI",10);
        var tabs=new TabControl{Dock=DockStyle.Fill};tabs.TabPages.Add(BuildReceptionTab());tabs.TabPages.Add(BuildRoomCrudTab());tabs.TabPages.Add(BuildRoomSearchTab());Controls.Add(tabs);Shown+=(_,_)=>LoadData();
    }
    private TabPage BuildReceptionTab()
    {
        var page=new TabPage("Lễ tân / Check-in / Check-out");
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=2,Padding=new Padding(12)};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,48));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,52));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,250));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));page.Controls.Add(layout);
        var check=new GroupBox{Text="Nhận phòng - tra cứu khách bằng CCCD",Dock=DockStyle.Fill,Padding=new Padding(12)};layout.Controls.Add(check,0,0);
        AddLabeled(check,"Số phòng",cboRoom,14,30,300);cboRoom.DropDownStyle=ComboBoxStyle.DropDownList;
        AddLabeled(check,"CCCD",txtCCCD,14,72,300);AddLabeled(check,"Họ và tên",txtName,14,114,300);AddLabeled(check,"Điện thoại",txtPhone,14,156,300);
        var inBtn=new Button{Text="Check-in",Location=new Point(100,200),Size=new Size(120,35),BackColor=Color.FromArgb(0,120,215),ForeColor=Color.White};inBtn.Click+=(_,_)=>CheckIn();check.Controls.Add(inBtn);
        var serviceBox=new GroupBox{Text="Gọi dịch vụ / Thanh toán",Dock=DockStyle.Fill,Padding=new Padding(12)};layout.Controls.Add(serviceBox,1,0);
        AddLabeled(serviceBox,"Dịch vụ",cboService,14,30,360);cboService.DropDownStyle=ComboBoxStyle.DropDownList;
        var qtyLabel=new Label{Text="Số lượng",Location=new Point(14,78),Size=new Size(100,28)};serviceBox.Controls.Add(qtyLabel);nudQty.Location=new Point(114,78);nudQty.Width=90;nudQty.Minimum=1;nudQty.Maximum=100;serviceBox.Controls.Add(nudQty);
        var serviceBtn=new Button{Text="Gọi dịch vụ",Location=new Point(14,120),Size=new Size(130,36)};serviceBtn.Click+=(_,_)=>AddService();serviceBox.Controls.Add(serviceBtn);
        var outBtn=new Button{Text="Check-out & xuất hóa đơn",Location=new Point(160,120),Size=new Size(210,36),BackColor=Color.SeaGreen,ForeColor=Color.White};outBtn.Click+=(_,_)=>CheckOut();serviceBox.Controls.Add(outBtn);
        staysGrid.Dock=DockStyle.Fill;staysGrid.ReadOnly=true;staysGrid.AllowUserToAddRows=false;staysGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;staysGrid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;layout.Controls.Add(new GroupBox{Text="Khách đang lưu trú",Dock=DockStyle.Fill},0,1);((GroupBox)layout.GetControlFromPosition(0,1)!).Controls.Add(staysGrid);
        roomsGrid.Dock=DockStyle.Fill;roomsGrid.ReadOnly=true;roomsGrid.AllowUserToAddRows=false;roomsGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;roomsGrid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;roomsGrid.SelectionChanged+=(_,_)=>SetSelectedRoom();layout.Controls.Add(new GroupBox{Text="Tình trạng phòng",Dock=DockStyle.Fill},1,1);((GroupBox)layout.GetControlFromPosition(1,1)!).Controls.Add(roomsGrid);
        return page;
    }
    private TabPage BuildRoomSearchTab()
    {
        var page=new TabPage("Lọc phòng (DataView)");var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=70,Padding=new Padding(12)};
        bar.Controls.Add(new Label{Text="Loại phòng",AutoSize=true,Padding=new Padding(0,8,0,0)});cboType.DropDownStyle=ComboBoxStyle.DropDownList;cboType.Items.AddRange(["Tất cả","Đơn","Đôi","VIP"]);cboType.SelectedIndex=0;cboType.Width=140;cboType.SelectedIndexChanged+=(_,_)=>FilterRooms();bar.Controls.Add(cboType);
        bar.Controls.Add(new Label{Text="Trạng thái",AutoSize=true,Padding=new Padding(10,8,0,0)});cboStatus.DropDownStyle=ComboBoxStyle.DropDownList;cboStatus.Items.AddRange(["Tất cả","Trống","Đang ở","Đang dọn dẹp"]);cboStatus.SelectedIndex=0;cboStatus.Width=160;cboStatus.SelectedIndexChanged+=(_,_)=>FilterRooms();bar.Controls.Add(cboStatus);
        page.Controls.Add(bar);var grid=new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};grid.DataBindingComplete+=(_,_)=>{if(grid.Columns.Contains("GiaTheoGio")){grid.Columns["GiaTheoGio"].HeaderText="Giá theo giờ";grid.Columns["GiaTheoGio"].DefaultCellStyle.Format="N0";}};page.Controls.Add(grid);
        cboType.Tag=grid;return page;
    }
    private TabPage BuildRoomCrudTab()
    {
        var page=new TabPage("Quản lý phòng / CRUD");var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=105,Padding=new Padding(10),WrapContents=true};
        bar.Controls.Add(new Label{Text="Số phòng",AutoSize=true,Padding=new Padding(0,7,0,0)});txtRoomCode.Width=100;bar.Controls.Add(txtRoomCode);
        bar.Controls.Add(new Label{Text="Loại phòng",AutoSize=true,Padding=new Padding(8,7,0,0)});cboRoomType.DropDownStyle=ComboBoxStyle.DropDownList;cboRoomType.Items.AddRange(["Đơn","Đôi","VIP"]);cboRoomType.SelectedIndex=0;cboRoomType.Width=120;bar.Controls.Add(cboRoomType);
        bar.Controls.Add(new Label{Text="Giá/giờ",AutoSize=true,Padding=new Padding(8,7,0,0)});nudRoomRate.Maximum=100000000;nudRoomRate.ThousandsSeparator=true;nudRoomRate.DecimalPlaces=0;nudRoomRate.Width=140;bar.Controls.Add(nudRoomRate);
        var save=new Button{Text="Thêm / Cập nhật",Width=135,Height=32};save.Click+=(_,_)=>SaveRoom();bar.Controls.Add(save);var del=new Button{Text="Xóa phòng",Width=100,Height=32};del.Click+=(_,_)=>DeleteRoom();bar.Controls.Add(del);var clear=new Button{Text="Làm mới",Width=90,Height=32};clear.Click+=(_,_)=>ClearRoom();bar.Controls.Add(clear);
        page.Controls.Add(bar);roomCrudGrid.Dock=DockStyle.Fill;roomCrudGrid.ReadOnly=true;roomCrudGrid.AllowUserToAddRows=false;roomCrudGrid.MultiSelect=false;roomCrudGrid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;roomCrudGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;roomCrudGrid.BackgroundColor=Color.White;roomCrudGrid.SelectionChanged+=(_,_)=>RoomSelected();page.Controls.Add(roomCrudGrid);return page;
    }
    private static void AddLabeled(Control parent,string caption,Control input,int x,int y,int width){parent.Controls.Add(new Label{Text=caption,Location=new Point(x,y+3),Size=new Size(100,27)});input.Location=new Point(x+100,y);input.Size=new Size(width-100,30);parent.Controls.Add(input);}
    private void LoadData()
    {
        try{roomTable=bus.Rooms();roomsGrid.DataSource=roomTable;roomCrudGrid.DataSource=roomTable.Copy();if(roomCrudGrid.Columns.Contains("GiaTheoGio")){roomCrudGrid.Columns["GiaTheoGio"].HeaderText="Giá/giờ";roomCrudGrid.Columns["GiaTheoGio"].DefaultCellStyle.Format="N0";}if(roomCrudGrid.Columns.Contains("SoPhong"))roomCrudGrid.Columns["SoPhong"].HeaderText="Số phòng";if(roomCrudGrid.Columns.Contains("LoaiPhong"))roomCrudGrid.Columns["LoaiPhong"].HeaderText="Loại phòng";if(roomCrudGrid.Columns.Contains("TrangThai"))roomCrudGrid.Columns["TrangThai"].HeaderText="Trạng thái";staysGrid.DataSource=bus.ActiveStays();cboRoom.DataSource=roomTable.Copy();cboRoom.DisplayMember="SoPhong";cboRoom.ValueMember="SoPhong";cboService.DataSource=bus.Services();cboService.DisplayMember="TenDV";cboService.ValueMember="MaDV";FormatRooms();FilterRooms();}
        catch(Exception ex){MessageBox.Show("Hãy chạy Database/HotelDB.sql trên localhost\\SQLEXPRESS.\n\n"+ex.Message,"Kết nối SQL Server",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
    private void FormatRooms(){if(roomsGrid.Columns.Contains("GiaTheoGio")){roomsGrid.Columns["GiaTheoGio"].HeaderText="Giá/giờ";roomsGrid.Columns["GiaTheoGio"].DefaultCellStyle.Format="N0";}}
    private void FilterRooms()
    {
        if(roomTable is null)return;var view=new DataView(roomTable);var filters=new List<string>();if(cboType.SelectedIndex>0)filters.Add($"LoaiPhong='{cboType.Text.Replace("'","''")}'");if(cboStatus.SelectedIndex>0)filters.Add($"TrangThai='{cboStatus.Text.Replace("'","''")}'");view.RowFilter=string.Join(" AND ",filters);if(cboType.Tag is DataGridView grid){grid.DataSource=view;}
    }
    private void SetSelectedRoom(){if(roomsGrid.CurrentRow?.DataBoundItem is not DataRowView row)return;var room=Convert.ToString(row["SoPhong"]);if(room is not null&&cboRoom.Items.Count>0)cboRoom.SelectedValue=room;}
    private void RoomSelected(){if(roomCrudGrid.CurrentRow?.DataBoundItem is not DataRowView row)return;txtRoomCode.Text=Convert.ToString(row["SoPhong"])??"";cboRoomType.SelectedItem=Convert.ToString(row["LoaiPhong"]);nudRoomRate.Value=Math.Clamp(Convert.ToDecimal(row["GiaTheoGio"]),nudRoomRate.Minimum,nudRoomRate.Maximum);}
    private void SaveRoom(){try{bus.SaveRoom(txtRoomCode.Text,Convert.ToString(cboRoomType.SelectedItem)??"",nudRoomRate.Value);LoadData();MessageBox.Show("Lưu phòng thành công.");}catch(Exception ex){MessageBox.Show(ex.Message,"Không thể lưu phòng",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void DeleteRoom(){if(string.IsNullOrWhiteSpace(txtRoomCode.Text))return;if(MessageBox.Show("Xóa phòng đang chọn?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;try{bus.DeleteRoom(txtRoomCode.Text);ClearRoom();LoadData();}catch(Exception ex){MessageBox.Show("Phòng đã có lịch sử lưu trú nên không thể xóa.\n"+ex.Message,"Không thể xóa phòng",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void ClearRoom(){txtRoomCode.Clear();nudRoomRate.Value=0;roomCrudGrid.ClearSelection();}
    private string CurrentRoom()=>Convert.ToString(cboRoom.SelectedValue)??"";
    private void CheckIn(){try{bus.CheckIn(CurrentRoom(),txtName.Text,txtCCCD.Text,txtPhone.Text);MessageBox.Show("Check-in thành công.");txtName.Clear();txtCCCD.Clear();txtPhone.Clear();LoadData();}catch(Exception ex){MessageBox.Show(ex.Message,"Check-in thất bại",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void AddService(){try{if(cboService.SelectedValue is not int id)throw new InvalidOperationException("Chọn dịch vụ.");bus.AddService(CurrentRoom(),id,(int)nudQty.Value);MessageBox.Show("Đã ghi nhận dịch vụ cho phòng.");}catch(Exception ex){MessageBox.Show(ex.Message,"Gọi dịch vụ thất bại",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private void CheckOut(){try{var total=bus.CheckOut(CurrentRoom());MessageBox.Show($"Check-out thành công. Tổng thanh toán: {total:N0} đ","Hóa đơn",MessageBoxButtons.OK,MessageBoxIcon.Information);LoadData();}catch(Exception ex){MessageBox.Show(ex.Message,"Check-out thất bại",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}
