using Microsoft.Data.SqlClient;
using System.Data;

namespace CafePOS.DAL;

public sealed class CafeDal
{
    private const string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=CafePOSDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
    private SqlConnection Open() => new(ConnectionString);

    private static DataTable Query(SqlConnection cn, string sql, params SqlParameter[] ps)
    {
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(ps);
        using var adapter = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt;
    }

    public DataTable Tables() { using var cn=Open(); return Query(cn,"SELECT MaBan,TenBan,TrangThai FROM dbo.BanAn ORDER BY MaBan"); }
    public DataTable Categories() { using var cn=Open(); return Query(cn,"SELECT MaDanhMuc,TenDanhMuc FROM dbo.DanhMuc ORDER BY TenDanhMuc"); }
    public DataTable Menu() { using var cn=Open(); return Query(cn,"SELECT m.MaMon,m.TenMon,m.MaDanhMuc,d.TenDanhMuc,m.DonGia FROM dbo.MonAn m JOIN dbo.DanhMuc d ON d.MaDanhMuc=m.MaDanhMuc ORDER BY d.TenDanhMuc,m.TenMon"); }
    public DataTable MenuByCategory(int categoryId) { using var cn=Open(); return Query(cn,"SELECT MaMon,TenMon,DonGia FROM dbo.MonAn WHERE MaDanhMuc=@id ORDER BY TenMon",new SqlParameter("@id",categoryId)); }
    public DataTable CurrentOrder(int tableId) { using var cn=Open(); return Query(cn,"SELECT ct.MaMon,m.TenMon,ct.SoLuong,ct.DonGia,ct.SoLuong*ct.DonGia AS ThanhTien FROM dbo.HoaDon h JOIN dbo.ChiTietHoaDon ct ON ct.MaHoaDon=h.MaHoaDon JOIN dbo.MonAn m ON m.MaMon=ct.MaMon WHERE h.MaBan=@ban AND h.TrangThai=0 ORDER BY m.TenMon",new SqlParameter("@ban",tableId)); }

    public void AddOrder(int tableId,int itemId,int quantity)
    {
        using var cn=Open(); cn.Open(); using var tx=cn.BeginTransaction();
        try
        {
            using var bill=new SqlCommand("SELECT TOP(1) MaHoaDon FROM dbo.HoaDon WITH(UPDLOCK,HOLDLOCK) WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC",cn,tx);
            bill.Parameters.Add("@ban",SqlDbType.Int).Value=tableId;
            var value=bill.ExecuteScalar(); int billId;
            if(value is null || value==DBNull.Value)
            {
                using var create=new SqlCommand("INSERT dbo.HoaDon(MaBan) OUTPUT INSERTED.MaHoaDon VALUES(@ban)",cn,tx);
                create.Parameters.Add("@ban",SqlDbType.Int).Value=tableId; billId=(int)create.ExecuteScalar()!;
            } else billId=(int)value;
            using var detail=new SqlCommand("IF EXISTS(SELECT 1 FROM dbo.ChiTietHoaDon WHERE MaHoaDon=@bill AND MaMon=@item) UPDATE dbo.ChiTietHoaDon SET SoLuong=SoLuong+@qty WHERE MaHoaDon=@bill AND MaMon=@item ELSE INSERT dbo.ChiTietHoaDon(MaHoaDon,MaMon,SoLuong,DonGia) SELECT @bill,MaMon,@qty,DonGia FROM dbo.MonAn WHERE MaMon=@item; UPDATE dbo.BanAn SET TrangThai=N'Có khách' WHERE MaBan=@ban;",cn,tx);
            detail.Parameters.Add("@bill",SqlDbType.Int).Value=billId; detail.Parameters.Add("@item",SqlDbType.Int).Value=itemId; detail.Parameters.Add("@qty",SqlDbType.Int).Value=quantity; detail.Parameters.Add("@ban",SqlDbType.Int).Value=tableId; detail.ExecuteNonQuery(); tx.Commit();
        } catch { tx.Rollback(); throw; }
    }

    public decimal Settle(int tableId)
    {
        using var cn=Open(); cn.Open(); using var tx=cn.BeginTransaction();
        try
        {
            using var cmd=new SqlCommand("DECLARE @id INT=(SELECT TOP(1) MaHoaDon FROM dbo.HoaDon WITH(UPDLOCK,HOLDLOCK) WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC); IF @id IS NULL THROW 50001,N'Bàn chưa có hóa đơn đang mở.',1; DECLARE @total DECIMAL(18,2)=(SELECT COALESCE(SUM(SoLuong*DonGia),0) FROM dbo.ChiTietHoaDon WHERE MaHoaDon=@id); UPDATE dbo.HoaDon SET TongTien=@total,GioRa=SYSDATETIME(),TrangThai=1 WHERE MaHoaDon=@id; UPDATE dbo.BanAn SET TrangThai=N'Trống' WHERE MaBan=@ban; SELECT @total;",cn,tx);
            cmd.Parameters.Add("@ban",SqlDbType.Int).Value=tableId; var total=Convert.ToDecimal(cmd.ExecuteScalar()); tx.Commit(); return total;
        } catch { tx.Rollback(); throw; }
    }

    public void SaveMenuItem(int id,string name,int categoryId,decimal price)
    {
        using var cn=Open(); cn.Open(); using var cmd=new SqlCommand(id==0?"INSERT dbo.MonAn(TenMon,MaDanhMuc,DonGia) VALUES(@name,@cat,@price)":"UPDATE dbo.MonAn SET TenMon=@name,MaDanhMuc=@cat,DonGia=@price WHERE MaMon=@id",cn);
        cmd.Parameters.Add("@name",SqlDbType.NVarChar,100).Value=name; cmd.Parameters.Add("@cat",SqlDbType.Int).Value=categoryId; cmd.Parameters.Add("@price",SqlDbType.Decimal).Value=price; if(id!=0) cmd.Parameters.Add("@id",SqlDbType.Int).Value=id; cmd.ExecuteNonQuery();
    }
    public void DeleteMenuItem(int id) { using var cn=Open(); cn.Open(); using var cmd=new SqlCommand("DELETE dbo.MonAn WHERE MaMon=@id",cn); cmd.Parameters.Add("@id",SqlDbType.Int).Value=id; cmd.ExecuteNonQuery(); }
}
