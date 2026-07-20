# 📋 Lộ Trình Thực Tập (2 Tháng)
## Đề tài: Hệ thống giám sát Mực nước (Vision) & Nhiệt độ phòng (Arduino) qua Giao diện C#

Tài liệu này chứa toàn bộ lộ trình học tập lý thuyết, bài tập thực hành và kế hoạch triển khai dự án chi tiết từng ngày dành cho sinh viên thực tập năm 3 ngành Điện tử - Truyền thông & Máy tính.

---

## 📅 GIAI ĐOẠN 1: NỀN TẢNG LẬP TRÌNH C# & QUẢN LÝ CODE (14 NGÀY)

### 🧩 Tuần 1: Lập trình hướng đối tượng (OOP) với C#

| Ngày | Bài học Lý thuyết | Bài tập Thực hành (C# Console App) | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :--- | :---: | :--- |
| **Day 1** | **Class & Object cơ bản**:<br>- Từ C sang C# OOP<br>- Thành phần Class (Fields, Methods)<br>- Constructor & Con trỏ `this` | Tạo class `WaterCan` mô phỏng cái can nhựa chứa nước (`maxCapacity`, `canHeight`, `currentWaterLevel`). Viết hàm `PourWater()` và `PrintInfo()`. | ✅ `Done` | [Link]() |
| **Day 2** | **Tính Đóng gói (Encapsulation)**:<br>- Che giấu dữ liệu (`private`, `public`)<br>- Sử dụng Properties (`get/set`) kiểm soát đầu vào | Nâng cấp class `WaterCan`, chuyển các fields thành `private`. Viết logic trong `set` để chặn gán giá trị mực nước âm hoặc vượt quá dung tích tối đa. |  ✅ `Done` | [Link]() |
| **Day 3** | **Tính Kế thừa (Inheritance)**:<br>- Mối quan hệ IS-A<br>- Từ khóa `:` kế thừa<br>- Gọi constructor cha với `base` | Tạo lớp cha `Device` (`DeviceId`, `IsConnected`). Tạo các lớp con `TemperatureSensor` (`Unit`) và `WebcamDevice` (`Resolution`) kế thừa từ `Device`. |  ✅ `Done` | [Link]() |
| **Day 4** | **Tính Đa hình & Interface**:<br>- Định nghĩa Interface<br>- Đa hình động (Overriding)<br>- Mối quan hệ HAS-A | Định nghĩa Interface `IConnectable` chứa hàm `ConnectSerial()`. Cho `TemperatureSensor` và `WebcamDevice` thực thi interface này với logic in ra màn hình khác nhau. |  ✅ `Done`| [Link]() |
| **Day 5** | **Tổng kết OOP - Thiết kế Hệ thống**:<br>- Tư duy phân rã bài toán thực tế<br>- Nguyên lý Đơn nhiệm (SRP) | Trong hàm `Main`, khởi tạo một mảng/danh sách các đối tượng kiểu `IConnectable` bao gồm cả Camera và Cảm biến. Dùng `foreach` để gọi kết nối đồng loạt. |  ✅ `Done` | [Link]() |

### 🗂️ Tuần 2: Cấu trúc dữ liệu & Giải thuật Ứng dụng trên C#

| Ngày | Bài học Lý thuyết | Bài tập Thực hành (C# Console App) | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :--- | :---: | :--- |
| **Day 6** | **Mảng động với `List<T>`**:<br>- Bản chất bộ nhớ Array vs List<br>- Đánh giá độ phức tạp thuật toán cơ bản | Khởi tạo `List<double> tempHistory`. Viết vòng lặp cho phép nhập nhiệt độ liên tục đến khi nhập `-99`. Sử dụng Linq tính Toán `.Average()`, `.Max()`. |  ✅ `Done` | [Link]() |
| **Day 7** | **Bộ đệm với `Queue<T>` (FIFO)**:<br>- Cơ chế Vào trước - Ra trước<br>- Ứng dụng quản lý Serial Buffer | Tạo `Queue<string> packetBuffer`. Giả lập nạp dữ liệu dồn dập bằng `.Enqueue()`. Dùng vòng lặp `while` bóc tách từng gói tin bằng `.Dequeue()` để xử lý tuần tự. |  ✅ `Done` | [Link]() |
| **Day 8** | **Tra cứu với `Dictionary<K, V>`**:<br>- Cơ chế cặp Key - Value<br>- Hàm băm và tốc độ tra cứu $O(1)$ | Khai báo `Dictionary<string, Device> deviceRegistry`. Thêm các thiết bị từ Day 3 với Key là mã thiết bị. Viết hàm tìm kiếm tức thời trạng thái thiết bị bằng `.ContainsKey()`. |  ✅ `Done` | [Link]() |
| **Day 9** | **Xử lý chuỗi (String Manipulation)**:<br>- Bản chất String trong C#<br>- Hàm `.Split()`, `.Replace()`, Parsing dữ liệu | Cho trước chuỗi thô: `"$DATA\|TEMP:28.5\|WATER:45.2#"`. Kiểm tra hợp lệ (`StartsWith`, `EndsWith`), cắt chuỗi bóc tách giá trị số và ép kiểu về `double`. | ✅ `Done` | [Link]() |
| **Day 10** | **Xử lý ngoại lệ (Exception Handling)**:<br>- Runtime Error vs Syntax Error<br>- Khối `try-catch-finally` | Truyền chuỗi lỗi `"$DATA\|TEMP:ERROR\|WATER:45.2#"` vào hàm Day 9. Bọc code bóc tách vào `try-catch`, bắt lỗi `FormatException`, giải phóng luồng trong `finally`. | ✅ `Done` | [Link]() |
| **Day 11** | **Lưu trữ dữ liệu phẳng (File I/O)**:<br>- Sử dụng `System.IO`<br>- Cơ chế đọc/ghi file CSV bằng `StreamWriter` | Tạo lớp `Logger` có phương thức `WriteLog(double temp, double waterLevel)`. Ghi nối tiếp (Append) dữ liệu kèm mốc thời gian thực `DateTime.Now` vào file `log_data.csv`. | ✅ `Done` | [Link]() |
| **Day 12** | **Bài tập Tổng hợp (Capstone)**:<br>- Khóa kết nối toàn bộ tư duy cũ<br>- Quản lý code sạch | Viết app Console chạy vòng lặp vô hạn, mỗi 2 giây giả lập nhận chuỗi thô từ Queue, bóc tách dữ liệu dưới khối `try-catch`, ghi log vào CSV. Đổi màu text Console khi lỗi. | ✅ `Done` | [Link]() |

### 🚀 Tuần 3 (Đầu): Git / GitHub Chuyên Nghiệp Dành Cho Dự Án

| Ngày | Bài học Lý thuyết | Bài tập Thực hành (Git/GitHub) | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :--- | :---: | :--- |
| **Day 13** | **Git cơ bản & Portfolio Cloud**:<br>- Kiến trúc Git (Staged, Committed)<br>- Cách viết Conventional Commits | Khởi tạo local repo. Kết nối remote GitHub. Đẩy toàn bộ Solution bài tập từ Day 1 đến Day 12 lên GitHub thành một portfolio hoàn chỉnh. | ✅ `Done` | [Link]() |
| **Day 14** | **Git Flow & Xử lý xung đột**:<br>- Quản lý nhánh (`feature/...`)<br>- Pull Request (PR) & Code Review | Giả lập phối hợp chỉnh sửa file trên 2 nhánh khác nhau để tạo xung đột (Merge Conflict). Hướng dẫn sinh viên cách dùng IDE đọc thẻ conflict và xử lý. | ⬜ ToDo | [Link]() |

---

## 🛠️ GIAI ĐOẠN 2: TRIỂN KHAI PROJECT (TUẦN 3 - TUẦN 8)

### 🔌 Tuần 3 & 4: Làm quen Phần cứng Arduino & Xây dựng Giao diện PC (Serial Port)

| Ngày | Công việc triển khai chi tiết | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :---: | :--- |
| **Day 15** | Cài đặt Arduino IDE. Lập trình đọc dữ liệu từ Cảm biến nhiệt độ phòng. Định dạng chuỗi truyền thông chuẩn gửi qua cổng Serial (USB). | ⬜ ToDo | [Link]() |
| **Day 16** | Tạo project **C# WPF App** (hoặc WinForms). Thiết kế UI: Khung kết nối COM Port, Nút bấm Connect/Disconnect, các ô hiển thị số liệu. | ⬜ ToDo | [Link]() |
| **Day 17** | Sử dụng thư viện `System.IO.Ports.SerialPort` trong C# để thiết lập kết nối, bắt sự kiện nhận dữ liệu tự động `DataReceived`. | ⬜ ToDo | [Link]() |
| **Day 18** | Viết hàm giải mã (Parse) chuỗi dữ liệu nhận được từ Arduino thời gian thực trên C# App. | ⬜ ToDo | [Link]() |
| **Day 19** | Áp dụng cơ chế bất đồng bộ (`Dispatcher.Invoke` trong WPF) để đẩy dữ liệu cảm biến lên UI một cách an toàn mà không gây đơ (freeze) giao diện. | ⬜ ToDo | [Link]() |

### 📸 Tuần 4 (Cuối) & Tuần 5: Tích hợp Webcam & Xử lý ảnh Vision Tìm Mực Nước (EmguCV)

| Ngày | Công việc triển khai chi tiết | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :---: | :--- |
| **Day 20** | Cài đặt thư viện **EmguCV** (OpenCV for C#) vào Project thông qua Nuget Package Manager. | ⬜ ToDo | [Link]() |
| **Day 21** | Lập trình nhúng luồng Webcam lên giao diện C# App. Đảm bảo hiển thị luồng video thời gian thực mượt mà. | ⬜ ToDo | [Link]() |
| **Day 22** | Nghiên cứu giải thuật lọc nhiễu ảnh và **Chuyển đổi ảnh xám (Grayscale)**, **Nhị phân hóa (Thresholding)** để tách biệt vùng nước và vùng trống trong can nhựa. | ⬜ ToDo | [Link]() |
| **Day 23** | Áp dụng thuật toán **Phát hiện biên (Canny Edge Detection)** hoặc **Tìm đường bao (Find Contours)** để định vị đường thẳng nằm ngang của mực nước. | ⬜ ToDo | [Link]() |
| **Day 24** | Viết thuật toán quy đổi toán học: Từ vị trí tọa độ Y (Pixel) của đường mực nước trong ảnh ra chiều cao mực nước thực tế (cm). | ⬜ ToDo | [Link]() |

### 🚨 Tuần 5 (Cuối) & Tuần 6: Xử lý logic Cảnh báo & Tối ưu luồng hệ thống

| Ngày | Công việc triển khai chi tiết | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :---: | :--- |
| **Day 25** | Thiết kế ô nhập "Ngưỡng cảnh báo" (Threshold) cho Nhiệt độ và Mực nước trực tiếp trên giao diện C# App. | ⬜ ToDo | [Link]() |
| **Day 26** | Viết logic so sánh liên tục dữ liệu đo (từ Arduino và từ Vision) với Ngưỡng cài đặt. Nếu vượt ngưỡng, kích hoạt trạng thái báo động. | ⬜ ToDo | [Link]() |
| **Day 27** | Lập trình hiệu ứng giao diện: **Nhấp nháy Đỏ/Trắng** tại vùng cảnh báo để thu hút sự chú ý của người dùng. | ⬜ ToDo | [Link]() |
| **Day 28** | Tích hợp tính năng tự động: Khi có Cảnh báo đỏ, ứng dụng tự động chụp lại một bức ảnh từ Webcam (Snapshot) lưu vào máy tính để lưu vết. | ⬜ ToDo | [Link]() |
| **Day 29** | Thiết lập truyền thông ngược: C# gửi lệnh xuống Arduino để bật còi Buzzer hoặc đèn LED cảnh báo vật lý trên bo mạch khi phần mềm báo động. | ⬜ ToDo | [Link]() |

### 📊 Tuần 7: Hoàn thiện Hệ thống, Ghi dữ liệu & Đóng gói

| Ngày | Công việc triển khai chi tiết | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :---: | :--- |
| **Day 30** | Tích hợp module `Logger` (đã học ở tuần 2) để tự động lưu lịch sử các phiên cảnh báo vào file `history_alarm.csv` (Thời gian, Giá trị, Tên file ảnh lưu vết). | ⬜ ToDo | [Link]() |
| **Day 31** | **Refactoring Code**: Tối ưu hóa cấu trúc mã nguồn, chia tách các tầng xử lý (UI, Serial, Vision, IO) thành các Class/Namespace riêng biệt theo chuẩn OOP. | ⬜ ToDo | [Link]() |
| **Day 32** | Thực hiện các bài kiểm thử chịu tải (Stress test), tối ưu hiệu năng xử lý ảnh để giảm thiểu mức chiếm dụng tài nguyên CPU của máy tính. | ⬜ ToDo | [Link]() |
| **Day 33** | Đóng gói cố định phần cứng (Bo mạch Arduino, Cảm biến nhiệt độ) gọn gàng. Đóng gói phần mềm C# thành file cài đặt độc lập (`.exe`). | ⬜ ToDo | [Link]() |
| **Day 34** | Sinh viên hoàn thiện file `README.md` chính thức cho dự án: Mô tả hệ thống, sơ đồ khối kết nối, hướng dẫn cài đặt và sử dụng kèm hình ảnh demo. | ⬜ ToDo | [Link]() |

### 🎓 Tuần 8: Viết báo cáo Đồ án & Bảo vệ thử (Mock Defense)

| Ngày | Công việc triển khai chi tiết | Trạng thái | Tài liệu / Source Code |
| :---: | :--- | :---: | :--- |
| **Day 35 - 36** | Hướng dẫn cấu trúc quyển báo cáo đồ án kỹ thuật. Sinh viên tập trung viết nội dung (Cơ sở lý thuyết, Thuật toán xử lý ảnh EmguCV, Kết quả đạt được). | ⬜ ToDo | [Link]() |
| **Day 37 - 38** | Thiết kế Slide thuyết trình đồ án chuyên nghiệp. Thực hiện chạy thử nghiệm liên tục hệ thống trong phòng Lab để thu thập số liệu thực nghiệm. | ⬜ ToDo | [Link]() |
| **Day 39** | Hướng dẫn sinh viên kỹ năng thuyết trình, kỹ năng demo sản phẩm trực quan trước hội đồng. | ⬜ ToDo | [Link]() |
| **Day 40** | **Buổi Bảo vệ thử (Mock Defense)**: Sinh viên báo cáo, Mentor đóng vai trò Hội đồng phản biện đặt câu hỏi chất vấn (Về kiến thức OOP, Luồng xử lý ảnh, C# Async). Tổng kết và kết thúc thực tập. | ⬜ ToDo | [Link]() |

---
## 💡 HƯỚNG DẪN DÀNH CHO SINH VIÊN
1. **Quản lý tiến độ**: Thay đổi ký hiệu trạng thái trong bảng thành:
   - ⬜ `ToDo` (Chưa làm)
   - 🔄 `In Progress` (Đang làm)
   - ✅ `Done` (Đã hoàn thành và commit)
2. **Nhật ký hàng ngày**: Tạo một file DailyWork.md để ghi chú những gì đã học và commit cùng source code mỗi ngày.
   - Hôm nay làm được gì?
   - Vướng mắc/Bug gặp phải là gì?
   - Đã giải quyết thế nào/Cần Mentor hỗ trợ gì?

