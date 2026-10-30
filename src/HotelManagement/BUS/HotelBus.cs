using HotelManagement.DAL;
using System.Data;
namespace HotelManagement.BUS;
public sealed class HotelBus
{
    private readonly HotelDal dal=new();
    public DataTable Rooms()=>dal.Rooms();public DataTable Services()=>dal.Services();public DataTable ActiveStays()=>dal.ActiveStays();
    public void SaveRoom(string room,string type,decimal rate){if(string.IsNullOrWhiteSpace(room))throw new ArgumentException("Mã phòng là bắt buộc.");if(room.Trim().Length>10)throw new ArgumentException("Mã phòng tối đa 10 ký tự.");if(type is not ("Đơn" or "Đôi" or "VIP"))throw new ArgumentException("Chọn loại phòng hợp lệ.");if(rate<0)throw new ArgumentException("Giá theo giờ không được âm.");dal.SaveRoom(room.Trim(),type,rate);}
    public void DeleteRoom(string room){if(string.IsNullOrWhiteSpace(room))throw new ArgumentException("Chọn phòng cần xóa.");dal.DeleteRoom(room.Trim());}
    public void CheckIn(string room,string name,string cccd,string phone){if(string.IsNullOrWhiteSpace(room)||string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(cccd))throw new ArgumentException("Phòng, họ tên và CCCD là bắt buộc.");dal.CheckIn(room,name.Trim(),cccd.Trim(),phone.Trim());}
    public void AddService(string room,int service,int qty){if(qty<1)throw new ArgumentException("Số lượng phải lớn hơn 0.");dal.AddService(room,service,qty);}
    public decimal CheckOut(string room)=>dal.CheckOut(room);
}
