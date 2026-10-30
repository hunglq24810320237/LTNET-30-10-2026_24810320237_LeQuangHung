using InventoryManagement.DAL;
using InventoryManagement.DTO;
using System.Data;
namespace InventoryManagement.BUS;
public sealed class InventoryBus
{
    private readonly InventoryDal dal=new();
    public DataTable Materials(string? q=null)=>dal.Materials(q);public DataTable Categories()=>dal.Categories();public DataTable Suppliers()=>dal.Suppliers();public DataTable Vouchers()=>dal.Vouchers();public DataTable LowStock(decimal n)=>dal.LowStock(n);public decimal StockValue()=>dal.StockValue();
    public void SaveMaterial(string id,string name,int? cat,string unit){if(string.IsNullOrWhiteSpace(id)||string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(unit))throw new ArgumentException("Mã, tên vật tư và đơn vị tính là bắt buộc.");dal.SaveMaterial(id.Trim(),name.Trim(),cat,unit.Trim());}
    public void DeleteMaterial(string id)=>dal.DeleteMaterial(id);
    public void SaveSupplier(int id,string name,string phone,string address){if(string.IsNullOrWhiteSpace(name))throw new ArgumentException("Tên nhà cung cấp không được trống.");dal.SaveSupplier(id,name.Trim(),phone.Trim(),address.Trim());}
    public void DeleteSupplier(int id)=>dal.DeleteSupplier(id);
    public int SaveVoucher(string type,int? supplier,IReadOnlyCollection<StockLineDto> lines){if(type is not("NHAP" or "XUAT"))throw new ArgumentException("Chọn loại phiếu hợp lệ.");if(lines.Count==0)throw new ArgumentException("Phiếu cần có ít nhất một dòng vật tư.");if(type=="NHAP"&&supplier is null)throw new ArgumentException("Phiếu nhập phải chọn nhà cung cấp.");if(lines.Any(x=>x.Quantity<=0||x.UnitPrice<0))throw new ArgumentException("Số lượng phải > 0, đơn giá phải >= 0.");return dal.SaveVoucher(type,supplier,lines);}
}
