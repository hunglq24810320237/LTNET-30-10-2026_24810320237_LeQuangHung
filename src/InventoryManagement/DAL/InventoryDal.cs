using InventoryManagement.DTO;
using Microsoft.Data.SqlClient;
using System.Data;

namespace InventoryManagement.DAL;
public sealed class InventoryDal
{
    private const string Cs="Server=localhost\\SQLEXPRESS;Database=InventoryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
    private SqlConnection Open()=>new(Cs);
    private static DataTable Query(SqlConnection c,string sql,params SqlParameter[] ps){using var cmd=new SqlCommand(sql,c);cmd.Parameters.AddRange(ps);using var a=new SqlDataAdapter(cmd);var d=new DataTable();a.Fill(d);return d;}
    public DataTable Materials(string? search=null){using var c=Open();return Query(c,"SELECT v.MaVatTu,v.TenVatTu,v.MaDanhMuc,d.TenDanhMuc,v.DVT,v.SoLuongTon FROM dbo.VatTu v LEFT JOIN dbo.DanhMucVatTu d ON d.MaDanhMuc=v.MaDanhMuc WHERE (@q IS NULL OR v.MaVatTu LIKE @like OR v.TenVatTu LIKE @like) ORDER BY v.MaVatTu",new SqlParameter("@q",SqlDbType.NVarChar,150){Value=string.IsNullOrWhiteSpace(search)?DBNull.Value:search},new SqlParameter("@like",SqlDbType.NVarChar,160){Value="%"+(search??"")+"%"});}
    public DataTable Categories(){using var c=Open();return Query(c,"SELECT MaDanhMuc,TenDanhMuc FROM dbo.DanhMucVatTu ORDER BY TenDanhMuc");}
    public DataTable Suppliers(){using var c=Open();return Query(c,"SELECT MaNCC,TenNCC,DienThoai,DiaChi FROM dbo.NhaCungCap ORDER BY TenNCC");}
    public DataTable Vouchers(){using var c=Open();return Query(c,"SELECT p.MaPhieu,p.LoaiPhieu,p.NgayLap,n.TenNCC,p.TongTien FROM dbo.PhieuKho p LEFT JOIN dbo.NhaCungCap n ON n.MaNCC=p.MaNCC ORDER BY p.MaPhieu DESC");}
    public DataTable LowStock(decimal threshold){using var c=Open();return Query(c,"SELECT MaVatTu,TenVatTu,DVT,SoLuongTon FROM dbo.VatTu WHERE SoLuongTon<@min ORDER BY SoLuongTon",new SqlParameter("@min",SqlDbType.Decimal){Precision=18,Scale=2,Value=threshold});}
    public decimal StockValue(){using var c=Open();c.Open();using var cmd=new SqlCommand("SELECT COALESCE(SUM(SoLuongTon*GiaNhap),0) FROM (SELECT v.SoLuongTon,COALESCE((SELECT TOP(1) ct.DonGia FROM dbo.ChiTietPhieuKho ct JOIN dbo.PhieuKho p ON p.MaPhieu=ct.MaPhieu WHERE ct.MaVatTu=v.MaVatTu AND p.LoaiPhieu=N'NHAP' ORDER BY p.NgayLap DESC),0) GiaNhap FROM dbo.VatTu v) x",c);return Convert.ToDecimal(cmd.ExecuteScalar());}
    public void SaveMaterial(string id,string name,int? category,string unit){using var c=Open();c.Open();using var cmd=new SqlCommand("IF EXISTS(SELECT 1 FROM dbo.VatTu WHERE MaVatTu=@id) UPDATE dbo.VatTu SET TenVatTu=@name,MaDanhMuc=@cat,DVT=@unit WHERE MaVatTu=@id ELSE INSERT dbo.VatTu(MaVatTu,TenVatTu,MaDanhMuc,DVT) VALUES(@id,@name,@cat,@unit)",c);cmd.Parameters.Add("@id",SqlDbType.VarChar,20).Value=id;cmd.Parameters.Add("@name",SqlDbType.NVarChar,150).Value=name;cmd.Parameters.Add("@cat",SqlDbType.Int).Value=category??(object)DBNull.Value;cmd.Parameters.Add("@unit",SqlDbType.NVarChar,20).Value=unit;cmd.ExecuteNonQuery();}
    public void DeleteMaterial(string id){using var c=Open();c.Open();using var cmd=new SqlCommand("DELETE dbo.VatTu WHERE MaVatTu=@id",c);cmd.Parameters.Add("@id",SqlDbType.VarChar,20).Value=id;cmd.ExecuteNonQuery();}
    public void SaveSupplier(int id,string name,string phone,string address){using var c=Open();c.Open();using var cmd=new SqlCommand(id==0?"INSERT dbo.NhaCungCap(TenNCC,DienThoai,DiaChi) VALUES(@name,@phone,@address)":"UPDATE dbo.NhaCungCap SET TenNCC=@name,DienThoai=@phone,DiaChi=@address WHERE MaNCC=@id",c);cmd.Parameters.Add("@name",SqlDbType.NVarChar,150).Value=name;cmd.Parameters.Add("@phone",SqlDbType.VarChar,15).Value=string.IsNullOrWhiteSpace(phone)?DBNull.Value:phone;cmd.Parameters.Add("@address",SqlDbType.NVarChar,255).Value=string.IsNullOrWhiteSpace(address)?DBNull.Value:address;if(id!=0)cmd.Parameters.Add("@id",SqlDbType.Int).Value=id;cmd.ExecuteNonQuery();}
    public void DeleteSupplier(int id){using var c=Open();c.Open();using var cmd=new SqlCommand("DELETE dbo.NhaCungCap WHERE MaNCC=@id",c);cmd.Parameters.Add("@id",SqlDbType.Int).Value=id;cmd.ExecuteNonQuery();}
    public int SaveVoucher(string type,int? supplier,IReadOnlyCollection<StockLineDto> lines)
    {
        using var c=Open();c.Open();using var tx=c.BeginTransaction();
        try
        {
            using var head=new SqlCommand("INSERT dbo.PhieuKho(LoaiPhieu,MaNCC,TongTien) OUTPUT INSERTED.MaPhieu VALUES(@type,@supplier,0)",c,tx);head.Parameters.Add("@type",SqlDbType.NVarChar,10).Value=type;head.Parameters.Add("@supplier",SqlDbType.Int).Value=supplier??(object)DBNull.Value;int id=(int)head.ExecuteScalar()!;decimal total=0;
            foreach(var line in lines)
            {
                using var detail=new SqlCommand("INSERT dbo.ChiTietPhieuKho(MaPhieu,MaVatTu,SoLuong,DonGia) VALUES(@bill,@item,@qty,@price); UPDATE dbo.VatTu SET SoLuongTon=SoLuongTon+CASE WHEN @type=N'NHAP' THEN @qty ELSE -@qty END WHERE MaVatTu=@item AND (@type=N'NHAP' OR SoLuongTon>=@qty); IF @@ROWCOUNT=0 THROW 50011,N'Không đủ hàng trong kho hoặc mã vật tư không tồn tại.',1;",c,tx);
                detail.Parameters.Add("@bill",SqlDbType.Int).Value=id;detail.Parameters.Add("@item",SqlDbType.VarChar,20).Value=line.MaterialId;detail.Parameters.Add("@qty",SqlDbType.Decimal).Value=line.Quantity;detail.Parameters.Add("@price",SqlDbType.Decimal).Value=line.UnitPrice;detail.Parameters.Add("@type",SqlDbType.NVarChar,10).Value=type;detail.ExecuteNonQuery();total+=line.Quantity*line.UnitPrice;
            }
            using var sum=new SqlCommand("UPDATE dbo.PhieuKho SET TongTien=@total WHERE MaPhieu=@id",c,tx);sum.Parameters.Add("@total",SqlDbType.Decimal).Value=total;sum.Parameters.Add("@id",SqlDbType.Int).Value=id;sum.ExecuteNonQuery();tx.Commit();return id;
        }catch{tx.Rollback();throw;}
    }
}
