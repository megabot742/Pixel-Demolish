# Pixel Demolish
Casual Pixel Destruction game

# Unity version: 2022.3.62f3 

# Hướng dẫn mở project
- Clone từ github về, lựa chọn version Unity phù hợp
- Lưu ý là Project ở định dạng URP
- Mở thư mục Scene, chọn scene Menu làm nơi bắt đầu và chạy game

# Các code system chính:
- Liên quan đến các khối:
  - PixelCube.cs: quản lí tách rời khỏi thực thể cha, phá hủy khối
  - Enity.cs: quản lí vật lí của thực thể bao gồm nhiều PixelCube liên kết lại. Và tạo tác hàm xử lí khi pha chạm hoặc tách thành thực thể nhỏ hơn
  - Saw.cs: xử lí va chạm từ các PixelCube, từ đó mới tách PixelCube đã va chạm ra khỏi Enity cha
  - Gear.cs: xử lí va chạm và tiêu thủ các khối PixelCube

- Liên quan đến xây dựng Saw:
  - BuilldPromptUI.cs : xử lí của UI button world space, đùng để gọi BuildManager.cs khi người chơi tương tác và điểm Button tại vị trí xuất hiện trên mình hình
  - BuildManager.cs :  xử lí sinh ra Saw ở tại điểm PointBuild, thông qua kiểm tra lượng Coin trong CoinManager 
  - PointBuild.cs: là script định danh nằm ở các gameobject PointBuild nằm rải rác ở từng level. giúp mỗi khi load scene thì buildManager.cs sẽ tự động bắt dc PointBuild đúng ở từng scene. Điều này giúp sinh ra Saw đúng vị trí, và bỏ các UI button sau khi build xong tại điểm PointBuild nào đấy

- Liên quan đến quản lí: 
  - UIManager: xủ lí UI ẩn hiện
  - UIEventManager: xử lí event liên quan đến UI
  - AudioManager: xử lí âm thanh, nhạc thông qua kiểm soát Audio Mixed và lưu bằng PlayerPrefs
  - SpawnManager: xử lí spawn các Enity ở các màn chơi
  - CoinManager: quản lí xu ở các màn chơi, tăng lên khi Gear phá hủy PixelCube, giảm khi xây dựng Saw
  - Resualt: quản lí exp ở từng màng chơi, tăng lên khi Gear phá hủy PixelCube

# Những cải thiện nếu có thêm thời gian
  - Thêm màn, thêm Enity
  - Làm tool giúp tạo Enity nhanh hơn
