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
Nhật kí hằng ngày. ngày : 05/ 07/ 2026
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
Ngày 05/07/2026
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
Ngày 06/07/2026
- Hôm nay làm được gì?
 - Về mặt kiến thức: Làm chủ tư duy áp dụng Tính đa hình (Polymorphism) và Tính trừu tượng (Abstraction). Phân biệt được khi nào dùng Abstract Class (cho các đối tượng cùng bản chất) và khi nào dùng Interface (khi cần ép các đối tượng khác bản chất tuân thủ một tập năng lực chung).
 - Định nghĩa Interface IConnectable có phương thức chung là ConnectSerial(). 
- Bài tập nhỏ: Tiến hành tích hợp interface này cho cả Camera và Cảm biến nhiệt độ nhưng xử lý logic kết nối riêng biệt phù hợp với phần cứng: Camera thì mở luồng nhận luồng video (Stream), còn Cảm biến thì mở cổng COM để bắt tín hiệu số.
- Vướng mắc/Bug gặp phải là gì?
 - Gặp lỗi trong việc ép kiểu dữ liệu khi duyệt danh sách. Do danh sách quản lý chung bằng kiểu lớp cha ThietBiDo nên trình biên dịch không cho gọi trực tiếp hàm kết nối của Interface. Nếu ép kiểu thủ công kiểu cũ, chương trình rất dễ bị sập (Crash) nếu xuất hiện thiết bị không có năng lực kết nối
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Sử dụng giải pháp kiểm tra kiểu an toàn bằng toán tử is của C# hiện đại: if (thietBi is IConnectable thietBiKetNoi). Chương trình chỉ thực hiện ép kiểu khi thiết bị đó thực sự có khả năng kết nối, loại bỏ hoàn toàn nguy cơ sập phần mềm.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 07/07/2026
- Hôm nay làm được gì?
 - Về mặt kiến thức: Đánh giá lại toàn bộ tư duy thiết kế hướng đối tượng qua bài toán thực tế quản lý thiết bị phòng Lab. Tiếp cận nguyên lý Đơn nhiệm (Single Responsibility Principle) trong SOLID để giữ cho mã nguồn mạch lạc, mỗi khối lệnh chỉ làm một nhiệm vụ duy nhất.
 - Sản phẩm thực tế: Xây dựng cấu trúc mảng động List<ThietBiDo> tập trung trong file Program.cs. Nạp các thiết bị cụ thể vào hệ thống, viết vòng lặp foreach tự động duyệt danh sách, gọi đúng phương thức hiển thị thông tin và kích hoạt kết nối nghiệm thu thiết bị hoàn toàn tự động dựa trên tính đa hình.
- Vướng mắc/Bug gặp phải là gì?
 - Do thói quen viết code tập trung, ban đầu em đã viết gộp tất cả các tác vụ (khởi tạo, vòng lặp nhập liệu, kiểm tra interface) chung vào một khối trong hàm Main, dẫn đến tình trạng rối mã nguồn (Spaghetti code), rất khó để mở rộng hay dò lỗi sau này.
- Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?
 - Tiến hành cấu trúc lại code (Refactoring): Tách biệt rõ ràng ranh giới giữa phần quản lý danh sách thiết bị cố định và phần xử lý nhập liệu tương tác. Mã nguồn hiện tại đã rất sáng, dễ đọc và dễ bảo trì.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 08/07/2026
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
Ngày 09/07/2026
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
 10/07/2026
 Nội dung lý thuyết:
 - em hiểu được Khái niệm cặp Khóa - Giá trị (Key - Value): Là cấu trúc lưu trữ dữ liệu gồm một Khóa (Key) duy nhất đóng vai trò làm định danh (như Mã thiết bị "CAM_01") và một Giá trị (Value) đi kèm (như đối tượng thiết bị chứa thông tin tên, trạng thái).
 - nắm rõ được Bản chất toán học của Hàm băm (Hash function): Hàm băm nhận đầu vào là một Khóa (Key) có độ dài bất kỳ, tính toán và chuyển đổi nó thành một chỉ số số nguyên (Hash Code) cố định. Chỉ số này trỏ thẳng tới một vị trí ô nhớ trong bộ mảng vật lý bên dưới.
 - em tìm hiểu được về Cơ chế tìm kiếm với độ phức tạp $O(1)$: Nhờ hàm băm tính toán ra ngay vị trí ô nhớ của dữ liệu mà không cần tìm kiếm tuần tự, thời gian truy xuất dữ liệu luôn là hằng số (không đổi) dù hệ thống có quản lý hàng ngàn hay hàng triệu thiết bị.
 - Và cũng hiểu và so sánh được với list trong ngày 6 giống và khác đồng thời thấy dược điểm tối ưu như nào.
 Nội dung thưucj hành:
 - Em đã hoàn thành toàn bộ các yêu cầu đồng thời kết hợp với project để chạy dược đồng thời.
 /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 11 - 12
Nội dung lý thuyết:
Bộ nhớ String: Tìm hiểu về tính bất biến (Immutability) của chuỗi trong C# (mỗi khi thao tác thay đổi chuỗi, hệ thống sẽ tạo một đối tượng mới trên bộ nhớ thay vì sửa trực tiếp, giúp an toàn nhưng cần lưu ý hiệu năng khi xử lý vòng lặp lớn).
Kỹ thuật xử lý chuỗi nâng cao: Nắm vững cách sử dụng các phương thức Split, Contains, IndexOf, Trim, kết hợp với CultureInfo để định dạng dữ liệu chuẩn xác.
Tư duy thiết kế "Khung dữ liệu" (Data Frame): Hiểu cách đóng gói dữ liệu trong truyền thông nối tiếp (Serial) thông qua ký tự bắt đầu (Start marker), ký tự kết thúc (End marker) và ký tự phân cách (Delimiter).
Nội dung thực hành:
Xây dựng hàm kiểm tra tính hợp lệ của chuỗi thô (Raw string) nhận từ thiết bị giả lập Arduino ($LOG,2026-07-03,28.5,45#).
Viết thuật toán bóc tách dữ liệu (Parsing): Kiểm tra điều kiện đầu/cuối chuỗi, cắt bỏ ký tự đặc biệt, thực hiện Split theo dấu phẩy và chuyển đổi sang các biến có kiểu dữ liệu phù hợp (string, DateTime, double, int).
Những khó khăn khi làm bài:
Xác thực và làm sạch dữ liệu: Ban đầu chưa kiểm soát chặt các trường hợp chuỗi bị thiếu ký tự mở đầu ($) hoặc kết thúc (#), dẫn đến việc cắt chuỗi bị lỗi index hoặc văng ngoại lệ (ArgumentOutOfRangeException).
Xung đột định dạng số thực (Region Settings): Khi ép kiểu chuỗi số thập phân (28.5), một số hệ điều hành Windows dùng dấu phẩy (,) làm phân cách phần thập phân, gây lỗi khi gọi hàm double.TryParse.
Hướng giải quyết:
Sử dụng phương thức StartsWith("$") và EndsWith("#") kết hợp với Substring để loại bỏ ký tự điều khiển trước khi tách mảng, đảm bảo dữ liệu đưa vào Split luôn sạch sẽ.
Sử dụng CultureInfo.InvariantCulture khi chuyển đổi chuỗi sang số (double.TryParse) để hệ thống luôn hiểu dấu chấm (.) là phân cách thập phân, triệt tiêu sự phụ thuộc vào cấu hình vùng miền của máy tính.
  /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 13 - 15
Nội dung lý thuyết:
Phân biệt lỗi: Hiểu rõ sự khác biệt giữa Lỗi cú pháp (Syntax Error - phát hiện ngay lúc biên dịch) và Lỗi thực thi (Runtime Error - chỉ phát sinh khi chương trình đang chạy do dữ liệu hoặc phần cứng bất ngờ).
Khối lệnh try - catch - finally: Cách cô lập đoạn code dễ sinh lỗi trong try, bắt các ngoại lệ cụ thể (NullReferenceException, IndexOutOfRangeException, DivideByZeroException) trong catch, và đảm bảo đoạn code dọn dẹp luôn được thực thi trong finally.
Quản lý tài nguyên phần cứng: Tầm quan trọng của việc giải phóng tài nguyên (Cổng Serial, luồng file, kết nối database) để tránh rò rỉ bộ nhớ (memory leak) hoặc treo phần cứng khi có lỗi xảy ra.
Nội dung thực hành:
Xây dựng hàm xử lý tính toán chia tỷ lệ dữ liệu đọc từ cảm biến.
Viết mã bắt ngoại lệ DivideByZeroException khi mẫu dữ liệu cảm biến trả về giá trị 0 gây lỗi chia.
Ứng dụng khối finally để thiết lập cơ chế đảm bảo cổng kết nối Serial luôn được đóng an toàn dù quá trình đọc và xử lý dữ liệu thành công hay văng lỗi.
Những khó khăn khi làm bài:
Bắt lỗi chung chung: Ban đầu em có thói quen dùng catch (Exception) tổng quát cho mọi trường hợp, khiến việc xác định nguyên nhân thực sự của lỗi gặp khó khăn và che giấu các vấn đề logic khác.
Phát sinh lỗi kép trong khối finally: Khi gọi hàm đóng cổng Serial trong finally mà cổng chưa từng được mở thành công (hoặc đã bị lỗi từ trước), chương trình tiếp tục văng ra một ngoại lệ mới (InvalidOperationException).
Hướng giải quyết:
Chuyển sang bắt các ngoại lệ cụ thể (DivideByZeroException, FormatException) để in ra thông báo lỗi chi tiết, giúp định hướng sửa code chính xác hơn.
Thêm điều kiện kiểm tra trạng thái kết nối (ví dụ: if (serialPort != null && serialPort.IsOpen)) trước khi gọi phương thức đóng cổng trong khối finally, triệt tiêu hoàn toàn nguy cơ phát sinh ngoại lệ phụ. 
  /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
Ngày 16 - 17
Nội dung lý thuyết:
Luồng dữ liệu (Stream) & File I/O: Tìm hiểu về cách .NET quản lý luồng đọc/ghi dữ liệu xuống ổ cứng thông qua các lớp StreamWriter và StreamReader, cũng như các phương thức hỗ trợ nhanh từ lớp File để thao tác với tệp văn bản.
Định dạng CSV (Comma-Separated Values): Nắm vững cấu trúc lưu trữ dữ liệu dạng bảng phẳng, ngăn cách bởi dấu phẩy, giải pháp nhẹ nhàng và tối ưu để ghi nhận lịch sử (log) cho các hệ thống IoT quy mô nhỏ mà không cần cài đặt cơ sở dữ liệu cồng kềnh.
Nội dung thực hành:
Thiết kế và xây dựng class Logger độc lập chuyên trách việc ghi log hệ thống.
Tích hợp class Logger vào luồng xử lý bóc tách dữ liệu từ Ngày 9, thiết lập cơ chế ghi nối tiếp (Append) thông tin dòng log bao gồm Thời gian, Nhiệt độ, Mực nước xuống tệp history.csv mỗi khi có bản tin mới từ thiết bị.
Những khó khăn khi làm bài:
Xung đột khóa file (File Sharing Violation): Khi hệ thống chạy đa luồng hoặc gọi hàm ghi file liên tục mà đối tượng StreamWriter trước đó chưa được đóng hoặc giải phóng, chương trình lập tức văng ngoại lệ IOException do tệp đang bị tiến trình khác chiếm dụng.
Quản lý đường dẫn tệp: Gặp bối rối khi file history.csv bị sinh ra ngầm định bên trong thư mục bin/Debug của ứng dụng thay vì thư mục quản lý dữ liệu mong muốn.
Hướng giải quyết:
Sử dụng cú pháp using (hoặc các phương thức tĩnh an toàn như File.AppendAllText) để đảm bảo luồng ghi file luôn tự động đóng và giải phóng tài nguyên ngay sau khi thực thi xong, triệt để ngăn chặn tình trạng khóa file.
Sử dụng biến môi trường hoặc đường dẫn kết hợp AppDomain.CurrentDomain.BaseDirectory để định vị chính xác thư mục lưu trữ file history.csv, giúp dễ dàng kiểm tra dữ liệu sau khi chạy chương trình.
  /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
  Ngày 18 - 19 - 20
- Hôm nay làm được gì?
 Tái cấu trúc luồng chạy chính: Chuyển đổi toàn bộ chương trình từ nhập liệu tuần tự sang mô hình bất đồng bộ (async Task Main), tích hợp chạy song song luồng xử lý nền (BackgroundProcessor) và luồng giả lập cảm biến (SensorSimulator) thông qua Task.WaitAll.
 Nâng cấp tính năng quản lý thiết bị & bình chứa: Nâng cấp lớp BinhNuoc01 cho phép người dùng nhập chi tiết dung tích, chiều cao linh hoạt từ bàn phím; đồng thời liên kết trực tiếp giá trị mực nước thực tế vào chuỗi dữ liệu giả lập. Nâng cấp cấu trúc lưu trữ danh sách thiết bị sang Dictionary<string, thietbido?> để truy xuất nhanh $O(1)$.
 Bổ sung tính năng xử lý dữ liệu: Rút ngắn thời gian giả lập gửi log từ 10 giây xuống 3 giây/lần giúp phản hồi tiệm cận thời gian thực; bổ sung cơ chế tự động tính nhiệt độ trung bình và tìm nhiệt độ cực đại kèm vị trí chu kỳ đo sau mỗi 10 gói tin $LOG.
 Ghi vết dữ liệu: Kết nối lưu lịch sử đo vào file CSV thông qua lớp CSVhisstory.
- Vướng mắc / Bug gặp phải là gì?
Xung đột đa luồng (Race Condition): Luồng BackgroundProcessor và SensorSimulator cùng truy cập/chỉnh sửa DATAqueue và dictDATABASE gây nguy cơ bất đồng bộ và tranh chấp dữ liệu.
Xử lý Null Safety: Xuất hiện cảnh báo kiểu dữ liệu có thể chứa null (thietbido?) khi truy xuất thiết bị trong dictDATABASE.
Trôi biến thống kê: Biến cộng dồn nhiệt độ (totalTemp), vị trí đo (maxTempIndex) và chỉ số cực đại (maxTemp) dễ bị sai lệch nếu không reset đúng chu kỳ sau khi đủ 10 gói tin.
- Đã giải quyết thế nào / Cần Mentor hỗ trợ gì?
Cách giải quyết:Áp dụng khóa an toàn lock (DATAlock) cho thao tác Enqueue/Dequeue trên Queue và lock (dictDATA) cho Dictionary để đảm bảo an toàn đa luồng.Khắc phục cảnh báo Null bằng cách kết hợp TryGetValue với điều kiện kiểm tra tồn tại != null.
Đặt lại logic reset toàn bộ biến thống kê (totalTemp = 0.0, maxTemp = double.MinValue, maxTempIndex = 0) ngay trong khối lệnh xử lý chu kỳ 10 gói tin (logCount % 10 == 0).
Đề xuất / Cần Mentor hỗ trợ:Cần Mentor review giúp đoạn mã xử lý an toàn đa luồng xem cơ chế lock hiện tại đã tối ưu chưa hay nên chuyển sang dùng ConcurrentQueue và ConcurrentDictionary của .NET để đạt hiệu năng cao hơn.
- Ghi chú kỹ thuật về Lập trình Bất đồng bộ (Async):Lập trình bất đồng bộ (async/await) là cơ chế cho phép chương trình thực thi các tác vụ tốn thời gian (như đọc/ghi file, nhận dữ liệu mạng hay chạy bộ giả lập) mà không làm đóng băng (block) luồng chính của ứng dụng. Chức năng hoạt động cốt lõi của nó là tạm giải phóng luồng hiện tại để xử lý công việc khác trong lúc chờ một tác vụ nền hoàn thành, sau đó tự động quay lại tiếp tục đoạn mã ngay tại điểm dừng. Trong ứng dụng này, async được áp dụng để duy trì đồng thời hai tiến trình riêng biệt: vừa giả lập phát dữ liệu cảm biến liên tục, vừa xử lý và lưu trữ dữ liệu nền một cách song song, giúp tối ưu tối đa hiệu năng hệ thống.
 /~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
 ngày 21 - 22 
- Day 15: Cấu hình phần cứng & Giao tiếp Serial (Arduino)
Cài đặt và cấu hình hoàn thiện môi trường phát triển Arduino IDE.
Viết chương trình nạp cho vi điều khiển để đọc dữ liệu đo thực tế từ cảm biến nhiệt độ phòng.
Thiết kế và đóng gói dữ liệu theo chuỗi định dạng truyền thông chuẩn (ví dụ: $TEMP,28.5#) để truyền phát dữ liệu ổn định qua cổng Serial (USB) với baudrate chuẩn (9600 bps).
- Day 16: Thiết kế giao diện người dùng (C# Desktop App)
Khởi tạo dự án ứng dụng giao diện C# (WPF / WinForms).
Thiết kế khung điều khiển kết nối: Bổ sung ComboBox cho phép quét/chọn cổng COM Port, tùy chỉnh Baudrate và các nút bấm Connect / Disconnect.
Xây dựng giao diện hiển thị: Tạo các ô Dashboard/Text Block để cập nhật trực quan các thông số nhiệt độ thực tế và trạng thái kết nối phần cứng theo thời gian thực.- Vướng mắc / Bug gặp phải là gì?
Lỗi trôi/ngắt chuỗi Serial (Day 15): Khi truyền dữ liệu tốc độ cao qua cổng USB, gói tin thỉnh thoảng bị chia nhỏ làm C# đọc thiếu ký tự đầu/cuối.
Xung đột luồng UI trong C# (Day 16): Sự kiện nhận dữ liệu SerialPort.DataReceived chạy ở luồng nền (Background Thread), khi cập nhật trực tiếp lên các ô hiển thị UI sẽ gây ra lỗi Cross-thread operation not valid.
Tự động nhận diện cổng COM (Day 16): Cổng COM chưa tự động cập nhật lại danh sách khi cắm hoặc rút cáp USB đột ngột.
- Đã giải quyết thế nào / Cần Mentor hỗ trợ gì?
Cách giải quyết:
Xử lý chuỗi Serial: Thêm ký tự bắt đầu $ và kết thúc # (hoặc \n) trong code Arduino, kết hợp sử dụng Serial.ReadLine() ở phía C# để đảm bảo nhận trọn vẹn một khung dữ liệu.
Cập nhật UI an toàn: Sử dụng Dispatcher.Invoke (đối với WPF) hoặc Control.Invoke (đối với WinForms) để đưa thao tác cập nhật giao diện về đúng luồng UI chính (UI Thread).
Quản lý Cổng COM: Viết hàm quét danh sách cổng SerialPort.GetPortNames() và đổ vào ComboBox mỗi khi bấm làm mới hoặc mở ứng dụng.
/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~/
ngày 23



