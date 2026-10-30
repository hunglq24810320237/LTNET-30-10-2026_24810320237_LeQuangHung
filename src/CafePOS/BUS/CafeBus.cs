using CafePOS.DAL;
using System.Data;

namespace CafePOS.BUS;

public sealed class CafeBus
{
    private readonly CafeDal dal = new();
    public DataTable Tables()=>dal.Tables();
    public DataTable Categories()=>dal.Categories();
    public DataTable Menu()=>dal.Menu();
    public DataTable MenuByCategory(int id)=>dal.MenuByCategory(id);
    public DataTable CurrentOrder(int table)=>dal.CurrentOrder(table);
    public void AddOrder(int table,int item,int quantity){if(quantity<1)throw new ArgumentException("Số lượng phải lớn hơn 0.");dal.AddOrder(table,item,quantity);}
    public decimal Settle(int table)=>dal.Settle(table);
    public void SaveMenu(int id,string name,int category,decimal price){if(string.IsNullOrWhiteSpace(name))throw new ArgumentException("Tên món không được để trống.");if(price<0)throw new ArgumentException("Đơn giá không được âm.");dal.SaveMenuItem(id,name.Trim(),category,price);}
    public void DeleteMenu(int id)=>dal.DeleteMenuItem(id);
}
