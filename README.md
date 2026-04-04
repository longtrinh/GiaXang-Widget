# ⛽ Widget Giá Xăng Dầu Petrolimex

Một ứng dụng widget nhỏ gọn trên Desktop dành cho hệ điều hành Windows, giúp bạn theo dõi giá bán lẻ xăng dầu trực tiếp từ trang chủ Petrolimex một cách trực quan, nhanh chóng và không làm phiền đến không gian làm việc.
<p align="center"><img width="404" height="283" alt="image" src="https://github.com/user-attachments/assets/4b04b3df-7217-42ec-a465-08dfe60272e8" /></p>

## 🌟 Chức năng nổi bật

* **Cập nhật thời gian thực:** Tự động đồng bộ và lấy dữ liệu giá xăng dầu (RON 95, E5, DO, Dầu hỏa...) mới nhất trực tiếp từ web Petrolimex mỗi 1 tiếng.
* **Giao diện "Tàng hình" (Minimalist):** Thiết kế không viền (borderless), bán trong suốt, hòa hợp hoàn toàn với hình nền desktop của bạn và tự động nằm dưới các cửa sổ làm việc khác.
* **Siêu nhẹ & Tối ưu RAM:** Được tối ưu hóa sâu ở mức hệ thống. Ứng dụng tự động đóng băng tiến trình WebView2 và giải phóng RAM (đẩy bộ nhớ vào disk) ngay sau khi lấy dữ liệu xong, giúp máy tính hoạt động mượt mà dù treo app 24/7.
* **Khởi động cùng hệ thống:** Tự động kích hoạt khi bạn khởi động Windows.
* **Khay hệ thống (System Tray):** Hoạt động tĩnh lặng dưới khay hệ thống, cung cấp menu ngữ cảnh tiện lợi để ẩn icon hoặc đóng ứng dụng.
* **Ghi nhớ vị trí thông minh:** Tự động lưu lại tọa độ vị trí cuối cùng trên màn hình để sử dụng cho lần khởi động tiếp theo.

## 💻 Yêu cầu hệ thống

* **Hệ điều hành:** Windows 10 hoặc Windows 11 (64-bit).
* **Kết nối Internet:** Bắt buộc để tải về bảng giá.

## 🚀 Hướng dẫn cài đặt và sử dụng

### 1. Cài đặt nhanh
1. Truy cập vào mục **[Releases]** ở menu bên phải của Repository này.
2. Tải xuống file `PetrolimexWidget.zip` ở phiên bản mới nhất.
3. Giải nén file `PetrolimexWidget.zip` vào một thư mục cố định trên máy (ví dụ: `D:\Tools\PetrolimexWidget\`).
4. Click đúp vào file PetrolimexWidget.exe để khởi chạy ứng dụng trực tiếp.
5. Widget sẽ tự động khởi chạy cùng Windows.

### 2. Các thao tác điều khiển Widget
* **Di chuyển:** Nhấn giữ chuột trái vào dòng tiêu đề, sau đó kéo để di chuyển Widget đến vị trí bạn muốn và lưu lại vị trí này cho lần khởi động sau.
* **Tương tác qua khay hệ thống (Góc dưới bên phải Taskbar):**
    * Click chuột phải vào biểu tượng của Widget.
    * Chọn **"Hide taskbar tray icon"** nếu bạn muốn ẩn hẳn biểu tượng này đi cho gọn mắt (ứng dụng vẫn sẽ chạy ngầm và hiển thị trên màn hình).
    * Chọn **"Close widget"** để tắt ứng dụng.

## 🛠 Hướng dẫn cho Lập trình viên (Build từ Source Code)

Nếu bạn muốn tự tùy biến hoặc đóng gói lại ứng dụng:
1. Clone project về máy tính: `git clone https://github.com/longtrinh/GiaXang-Widget.git`
2. Mở file Solution (`.sln`) bằng **Visual Studio 2026** (hoặc các phiên bản tương thích).
3. Đảm bảo chế độ Build đang được đặt là **Release**.
4. Click chuột phải vào Project `PetrolimexWidget` trong Solution Explorer > Chọn **Publish**.
5. Cấu hình Target là **Folder**. Ở phần cài đặt Publish (Show all settings), chọn **Produce single file** và **Framework-dependent** để xuất ra một file `.exe` duy nhất cho gọn nhẹ.
6. Nhấn **Publish** và lấy file thành phẩm trong thư mục đích.

---
*Phát triển bởi Long (ttlong) sử dụng Gemini AI 100%.*
