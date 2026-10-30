using Microsoft.Data.SqlClient;
using System.Data;

namespace HotelManagement.DAL;
public sealed class HotelDal
{
    private const string Cs="Server=localhost\\SQLEXPRESS;Database=HotelDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
    private SqlConnection Open()=>new(Cs);
    public DataTable Rooms(){using var c=Open();using var a=new SqlDataAdapter("SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM dbo.Phong ORDER BY SoPhong",c);var d=new DataTable();a.Fill(d);return d;}
    public void SaveRoom(string room,string type,decimal rate)
    {
        using var c=Open();using var cmd=new SqlCommand("IF EXISTS(SELECT 1 FROM dbo.Phong WHERE SoPhong=@room) UPDATE dbo.Phong SET LoaiPhong=@type,GiaTheoGio=@rate WHERE SoPhong=@room ELSE INSERT dbo.Phong(SoPhong,LoaiPhong,GiaTheoGio) VALUES(@room,@type,@rate)",c);
        cmd.Parameters.Add("@room",SqlDbType.VarChar,10).Value=room;cmd.Parameters.Add("@type",SqlDbType.NVarChar,50).Value=type;cmd.Parameters.Add("@rate",SqlDbType.Decimal).Value=rate;cmd.Parameters["@rate"].Precision=18;cmd.Parameters["@rate"].Scale=2;cmd.ExecuteNonQuery();
    }
    public void DeleteRoom(string room){using var c=Open();using var cmd=new SqlCommand("DELETE dbo.Phong WHERE SoPhong=@room",c);cmd.Parameters.Add("@room",SqlDbType.VarChar,10).Value=room;cmd.ExecuteNonQuery();}
    public DataTable Services(){using var c=Open();using var a=new SqlDataAdapter("SELECT MaDV,TenDV,DonGia FROM dbo.DichVu ORDER BY TenDV",c);var d=new DataTable();a.Fill(d);return d;}
    public DataTable ActiveStays(){using var c=Open();using var a=new SqlDataAdapter("SELECT p.MaPhieu,p.SoPhong,k.HoTen,k.CCCD,p.NgayNhanPhong FROM dbo.PhieuDatPhong p JOIN dbo.KhachHang k ON k.MaKH=p.MaKH WHERE p.NgayTraPhong IS NULL ORDER BY p.NgayNhanPhong",c);var d=new DataTable();a.Fill(d);return d;}
    public void CheckIn(string room,string name,string cccd,string phone)
    {
        using var c=Open();c.Open();using var tx=c.BeginTransaction();
        try
        {
            using var q=new SqlCommand("SELECT MaKH FROM dbo.KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@cccd",c,tx);q.Parameters.Add("@cccd",SqlDbType.VarChar,20).Value=cccd;var customer=q.ExecuteScalar();int customerId;
            if(customer is null){using var add=new SqlCommand("INSERT dbo.KhachHang(HoTen,CCCD,SoDienThoai) OUTPUT INSERTED.MaKH VALUES(@name,@cccd,@phone)",c,tx);add.Parameters.Add("@name",SqlDbType.NVarChar,100).Value=name;add.Parameters.Add("@cccd",SqlDbType.VarChar,20).Value=cccd;add.Parameters.Add("@phone",SqlDbType.VarChar,15).Value=string.IsNullOrWhiteSpace(phone)?DBNull.Value:phone;customerId=(int)add.ExecuteScalar()!;}
            else customerId=(int)customer;
            using var check=new SqlCommand("IF NOT EXISTS(SELECT 1 FROM dbo.Phong WITH(UPDLOCK,HOLDLOCK) WHERE SoPhong=@room AND TrangThai=N'Trống') THROW 50002,N'Phòng không còn trống.',1; INSERT dbo.PhieuDatPhong(SoPhong,MaKH) VALUES(@room,@customer); UPDATE dbo.Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@room",c,tx);check.Parameters.Add("@room",SqlDbType.VarChar,10).Value=room;check.Parameters.Add("@customer",SqlDbType.Int).Value=customerId;check.ExecuteNonQuery();tx.Commit();
        }catch{tx.Rollback();throw;}
    }
    public void AddService(string room,int serviceId,int quantity)
    {
        using var c=Open();c.Open();using var cmd=new SqlCommand("DECLARE @p INT=(SELECT TOP(1) MaPhieu FROM dbo.PhieuDatPhong WHERE SoPhong=@room AND NgayTraPhong IS NULL ORDER BY MaPhieu DESC); IF @p IS NULL THROW 50003,N'Phòng chưa check-in.',1; INSERT dbo.SuDungDichVu(MaPhieu,MaDV,SoLuong,DonGia) SELECT @p,MaDV,@qty,DonGia FROM dbo.DichVu WHERE MaDV=@dv",c);cmd.Parameters.Add("@room",SqlDbType.VarChar,10).Value=room;cmd.Parameters.Add("@dv",SqlDbType.Int).Value=serviceId;cmd.Parameters.Add("@qty",SqlDbType.Int).Value=quantity;cmd.ExecuteNonQuery();
    }
    public decimal CheckOut(string room)
    {
        using var c=Open();c.Open();using var tx=c.BeginTransaction();
        try
        {
            using var cmd=new SqlCommand("DECLARE @id INT=(SELECT TOP(1) MaPhieu FROM dbo.PhieuDatPhong WITH(UPDLOCK,HOLDLOCK) WHERE SoPhong=@room AND NgayTraPhong IS NULL ORDER BY MaPhieu DESC); IF @id IS NULL THROW 50004,N'Phòng chưa có phiếu lưu trú.',1; DECLARE @total DECIMAL(18,2); SELECT @total=CEILING(DATEDIFF(MINUTE,NgayNhanPhong,SYSDATETIME())/60.0)*GiaTheoGio FROM dbo.PhieuDatPhong p JOIN dbo.Phong r ON r.SoPhong=p.SoPhong WHERE MaPhieu=@id; SET @total=@total+COALESCE((SELECT SUM(SoLuong*DonGia) FROM dbo.SuDungDichVu WHERE MaPhieu=@id),0); UPDATE dbo.PhieuDatPhong SET NgayTraPhong=SYSDATETIME(),TongTienThanhToan=@total WHERE MaPhieu=@id; UPDATE dbo.Phong SET TrangThai=N'Đang dọn dẹp' WHERE SoPhong=@room; SELECT @total;",c,tx);cmd.Parameters.Add("@room",SqlDbType.VarChar,10).Value=room;var total=Convert.ToDecimal(cmd.ExecuteScalar());tx.Commit();return total;
        }catch{tx.Rollback();throw;}
    }
}
