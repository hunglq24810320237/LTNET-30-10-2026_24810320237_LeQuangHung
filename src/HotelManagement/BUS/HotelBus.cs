using HotelManagement.DAL;
using System.Data;
namespace HotelManagement.BUS;
public sealed class HotelBus
{
    private readonly HotelDal dal=new();
    public DataTable Rooms()=>dal.Rooms();public DataTable Services()=>dal.Services();public DataTable ActiveStays()=>dal.ActiveStays();
    public void CheckIn(string room,string name,string cccd,string phone){if(string.IsNullOrWhiteSpace(room)||string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(cccd))throw new ArgumentException("Phòng, họ tên và CCCD là bắt buộc.");dal.CheckIn(room,name.Trim(),cccd.Trim(),phone.Trim());}
    public void AddService(string room,int service,int qty){if(qty<1)throw new ArgumentException("Số lượng phải lớn hơn 0.");dal.AddService(room,service,qty);}
    public decimal CheckOut(string room)=>dal.CheckOut(room);
}
