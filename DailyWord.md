 Nhật kí hằng ngày. ngày thứ 1 : 03/ 07/ 2026
- Hôm nay làm được gì?
Em đã hoàn thành các nội dung kiến thức của ngày 1 mà anh đã cho.
  Nội dung lý thuyết:
  - Em đã hiểu được Lập trình hướng thủ tục (Procedural Programming) và Hướng đối tượng (OOP) có các điểm khác nhau của nó, và tầm quan trọng của OOP trong 1 hệ thông lớn.
  - nắm rõ được khái niệm của class, object. con trỏ'this' cách sử dụng và đặc điểm của con trỏ.
  - làm rõ được thành phần của 1 class có những gì vai trò của từng thành phần.
  - ngoài ra em cũng xem lại các kiến thức cơ bản về c,c++. làm quen học cách sử dụng ngôn ngữ C#.
  Nội dung thực hành:
  - Em hoàn thành bài tập nhỏ đã được giao.
-  Vướng mắc/Bug gặp phải là gì?
  - Do mới làm quen và sử dụng C# nên có chút chưa quen thuộc ạ.
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
  - với vấn đề chưa quen thuộc về C# em đã tự khắc phục bằng cách nhờ AI giao cho các dạng bài tương tự để luyện tay, và kiểm tra bài sau khi đã code xong
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Nhật kí hằng ngày. ngày thứ 2 : 05/ 07/ 2026
Em đã hoàn thành các nội dung kiến thức của ngày 2 theo lộ trình:
- Hôm nay làm được gì?
Nội dung lý thuyết:
 - Hiểu sâu về tính Đóng gói (Encapsulation) trong OOP: Tại sao cần che giấu dữ liệu và cách bảo vệ tính toàn vẹn của đối tượng.
 - Phân biệt và nắm rõ phạm vi sử dụng của các Access Modifiers: private, public, protected, internal.
 - Làm quen với cách viết Getter/Setter và đặc biệt là cách sử dụng Properties trong C# để kiểm soát logic gán dữ liệu.
Nội dung thực hành:
 - Hoàn thành bài tập cập nhật class CanNhua: Chuyển đổi các thuộc tính sang private.
 - Xây dựng phương thức gán giá trị (Setter) cho MucNuocHienTai kèm theo điều kiện (kiểm tra không cho phép giá trị âm hoặc vượt quá dung tích tối đa).
- Vướng mắc/Bug gặp phải là gì?
 - Khi mới bắt đầu chuyển đổi sang Properties trong C#, em gặp khó trong việc hiểu cách sử dụng value vào khối set.
 - Việc xử lý thông báo lỗi trong setter cho chuẩn cũng là 1 điều mới và cần sử dụng nhiều lần.
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Cách giải quyết: Em đã tìm hiểu tài liệu về value keyword trong C# và thực hiện test các trường hợp biên (nhập 0, nhập số âm, nhập vượt mức) để đảm bảo class hoạt động đúng yêu cầu.
 -nhận thêm bài tập được AI cho thêm để luyện kĩ năng, hiều bài và cách sử dụng C#
 /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 3: 05/07/2026
- Hôm nay làm được gì?
  - Về mặt kiến thức: Nghiên cứu và áp dụng Tính kế thừa (Inheritance) để tối ưu việc tái sử dụng mã nguồn, giảm thiểu 
việc viết lặp code. Hiểu rõ luồng chạy của Constructor: Khi khởi tạo một lớp con thì Constructor của lớp cha luôn được kích hoạt trước thông qua từ khóa base để dựng khung nền tảng.
- bài tập nhỏ:
 - Em đã hoàn thành Thiết lập thành công lớp cha ThietBiDo gom các thuộc tính dùng chung là MaThietBi và TrangThai.
Phát triển 2 lớp con kế thừa trực tiếp: CumCamera (bổ sung thuộc tính DoPhanGiai) và CamBienNhietDo (bổ sung thuộc tính DonViDo).
- Vướng mắc/Bug gặp phải là gì?
 - Một số kiến thứ nâng cao hơn và áp dụng tính kế thừa chưa được nhuần nhuyễn. 
 - Gặp loạt cảnh báo vàng Warning CS8618 tại các dòng khai báo biến kiểu chữ (string).
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Đã xử lý triệt để bằng cách gán sẵn một chuỗi rỗng mặc định ngay khi khai báo thuộc tính: public string MaThietBiCamera { get; set; } = string.Empty;. Code hiện tại đã sạch bóng cảnh báo này.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 4: 06/07/2026
- Hôm nay làm được gì?
 - Về mặt kiến thức: Làm chủ tư duy áp dụng Tính đa hình (Polymorphism) và Tính trừu tượng (Abstraction). Phân biệt được khi nào dùng Abstract Class (cho các đối tượng cùng bản chất) và khi nào dùng Interface (khi cần ép các đối tượng khác bản chất tuân thủ một tập năng lực chung).
 - Định nghĩa Interface IConnectable có phương thức chung là ConnectSerial(). 
- Bài tập nhỏ: Tiến hành tích hợp interface này cho cả Camera và Cảm biến nhiệt độ nhưng xử lý logic kết nối riêng biệt phù hợp với phần cứng: Camera thì mở luồng nhận luồng video (Stream), còn Cảm biến thì mở cổng COM để bắt tín hiệu số.
- Vướng mắc/Bug gặp phải là gì?
 - Gặp lỗi trong việc ép kiểu dữ liệu khi duyệt danh sách. Do danh sách quản lý chung bằng kiểu lớp cha ThietBiDo nên trình biên dịch không cho gọi trực tiếp hàm kết nối của Interface. Nếu ép kiểu thủ công kiểu cũ, chương trình rất dễ bị sập (Crash) nếu xuất hiện thiết bị không có năng lực kết nối
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Sử dụng giải pháp kiểm tra kiểu an toàn bằng toán tử is của C# hiện đại: if (thietBi is IConnectable thietBiKetNoi). Chương trình chỉ thực hiện ép kiểu khi thiết bị đó thực sự có khả năng kết nối, loại bỏ hoàn toàn nguy cơ sập phần mềm.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 5: 07/07/2026
- Hôm nay làm được gì?
 - Về mặt kiến thức: Đánh giá lại toàn bộ tư duy thiết kế hướng đối tượng qua bài toán thực tế quản lý thiết bị phòng Lab. Tiếp cận nguyên lý Đơn nhiệm (Single Responsibility Principle) trong SOLID để giữ cho mã nguồn mạch lạc, mỗi khối lệnh chỉ làm một nhiệm vụ duy nhất.
 - Sản phẩm thực tế: Xây dựng cấu trúc mảng động List<ThietBiDo> tập trung trong file Program.cs. Nạp các thiết bị cụ thể vào hệ thống, viết vòng lặp foreach tự động duyệt danh sách, gọi đúng phương thức hiển thị thông tin và kích hoạt kết nối nghiệm thu thiết bị hoàn toàn tự động dựa trên tính đa hình.
- Vướng mắc/Bug gặp phải là gì?
 - Do thói quen viết code tập trung, ban đầu em đã viết gộp tất cả các tác vụ (khởi tạo, vòng lặp nhập liệu, kiểm tra interface) chung vào một khối trong hàm Main, dẫn đến tình trạng rối mã nguồn (Spaghetti code), rất khó để mở rộng hay dò lỗi sau này.
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Tiến hành cấu trúc lại code (Refactoring): Tách biệt rõ ràng ranh giới giữa phần quản lý danh sách thiết bị cố định và phần xử lý nhập liệu tương tác. Mã nguồn hiện tại đã rất sáng, dễ đọc và dễ bảo trì.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 6: 08/07/2026
- Hôm nay làm được gì?
 - Về mặt kiến thức: Tìm hiểu bản chất của Mảng tĩnh (Array - tốc độ truy cập cực nhanh nhưng kích thước cố định) và Mảng động (tự động mở rộng dung lượng khi đầy). Học cách ứng dụng Linq để tính toán xử lý nhanh trên tập hợp dữ liệu.
- Sản phẩm thực tế: Triển khai thành công List<double> LichSuDo để lưu liên tục lịch sử nhiệt độ do người dùng nhập vào cho đến khi nhận lệnh dừng là từ khóa "ok". Sử dụng các hàm mở rộng Linq .Average() (định dạng 2 số thập phân :F2) và .Max() để xuất nhanh số liệu thống kê nhiệt độ trung bình và cao nhất.
- Vướng mắc/Bug gặp phải là gì?
 - Lỗi đỏ CS0136: Bị xung đột do khai báo trùng tên biến input hai lần trong cùng một phạm vi hàm Main (vừa khai báo bên ngoài, vừa viết kèm từ khóa string ở trong vòng lặp).
 - Warning CS8600/CS8602: Cảnh báo biến nhận giá trị trống từ Console.ReadLine().
 - Lỗi hệ thống MSB3027: Lỗi biên dịch không thể ghi đè file .exe mới do tiến trình chạy thử trước đó vẫn đang chạy ngầm và chiếm quyền khóa file của hệ điều hành.
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Về lỗi code: Đưa dòng khai báo string input; ra duy nhất một vị trí phía trên vòng lặp. Phía trong vòng lặp chỉ dùng lệnh gán thuần túy kết hợp toán tử chống null: input = Console.ReadLine() ?? string.Empty;. Cách này xử lý dứt điểm cả lỗi trùng tên lẫn cảnh báo null.
 - Về lỗi khóa file: Nhấp vào Terminal và nhấn tổ hợp phím Ctrl + C để tắt hẳn tiến trình chạy ngầm cũ. Sau đó gõ dotnet run, chương trình đã biên dịch và chạy thành công 100%, không còn lỗi tồn đọng.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 7: 09/07/2026
- Nội dung lý thuyết:
 - Ngăn xếp (Stack): Hoạt động theo cơ chế LIFO (Last In, First Out - Vào sau, ra trước). Đây là cấu trúc dữ liệu cơ bản để quản lý tiến trình (Call Stack của hàm) và các tính năng quay lui (Undo/Redo).
 - Hàng đợi (Queue): Hoạt động theo cơ chế FIFO (First In, First Out - Vào trước, ra trước). Cực kỳ quan trọng trong việc xây dựng bộ đệm (Buffer) để xử lý dữ liệu truyền thông hoặc quản lý hàng đợi tin nhắn (Message Queue), giúp hệ thống không bị "nghẽn cổ chai" khi dữ liệu về quá nhanh.
- Nội dung thực hành:
 - Hoàn thành bài tập: Giả lập bộ đệm tín hiệu Serial.
 - Triển khai cấu trúc Queue<string> để lưu trữ dữ liệu từ cảm biến gửi về.
 - Viết hàm Background định kỳ lấy dữ liệu ra khỏi Queue để xử lý, đảm bảo luồng chính (Main thread) không bị treo.
- Vướng mắc/Bug gặp phải là gì?
 - tranh chấp dữ liệu (Race condition) khi nhiều luồng cùng truy cập Queue.
 - gặp 1 số lỗi tràn dữ liệu , tràn bộ nhớ .
 - 1 số dòng code chưa được tối ưu .
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Không dùng từ khóa lock (truy cập trực tiếp), Sử dụng lock (_queueLock) hoặc chuyển sang dùng ConcurrentQueue<T>. vì trong lập trình đa luồng (IoT/Serial), nếu em code và không khóa hoặc dùng cấu trúc an toàn, dữ liệu sẽ bị ghi đè hoặc gây lỗi InvalidOperationException.
 - Em đã sử dụng lock (_queueLock) để bao bọc quá trình Enqueue và Dequeue, giúp chương trình chạy ổn định mà không bị xung đột dữ liệu giữa luồng đọc Serial và luồng xử lý chính
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 8: 10/07/2026
 Nội dung lý thuyết:
 - em hiểu được Khái niệm cặp Khóa - Giá trị (Key - Value): Là cấu trúc lưu trữ dữ liệu gồm một Khóa (Key) duy nhất đóng vai trò làm định danh (như Mã thiết bị "CAM_01") và một Giá trị (Value) đi kèm (như đối tượng thiết bị chứa thông tin tên, trạng thái).
 - nắm rõ được Bản chất toán học của Hàm băm (Hash function): Hàm băm nhận đầu vào là một Khóa (Key) có độ dài bất kỳ, tính toán và chuyển đổi nó thành một chỉ số số nguyên (Hash Code) cố định. Chỉ số này trỏ thẳng tới một vị trí ô nhớ trong bộ mảng vật lý bên dưới.
 - em tìm hiểu được về Cơ chế tìm kiếm với độ phức tạp $O(1)$: Nhờ hàm băm tính toán ra ngay vị trí ô nhớ của dữ liệu mà không cần tìm kiếm tuần tự, thời gian truy xuất dữ liệu luôn là hằng số (không đổi) dù hệ thống có quản lý hàng ngàn hay hàng triệu thiết bị.
 - Và cũng hiểu và so sánh được với list trong ngày 6 giống và khác đồng thời thấy dược điểm tối ưu như nào.
 Nội dung thưucj hành:
 - Em đã hoàn thành toàn bộ các yêu cầu đồng thời kết hợp với project để chạy dược đồng thời.
 /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
 